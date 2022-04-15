using Say32.DB.Store;
using Say32.DB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Say32.DB.Store.IO;
using Nito.AsyncEx;

#pragma warning disable CS1998

namespace Say32.DB.Core.Indexes
{
    /// <summary>
    ///     Collection of keys for a given entity
    /// </summary>
    internal class IndexCollection<T, TIndex, TKey> : IIndexCollection, IEnumerable<IndexDef<T, TIndex, TKey>> 
        where T : class //, new()
    {
        protected readonly DataStoreID _tableID;
        protected readonly AsyncLock _lock = new AsyncLock();
        protected readonly DbDriver Driver;
        protected readonly Func<TKey, IoContext, Task<T>> _getObjectByKeyFunc;
        private Func<T, TIndex> _indexingFunc;
        protected string Name;
        
        /// <summary>
        ///     True if it is a tuple
        /// </summary>
        protected bool IsTuple { get; set; }

        /// <summary>
        ///     Set when keys change
        /// </summary>
        public bool IsDirty { get; private set; }

        /// <summary>
        ///     Initialize the key collection
        /// </summary>
        /// <param name="name">name of the index</param>
        /// <param name="driver">SayDB driver</param>
        /// <param name="indexingFunc">How to resolve the index</param>
        /// <param name="getObjectByKeyFunc">The resolver for loading the object</param>
        public IndexCollection(string name, DataStoreID tableID, DbDriver driver, Func<T, TIndex> indexingFunc, Func<TKey, IoContext, Task<T>> getObjectByKeyFunc)
        {            
            Driver = driver;
            _tableID = tableID;

            Name = name;
            _indexingFunc = indexingFunc;
            _getObjectByKeyFunc = getObjectByKeyFunc;

            DeserializeIndexesAsync().Wait();

            // is order imortant??? - sehc
            IsDirty = false;
        }        

        /// <summary>
        ///     The list of indexes
        /// </summary>
        protected readonly List<IndexDef<T, TIndex, TKey>> IndexList = new List<IndexDef<T, TIndex, TKey>>();

        /// <summary>
        ///     Query the indexes
        /// </summary>
        public IEnumerable<DbIndex<T, TIndex, TKey>> Query(IoContext context)
        {
            return IndexList.Select(d => d.CreateIndex(context));
        }

        /// <summary>
        ///     Deserialize the indexes
        /// </summary>
        protected virtual async Task DeserializeIndexesAsync()
        {
            IndexList.Clear();

            var result = await Driver.DeserializeIndexAsync<TKey, TIndex>( _tableID, Name ).ConfigureAwait( false );

            foreach ( var index in result ?? new Dictionary<TKey, TIndex>() )
            {
                IndexList.Add( new IndexDef<T, TIndex, TKey>( index.Value, index.Key, _getObjectByKeyFunc ) );
            }
        }

        /// <summary>
        ///     Serializes the key list
        /// </summary>
        protected virtual async Task SerializeIndexesAsync()
        {
            var dictionary = IndexList.ToDictionary( item => item.Key, item => item.Index );

            await Driver.SerializeIndexAsync(_tableID, Name, dictionary ).ConfigureAwait( false );
        }
        
        /// <summary>
        ///     Serialize
        /// </summary>
        public async Task FlushAsync()
        {
            using ( await _lock.LockAsync().ConfigureAwait( false ) )
            {
                if ( IsDirty )
                {
                    await SerializeIndexesAsync().ConfigureAwait( false );
                }

                IsDirty = false;
            }
        }             

        /// <summary>
        ///     Refresh the list
        /// </summary>
        public async Task RefreshAsync()
        {
            using ( await _lock.LockAsync().ConfigureAwait( false ) )
            {
                if ( IsDirty )
                {
                    await SerializeIndexesAsync().ConfigureAwait( false );
                }

                await DeserializeIndexesAsync().ConfigureAwait( false );

                IsDirty = false;
            }
        }

        /// <summary>
        ///     Truncate index
        /// </summary>
        public async Task TruncateAsync()
        {
            IsDirty = false;

            await RefreshAsync().ConfigureAwait( false );
        }
      
        /// <summary>
        ///     Add an index to the list
        /// </summary>
        /// <param name="instance">The instance</param>
        /// <param name="key">The related key</param>
        public async Task AddIndexAsync(object instance, object key)
        {
            var newIndex = new IndexDef<T, TIndex, TKey>( _indexingFunc( (T) instance ), (TKey) key, _getObjectByKeyFunc );

            using ( await _lock.LockAsync().ConfigureAwait( false ) )
            {
                if ( !IndexList.Contains( newIndex ) )
                {
                    IndexList.Add( newIndex );
                }
                else
                {
                    IndexList[ IndexList.IndexOf( newIndex ) ] = newIndex;
                }

                IsDirty = true;
            }
        }

        /// <summary>
        ///     Update the index
        /// </summary>
        /// <param name="instance">The instance</param>
        /// <param name="key">The key</param>
        public async Task UpdateIndexAsync(object instance, object key)
        {
            var index = ( from i in IndexList where i.Key.Equals( key ) select i ).FirstOrDefault();

            if ( index == null ) return;

            index.Index = _indexingFunc( (T) instance );

            //index.Refresh();

            IsDirty = true;

        }

        /// <summary>
        ///     Remove an index from the list
        /// </summary>
        /// <param name="key">The key</param>
        public async Task RemoveIndexAsync(object key)
        {
            var index = ( from i in IndexList where i.Key.Equals( key ) select i ).FirstOrDefault();

            if ( index == null ) return;

            using ( await _lock.LockAsync().ConfigureAwait( false ) )
            {
                if ( !IndexList.Contains( index ) ) return;

                IndexList.Remove( index );

                IsDirty = true;
            }
        }

        public IEnumerator<IndexDef<T, TIndex, TKey>> GetEnumerator() => IndexList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => IndexList.GetEnumerator();
    }
}
