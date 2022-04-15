using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace Say32.Xml.Deserialize.Collection
{
    class CollectionItemsDeserializerBase
    {
        protected readonly Type? _collectionType;
        protected readonly XmlTypeMap _xmlTypeMap;
        protected readonly string _collectionItemTag;

        public CollectionItemsDeserializerBase( Type? type, XmlTypeMap objectCreator, string collectionItemTag)
        {
            _collectionType = type;
            _xmlTypeMap = objectCreator;
            _collectionItemTag = collectionItemTag;
        }

        /*
        private ICollectionDeserializer GetCollectionDeserializer(Type collectionType)
            => typeof(IDictionary).IsAssignableFrom(collectionType) ? (ICollectionDeserializer)new DictionaryDeserializer(collectionType, _objectCreator) :
                collectionType.IsCollection() ? new ListOrArrayDeserializer(collectionType, _objectCreator, _collectionItemTag) :
                throw new ArgumentException("Collection type is not collection");
        */

        protected ICollectionDeserializer GetCollectionDeserializer(Type collectionType)
            => DictionaryDeserializer.CanHandle(collectionType) ? (ICollectionDeserializer)new DictionaryDeserializer(collectionType, _xmlTypeMap) :
                ListOrArrayDeserializer.CanHandle(collectionType) ? new ListOrArrayDeserializer(collectionType, _xmlTypeMap, _collectionItemTag) :
                throw new ArgumentException("Collection type is not collection");

    }

    class CollectionItemsDeserializer : CollectionItemsDeserializerBase
    {
        public CollectionItemsDeserializer(Type? collectionType, XmlTypeMap xmlTypeMap, string collectionItemTag) : base(collectionType, xmlTypeMap, collectionItemTag)
        {
        }

        public object DeserializeCollectionItemXElements(IEnumerable<XElement> collectionItemElements, Type collectionType)
        {
            XmlIoDebug.Deserializer.Log("COL-ITEMS", collectionType.Name);

            collectionType = _xmlTypeMap.GetMoreDetailedType(collectionType, _collectionType);

            var collectionDeserializer = GetCollectionDeserializer(collectionType);
            return collectionDeserializer.Deserialize(collectionItemElements);
        }
    }

    class CollectionInheritedClassDeserializer : CollectionItemsDeserializerBase
    {
        public CollectionInheritedClassDeserializer( Type? type, XmlTypeMap objectCreator, string collectionItemTag) : base(type, objectCreator, collectionItemTag)
        {
        }


        public object? DeserializeCollectionXElement(XElement collectionElement, CollectionXmlType collectionXmlType, Type collectionType)
        {
            XmlIoDebug.Deserializer.Log("COL-INHERITED", collectionXmlType.ToString());

            if (collectionElement == null)
            {
                XmlIoDebug.Deserializer.SubLog("collection is null");
                return null;
            }

            if (false && XmlIoDebug.Deserializer.CanLog(XmlIoDebugLevel.DETAIL))
            {
                var i = 0;
                foreach (var xele in collectionElement.Elements())
                {
                    XmlIoDebug.Deserializer.Log("COL", $"xelement[{i++}]", XmlIoDebugLevel.DETAIL);
                    XmlIoDebug.Deserializer.SubLog(() => xele.ToString(), XmlIoDebugLevel.DETAIL);
                }
            }

            if (collectionXmlType == CollectionXmlType.UNKNOWN)
            {
                XmlIoDebug.Deserializer.SubLog($"collection xml type is unknown, checking '{XmlIoTags.ITEMS_TAG}' element...", XmlIoDebugLevel.DETAIL);
                collectionXmlType = collectionElement.Element(XmlIoTags.ITEMS_TAG) != null ? CollectionXmlType.WithAll : CollectionXmlType.WithType;
            }

            XmlIoDebug.Deserializer.SubLog($"collection xml type = {collectionXmlType}");

            if (collectionXmlType == CollectionXmlType.WithType)
            {
                GetCollectionTypeAndDeserializer(collectionElement, out collectionType, out var collectionDeserializer);
                return collectionDeserializer.Deserialize(collectionElement.Elements());
            }
            else if (collectionXmlType == CollectionXmlType.WithAll)
            {
                GetCollectionTypeAndDeserializer(collectionElement, out collectionType, out var collectionDeserializer);

                var itemElements = collectionElement.Element(XmlIoTags.ITEMS_TAG).Elements();
                var collection = collectionDeserializer.Deserialize(itemElements);

                // additional property xmls
                DeserializeAdditionalPropertiesForWithAll(collectionElement, collectionType, collection);

                return collection;
            }
            else
            {
                throw new ArgumentException($"Only '{CollectionXmlType.WithType}' or '{CollectionXmlType.WithAll}' are supported", nameof(collectionXmlType));
            }
        }

        private void GetCollectionTypeAndDeserializer(XElement collectionElement, out Type collectionType, out ICollectionDeserializer collectionDeserializer)
        {
            var tmpCollectionType = _xmlTypeMap.GetTypeSafe(collectionElement.Name.LocalName);
            tmpCollectionType = _xmlTypeMap.GetMoreDetailedType(tmpCollectionType, _collectionType);

            if (tmpCollectionType == null)
                throw new Exception($"Can't get collection type. XML = '<{collectionElement.Name}>'");

            collectionType = tmpCollectionType;

            collectionDeserializer = GetCollectionDeserializer(collectionType);
        }

        private void DeserializeAdditionalPropertiesForWithAll(XElement typeXml, Type collectionType, object collection)
        {
            foreach (var propXml in typeXml.Elements().Where(x => x.Name.LocalName != XmlIoTags.ITEMS_TAG))
            {
                var propName = propXml.Name.LocalName;

                var prop = XmlProperty.Create(collectionType, propName);
                if (prop != null)
                {
                    Debug.WriteLine($" - collection additional property: {propName}");
                    new PropertyDeserializer(_xmlTypeMap).DeserializePropXElement(collection, propXml);
                }
            }
        }
    }





}
