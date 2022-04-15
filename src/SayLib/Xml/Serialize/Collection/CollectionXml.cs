using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Say32.Xml.Serialize.Collection
{
    class CollectionXml
    {
        public CollectionXml(IEnumerable collection, List<object> xmlItems)
        {
            Collection = collection;
            XmlItems = xmlItems;
        }

        public static bool IsListOrArray(object value) => value is IList || value is Array;

        public IEnumerable Collection { get;  }
        public bool IgnoreNullOrDefault { get; set; }

        public List<object> XmlItems { get; set; }
        public List<XElement>? DeclaredProperties { get; set; }

        public string? TypeName { get; set; }

        public XElement Simple => new XElement(TypeName, XmlItems);
        public XElement Detailed
        {
            get
            {
                var xele = new ClassObjectSerializer(Collection, IgnoreNullOrDefault, true).Serialize()
                    ?? throw new Exception();

                xele.Add(new XElement(XmlIoTags.ITEMS_TAG, XmlItems));

                return xele;
            }
        }

        public CollectionXmlType? OutputType { get; internal set; }

        public object Result
        {
            get
            {
                XmlIoDebug.Serializer.Log("COL-XML", $"{OutputType ?? CollectionXmlType.ItemsOnly}");

                switch (OutputType)
                {
                    case null: return XmlItems;
                    case CollectionXmlType.ItemsOnly: return XmlItems;
                    case CollectionXmlType.WithType: return Simple;
                    case CollectionXmlType.WithAll: return Detailed;
                }
                throw new Exception($"can't reach here. - {nameof(PropValueSerializer)}.{nameof(Result)}");
            }
        }
    }
}