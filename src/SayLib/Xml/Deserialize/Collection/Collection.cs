using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Say32.Xml.Deserialize.Collection
{
    interface ICollectionDeserializer
    {
        //bool CanHandle(Type collectionType);

        object Deserialize(IEnumerable<XElement> itemElements);

        //IEnumerable<object> DeserializeCollectionItems(IEnumerable<XElement> itemElements);

        //object CreateCollection(IEnumerable<object> values);
    }


    abstract class CollectionDeserializer : ICollectionDeserializer
    {

        public object Deserialize(IEnumerable<XElement> itemElements)
        {
            if (false && XmlIoDebug.Deserializer.CanLog(XmlIoDebugLevel.DETAIL))
            {
                var i = 0;
                foreach (var itemElement in itemElements)
                {
                    XmlIoDebug.Deserializer.Log("COL", $"item[{i++}] xml", XmlIoDebugLevel.DETAIL);
                    XmlIoDebug.Deserializer.SubLog(() => itemElement.ToString(), XmlIoDebugLevel.DETAIL);
                }
            }

            var itemValues = DeserializeCollectionItems(itemElements);
            return CreateCollection(itemValues);
        }


        protected abstract IEnumerable<object?> DeserializeCollectionItems(IEnumerable<XElement> itemElements);

        protected abstract object CreateCollection(IEnumerable<object?> values);
    }



}
