using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Say32.DB.Core;
using Say32.DB.Core.Database;
using Say32.DB.Core.Serialization;
using Say32.DB.Store;

namespace Say32.DB.Driver
{
    /// <summary>
    ///     Default in-memory driver
    /// </summary>
    public class MemoryDriver : DbDriver 
    {
        public MemoryDriver()
        {            
        }

        /// <summary>
        ///     Keys
        /// </summary>
        private readonly Dictionary<DataStoreID, object> _keyCache = new Dictionary<DataStoreID, object>();

        /// <summary>
        ///     Indexes
        /// </summary>
        private readonly Dictionary<DataStoreID, Dictionary<string, object>> _indexCache = new Dictionary<DataStoreID, Dictionary<string, object>>();

        /// <summary>
        ///     Objects
        /// </summary>
        private readonly Dictionary<Tuple<string,int>, byte[]> _objectCache = new Dictionary<Tuple<string,int>, byte[]>();

        private readonly AsyncLock _lock = new AsyncLock();

        /// <summary>
        ///     Serialize the keys
        /// </summary>
        /// <param name="id">Type of the parent table</param>
        /// <param name="keyType">Type of the key</param>
        /// <param name="keyMap">Key map</param>
        public override async Task SerializeKeysAsync(DataStoreID id, Type keyType, IDictionary keyMap)
        {
            using ( await _lock.LockAsync().ConfigureAwait( false ) )
            {
                _keyCache[ id ] = keyMap;
            }
        }

        /// <summary>
        ///     Deserialize keys without generics
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="keyType">Type of the key</param>
        /// <param name="template">The template</param>
        /// <returns>The keys without the template</returns>
        public override async Task<IDictionary> DeserializeKeysAsync(DataStoreID type, Type keyType, IDictionary template)
        {
            using ( await _lock.LockAsync().ConfigureAwait( false ) )
            {
                return _keyCache.ContainsKey( type ) ? _keyCache[ type ] as IDictionary : template;
            }
        }

        /// <summary>
        ///     Serialize a single index 
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TIndex">The type of the index</typeparam>
        /// <param name="id">The type of the parent table</param>
        /// <param name="indexName">The name of the index</param>
        /// <param name="indexMap">The index map</param>
        public override async Task SerializeIndexAsync<TKey, TIndex>(DataStoreID id, string indexName, Dictionary<TKey, TIndex> indexMap)
        {
            using ( await _lock.LockAsync().ConfigureAwait( false ) )
            {
                if ( !_indexCache.ContainsKey( id ) )
                {
                    _indexCache.Add( id, new Dictionary<string, object>() );
                }

                var indexCache = _indexCache[ id ];

                indexCache[ indexName ] = indexMap;
            }
        }

        /// <summary>
        ///     Serialize a double index 
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TIndex1">The type of the first index</typeparam>
        /// <typeparam name="TIndex2">The type of the second index</typeparam>
        /// <param name="type">The type of the parent table</param>
        /// <param name="indexName">The name of the index</param>
        /// <param name="indexMap">The index map</param>        
        public override async Task SerializeIndexAsync<TKey, TIndex1, TIndex2>(DataStoreID type, string indexName, Dictionary<TKey, Tuple<TIndex1, TIndex2>> indexMap)
        {
            using ( await _lock.LockAsync().ConfigureAwait( false ) )
            {
                if ( !_indexCache.ContainsKey( type ) )
                {
                    _indexCache.Add( type, new Dictionary<string, object>() );
                }

                var indexCache = _indexCache[ type ];

                indexCache[ indexName ] = indexMap;
            }
        }

        /// <summary>
        ///     Deserialize a single index
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TIndex">The type of the index</typeparam>
        /// <param name="type">The type of the parent table</param>
        /// <param name="indexName">The name of the index</param>        
        /// <returns>The index map</returns>
        public override async Task<Dictionary<TKey, TIndex>> DeserializeIndexAsync<TKey, TIndex>(DataStoreID type, string indexName)
        {
            using ( await _lock.LockAsync().ConfigureAwait( false ) )
            {
                if ( !_indexCache.ContainsKey( type ) )
                    return null;

                var indexCache = _indexCache[ type ];

                if ( !indexCache.ContainsKey( indexName ) )
                    return null;

                return indexCache[ indexName ] as Dictionary<TKey, TIndex>;
            }
        }

        /// <summary>
        ///     Deserialize a double index
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TIndex1">The type of the first index</typeparam>
        /// <typeparam name="TIndex2">The type of the second index</typeparam>
        /// <param name="type">The type of the parent table</param>
        /// <param name="indexName">The name of the index</param>        
        /// <returns>The index map</returns>        
        public override async Task<Dictionary<TKey, Tuple<TIndex1, TIndex2>>> DeserializeIndexAsync<TKey, TIndex1, TIndex2>(DataStoreID type, string indexName)
        {
            using ( await _lock.LockAsync().ConfigureAwait( false ) )
            {
                if ( !_indexCache.ContainsKey( type ) )
                    return null;

                var indexCache = _indexCache[ type ];

                if ( !indexCache.ContainsKey( indexName ) )
                    return null;

                return indexCache[ indexName ] as Dictionary<TKey, Tuple<TIndex1, TIndex2>>;
            }
        }

        /// <summary>
        ///     Publish the list of tables
        /// </summary>
        /// <param name="tables">The list of tables</param>
        public override void PublishDataStores( Dictionary<DataStoreID, IStoreDefinition> tables, Func<string, Type> resolveType )
        {
            return;
        }

        /// <summary>
        ///     Serialize the type master
        /// </summary>
        public override Task SerializeTypesAsync()
        {
            return Task.Factory.StartNew( () => { } );
        }

        /// <summary>
        ///     Save operation
        /// </summary>
        /// <param name="type">Type of the parent</param>
        /// <param name="keyIndex">Index for the key</param>
        /// <param name="bytes">The byte stream</param>
        public override async Task SaveAsync(DataStoreID type, int keyIndex, byte[] bytes)
        {
            var key = Tuple.Create( type.KeyName, keyIndex );

            using ( await _lock.LockAsync().ConfigureAwait( false ) )
            {
                _objectCache[ key ] = bytes;
            }
        }

        /// <summary>
        ///     Load from the store
        /// </summary>
        /// <param name="type">The type of the parent</param>
        /// <param name="keyIndex">The index of the key</param>
        /// <returns>The byte stream</returns>
        public override async Task<BinaryReader> LoadAsync(DataStoreID type, int keyIndex)
        {
            var key = Tuple.Create( type.KeyName, keyIndex );
            byte[] bytes;

            using ( await _lock.LockAsync().ConfigureAwait( false ) )
            {
                bytes = _objectCache[ key ];
            }

            var memStream = new MemoryStream( bytes );
            return new BinaryReader( memStream );
        }

        /// <summary>
        ///     Delete from the store
        /// </summary>
        /// <param name="type">The type of the parent</param>
        /// <param name="keyIndex">The index of the key</param>
        public override async Task DeleteAsync(DataStoreID type, int keyIndex)
        {
            var key = Tuple.Create( type.KeyName, keyIndex );

            using ( await _lock.LockAsync().ConfigureAwait( false ) )
            {
                if ( _objectCache.ContainsKey( key ) )
                {
                    _objectCache.Remove( key );
                }
            }
        }

        /// <summary>
        ///     Truncate a type
        /// </summary>
        /// <param name="id">The type to truncate</param>
        public override async Task TruncateAsync(DataStoreID id)
        {
            var typeString = id.KeyName;

            using ( await _lock.LockAsync().ConfigureAwait( false ) )
            {
                var keys = from key in _objectCache.Keys where key.Item1.Equals( typeString ) select key;

                foreach ( var key in keys.ToList() )
                {
                    _objectCache.Remove( key );
                }

                var indexes = from index in _indexCache.Keys where _indexCache.ContainsKey( id ) select index;

                foreach ( var index in indexes.ToList() )
                {
                    _indexCache.Remove( index );
                }

                if ( _keyCache.ContainsKey( id ) )
                {
                    _keyCache.Remove( id );
                }
            }
        }

        /// <summary>
        ///     Purge the database
        /// </summary>
        public override async Task PurgeAsync()
        {
            var types = from key in _keyCache.Keys select key;

            foreach ( var type in types.ToList() )
            {
                await TruncateAsync( type ).ConfigureAwait( false );
            }
        }
    }
}