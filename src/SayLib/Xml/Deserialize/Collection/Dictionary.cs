using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml.Linq;
using System.Diagnostics;

namespace Say32.Xml.Deserialize.Collection
{

    class DictionaryDeserializer : CollectionDeserializer
    {
        private readonly XmlTypeMap _objectCreator;
        private readonly Type _collectionType;

        public DictionaryDeserializer(Type collectionType, XmlTypeMap objectCreator)
        {
            _collectionType = collectionType;
            _objectCreator = objectCreator;
        }

        public static bool CanHandle(Type collectionType) => typeof(IDictionary).IsAssignableFrom(collectionType);

        protected override IEnumerable<object> DeserializeCollectionItems(IEnumerable<XElement> itemElements)
        {
            var itemDeserializer = new DictionaryItemXmlDeserializer(_objectCreator);
            return itemElements.Select(ix => (object)itemDeserializer.Deserialize(ix, _collectionType));
        }

        protected override object CreateCollection(IEnumerable<object?> values)
        {
            var dic = (IDictionary)Activator.CreateInstance(_collectionType);

            foreach (var obj in values)
            {
                var kvTuple = obj ?? throw new Exception("Dictionary Item(KeyValue) can't be null"); ;
                var kv = ((object key, object? value))kvTuple;
                
                dic.Add(kv.key, kv.value);
            }

            return dic;
        }


    }

}
