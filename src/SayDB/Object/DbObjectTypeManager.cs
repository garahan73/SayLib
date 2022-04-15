using Say32.DB.Core;
using Say32.DB.Store;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Say32.DB.Object.Properties
{
    public class DbObjectTypeManager
    {
        private readonly Dictionary<DbObjectTypeKey, DbObjectType> _cache = new Dictionary<DbObjectTypeKey, DbObjectType>();

        private readonly Func<string, Task<int>> _typeResolver;

        private readonly DbCore _db;

        private class DbObjectTypeKey
        {
            public Type Type;
            public DataStoreID Store;

            public override bool Equals(object obj)
            {
                return obj != null && obj is DbObjectTypeKey key &&
                        (Type == key.Type && Store == key.Store);
            }

            public override int GetHashCode()
            {
                return $"{Type?.AssemblyQualifiedName ?? ""}{Store?.KeyName ?? ""}".GetHashCode();
            }
        }

        internal DbObjectTypeManager(Func<string, Task<int>> typeResolver, DbCore db)
        {
            _typeResolver = typeResolver;
            _db = db;
        }

        /// <summary>
        ///     Cache the properties for a type so we don't reflect every time
        /// </summary>
        /// <param name="type">The type to manage</param>
        internal DbObjectType GetDbObjectType(Type type, DataStoreID storeID)
        {
            var key = new DbObjectTypeKey { Type = type, Store = storeID };

            // if already cached, return from cache
            lock (_cache)
            {
                if (_cache.ContainsKey(key))
                    return _cache[key];
            }

            // if not cached, create one and cache it
            var dbObjType = CreateDbObjectType(storeID, type);
            if (dbObjType != null)
            {
                lock (_cache)
                {
                    _cache.Add(key, dbObjType);
                }

                InvokeTypeResolvers(dbObjType);
            }

            return dbObjType;
        }

        internal DbObjectType CreateDbObjectType(DataStoreID store, Type type)
        {
            if (DbWorkSpace.Serializers.CanHandle(type))
                return new DbSerializerObjectType(type, _db.Serializer);

            var dbObjType = _db.StoreMap.GetDbObjectType(store, type);

            if (dbObjType == null)
                dbObjType = new DbClassObjectType(type, null, _db);

            return dbObjType;
        }


        internal void Register(DataStoreID storeID, DbObjectType dbObject)
        {
            var key = new DbObjectTypeKey { Store = storeID };
            lock (_cache)
            {
                _cache.Add(key, dbObject);
            }
            InvokeTypeResolvers(dbObject);
        }

        internal void Unregister( DataStoreID storeID )
        {
            var key = new DbObjectTypeKey { Store = storeID };
            lock (_cache)
            {
                _cache.Remove(key);
            }
        }

        private void InvokeTypeResolvers(DbObjectType dbObjType)
        {
            foreach (var prop in dbObjType.PropertyAccessors.Values)
            {
                _typeResolver(prop.PropType.AssemblyQualifiedName);
            }
        }


    }


}
