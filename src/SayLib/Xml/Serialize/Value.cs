using Say32.Xml.Serialize.Collection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Say32.Xml.Serialize
{
    class XmlValueSerializer
    {
        public static object? Serialize(object? value, XmlOutputType outputType, bool ignoreNullOrDefaultValue, bool asCData=false)
        {
            if (value == null) return null;

            XmlIoDebug.Serializer.Log("VALUE", value.GetType().Name);

            //var customSerializerAttrib = (XmlCustomSerializerAttribute)value.GetType().GetCustomAttributes(typeof(XmlCustomSerializerAttribute), true).SingleOrDefault();

            if (CustomSerializer.TryToSerialize(value, null, out var serializedXml))
                return serializedXml;

            if (value is Array || value is ICollection)
            {
                return CollectionValueSerializer.Serialize((IEnumerable)value, outputType, ignoreNullOrDefaultValue).Result;
            }
            else
            {
                return SerializeSingleValue(value, outputType, ignoreNullOrDefaultValue, asCData);
            }
           
        }

        private static object? SerializeSingleValue(object? value, XmlOutputType outputType, bool ignoreNullOrDefaultValue, bool asCData)
        {
            if (value == null) return null;

            XmlIoDebug.Serializer.Log("SINGLE VALUE", outputType.ToString());

            if (outputType == XmlOutputType.AsXml)
                return new ClassObjectSerializer(value, ignoreNullOrDefaultValue).Serialize();

            if (!asCData)
                asCData = value.GetType().GetCustomAttribute<XmlCDataAttribute>() != null;

            switch (outputType)
            {
                case XmlOutputType.AsPrimitive: return XmlPrimitiveValueSerializer.Serialize(value, asCData);
                case XmlOutputType.Auto: 
                    return XmlPrimitiveValueSerializer.IsPrimitive(value.GetType()) ? 
                        XmlPrimitiveValueSerializer.Serialize(value, asCData) : 
                        new ClassObjectSerializer(value, ignoreNullOrDefaultValue).Serialize();
            }
            throw new Exception($"Can't reach here - {nameof(PropValueSerializer)}.{nameof(Serialize)}()");
        }
    }

    class XmlPrimitiveValueSerializer
    {
        public static bool IsPrimitive(Type type) => type.IsPrimitiveExt();

        public static object? Serialize(object value, bool asCData)
        {
            return asCData ? value == null ? null : new XCData(value.ToString()) : value;
        }
    }
}
