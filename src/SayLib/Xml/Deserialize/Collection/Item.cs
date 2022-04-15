using System;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace Say32.Xml.Deserialize.Collection
{
    class ItemXmlDeserializer 
    {
        private readonly XmlTypeMap _xmlTypeMap;
        private readonly string _xmlName;

        public ItemXmlDeserializer(XmlTypeMap objectCreator, string collectionItemTag)
        {
            _xmlTypeMap = objectCreator;
            _xmlName = collectionItemTag;            
        }

        public object? Deserialize(XElement itemXml, Type? itemType = null)
        {
            try
            {
                XmlIoDebug.Deserializer.Log("COL-ITEM", itemType?.Name);
                XmlIoDebug.Deserializer.Raw(() => itemXml.ToString(), XmlIoDebugLevel.DETAIL);

                // primitive
                if (itemType != null && PrimitiveValueDeserializer.IsPrimitiveType(itemType))
                {
                    XmlIoDebug.Deserializer.SubLog($"primitive value={itemXml.Value}");
                    return PrimitiveValueDeserializer.Deserialize(itemXml.Value, itemType);
                }

                // is wrapped <Item>....</item>
                var isWrapped = itemXml.Name.LocalName == _xmlName;
                //Ex.Assert<XmlNameMismatchException>(itemXml.Name.LocalName == _xmlName, $"expected = '{_xmlName}', actual = '{itemXml.Name.LocalName}'");

                var valueXml = isWrapped ? itemXml.Elements().Single() : itemXml;
                XmlIoDebug.Deserializer.SubLog($"value xml = \n{valueXml}", XmlIoDebugLevel.DETAIL);

                var typeName = valueXml.Name.LocalName;

                // class object
                if (itemType == null || typeName != itemType.Name)
                {
                    XmlIoDebug.Deserializer.SubLog($"item type name = '{typeName}'", XmlIoDebugLevel.DETAIL);
                    //Debug.WriteLine(_objectCreator.PrintReferenceTypes());
                    itemType = _xmlTypeMap.GetTypeSafe(typeName);
                    Ex.Assert<MissingTypeException>(itemType != null, $"Can't get type from type name '{typeName}'");
                    XmlIoDebug.Deserializer.SubLog($"item type changed = {itemType?.Name}");
                }

                return new XNodeValueDeserializer(_xmlTypeMap).Deserialize(valueXml, itemType);
            }
            catch  (Exception ex)
            {
                throw new XmlDeserializationException($"{nameof(itemType)} = '{itemType}'", itemXml, ex);
            }
        }
    }

    class DictionaryItemXmlDeserializer 
    {
        private readonly XmlTypeMap _objectCreator;
        
        public DictionaryItemXmlDeserializer(XmlTypeMap objectCreator)
        {
            _objectCreator = objectCreator;        
        }

        public (object key, object? value) Deserialize(XElement itemXml, Type collectionType)
        {
            XmlIoDebug.Deserializer.Log("dictionary item xml", $"\ndic type = '{collectionType.Name}'\nxml={itemXml}");

            // key
            var keyType = collectionType.GetGenericArguments()[0];
            keyType = keyType == typeof(object) ? null : keyType;
            var key = new ItemXmlDeserializer(_objectCreator, XmlIoTags.KEY_TAG).Deserialize(itemXml.Element(XmlIoTags.KEY_TAG), keyType);

            if (key == null)
                throw new Exception("Collection Dictionary Item Key is null");

            // value
            var valueType = collectionType.GetGenericArguments()[1];
            valueType = valueType == typeof(object) ? null : valueType;
            var value = new ItemXmlDeserializer(_objectCreator, XmlIoTags.VALUE_TAG).Deserialize(itemXml.Element(XmlIoTags.VALUE_TAG), valueType);

            return (key, value);
        }
    }
}
