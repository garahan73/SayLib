using Say32.DB.Store;
using Say32.DB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Say32.DB.Store.IO;
using Nito.AsyncEx;

namespace Say32.DB.Core.Keys
{
    /// <summary>
    ///     Collection of keys for a given entity
    /// </summary>
    internal class KeyCollection<T,TKey> : IKeyCollection where T: class 
    {
        private readonly DataStoreID _storeID;
        private readonly AsyncLock _lock = new AsyncLock();
        private readonly Func<TKey, IoContext, Task<T>> _getObjectFunc;
        private readonly DbDriver _driver;
        
        /// <summary>
        ///     Set when keys change
        /// </summary>
        public bool IsDirty { get; private set; }

        /// <summary>
        ///     Initialize the key collection
        /// </summary>
        /// <param name="driver">Driver</param>
        /// <param name="getObjectFunc">The resolver for loading the object</param>
        public KeyCollection(DataStoreID tableID, DbDriver driver, Func<TKey, IoContext, Task<T>> getObjectFunc)
        {
            _storeID = tableID;
            _driver = driver;
            _getObjectFunc = getObjectFunc;

            _DeserializeKeysAsync().Wait();

            IsDirty = false;        
        }        

        /// <summary>
        ///     The list of keys
        /// </summary>
        private readonly List<DbKeyDef<T,TKey>> _keyList = new List<DbKeyDef<T, TKey>>();

        /// <summary>
        ///     Map for keys in the set
        /// </summary>
        private readonly Dictionary<TKey,int> _keyMap = new Dictionary<TKey, int>();

        public int Count => _keyList.Count;

        /// <summary>
        ///     Force to a new list so the internal one cannot be manipulated directly
        /// </summary>
        public IEnumerable<DbKey<T, TKey>> Query(IoContext context)
        {
            return _keyList.Select(kd => kd.CreateKey(context));
        }

        private async Task _DeserializeKeysAsync()
        {
            _keyList.Clear();
            _keyMap.Clear();

            var keyMap = await _driver.DeserializeKeysAsync( _storeID, typeof( TKey ), new Dictionary<TKey, int>() ).ConfigureAwait( false );

            if ( keyMap == null )
            {
                keyMap = new Dictionary<TKey, int>();
            }

            if ( keyMap.Count > 0 )
            {
                foreach ( var key in keyMap.Keys )
                {
                    var idx = (int) keyMap[ key ];

                    if ( idx >= NextKey )
                    {
                        NextKey = idx + 1;
                    }
                    
                    _keyMap.Add( (TKey) key, idx );
                    
                    _keyList.Add( new DbKeyDef<T, TKey>( (TKey) key, _getObjectFunc ) );
                }
            }
            else
            {
                NextKey = 0;
            }
        }

        /// <summary>
        ///     Serializes the key list
        /// </summary>
        private async Task _SerializeKeysAsync()
        {
            await _driver.SerializeKeysAsync( _storeID, typeof( TKey ), _keyMap ).ConfigureAwait( false );
        }

        /// <summary>
        ///     The next key
        /// </summary>
        internal int NextKey { get; private set; } 

        /// <summary>
        ///     Serialize
        /// </summary>
        public async Task FlushAsync()
        {
            using ( await _lock.LockAsync().ConfigureAwait( false ) )
            {
                if ( IsDirty )
                {
                    await _SerializeKeysAsync().ConfigureAwait( false );
                }

                IsDirty = false;
            }
        }

        /// <summary>
        ///     Get the index for a key
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The index</returns>
        public int GetIndexForKeyAsync(object key)
        {            
            using (_lock.Lock())
            //using ( await _lock.LockAsync().ConfigureAwait( false ) )
            {
                int result;

                if ( _keyMap.TryGetValue( (TKey) key, out result ) == false )
                {
                    result = -1;
                }

                return result;
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
                    await _SerializeKeysAsync().ConfigureAwait( false );
                }

                await _DeserializeKeysAsync().ConfigureAwait( false );

                IsDirty = false;
            }
        }

        /// <summary>
        ///     Truncate the collection
        /// </summary>
        public async Task TruncateAsync()
        {
            IsDirty = false;

            await RefreshAsync().ConfigureAwait( false );
        }       

        /// <summary>
        ///     Add a key to the list
        /// </summary>
        /// <param name="key">The key</param>
        public async Task<int> AddKeyAsync(object key)
        {
            using ( await _lock.LockAsync().ConfigureAwait( false ) )
            {
                var newKey = new DbKeyDef<T, TKey>( (TKey) key, _getObjectFunc );

                if ( !_keyList.Contains( newKey ) )
                {
                    _keyList.Add( newKey );
                    _keyMap.Add( (TKey) key, NextKey++ );
                    IsDirty = true;
                }
                else
                {
                    //var idx = _keyList.IndexOf( newKey );
                    //_keyList[ idx ].Refresh();
                }
            }

            return _keyMap[ (TKey) key ];
        }

        /// <summary>
        ///     Remove a key from the list
        /// </summary>
        /// <param name="key">The key</param>
        public async Task RemoveKeyAsync(object key)
        {
            using ( await _lock.LockAsync().ConfigureAwait( false ) )
            {
                var checkKey = new DbKeyDef<T, TKey>( (TKey) key, _getObjectFunc );

                if ( !_keyList.Contains( checkKey ) ) return;

                _keyList.Remove( checkKey );
                
                _keyMap.Remove( (TKey) key );
                
                IsDirty = true;
            }
        }
    }
}
