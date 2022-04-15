using Say32.DB.IO;
using Say32.DB.Object.Serialization;
using Say32.DB.Store;
using Say32.DB.Store.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Say32.DB.Object
{
    public abstract class DbObjectType
    {
        public abstract DataStoreID StoreID { get; }
        public bool IsRegistered => StoreID != null;

        public abstract Type ObjectType { get; }

        public abstract Dictionary<string, PropertyAccessor> PropertyAccessors { get; }
        public PropertyAccessor this[string propName] => PropertyAccessors.ContainsKey(propName) ? PropertyAccessors[propName] : null;

        public abstract object CreateEmptyObject(BinaryReader br = null);

        public bool SetPropertyValue(string propName, object targetObject, object value)
        {
            if (!PropertyAccessors.ContainsKey(propName))
                return false;

            PropertyAccessors[propName].SetPropValue(targetObject, value);
            return true;
        }

        public bool GetPropertyValue(string propName, object targetObject, out object value)
        {
            value = null;

            if (!PropertyAccessors.ContainsKey(propName))
                return false;

            value = PropertyAccessors[propName].GetPropValue(targetObject);
            return true;
        }

        internal virtual async Task SerializeObject(IoContext context, object instance, BinaryWriter bw, IoHeader header)
        {
            var serializer = new ClassSerializer(this, context);
            await serializer.Start(instance, bw, header);
        }

        protected internal virtual async Task SerializePropertyValue(object item, IoContext context, BinaryWriter bw, IoHeader header)
        {
            header.StoreKeyName = StoreID?.KeyName;

            if (IsRegistered)
            {
                var io = new ExternalPropertyIO(context);
                io.SaveObject_SerializeKey(StoreID, header, item, bw);
            }
            else
            {
                await SerializeObject(context, item, bw, header);
            }
        }


        protected internal virtual async Task DeserializePropertyValue(IoContext context, BinaryReader br, IoHeader header, SetPropertyValueMethod setPropValue)
        {
            if (IsRegistered)
            {
                var externalPropertyIO = new ExternalPropertyIO(context);
                externalPropertyIO.DeserializeKey_LoadObject(StoreID, ObjectType, br, setPropValue);
            }
            else
            {
                var propValue = await DeserializeObject(context, br, null, null, header);

                // for value type(enum) properties should be loaded first
                // and completed value is set as property value later
                setPropValue(propValue);
            }
        }



        internal virtual async Task<object> DeserializeObject(IoContext context, BinaryReader br, DataStoreID storeID, object key, IoHeader header)
        {            
            var deserializer = new ClassDeserializer(this, context);

            // create empty object without property settings
            var obj = await deserializer.CreateEmptyObject(br, header);

            // cache object before deserializing properties to avoid referecing conflict
            if (storeID != null || key != null)
                context.Cache.CacheObject(storeID, key, obj);

            // deserialize stream to object
            if (obj != null)
                await deserializer.DeserializeProperties();

            return obj;
        }
    }
}
