using Say32.DB.Core.Serialization;
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
    class DbSerializerObjectType : DbObjectType
    {
        private readonly IDbSerializer _serializer;

        public override Type ObjectType => _objType;

        public DbSerializerObjectType(Type objectType, IDbSerializer serializer)
        {
            _objType = objectType;
            _serializer = serializer;
        }

        private readonly Type _objType;

        public override DataStoreID StoreID { get; } = null;

        public override Dictionary<string, PropertyAccessor> PropertyAccessors { get; } = new Dictionary<string, PropertyAccessor>();

        public override object CreateEmptyObject(BinaryReader br=null)
        {
            return _serializer.Deserialize(ObjectType, br);
        }

        internal override Task<object> DeserializeObject(IoContext context, BinaryReader br, DataStoreID storeID, object key, IoHeader header)
        {
            return Task.FromResult(CreateEmptyObject(br));
        }

        internal override Task SerializeObject(IoContext _context, object instance, BinaryWriter bw, IoHeader header)
        {
            _serializer.Serialize(instance, bw);
            return Task.FromResult(0);
        }

        protected internal override async Task SerializePropertyValue(object item, IoContext context, BinaryWriter bw, IoHeader header)
        {
            header.Serialize(bw);
            _serializer.Serialize(item, bw);

            await Task.FromResult<object>(null);
        }

        protected internal override Task DeserializePropertyValue(IoContext context, BinaryReader br, IoHeader header, SetPropertyValueMethod setPropValue)
        {
            var value = _serializer.Deserialize(ObjectType, br);
            setPropValue(value);
            return Task.FromResult(value);
        }


    }
}