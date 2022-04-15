using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml.Linq;
using System.Diagnostics;

namespace Say32.Xml.Deserialize.Collection
{
    class ListOrArrayDeserializer : CollectionDeserializer
    {
        private readonly XmlTypeMap _xmlTypeMap;
        private readonly Type _collectionType;
        private readonly string _itemTagName;
        private readonly Type _itemType;

        public ListOrArrayDeserializer(Type collectionType, XmlTypeMap objectCreator, string itemTagName)
        {
            _collectionType = collectionType;
            _xmlTypeMap = objectCreator;
            _itemTagName = itemTagName;
            _itemType = collectionType.IsArray ? collectionType.GetElementType() : collectionType.GetGenericArguments().FirstOrDefault();
        }

        public static bool CanHandle(Type collectionType) => collectionType.IsArray || typeof(IList).IsAssignableFrom(collectionType);

        protected override IEnumerable<object?> DeserializeCollectionItems(IEnumerable<XElement> itemElements)
        {
            XmlIoDebug.Deserializer.Log("LIST/ARRAY", $"{_collectionType.Name}[{_itemType?.Name}]");
            //XmlIoDebug.Deserializer.SubLog($"item type = {_itemType.Name}");

            var itemDeserializer = new ItemXmlDeserializer(_xmlTypeMap, _itemTagName);
            return itemElements.Select(itemXml => itemDeserializer.Deserialize(itemXml, _itemType));
        }

        protected override object CreateCollection(IEnumerable<object?> values)
        {
            return _collectionType.IsArray ? CreateArray(values, _itemType) : CreateList(values, _collectionType);
        }

        private object CreateArray(IEnumerable<object?> values, Type itemType)
        {
            var array = Array.CreateInstance(itemType, values.Count());

            var i = 0;
            foreach (var item in values)
            {
                array.SetValue(item, i++);
            }

            return array;
        }       

        private IList CreateList(IEnumerable<object?> values, Type collectionType)
        {
            var list = (IList)Activator.CreateInstance(collectionType);
            foreach (var item in values)
            {
                list.Add(item);
            }
            return list;
        }
    }
}
