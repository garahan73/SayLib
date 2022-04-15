using System.Collections.Generic;
using System.Xml.Serialization;

namespace Say32.Xml
{
    class Any
    {
        public List<string> Strings { get; set; }


        public Any Sub { get; set; }
        [AsXml] public List<Any> Anys { get; set; }
        public Any[] AnysArray { get; set; }

        public AnyList AnysSimple { get; set; }
        public AnyListAuto AnysAuto { get; set; }

        [XmlCollection(CollectionXmlType.WithAll)]
        public AnyList AnysDetail { get; set; }

        public Dictionary<int, string> PrimitiveDic { get; set; }
        public Dictionary<int, Any> AnyDic { get; set; }

        [XmlAttribute] public string AttribText { get; set; }
        [XmlAttribute] public string CamelName { get; set; }
        public string Text { get; set; }
        public int Integer { get; set; }
        public double Number { get; set; }

    }

    [XmlCollection(CollectionXmlType.WithType)]
    class AnyList : List<Any>
    {
        public int Num;
    }

    [XmlCollection(CollectionXmlType.WithType)]
    class AnyListAuto : List<Any>
    {
        public int Num = 0;
    }

}
