using System;
using System.Collections.Generic;
using System.Text;

namespace Say32.DB.Store
{
    public class PropertyMap
    {
        private object _lock = new object();

        public DataStoreID ClassStore { get; private set; }
        public Type ClassType { get; private set; }

        public PropertyMap(DataStoreID store, Type type)
        {
            ClassStore = store;
            ClassType = type;
        }

        private readonly Dictionary<string, DataStoreID> _propMap = new Dictionary<string, DataStoreID>();

        public void MapWithProperty(string propName, DataStoreID store)
        {
            lock (_lock)
            {
                _propMap[propName] = store;
            }
        }

        public bool IsStoreRegistered(string propName)
        {
            lock (_lock)
            {
                return _propMap.ContainsKey(propName);
            }
        }

        public DataStoreID GetPropertyStore(string propName)
        {
            lock (_lock)
            {
                return _propMap.ContainsKey(propName) ? _propMap[propName] : null;
            }
        }

    }
}
