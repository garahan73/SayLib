using Say32.DB.Store;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Say32.DB.Store.IO
{
    /// <summary>
    ///     Cycle cache for cycle detection
    /// </summary>
    public class IoCache
    {
        private readonly Dictionary<ObjectCacheKey, object> _objectCache = new Dictionary<ObjectCacheKey, object>();
        private readonly List<Task> _taskCache = new List<Task>();

        public void CacheObject(DataStoreID tableID, object key, object instance)
        {
            if (key == null)
            {
                return;
            }

            var tempKey = new ObjectCacheKey { TableID = tableID, Key = key };

            lock (_objectCache)
            {
                if (!_objectCache.ContainsKey(tempKey))
                {
                    _objectCache.Add(tempKey, instance);

                    //Debug.WriteLine("-- Object was caching for cycling detection");
                    Debug.Indent();
                    //Debug.WriteLine($"[type] {tableID}");
                    //Debug.WriteLine($"[key] {key}");
                    //Debug.WriteLine($"[object] {instance}");
                    Debug.Unindent();
                    //Debug.WriteLine("-- Object caching done.");
                }
                else
                {
                    throw new SayDbException($"Already cached. {tableID}.{key}, {instance}");
                }
            }
        }

        public bool IsObjectCached(DataStoreID tableID, object key)
        {
            if (key == null)
            {
                return false;
            }

            var keyInfo = new ObjectCacheKey { TableID = tableID, Key = key };

            lock (_objectCache)
            {
                return _objectCache.ContainsKey(keyInfo);
            }
        }

        /// <summary>
        ///     Check for existance based on key and return if there
        /// </summary>
        /// <param name="tableID">The type</param>
        /// <param name="key">The key</param>
        /// <returns>The cached instance, if it exists</returns>
        public object GetCachedObject(DataStoreID tableID, object key)
        {
            if (key == null)
            {
                return null;
            }

            var keyInfo = new ObjectCacheKey { TableID = tableID, Key = key };

            lock (_objectCache)
            {
                return _objectCache.ContainsKey(keyInfo) ? _objectCache[keyInfo] : null;
            }
        }

        internal void CacheTask(Task task)
        {
            lock (_taskCache)
            {
                _taskCache.Add(task);
            }
        }

        public Task<bool> WaitTasksAsync(TimeSpan timeout)
        {
            Task[] array;
            lock (_taskCache) { array = _taskCache.ToArray(); }

            return Task.Run(() => Task.WaitAll(array, timeout));
        }

        private class ObjectCacheKey
        {
            public DataStoreID TableID { get; set; }
            public object Key { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as ObjectCacheKey;
                return other != null &&
                       other.TableID.Equals(TableID) &&
                       other.Key.Equals(Key);

            }

            public override int GetHashCode()
            {
                return TableID.GetHashCode() + Key.GetHashCode();
            }
        }
    }




}