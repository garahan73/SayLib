using Say32.DB.Core.Exceptions;
using Say32.DB.etc;
using Say32.DB.Log;
using Say32.DB.Object.Serialization;
using Say32.DB.Store;
using Say32.DB.Store.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Say32.DB.IO
{
    public class ExternalPropertyIO
    {
        private const int LOAD_TIMEOUT = 10;
        private const int SAVE_TIMEOUT = 10;

        private readonly IoContext _context;
        private readonly StoreMap _storeMap;

        public ExternalPropertyIO(IoContext ctx)
        {
            _context = ctx;
            _storeMap = _context.DbCore.StoreMap;
        }

        /// <summary>
        ///     Serialize a class
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="type">The type</param>
        /// <param name="instance">The referenced type</param>
        /// <param name="bw">The writer</param>
        /// <param name="cache">Cycle cache</param>
        public void SaveObject_SerializeKey(DataStoreID storeID, IoHeader header, object instance, BinaryWriter bw)
        {
            //Debug.WriteLine("------> Save foreign object....");

            header = header ?? new IoHeader();
            header.StoreKeyName = storeID.KeyName;
            header.Serialize(bw);

            var store = _context.DbCore.StoreMap[storeID];

            // invoke async save
            //var task = SaveAsync(instance, store);
            var task = store.SaveAsync(instance, _context);

            // cache task
            _context.Cache.CacheTask(task);

            // need to be able to serialize the key 
            var serializer = _context.Serializer;
            var logManager = DbWorkSpace.LogManager;

            // get foreign key
            var foreignKey = store.IDef.GetKey(instance);

            // check error
            if (!serializer.CanHandle(foreignKey.GetType()))
            {
                var exception = new SayDBSerializerException(serializer, foreignKey.GetType());
                logManager.Log(DbLogLevel.Error, exception.Message, exception);
                throw exception;
            }

            logManager.Log(DbLogLevel.Verbose,
                            string.Format(
                                "SayDB is saving foreign key of type {0} with value {1} for parent {2}",
                                foreignKey.GetType().FullName, foreignKey, instance.GetType().FullName), null);

            // serialize key
            serializer.Serialize(foreignKey, bw);

        }

        public void DeserializeKey_LoadObject(DataStoreID storeID, Type type, BinaryReader br, SetPropertyValueMethod setPropValue)
        {
            //Debug.WriteLine("-------------> Loading foreign object....");
            Debug.Indent();

            //Debug.WriteLine($"[Store] {storeID}");

            // get foreign object data store
            var store = _context.DbCore.StoreMap[storeID];

            // deserialize key
            var keyType = store.IDef.KeyType;
            var key = DbWorkSpace.Serializers.Deserialize(keyType, br);

            //Debug.WriteLine($"[Key] {key}");

            // check if the object is already cached and loaded
            if (_context.Cache.IsObjectCached(storeID, key))
            {
                var cached = _context.Cache.GetCachedObject(storeID, key);
                //Debug.WriteLine($"[Object cached] foreign object is already cached {cached}");
                setPropValue(cached);
                return;
            }

            // load object by key
            var task = LoadAsync(store, key, setPropValue);
            //var task = store.LoadAsync(key, _context);
            _context.Cache.CacheTask(task);

        }

        private async Task LoadAsync(DataStore store, object key, SetPropertyValueMethod setPropValue)
        {
            //Debug.WriteLine($"[Loading foreign object...] {storeID}, Key: {key}");

            var task = store.LoadAsync(key, _context);
            
            // check timeout (10 sec.)
            if (!await task.WaitAsync(TimeSpan.FromSeconds(LOAD_TIMEOUT)))
            {
                throw new TimeoutException($"Loading external property vlaue failed. Timeout in {LOAD_TIMEOUT} seconds.");
            }

            var obj = task.Result;
            setPropValue(obj);

            Debug.Unindent();
            //Debug.WriteLine($"[Object loaded] foreign object={obj}");
        }
    }
}
