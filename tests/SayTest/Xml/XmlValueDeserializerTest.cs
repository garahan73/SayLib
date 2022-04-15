using Microsoft.VisualStudio.TestTools.UnitTesting;
using Say32.Xml.Deserialize;
using Say32.Xml.Deserialize.Collection;
using Say32.Xml.Serialize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Say32.Xml
{
    [TestClass]
    public partial class XmlValueDeserializerTest
    {
        private XmlTypeMap _typeMap;
        private XNodeValueDeserializer _valueDeserializer;

        private Any _any;
        private XElement _anyXml;

        [TestInitialize]
        public void Init()
        {
            _typeMap = new XmlTypeMap();
            _typeMap.Register(typeof(Any));
            _typeMap.Register(typeof(AnyList));
            _typeMap.Register(typeof(AnyListAuto));

            _valueDeserializer = new XNodeValueDeserializer(_typeMap);

            _any = new Any
            {
                Strings = new List<string> { "aaa", "bbb" },

                Text = "abc",
                AttribText = "def",
                CamelName = "ghi",
                Integer = 1,
                Number = 1.23,
                Sub = new Any { Text = "sub" },
                Anys = new List<Any>
                {
                    new Any{ Text = "aa" },
                    new Any{ Text = "bb" }
                },
                AnysSimple = new AnyList
                {
                    new Any{ Text = "aa" },
                    new Any{ Text = "bb" }
                },
                AnysAuto = new AnyListAuto
                {
                    new Any{ Text = "aa" },
                    new Any{ Text = "bb" }
                },
                AnysDetail = new AnyList
                {
                    new Any{ Text = "aa" },
                    new Any{ Text = "bb" }
                },
                AnysArray = new Any[]
                    {
                    new Any{ Text = "aa" },
                    new Any{ Text = "bb" }

                    },
                PrimitiveDic = new Dictionary<int, string> { { 1, "aaa" }, { 2, "bbb" } },
                AnyDic = new Dictionary<int, Any>
                {
                    { 1, new Any { Text ="aa"  } },
                    { 2, new Any { Text = "bb" } }
                }
            };

            _any.AnysSimple.Num = 11;
            _any.AnysDetail.Num = 12;

            _anyXml = (XElement)_any.ToXElement();

            XmlIoDebug.Serializer.Enabled = false;
            XmlIoDebug.Deserializer.Enabled = true;
            XmlIoDebug.Deserializer.Level = XmlIoDebugLevel.DETAIL;
        }



        private void AssertAnyList(List<object> list) => AssertAnyList(list.Select(o => (XElement)o).ToList());

        private static void AssertAnyList(List<XElement> list)
        {
            Assert.AreEqual(2, list.Count);
            Console.WriteLine(list[0]);
            Console.WriteLine(list[1]);
            Assert.AreEqual(nameof(Any), list[0].Name.ToString());
            Assert.AreEqual("aa", list[0].Element(nameof(Any.Text)).Value);
            Assert.AreEqual("bb", list[1].Element(nameof(Any.Text)).Value);
        }

        private void AssertPropValue(XElement xml, XmlProperty prop)
        {
            var value = prop.DeserializePropValue(xml);
            Assert.AreEqual(prop.GetValue(_any), value);
        }


        private void AssertPrimitiveValue(object value)
        {
            var xml = XmlValueSerializer.Serialize(value, XmlOutputType.Auto, true);
            Assert.AreEqual(value, xml);

            var parsed = _valueDeserializer.Deserialize(new XText(xml.ToString()), value.GetType());
            Assert.AreEqual(value, parsed);
        }

        [TestMethod]
        public void DeserializeStringValue()
        {
            AssertPrimitiveValue(_any.Text);
        }

        [TestMethod]
        public void DeserializeIntegerValue()
        {
            AssertPrimitiveValue(_any.Integer);
        }

        [TestMethod]
        public void DeserializeFloatValue()
        {
            AssertPrimitiveValue(_any.Number);
        }


        [TestMethod]
        public void DeserializePropXmlValue()
        {
            var xml = new XElement(nameof(Any),
                               new XElement(nameof(Any.Text), "sub-text"));

            var value = (Any)_valueDeserializer.Deserialize(xml);
            Assert.AreEqual("sub-text", value.Text);
        }

        private XElement GetPropValueXml(string propName) => _anyXml.Element(propName).Elements().First();

        [TestMethod]
        public void DeserializeStringList()
        {
            var itemXmls = _anyXml.Element(nameof(Any.Strings)).Elements();
            Console.WriteLine(itemXmls);

            var list = (List<string>)new CollectionItemsDeserializer(null, _typeMap, XmlIoTags.ITEM_TAG).DeserializeCollectionItemXElements(itemXmls, _any.Strings.GetType());
            //Console.WriteLine(xml.);

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("aaa", list[0]);
            Assert.AreEqual("bbb", list[1]);
        }

        private static void AssertAnyList(List<Any> list)
        {
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("aa", list[0].Text);
            Assert.AreEqual("bb", list[1].Text);
        }

        [TestMethod]
        public void ItemsOnlyCollection()
        {
            var itemXmls = _anyXml.Element(nameof(Any.Anys)).Elements();
            Console.WriteLine(itemXmls);

            var list = (List<Any>)new CollectionItemsDeserializer(null, _typeMap, XmlIoTags.ITEM_TAG).DeserializeCollectionItemXElements(itemXmls, _any.Anys.GetType());         
            AssertAnyList(list);
        }



        [TestMethod]
        public void WithTypeCollection1()
        {
            var collectionXml = _anyXml.Element(nameof(Any.AnysSimple)).Elements().Single(); ;
            Console.WriteLine(collectionXml);

            var list = (AnyList)new CollectionInheritedClassDeserializer(null, _typeMap, XmlIoTags.ITEM_TAG).DeserializeCollectionXElement(collectionXml, CollectionXmlType.WithType, _any.AnysSimple.GetType());
            AssertAnyList(list);
            Assert.AreEqual(default(int), list.Num);
        }

        [TestMethod]
        public void WithTypeCollection2()
        {
            var collectionXml = _anyXml.Element(nameof(Any.AnysAuto)).Elements().Single(); ;
            Console.WriteLine(collectionXml);

            var list = (AnyListAuto)new CollectionInheritedClassDeserializer(null, _typeMap, XmlIoTags.ITEM_TAG).DeserializeCollectionXElement(collectionXml, CollectionXmlType.WithType, _any.AnysAuto.GetType());
            AssertAnyList(list);
            Assert.AreEqual(default(int), list.Num);
        }

        [TestMethod]
        public void WithAllCollection()
        {
            var collectionXml = _anyXml.Element(nameof(Any.AnysDetail)).Elements().Single();
            Console.WriteLine(collectionXml);

            var list = (AnyList)new CollectionInheritedClassDeserializer(null, _typeMap, XmlIoTags.ITEM_TAG).DeserializeCollectionXElement(collectionXml, CollectionXmlType.WithAll, _any.AnysDetail.GetType());
            AssertAnyList(list);
            Assert.AreEqual(_any.AnysDetail.Num, list.Num);
        }

        [TestMethod]
        public void ArrayCollection()
        {
            var collectionItemElements = _anyXml.Element(nameof(Any.AnysArray)).Elements();
            Console.WriteLine(collectionItemElements);

            var list = (Any[])new CollectionItemsDeserializer(null, _typeMap, XmlIoTags.ITEM_TAG).DeserializeCollectionItemXElements(collectionItemElements, _any.AnysArray.GetType());
            AssertAnyList(list.ToList());            
        }


        [TestMethod]
        public void PrimitiveDictionary()
        {
            //Dictionary<int, string>
            var collectionItemElements = _anyXml.Element(nameof(Any.PrimitiveDic)).Elements();
            Console.WriteLine(collectionItemElements);

            var dic = (Dictionary<int, string>)new CollectionItemsDeserializer(null, _typeMap, XmlIoTags.ITEM_TAG).DeserializeCollectionItemXElements(collectionItemElements, _any.PrimitiveDic.GetType());

            Assert.AreEqual(2, dic.Count);
            Console.WriteLine(dic[1]);
            Console.WriteLine(dic[2]);
            Assert.AreEqual(_any.PrimitiveDic[1], dic[1]);
            Assert.AreEqual(_any.PrimitiveDic[2], dic[2]);
        }

        [TestMethod]
        public void AnyDictionary()
        {
            var collectionItemElements = _anyXml.Element(nameof(Any.AnyDic)).Elements();
            Console.WriteLine(collectionItemElements);

            var dic = (Dictionary<int, Any>)new CollectionItemsDeserializer(null, _typeMap, XmlIoTags.ITEM_TAG).DeserializeCollectionItemXElements(collectionItemElements, _any.AnyDic.GetType());

            Assert.AreEqual(2, dic.Count);
            Console.WriteLine(dic[1]);
            Console.WriteLine(dic[2]);
            Assert.AreEqual(_any.AnyDic[1].Text, dic[1].Text);
            Assert.AreEqual(_any.AnyDic[2].Text, dic[2].Text);
        }

    }
}
