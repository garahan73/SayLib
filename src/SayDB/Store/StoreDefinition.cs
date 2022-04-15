using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Say32.DB.Core.Exceptions;
using Say32.DB.Core.Indexes;
using Say32.DB.Core.Keys;
using Say32.DB.Driver;
using Say32.DB.Store.IO;

namespace Say32.DB.Store
{
    /// <summary>
    ///     The definition of a table
    /// </summary>
    internal class StoreDefinition<T,TKey> : IStoreDefinition where T: class //, new()
    {
        private Func<TKey, IoContext, Task<T>> _getObjectByKeyFunc;
        private readonly Func<T, TKey> _getKeyFunc;

        private Predicate<T> _isDirty = obj => true;
        private DbDriver _driver;

        public DataStoreID ID { get; private set; }

        internal KeyCollection<T, TKey> KeyList { get; private set; }
        public Dictionary<string, IIndexCollection> Indices { get; } = new Dictionary<string, IIndexCollection>();

        /// <summary>
        ///     Construct 
        /// </summary>
        /// <param name="driver">SayDB driver</param>
        /// <param name="getObjectFunc">The resolver for the instance</param>
        /// <param name="getKeyFunc">The resolver for the key</param>
        public StoreDefinition(DataStoreID id, DbDriver driver, Func<TKey,IoContext,Task<T>> getObjectFunc, Func<T,TKey> getKeyFunc)
        {
            ID = id;
            _driver = driver;

            _getObjectByKeyFunc = getObjectFunc;
            _getKeyFunc = getKeyFunc;            


            KeyList = new KeyCollection<T, TKey>(id, driver, getObjectFunc);

        }

        internal StoreDefinition(DataStoreID id, Func<T, TKey> getKeyFunc)
        {
            ID = id;
            _getKeyFunc = getKeyFunc;
        }

        public void Setup(DbDriver driver, Func<TKey, IoContext, Task<T>> getObjectFunc) 
        {
            _driver = driver;

            _getObjectByKeyFunc = getObjectFunc;            

            KeyList = new KeyCollection<T, TKey>(ID, driver, getObjectFunc);
        }


        /// <summary>
        ///     Get a new dictionary (creates the generic)
        /// </summary>
        /// <returns>The new dictionary instance</returns>
        public IDictionary GetNewDictionary()
        {
            return new Dictionary<TKey, int>();
        }

        

        public void RegisterDirtyFlag(Predicate<T> isDirty)
        {
            _isDirty = isDirty;
        }

        /// <summary>
        ///     Registers an index with the table definition
        /// </summary>
        /// <typeparam name="TIndex">The type of the index</typeparam>
        /// <param name="name">A name for the index</param>
        /// <param name="indexingFunc">The function to retrieve the index</param>
        public void RegisterIndex<TIndex>(string name, Func<T,TIndex> indexingFunc)
        {
            if (Indices.ContainsKey(name))
            {
                throw new SayDBDuplicateIndexException(name, typeof(T), _driver.DatabaseInstanceName);
            }

            var indexCollection = new IndexCollection<T, TIndex, TKey>(name, ID, _driver, indexingFunc, _getObjectByKeyFunc);
            
            Indices.Add(name, indexCollection);
        }

        /// <summary>
        ///     Key list
        /// </summary>
        public IKeyCollection Keys { get { return KeyList; }}
        
        /// <summary>
        ///     Table type
        /// </summary>
        public Type ObjectType
        {
            get { return typeof(T); }
        }

        /// <summary>
        ///     Key type
        /// </summary>
        public Type KeyType
        {
            get { return typeof (TKey); }
        }

        

        /// <summary>
        ///     Refresh key list
        /// </summary>
        public async Task RefreshAsync()
        {
            await KeyList.RefreshAsync().ConfigureAwait( false );

            foreach ( var index in Indices.Values )
            {
                await index.RefreshAsync().ConfigureAwait( false );
            }
        }

        /// <summary>
        ///     Fetch the key for the instance
        /// </summary>
        /// <param name="instance">The instance</param>
        /// <returns>The key</returns>
        public object GetKey(object instance)
        {
            return _getKeyFunc((T) instance);
        }

        /// <summary>
        ///     Is the instance dirty?
        /// </summary>
        /// <returns>True if dirty</returns>
        public bool IsDirty(object instance)
        {
            return _isDirty((T) instance);
        }


    }
}
