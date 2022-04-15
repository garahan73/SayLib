using Nito.AsyncEx;
using Say32.DB.Core.Events;
using Say32.DB.Core.Exceptions;
using Say32.DB.Core.Serialization;
using Say32.DB.Core.TypeSupport;
using Say32.DB.Driver;
using Say32.DB.Object;
using Say32.DB.Object.Properties;
using Say32.DB.Store;
using Say32.DB.Store.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Say32.DB.Core
{

    /// <summary>
    ///     Base class for a SayDB database instance
    /// </summary>
    internal class DbCore
    {
        internal SayDB Database => DbWorkSpace.GetDatabase(Name);

        public DbDriver Driver { get; private set; }

        public StoreMap StoreMap { get; } = new StoreMap();

        public string Name => Driver.DatabaseInstanceName;

        internal IDbSerializer Serializer => DbWorkSpace.Serializers;

        internal TaskCounter TaskCounter { get; } = new TaskCounter();

        internal DbObjectTypeManager DbObjectTypeManager { get; private set; }

        internal TypeManager TypeManager { get; private set; }

        /// <summary>
        ///     List of triggers
        /// </summary>
        internal TriggerManager Triggers { get; } = new TriggerManager();

        private readonly AsyncLock _lock = new AsyncLock();

        internal IoContext CreateIoContext()
        {
            return new IoContext(Database);
        }


        /// <summary>
        ///     The table definitions
        /// </summary>
        internal Dictionary<DataStoreID, IStoreDefinition> StoreDefinitions => StoreMap.Defs;

        public DbCore(DbDriver driver)
        {
            Driver = driver;

            DbObjectTypeManager = new DbObjectTypeManager(Driver.GetTypeIndexAsync, this);

            TypeManager = new TypeManager(Driver);

            Driver.PublishDataStores(null, DbWorkSpace.TypeResolver.ResolveTableType);
        }

        internal SayDB CreateDatabase()
        {
            var database = new SayDB(this);
            DbOperationPerformed += (sender, arg) => database.RaiseOperationEvent(arg.Operation, arg.StoreID, arg.Key);

            return database;
        }

        internal DataStoreID RegisterDataStore<T, TKey>(DataStore<T, TKey> store) where T : class
        {
            lock (StoreMap)
            {
                if (StoreMap.Has(store.ID))
                {
                    throw new DbDuplicateTypeException(store.ID, Name);
                }

                //Debug.WriteLine($"[DB] Registering Data Store. {store.ID}");

                StoreMap.Add(store);
            }

            lock (Driver)
            {
                Driver.PublishDataStores(StoreDefinitions, DbWorkSpace.TypeResolver.ResolveTableType);
            }
            return store.Def.ID;
        }

        internal async Task DropStoreAsync( DataStoreID id )
        {
            DataStore store = null;

            lock (StoreMap)
            {
                if (!StoreMap.Has(id))
                {
                    return;
                }

                store = StoreMap.Drop(id);
            }

            var dbo = store.DbObjectType;
            DbObjectTypeManager.Unregister(id);

            using ( await _lock.LockAsync().ConfigureAwait(false))
            {
                await Driver.TruncateAsync(id);
            }
        }



        internal async Task<IDisposable> LockAsync()
        {
            return await _lock.LockAsync().ConfigureAwait(false);
        }





        #region PROPERTY CONVERSION

        private readonly Dictionary<Type, IDbPropertyConverter> _propertyConverters = new Dictionary<Type, IDbPropertyConverter>();

        internal bool TryGetPropertyConverter(Type type, out IDbPropertyConverter propertyConverter)
        {
            return _propertyConverters.TryGetValue(type, out propertyConverter);
        }

        /// <summary>
        ///     Registers a property converter.
        /// </summary>
        /// <param name="propertyConverter">The property converter</param>
        protected void RegisterPropertyConverter(IDbPropertyConverter propertyConverter)
        {
            _propertyConverters.Add(propertyConverter.IsConverterFor(), propertyConverter);
        }

        #endregion


        #region [IO] SAVE, LOAD, DELETE

        // save
        internal async Task<TKey> SaveAsync<TKey>(string storeName, object instance, IoContext context = null)
        {
            return await SaveAsync<TKey>(DataStoreID.FromName(storeName), instance, context);
        }

        internal async Task<TKey> SaveAsync<TKey>(Type type, object instance, IoContext context = null)
        {
            return await SaveAsync<TKey>(DataStoreID.FromType(type), instance, context);
        }

        internal async Task<TKey> SaveAsync<TKey>(DataStoreID storeID, object instance, IoContext context = null)
        {
            var store = StoreMap[storeID];
            return (TKey)await store.SaveAsync(instance, context);
        }

        // load
        internal async Task<T> LoadAsyncInternal<T, TKey>(string storeName, TKey key, IoContext context = null) where T : class
        {
            return await LoadAsyncInternal<T, TKey>(DataStoreID.FromName(storeName), key, context);
        }

        internal async Task<T> LoadAsyncInternal<T, TKey>(Type type, TKey key, IoContext context = null) where T : class
        {
            return await LoadAsyncInternal<T, TKey>(DataStoreID.FromType(type), key, context);
        }

        internal async Task<T> LoadAsyncInternal<T, TKey>(DataStoreID storeID, TKey key, IoContext context = null) where T : class
        {
            var store = StoreMap[storeID];
            return (T)await store.LoadAsync(key, context);
        }

        // delete
        internal async Task<bool> DeleteAsync(object instance)
        {
            if (instance == null)
                return false;

            var storeID = DataStoreID.FromType(instance.GetType());
            var store = StoreMap[storeID];
            return await store.DeleteAsync(instance);
        }

        internal async Task<bool> DeleteAsync(DataStoreID storeID, object key)
        {
            var store = StoreMap[storeID];
            return await store.DeleteAsync(key);
        }

        #endregion


        #region FLUSH, PURGE & TRUCATE

        /// <summary>
        ///     Flush all keys and indexes to storage
        /// </summary>
        internal async Task FlushAsync()
        {
            using (await LockAsync().ConfigureAwait(false))
            {
                TaskCounter.Increase();

                try
                {
                    foreach (var def in StoreDefinitions.Values)
                    {
                        //Debug.WriteLine($"[DB] flushing store {def.ID}");

                        await def.Keys.FlushAsync().ConfigureAwait(false);

                        foreach (var idx in def.Indices.Values)
                        {
                            await idx.FlushAsync().ConfigureAwait(false);
                        }
                    }

                    RaiseOperationEvent(DbOperation.Flush, null, Name);
                }
                finally
                {
                    TaskCounter.Decrease();
                }
            }
        }


        /// <summary>
        ///     Truncate all records for a store
        /// </summary>
        /// <param name="storeID">The store ID</param>
        internal async Task DeleteAllAsync(DataStoreID storeID) => await StoreMap[storeID].DeleteAllAsync();



        /// <summary>
        ///     Purge the entire database - wipe it clean!
        /// </summary>
        internal async Task PurgeAsync()
        {
            using (await LockAsync().ConfigureAwait(false))
            {
                if (TaskCounter.Count > 1)
                {
                    throw new SayDBException(
                        ExceptionMessageFormat.BaseDatabaseInstance_Cannot_purge_when_background_operations);
                }

                TaskCounter.Increase();

                try
                {
                    await Driver.PurgeAsync().ConfigureAwait(false);

                    // clear key lists from memory
                    foreach (var storeDef in StoreDefinitions.Values)
                    {
                        await TruncateStoreDefKeysAndIndexesAsync(storeDef).ConfigureAwait(false);
                    }

                    RaiseOperationEvent(DbOperation.Purge, null, Name);
                }
                finally
                {
                    TaskCounter.Decrease();
                }
            }
        }

        private static async Task TruncateStoreDefKeysAndIndexesAsync( IStoreDefinition storeDef )
        {
            await storeDef.Keys.TruncateAsync().ConfigureAwait(false);

            foreach (var index in storeDef.Indices.Values)
            {
                await index.TruncateAsync().ConfigureAwait(false);
            }
        }

        internal async Task RefreshAsync()
        {
            foreach (var storeDef in StoreDefinitions.Values)
            {
                await storeDef.RefreshAsync().ConfigureAwait(false);
            }
        }


        #endregion

        #region EVENT

        internal event EventHandler<DbOperationArgs> DbOperationPerformed;

        internal void RaiseOperationEvent(DbOperation operation, DataStoreID StoreID, object key)
        {
            DbOperationPerformed?.Invoke(this, new DbOperationArgs(operation, StoreID, key));
        }

        #endregion


    }
}