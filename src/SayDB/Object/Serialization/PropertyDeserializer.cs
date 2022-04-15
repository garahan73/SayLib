using Say32.DB.Core;
using Say32.DB.Core.Exceptions;
using Say32.DB.IO;
using Say32.DB.Object;
using Say32.DB.Store.IO;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Say32.DB.Object.Serialization
{
    internal class PropertyDeSerializer : BaseSerializer
    {
        private BinaryReader _br;
        private PropertyAccessor _propAccessor;

        public string PropName { get; private set; }

        public PropertyDeSerializer(DbObjectType dbObjectType, IoContext context) : base(dbObjectType, context)
        {
        }

        internal async Task Deserialize(BinaryReader br, object targetObject)
        {
            _br = br;
            _targetObject = targetObject;

            var header = IoHeader.Deserialize(_br);
            DbError.Assert<SayDbException>(header.HasName, "Property Name is not set in IO header during loading property.");

            // set property name
            PropName = header.PropertyName;
            _propAccessor = PropName != null ? _dbObjectType[PropName] : null;

            // deserialize property value
            await DeserializeValue(header);
        }

        private void SetPropValue(object value)
        {
            //Debug.Unindent();
            Debug.WriteLine($"<<PROP. LOAD: {PropName}>> = {value}");
            //Debug.Indent();

            if (_propAccessor != null)
            {
                _propAccessor.SetPropValue(_targetObject, value);
            }
            else
            {
                // unknown property, see if it should be converted or ignored
                if (_db.Core.TryGetPropertyConverter(_type, out IDbPropertyConverter propertyConverter))
                {
                    propertyConverter.SetValue(_targetObject, PropName, value);
                }
            }
        }



        private async Task DeserializeValue(IoHeader header)
        {
            ////Debug.WriteLine($"[Loading object...] {type?.Name ?? "type is null" }");
            //Debug.Indent();

            DbError.Assert<SayDBException>(header.HasTypeInfo, $"Type not specified in {nameof(IoHeader)}");

            // value is null
            if (header.IsNull)
            {
                //Debug.WriteLine($"[NULL loaded]");
                SetPropValue(null);
                return;
            }

            // get type from type index code1
            var typeResolved = await _ctx.GetType(header);

            //Debug.WriteLine($"[Object type] {typeResolved.Name}");

            // deserialize with registered serializer (primitive type)
            if (header.IsUsingSerializer)
            {
                //Debug.WriteLine($"(serializer...)");
                var value = _serializer.Deserialize(typeResolved, _br);
                SetPropValue(value);
            }
            // external
            else if (header.HasStore)
            {                
                var storeID = _ctx.DbCore.StoreMap.GetByStoreKeyName(header.StoreKeyName);

                var externalIo = new ExternalPropertyIO(_ctx);
                externalIo.DeserializeKey_LoadObject(storeID, _propAccessor.PropType , _br, SetPropValue);
            }
            // deserialize collection
            else if (header.IsCollection)
            {
                await DeserializeCollection(header, typeResolved, SetPropValue);
                return;
            }
            else
            // Default: deserialize by DB object type
            {
                var dbObjType = _ctx.DbCore.DbObjectTypeManager.GetDbObjectType(typeResolved, _propAccessor?.StoreID);
                DbError.Assert<SayDBException>(dbObjType.IsRegistered == header.HasStore, "Store info. mismatch between DB object type and IO header");

                await dbObjType.DeserializePropertyValue(_ctx, _br, header, SetPropValue);
            }
        }


        private async Task DeserializeCollection(IoHeader header, Type typeResolved, SetPropertyValueMethod setPropValue)
        {
            if (header.IsArray)
            {
                var count = _br.ReadInt32();

                var array = Array.CreateInstance(typeResolved.GetElementType(), count);
                setPropValue(array);
                new CollectionDeserializer(_ctx, _br, _propAccessor?.StoreID).DeserializeArrayItems(array, count);
            }
            else if (header.IsList)
            {
                var list = Activator.CreateInstance(typeResolved) as IList;
                setPropValue(list);
                await new CollectionDeserializer(_ctx, _br, _propAccessor?.StoreID).DeserializeListItemsAsync(list);
            }
            else if (header.IsDictionary)
            {
                var dictionary = Activator.CreateInstance(typeResolved) as IDictionary;
                setPropValue(dictionary);
                await new CollectionDeserializer(_ctx, _br, _propAccessor?.StoreID).DeserializeDictionaryItems(dictionary);
            }
            else
            {
                DbError.Fail<SayDBException>($"Collection type is not defined in IO header");
            }


        }
    }


}
