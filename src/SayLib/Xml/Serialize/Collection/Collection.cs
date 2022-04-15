using Say32.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;
using System.Linq;

namespace Say32.Xml.Serialize.Collection
{

    class CollectionXmlCreator
    {
        public static CollectionXml Serialize(XmlProperty? prop, IEnumerable collection, XmlOutputType itemOutputType, bool ignoreNullOrDefaultValue)
            => new CollectionXml(collection,
                CollectionItemsSerializer.Serialize(collection, GetCollectionItemXmlTagName(prop), itemOutputType, ignoreNullOrDefaultValue))
            {
                IgnoreNullOrDefault = ignoreNullOrDefaultValue,
                OutputType = GetXmlCollectionOutputType(collection, prop),
                TypeName = TypeNameExtractor.ExtractName(collection.GetType()),
                
            };

        private static CollectionXmlType? GetXmlCollectionOutputType(IEnumerable collection, XmlProperty? prop )
        {
            XmlIoDebug.Serializer.Log("COL-XML-CREATOR", () => $"prop = '{prop?.Name}', collection type = '{collection.GetType().Name}'");

            // xml output setting in property overides the setting in class definition
            var xmlOutputType = prop?.GetCustomAttribute<XmlCollectionAttribute>()?.XmlType ??
                                collection.GetType().GetCustomAttribute<XmlCollectionAttribute>()?.XmlType ??

                                // select output type automatically            
                                (IsNamedCollection(prop?.PropertyType) || IsNamedCollection(collection.GetType()) ? 
                                        CollectionXmlType.WithType : CollectionXmlType.ItemsOnly); 

            XmlIoDebug.Serializer.SubLog($"collection XML output type = {xmlOutputType}");

            return xmlOutputType;
        }

        private static bool IsNamedCollection( Type? type )
        {
            return type != null && 
                !type.IsGenericType && 
                !type.IsArray && 
                type.GetBaseClassesAndInterfaces(true, t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>)).DefaultIfEmpty() != null;
        }

        private static string GetCollectionItemXmlTagName( XmlProperty? prop )
            => prop?.GetCustomAttribute<XmlItemTagAttribute>()?.Name ?? XmlIoTags.ITEM_TAG;

    }

    class CollectionValueSerializer
    {
        public static CollectionXml Serialize(IEnumerable collection, XmlOutputType itemOutputType, bool ignoreNullOrDefaultValue)
        {
            XmlIoDebug.Serializer.Log("COL-VAL", () => $"collecton type = '{collection.GetType().Name}'");
            return CollectionXmlCreator.Serialize(null, collection, itemOutputType, ignoreNullOrDefaultValue);
        }
    }

    class CollectionItemsSerializer
    {
        public static List<object> Serialize(IEnumerable collection, string itemXmlTagName, XmlOutputType itemOutputType, bool ignoreNullValue)
        {
            XmlIoDebug.Serializer.Log("COL-ITEMS", ">>> START");

            var collectionItems = new List<object>();

            if (collection is IDictionary dic)
            {
                foreach (var key in dic.Keys)
                {
                    var value = dic[key];
                    var itemXml = CreateDictionaryItem(itemXmlTagName, ignoreNullValue, key, value);
                    collectionItems.Add(itemXml);
                }
            }
            else
            {
                foreach (var item in collection)
                {
                    /*
                    var itemXml = //IsCollection(item) ? (object)Serialize((IEnumerable)item, itemXmlTagName, itemAsXml, ignoreNullValue, ) :
                                    itemOutputType == XmlOutputType.AsXml ||
                                    itemOutputType == XmlOutputType.Auto && !item.GetType().IsPrimitive2() ?
                                    SerializeAsXml(ignoreNullValue, item) :
                                    SerializeAsPrimitive(itemXmlTagName, item);
                                    */

                    var itemXml = CreateListItem(itemXmlTagName, ignoreNullValue, item);

                    collectionItems.Add(itemXml);
                }
            }

            XmlIoDebug.Serializer.Log("COL-ITEMS", "<<< DONE");
            if (XmlIoDebug.Serializer.CanLog(XmlIoDebugLevel.DETAIL))
            {
                foreach (var item in collectionItems)
                {
                    XmlIoDebug.Serializer.Raw(item.ToString(), XmlIoDebugLevel.DETAIL);
                }
            }
            return collectionItems;
        }

        private static XElement CreateDictionaryItem(string itemXmlTagName, bool ignoreNullValue, object key, object value)
        {
            XmlIoDebug.Serializer.Log("DICTIONARY ITEM", ">>> START");

            var keyXml = XmlValueSerializer.Serialize(key, XmlOutputType.Auto, ignoreNullValue);
            var valueXml = XmlValueSerializer.Serialize(value, XmlOutputType.Auto, ignoreNullValue);

            var item  =new XElement(itemXmlTagName,
                    new XElement(XmlIoTags.KEY_TAG,  keyXml),
                    new XElement(XmlIoTags.VALUE_TAG, valueXml)
                );

            XmlIoDebug.Serializer.Log("DICTIONARY ITEM", "<<< DONE");
            XmlIoDebug.Serializer.Raw(() => item.ToString(), XmlIoDebugLevel.DETAIL);

            return item;
        }

        private static XElement CreateListItem(string itemXmlTagName, bool ignoreNullValue, object item)
        {
            XmlIoDebug.Serializer.Log("LIST ITEM", ">>> START");

            var valueXml = XmlValueSerializer.Serialize(item, XmlOutputType.Auto, ignoreNullValue);

            // wrap with <Item></Item> element if itemXml is primitive
            var itemXml = valueXml as XElement ?? new XElement(itemXmlTagName, valueXml);

            XmlIoDebug.Serializer.Log("LIST ITEM", "<<< DONE");
            XmlIoDebug.Serializer.Raw(() => itemXml.ToString(), XmlIoDebugLevel.DETAIL);

            return itemXml;
        }

    }
}