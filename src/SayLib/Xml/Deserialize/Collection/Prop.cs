using System;
using System.Linq;
using System.Xml.Linq;

namespace Say32.Xml.Deserialize.Collection
{
    class CollectionPropValueDeserializer
    {
        private readonly XmlTypeMap _xmlTypeMap;
        private readonly XmlProperty _prop;

        public CollectionPropValueDeserializer(XmlProperty prop, XmlTypeMap objectCreator)
        {
            _prop = prop;
            _xmlTypeMap = objectCreator;
        }

        
        /*
        public object DeserializeCollection0(XElement propXElement) => _prop.IsDictionary
        ? new DictionaryPropDeserializer(_prop, _objectCreator).DeserializeCollection(propXElement)
        : new ListOrArrayPropDeserializer(_prop, _objectCreator).DeserializeCollection(propXElement);
        */

        public object? DeserializeCollection(XElement propXElement)
        {
            if (!propXElement.HasElements) return null;

            //IEnumerable<XElement> valueElements = propXElement.Elements();

            var collectionXmlType = _prop.GetCustomAttributeFromPropertyAndValue<XmlCollectionAttribute>()?.XmlType ?? CollectionXmlType.ItemsOnly;
            XmlIoDebug.Deserializer.Log("PROP-COL", $"{collectionXmlType}");

            var collectionType = _prop.PropertyType;

            //var itemType = collectionType.IsArray ? collectionType.GetElementType() : collectionType.GetGenericArguments().FirstOrDefault();
            //XmlIoDebug.Deserializer.Log("PROP-COL", $"collection item type: {itemType?.Name}");

            var collectionItemTag = _prop.CollectionItemTag;
            return DeserializeCollection(propXElement, collectionXmlType, collectionType, collectionItemTag);

            //return new CollectionValueDeserializer(_objectCreator, _prop.CollectionItemTag).DeserializeCollection(propXElement.Elements(), collectionXmlType, _prop.PropertyType);
        }

        private object? DeserializeCollection(XElement propXElement, CollectionXmlType collectionXmlType, Type collectionType, string collectionItemTag)
        {
            if (collectionXmlType == CollectionXmlType.ItemsOnly)
                return new CollectionItemsDeserializer(_prop.ObjectType, _xmlTypeMap, collectionItemTag).DeserializeCollectionItemXElements(propXElement.Elements(), collectionType);
            else
                return new CollectionInheritedClassDeserializer(_prop.ObjectType, _xmlTypeMap, collectionItemTag)
                    .DeserializeCollectionXElement(propXElement.Elements().SingleOrDefault(), collectionXmlType, collectionType);
        }
    }



}
