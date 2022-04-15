using Nito.AsyncEx;
using Say32.DB.Core;
using Say32.DB.Core.Events;
using Say32.DB.Core.Exceptions;
using Say32.DB.Core.Indexes;
using Say32.DB.Core.Keys;
using Say32.DB.Object;
using Say32.DB.Store;
using Say32.DB.Store.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Say32.DB
{
    public abstract class DataStore
    {
        internal TaskCounter TaskCounter { get; } = new TaskCounter();

        internal DbCore DbCore { get; private set; }

        public PropertyMap PropertyMap { get; protected set; }

        public abstract IStoreDefinition IDef { get; }

        public DataStoreID ID => IDef.ID;

        public DbObjectType DbObjectType { get; }

        internal DataStore(DbCore core, DbObjectType dbObject)
        {
            DbCore = core;
            DbObjectType = dbObject;
        }

        public abstract bool CanHandle(Type propertyType);

        public abstract Task<object> SaveAsync(object instance, IoContext context = null);
        public abstract object Save(object instance, IoContext context = null);

        public abstract Task<object> LoadAsync(object key, IoContext context = null);
        public abstract object Load(object key, IoContext context = null);


        public abstract Task<bool> DeleteAsync(object keyOrInstance);
        public bool Delete(object keyOrInstance) => DeleteAsync(keyOrInstance).Result;

        /// <summary>
        ///     Truncate all records for a store
        /// </summary>
        /// <param name="storeID">The store ID</param>
        public async Task DeleteAllAsync() => await new StoreIoJob(this).DeleteAllAsync();

        public void DeleteAll() => DeleteAllAsync().Wait();

        internal async Task UpdateIndexes(object instance, object key)
        {
            foreach (var index in IDef.Indices.Values)
            {
                await index.AddIndexAsync(instance, key).ConfigureAwait(false);
            }
        }

        public int Count => IDef.Keys.Count;
    }


    public abstract class DataStore<T> : DataStore
    where T : class
    {

        internal DataStore(DbCore dbCore, DataStoreID id, DbObjectType dbObject) : base(dbCore, dbObject)
        {
        }

        public override bool CanHandle(Type type)
        {
            return typeof(T).IsAssignableFrom(type);
        }


    }



    public class DataStore<T, TKey> : DataStore<T>
        where T : class
    {

        internal StoreDefinition<T, TKey> Def { get; private set; }
        public override IStoreDefinition IDef => Def;

        internal DataStore(DbCore dbCore, DataStoreID id, Func<T, TKey> getKeyFunc, DbObjectType dbObject) : base(dbCore, id, dbObject)
        {
            id = id ?? DataStoreID.FromType(typeof(T));

            Def = new StoreDefinition<T, TKey>(id, dbCore.Driver, LoadAsync, getKeyFunc);

            PropertyMap = new PropertyMap(id, typeof(T));

        }

        public override bool CanHandle(Type type)
        {
            return typeof(T).IsAssignableFrom(type);
        }

        public DataStore<T, TKey> WithIndex<TIndex>(string name, Func<T, TIndex> indexer)
        {
            Def.RegisterIndex(name, indexer);
            return this;
        }

        #region LOAD
        
        public async Task<T> LoadAsync(TKey key, IoContext context = null)
        {
            var ioJob = new StoreIoJob(this, context);
            return await ioJob.Load<T>(key).ConfigureAwait(false);
        }

        public override async Task<object> LoadAsync(object key, IoContext context = null)
        {
            return await LoadAsync((TKey)key, context).ConfigureAwait(false);
        }


        public T Load(TKey key, IoContext context = null) => LoadAsync(key, context).Result;

        public override object Load(object key, IoContext context = null) => Load((TKey)key, context);

        public async Task<T[]> LoadAsync(IEnumerable<TKey> keys, IoContext context=null)
        {
            return await Task.Run(() =>
            {
                context = context ?? DbCore.CreateIoContext();

                var array = new T[keys.Count()];
                var tasks = new Task[keys.Count()];

                var index = 0;

                foreach (var key in keys)
                {
                    var i = index++;
                    tasks[i] = LoadAsync(key, context).ContinueWith(t => array[i] = t.Result);
                }

                Task.WaitAll(tasks);

                return array;
            });
        }

        public async Task<T[]> LoadAsync(params TKey[] keys) => await LoadAsync((IEnumerable<TKey>)keys, null);

        public async Task<IEnumerable<T>> LoadAllAsync() => await KeyQuery().Select(k => k.Value);

        public IEnumerable<T> LoadAll() => LoadAllAsync().Result;

        public async Task<T[]> LoadByIndexAsync<TIndex>(string indexName, TIndex indexValue, IoContext context=null)
        {
            return (await from i in IndexQuery<TIndex>(indexName) where i.Index.Equals(indexValue) select i.Value).ToArray();
        }

        #endregion

        #region SAVE

        internal async Task<TKey> SaveAsyncMain(T instance, IoContext context)
        {
            DbError.Assert<SayDBException>(instance != null, "Can't save null object");

            var saveJob = new StoreIoJob(this, context);
            return await saveJob.Save<TKey>(instance);
        }

        public override async Task<object> SaveAsync(object instance, IoContext context = null)
        {
            return await SaveAsyncMain((T)instance, context).ConfigureAwait(false);
        }

        public async Task<TKey> SaveAsync(T instance, IoContext context = null)
        {
            return await SaveAsyncMain(instance, context);
        }

        public TKey Save(T instance, IoContext context = null)
        {
            var key = SaveAsyncMain(instance, context).Result;
            _ = DbCore.FlushAsync();
            return key;
        }
        public override object Save(object instance, IoContext context = null) => Save((T)instance, context);

        #endregion

        #region SAVE MULTIPLE OBEJCTS

        public async Task<TKey[]> SaveAsync(IEnumerable<T> items, IoContext context = null)
        {
            return await Task.Run(() =>
            {
                context = context ?? DbCore.CreateIoContext();

                var array = new TKey[items.Count()];
                var tasks = new Task[items.Count()];

                var index = 0;

                foreach (var item in items)
                {
                    var i = index++;
                    tasks[i] = SaveAsync(item, context).ContinueWith(t => array[i] = t.Result);                    
                }

                Task.WaitAll(tasks);

                return array;
            });
        }

        public async Task<TKey[]> SaveAsync(params T[] items)
        {
            return await SaveAsync((IEnumerable<T>)items);
        }

        public TKey[] Save(IEnumerable<T> items)
        {
            var keys = SaveAsync(items).Result;
            _ = DbCore.FlushAsync();
            return keys;
        }

        public TKey[] Save(params T[] items)
        {
            return Save((IEnumerable<T>)items);
        }

        #endregion

        #region QUERIES

        public IEnumerable<DbKey<T, TKey>> KeyQuery()
        {
            return Def.KeyList.Query(DbCore.CreateIoContext());
        }

        public IEnumerable<DbIndex<T, TIndex, TKey>> IndexQuery<TIndex>(string indexName)
        {
            var indexCollection = Def.Indices[indexName] as IndexCollection<T, TIndex, TKey>;

            if (indexCollection == null)
            {
                throw new DbIndexNotFoundException(indexName, ID);
            }

            return indexCollection.Query(DbCore.CreateIoContext());
        }

        #endregion

        #region DELETE

        public async override Task<bool> DeleteAsync(object keyOrInstance) 
            =>  keyOrInstance == null ? false :
                keyOrInstance is TKey key ? await DeleteAsync(key).ConfigureAwait(false) :
                keyOrInstance is T instance ? await DeleteAsync(instance).ConfigureAwait(false) :
                throw new SayDbTypeException("Can't delete from DB. provided object is neither key or instance.");

        public async Task<bool> DeleteAsync(T instance)
        {
            if (instance == null)
                return false;

            var key = Def.GetKey(instance);
            return await DeleteAsync(key);
        }

        public async Task<bool> DeleteAsync(TKey key)
        {
            if (key == null)
                return false;

            DbCore.TaskCounter.Increase();

            try
            {
                var storeID = ID;

                // call any before save triggers 
                foreach (var trigger in DbCore.Triggers.GetStoreTriggers(storeID).Where(trigger => !trigger.BeforeDelete(storeID, key)))
                {
                    throw new SayDBTriggerException(string.Format(ExceptionMessageFormat.BaseDatabaseInstance_Delete_Delete_failed_for_type, storeID), trigger.GetType());
                }

                var storeDef = Def;

                var keyEntry = storeDef.Keys.GetIndexForKeyAsync(key);

                if (keyEntry < 0)
                    return false;

                await DbCore.Driver.DeleteAsync(storeID, keyEntry).ConfigureAwait(false);

                await storeDef.Keys.RemoveKeyAsync(key).ConfigureAwait(false);

                foreach (var index in storeDef.Indices.Values)
                {
                    await index.RemoveIndexAsync(key).ConfigureAwait(false);
                }

                DbCore.RaiseOperationEvent(DbOperation.Delete, storeID, key);

                return true;
            }
            finally
            {
                DbCore.TaskCounter.Decrease();
            }
        }


        #endregion


    }
}
