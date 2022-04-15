using Say32.Xml.Deserialize.Collection;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Say32.Xml.Deserialize
{



    class XNodeValueDeserializer : ObjectCreatorHolder
    {
        public XNodeValueDeserializer(XmlTypeMap xmlTypeMap) : base(xmlTypeMap)
        {
        }

        public object? Deserialize(XNode valueXml, Type? type = null)
        {
            XmlIoDebug.Deserializer.Log("SINGLE VALUE", type?.Name);
            XmlIoDebug.Deserializer.Log(() => $"\n{valueXml}", XmlIoDebugLevel.DETAIL);

            if (CustomDeserializer.TryToDeserialize(type, valueXml, out var deserializedObject))
                return deserializedObject;

            if (valueXml is XElement xele)
                return new XElementValueDeserializer(_xmlTypeMap).Deserialize(xele, type);
            else if (valueXml is XText xtext)
                return PrimitiveValueDeserializer.Deserialize(xtext.Value, type ?? throw new Exception("Primitive type can't be null"));
            else
                throw new XmlDeserializationException($"XNode type({valueXml?.GetType().Name}) is invalid.",valueXml);
        }

        //private object DeserializeXtext(XText xtext, Type type) => PrimitiveValueDeserializer.Deserialize(xtext.Value, type);

    }


    class XElementValueDeserializer : ObjectCreatorHolder
    {
        public XElementValueDeserializer(XmlTypeMap objectCreator) : base(objectCreator)
        {
        }

        public object? Deserialize(XElement valueXml, Type? type)
        {
            XmlIoDebug.Deserializer.SubLog($"deserialize XElement");

            // if type is null, get it from XmlName
            var xmlType = _xmlTypeMap.GetTypeSafe(valueXml.Name.LocalName);
            type = _xmlTypeMap.GetMoreDetailedType(xmlType, type);

            if (type == null || type.IsInterface)
                throw new MissingTypeException($"Can't find type '{valueXml.Name.LocalName}'. No type info. in CIM XML type map.");

            if (PrimitiveValueDeserializer.IsPrimitiveType(type))
            {
                return PrimitiveValueDeserializer.Deserialize(valueXml.Value, type);
            }
            else if (type.IsCollection())
            {
                return new CollectionInheritedClassDeserializer(type, _xmlTypeMap, XmlIoTags.ITEM_TAG).DeserializeCollectionXElement(valueXml, CollectionXmlType.UNKNOWN, type);
            }

            return new ClassObjectDeserializer(type, _xmlTypeMap).Deserialize(valueXml);
        }

    }


    class PrimitiveValueDeserializer
    {
        internal static bool IsPrimitiveType(Type type) => type.IsPrimitiveExt() || type.IsEnum;

        public static object Deserialize(string valueText, Type type)
        {
            XmlIoDebug.Deserializer.Log("PRIMITIVE VALUE", valueText);

            if (type.IsEnum)
                return Enum.Parse(type, valueText);
            else
            {
                return Convert.ChangeType(valueText, type);
            }

            //public static object Deserialize(XmlProperty prop, string valueText) => Deserialize(valueText, prop.PropertyType);
        }
    }


}
