using Say32.DB.Object;
using Say32.DB.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Say32.DB.Store
{
    public class StoreMap
    {
        private readonly Dictionary<DataStoreID, PropertyMap> _propMaps = new Dictionary<DataStoreID, PropertyMap>();
        
        private readonly Dictionary<DataStoreID, DataStore> _stores = new Dictionary<DataStoreID, DataStore>();
        private readonly Dictionary<string, DataStoreID> _storeMapByKeyName = new Dictionary<string, DataStoreID>();

        internal Dictionary<DataStoreID, IStoreDefinition> Defs { get; } = new Dictionary<DataStoreID, IStoreDefinition>();

        //private readonly Dictionary<StoreKey, StoreID> _globalMap = new Dictionary<StoreKey, StoreID>();

        internal void Add<T, TKey>(DataStore<T, TKey> store) where T : class
        {
            lock (_propMaps)
            {
                _propMaps.Add(store.ID, store.PropertyMap);                
            }

            lock (_storeMapByKeyName)
            {
                _storeMapByKeyName.Add(store.ID.KeyName, store.ID);
            }

            lock (_stores)
            {
                _stores.Add(store.ID, store);
            }

            lock (Defs)
            {
                Defs.Add(store.ID, store.Def);
            }

            /*
            lock (_globalMap)
            {
                _globalMap.Add(new StoreKey { PropertyType = typeof(T) }, store.ID);
            }
            */
        }

        public DataStore Drop( DataStoreID id )
        {
            if (!Has(id))
                return null;

            DataStore removed = null;

            lock (_propMaps)
            {
                _propMaps.Remove(id);
            }

            lock (_storeMapByKeyName)
            {
                _storeMapByKeyName.Remove(id.KeyName);
            }

            lock (_stores)
            {
                removed = _stores[id];
                _stores.Remove(id);
            }

            lock (Defs)
            {
                Defs.Remove(id);
            }

            return removed;
        }



        public DataStore GetByType(Type propertyType)
        {
            IEnumerable<DataStore> candidates;

            lock (_stores)
            {
                candidates = from store in _stores.Values where store.CanHandle(propertyType) select store;
            }

            if (candidates.Count() == 0)
                return null;
            else if (candidates.Count() == 1)
                return candidates.FirstOrDefault();
            else // multiple candidates
            {
                // find exact type match
                var result = (from store in candidates where store.IDef.ObjectType == propertyType select store).FirstOrDefault();
                if (result != null)
                    return result;
                else // no exact type match, return any
                    return candidates.First();
            }
        }

        public DataStore this[DataStoreID id]
        {
            get
            {
                lock(_stores)
                {
                    return _stores.ContainsKey(id) ? _stores[id] : null;
                }
            }
        }

        public bool Has(DataStoreID id)
        {
            lock (_stores)
            {
                return _stores.ContainsKey(id);
            }
        }

        public DataStore GetPropertyStore(DataStoreID classStoreId, string propertyName, Type propertyType)
        {
            lock (_propMaps)
            {
                // explicitly mapped to property name
                if (classStoreId != null && _propMaps.ContainsKey(classStoreId))
                {
                    var propStoreID = _propMaps[classStoreId].GetPropertyStore(propertyName);
                    if (propStoreID != null) return _stores[propStoreID];
                }
            }

            return GetByType(propertyType);            
        }

        public DataStoreID GetByStoreKeyName(string storeKeyName)
        {
            lock (_storeMapByKeyName)
            {
                return _storeMapByKeyName.ContainsKey(storeKeyName) ? _storeMapByKeyName[storeKeyName] : null;
            }
        }

        public DbObjectType GetDbObjectType(DataStoreID store, Type type)
        {
            if (store != null)
            {
                lock (_stores)
                {
                    if (_stores.ContainsKey(store))
                        return _stores[store].DbObjectType;
                }
            }

            return GetByType(type)?.DbObjectType;
        }


    }

    class StoreKey
    {
        public DataStoreID ParentStore;
        public string PropertyName;
        public Type PropertyType;

        public override int GetHashCode()
        {
            return ParentStore?.GetHashCode() ?? 0 + PropertyName?.GetHashCode() ?? 0 + PropertyType.GetHashCode(); 
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is StoreKey key &&
                (ParentStore == key.ParentStore && PropertyName == key.PropertyName && PropertyType == key.PropertyType);
        }
    }
}
