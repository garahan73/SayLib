using Say32.DB.IO;
using Say32.DB.Object;
using Say32.DB.Store.IO;
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;

namespace Say32.DB.Object.Serialization
{
    class PropertySerializer : BaseSerializer
    {
        private BinaryWriter _bw;
        private PropertyAccessor _propAccessor;

        public PropertySerializer(DbObjectType dbObjectType, IoContext context, BinaryWriter bw) : base(dbObjectType, context)
        {
            _bw = bw;
        }

        public async Task SerializeProperty(PropertyAccessor propertyAccessor, object propertyValue)
        {
            _propAccessor = propertyAccessor;

            //Debug.WriteLine($"[Saving property...] {propertyName}");
            //Debug.Indent();
            //Debug.WriteLine($"{propertyValue}");

            var type = propertyValue?.GetType() ?? _propAccessor.PropType;
            //Debug.WriteLine($"[Type] {type.Name}");

            // create IO header
            var header = new IoHeader
            {
                PropertyName = _propAccessor.PropertyName,
                TypeCode = await GetTypeCode(type),
                StoreKeyName = _propAccessor.StoreID?.KeyName
            };

            if (propertyValue == null)
            {
                IoHelper.SerializeNull(_bw, header);
            }
            else if (_serializer.CanHandle(type))
            {
                header.IsUsingSerializer = true;
                header.Serialize(_bw);

                _serializer.Serialize(propertyValue, _bw);
            }
            else if (_propAccessor.StoreID != null)
            {
                var externalIo = new ExternalPropertyIO(_ctx);
                externalIo.SaveObject_SerializeKey(_propAccessor.StoreID, header, propertyValue, _bw);
            }
            else if (PlatformAdapter.IsAssignableFrom(typeof(Array), type))
            {
                await new CollectionSerializer(_ctx, _bw, _propAccessor.StoreID).SerializeArray(header, propertyValue as Array);
            }
            else if (PlatformAdapter.IsAssignableFrom(typeof(IList), type))
            {
                await new CollectionSerializer(_ctx, _bw, _propAccessor.StoreID).SerializeList(header, propertyValue as IList);
            }
            else if (PlatformAdapter.IsAssignableFrom(typeof(IDictionary), type))
            {
                await new CollectionSerializer(_ctx, _bw, _propAccessor.StoreID).SerializeDictionary(header, propertyValue as IDictionary);
            }
            else // class type, need to serialize properties
            {
                var dbObjType = _ctx.DbCore.DbObjectTypeManager.GetDbObjectType(type, _propAccessor.StoreID);
                await dbObjType.SerializePropertyValue(propertyValue, _ctx, _bw, header);
            }

            //Debug.Unindent();
            //Debug.WriteLine($"[Save property done.] {propertyName}");
        }

        private async Task<int> GetTypeCode(Type type)
        {
            return await _ctx.TypeManager.GetTypeIndex(type.AssemblyQualifiedName);
        }
    }
}
