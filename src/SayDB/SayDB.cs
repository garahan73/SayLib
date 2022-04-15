using Say32.DB.Core;
using Say32.DB.Core.Database;
using Say32.DB.Core.Events;
using Say32.DB.Core.Exceptions;
using Say32.DB.Core.Indexes;
using Say32.DB.Core.Keys;
using Say32.DB.Driver;
using Say32.DB.Object;
using Say32.DB.Server.FileSystem;
using Say32.DB.Store;
using Say32.DB.Store.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Say32.DB
{
    public enum DbStorageType { Memory, File };

    /// <summary>
    ///     Base class for a SayDB database instance
    /// </summary>
    public partial class SayDB
    {
        internal DbCore Core { get; private set; }

        public DbDriver Driver => Core.Driver;

        /// <summary>
        ///     The name of the database instance
        /// </summary>
        public string Name => Core.Name;

        #region DATA STORE

        public StoreMap StoreMap => Core.StoreMap;


        /// <summary>
        ///     Non-generic registration check
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>True if it is registered</returns>
        public bool IsRegistered(Type type) => IsRegistered(DataStoreID.FromType(type));

        public bool IsRegistered(DataStoreID storeID) => StoreMap.Has(storeID);

        #endregion


        #region PRIVATE CORE DELEGATES
        private Task<IDisposable> LockAsync() => Core.LockAsync();

        private Dictionary<DataStoreID, IStoreDefinition> StoreDefinitions => Core.StoreDefinitions;

        private TaskCounter TaskCounter => Core.TaskCounter;

        #endregion

        internal SayDB(DbCore core)
        {
            Core = core;
        }

        public void Unload()
        {
            FlushAsync().Wait();
        }

        #region TRIGGER & INTERCEPTOR

        

        /// <summary>
        ///     Register a trigger
        /// </summary>
        /// <param name="trigger">The trigger</param>
        public void RegisterTrigger<T, TKey>(BaseDbTrigger<T, TKey> trigger) where T : class
            => Core.Triggers.RegisterTrigger<T, TKey>(trigger);

        /// <summary>
        ///     Unregister the trigger
        /// </summary>
        /// <param name="trigger">The trigger</param>
        public void UnregisterTrigger<T, TKey>(BaseDbTrigger<T, TKey> trigger) where T : class
            => Core.Triggers.UnregisterTrigger<T, TKey>(trigger);


        /// <summary>
        /// Registers the BaseSayDBByteInterceptor
        /// </summary>
        /// <typeparam name="T">The type of the interceptor</typeparam>
        public void RegisterInterceptor<T>() where T : BaseDbByteInterceptor => Core.Triggers.RegisterInterceptor<T>();

        public void UnRegisterInterceptor<T>() where T : BaseDbByteInterceptor => Core.Triggers.UnRegisterInterceptor<T>();

        /// <summary>
        /// Clears the _byteInterceptorList object
        /// </summary>
        public void UnRegisterInterceptors() => Core.Triggers.UnRegisterInterceptors();


        #endregion


        #region CREATE & DROP STORE

        public IoContext CreateIoContext() => Core.CreateIoContext();

        //todo: to be removed - temporarily remained for unit test compliance
        public DataStore<T, TKey> CreateDataStore<T, TKey>(Func<T, TKey> getKeyFunc, DbObjectType dbObject = null) where T : class
            => CreateDataStore<T, TKey>(DataStoreID.FromType<T>(), getKeyFunc, dbObject);

        //public DataStore<T, TKey> CreateDataStore<T, TKey>(string storeName, Func<T, TKey> getKeyFunc, DbObjectType dbObject = null) where T : class
            //=> CreateDataStore<T, TKey>(DataStoreID.FromName(storeName), getKeyFunc, dbObject);

        public DataStore<T, TKey> CreateDataStore<T, TKey>(DataStoreID id, Func<T, TKey> getKeyFunc, DbObjectType dbObjectType = null) where T : class
        {
            DbObjectType dbo = dbObjectType ?? CreateDbObjectType<T>(id);
            Core.DbObjectTypeManager.Register(id, dbo);

            var store = new DataStore<T, TKey>(Core, id, getKeyFunc, dbo);
            Core.RegisterDataStore(store);
            return store;
        }

        private DbObjectType CreateDbObjectType<T>(DataStoreID store) where T : class
        {
            var type = typeof(T);

            if (Core.Serializer.CanHandle(type))
                return new DbSerializerObjectType(type, Core.Serializer);
            else
                return new DbClassObjectType(type, store, Core);
        }

        public async Task DropStoreAsync(DataStoreID id)
        {
            await Core.DropStoreAsync(id);
        }

        #endregion


        #region QUERIES

        public List<DbKeyDef<T, TKey>> KeyQuery<T, TKey>() where T : class => KeyQuery<T, TKey>();

        /// <summary>
        ///     Query (keys only)
        /// </summary>
        /// <typeparam name="T">The type to query</typeparam>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <returns>The list of keys to query</returns>
        public IEnumerable<DbKey<T, TKey>> KeyQuery<T, TKey>(DataStoreID store) where T : class //, new()
        {
            if (!IsRegistered(store))
            {
                throw new DbStoreNotFoundException(store, Name);
            }

            return ((StoreDefinition<T, TKey>)Core.StoreDefinitions[store]).KeyList.Query(Core.CreateIoContext());
        }

        public IEnumerable<DbIndex<T, TIndex, TKey>> IndexQuery<T, TIndex, TKey>(string indexName) where T : class
            => IndexQuery<T, TIndex, TKey>(null, indexName);

        /// <summary>
        ///     Query an index
        /// </summary>
        /// <typeparam name="T">The table type</typeparam>
        /// <typeparam name="TIndex">The index type</typeparam>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <param name="indexName">The name of the index</param>
        /// <returns>The indexed items</returns>
        public IEnumerable<DbIndex<T, TIndex, TKey>> IndexQuery<T, TIndex, TKey>(DataStoreID storeID, string indexName) where T : class //, new()
        {
            storeID = storeID ?? DataStoreID.FromType<T>();

            if (string.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException("indexName");
            }

            var store = StoreMap[storeID];

            if (store == null)
            {
                throw new DbStoreNotFoundException(storeID, Name);
            }

            return ((DataStore<T, TKey>)store).IndexQuery<TIndex>(indexName);
            
        }

        #endregion

        #region SAVE

        /// <summary>
        ///     Save it
        /// </summary>
        /// <typeparam name="T">The instance type</typeparam>
        /// <typeparam name="TKey">Save it</typeparam>
        /// <param name="instance">The instance</param>
        public async Task<TKey> SaveAsync<T, TKey>(T instance, IoContext context = null) where T : class
        {
            return await Core.SaveAsync<TKey>(typeof(T), instance, context).ConfigureAwait(false);
        }


        /// <summary>
        ///     Save an instance against a base class table definition
        /// </summary>
        /// <typeparam name="T">The table type</typeparam>
        /// <typeparam name="TKey">Save it</typeparam>
        /// <param name="instance">An instance or sub-class of the table type</param>
        /// <returns></returns>
        public async Task<TKey> SaveAsAsync<T, TKey>(T instance) where T : class //, new()
        {
            return await Core.SaveAsync<TKey>(typeof(T), instance).ConfigureAwait(false);
        }

        /// <summary>
        ///     Save an instance against a base class table definition
        /// </summary>
        /// <typeparam name="T">The table type</typeparam>
        /// <param name="instance">An instance or sub-class of the table type</param>
        /// <returns></returns>
        public async Task<object> SaveAsAsync<T>(T instance) where T : class //, new()
        {
            return await Core.SaveAsync<object>(typeof(T), instance).ConfigureAwait(false);
        }

        /// <summary>
        ///     Save against a base class when key is not known
        /// </summary>
        /// <param name="type"></param>
        /// <param name="instance">The instance</param>
        /// <returns>The key</returns>
        public async Task<object> SaveAsAsync(Type type, object instance, IoContext context = null)
        {
            if (!DbWorkSpace.PlatformAdapter.IsSubclassOf(instance.GetType(), type) || instance.GetType() != type)
            {
                throw new SayDBException(string.Format("{0} is not of type {1}", instance.GetType().Name, type.Name));
            }

            return await Core.SaveAsync<object>(type, instance, context).ConfigureAwait(false);
        }

        /// <summary>
        ///     Entry point for save
        /// </summary>
        /// <param name="type">Type to save</param>
        /// <param name="instance">Instance</param>
        /// <returns>The key saved</returns>
        public async Task<object> SaveAsync(Type type, object instance, IoContext context = null)
        {
            return await Core.SaveAsync<object>(type, instance, context);
        }

        /// <summary>
        ///     Save when key is not known
        /// </summary>
        /// <typeparam name="T">The type of the instance</typeparam>
        /// <param name="instance">The instance</param>
        /// <returns>The key</returns>
        public async Task<object> SaveAsync<T>(T instance, IoContext context = null) where T : class //, new()
        {
            return await Core.SaveAsync<object>(typeof(T), instance, context);
        }

        public async Task<object[]> SaveAsync<T>(IEnumerable<T> items) where T : class //, new()
        {
            var context = Core.CreateIoContext();

            var list = new List<object>();

            foreach (T item in items)
            {
                list.Add(await SaveAsync(item, context));
            }
            return list.ToArray();
        }

        public object Save<T>(T instance) where T : class// //, new()
        {
            var key = SaveAsync(typeof(T), instance).Result;
            _ = FlushAsync();
            return key;
        }

        public object[] Save<T>(IEnumerable<T> items) where T : class //, new()
        {
            var keys = SaveAsync<T>(items).Result;
            _ = this.FlushAsync();
            return keys.ToArray();
        }

        public object[] Save<T>(params T[] items) where T : class => Save((IEnumerable<T>)items);

        #endregion
        
        #region LOAD

        public T Load<T>(object key) where T : class// //, new()
        {
            return LoadAsync<T>(key).Result;
        }

        public async Task<T> LoadAsync<T, TKey>(string storeName, TKey key) where T : class //, new()
        {
            using (await Core.LockAsync().ConfigureAwait(false))
            {
                return await Core.LoadAsyncInternal<T, TKey>(storeName, key).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Load it 
        /// </summary>
        /// <typeparam name="T">The type to load</typeparam>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <param name="key">The value of the key</param>
        /// <returns>The instance</returns>
        public async Task<T> LoadAsync<T, TKey>(TKey key) where T : class //, new()
        {
            using (await Core.LockAsync().ConfigureAwait(false))
            {
                return await Core.LoadAsyncInternal<T, TKey>(typeof(T), key).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Load it (key type not typed)
        /// </summary>
        /// <typeparam name="T">The type to load</typeparam>
        /// <param name="key">The key</param>
        /// <returns>The instance</returns>
        public async Task<T> LoadAsync<T>(object key) where T : class //, new()
        {
            using (await Core.LockAsync().ConfigureAwait(false))
            {
                return await Core.LoadAsyncInternal<T, object>(typeof(T), key).ConfigureAwait(false);
            }
        }

        public async Task<object> LoadAsync(DataStoreID storeID, object key)
        {
            using (await Core.LockAsync().ConfigureAwait(false))
            {
                return await Core.LoadAsyncInternal<object, object>(storeID, key).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Load it without knowledge of the key type
        /// </summary>
        /// <param name="type">The type to load</param>
        /// <param name="key">The key</param>
        /// <param name="cache">Cache queue</param>
        /// <returns>The instance</returns>
        public async Task<object> LoadAsync(Type type, object key, IoContext context = null)
        {
            using (await Core.LockAsync().ConfigureAwait(false))
            {
                return await Core.LoadAsyncInternal<object, object>(type, key, context).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Delete it 
        /// </summary>
        /// <typeparam name="T">The type to delete</typeparam>
        /// <param name="instance">The instance</param>
        public async Task DeleteAsync<T>(T instance) where T : class
        {
            var id = DataStoreID.FromType(typeof(T));
            await DeleteAsync(id, StoreDefinitions[id].GetKey(instance));
        }

        #endregion

        #region DELETE
        /// <summary>
        ///     Delete it (non-generic)
        /// </summary>
        /// <param name="id">The type</param>
        /// <param name="key">The key</param>
        public async Task DeleteAsync(DataStoreID id, object key) => await Core.DeleteAsync(id, key);
       

        /// <summary>
        ///     Truncate all records for a type
        /// </summary>
        /// <param name="id">The type</param>
        public async Task DeleteAllAsync(DataStoreID storeID) => await Core.DeleteAllAsync(storeID);
        public void DeleteAll(DataStoreID storeID) => DeleteAllAsync(storeID).Wait();

        #endregion

        #region ETC

        /// <summary>
        ///     Flush all keys and indexes to storage
        /// </summary>
        public async Task FlushAsync() => await Core.FlushAsync();

        /// <summary>
        ///     Purge the entire database - wipe it clean!
        /// </summary>
        public async Task PurgeAsync() => await PurgeAsync();

        /// <summary>
        ///     Refresh indexes and keys from disk
        /// </summary>
        public async Task RefreshAsync() => await Core.RefreshAsync();

        #endregion

        #region EVENT

        public event EventHandler<DbOperationArgs> DbOperationPerformed;

        internal void RaiseOperationEvent(DbOperation operation, DataStoreID StoreID, object key)
        {
            DbOperationPerformed?.Invoke(this, new DbOperationArgs(operation, StoreID, key));
        }

        #endregion
    }
}