using Microsoft.VisualStudio.TestTools.UnitTesting;
using Say32.Object;
using Say32.Xml.Serialize;
using Say32.Xml.Serialize.Collection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Say32.Xml
{
    [TestClass]
    public class XmlValueSerializerTest
    {
        private Any _any;
        //private ValueXmlSerializer serilalizer;

        [TestInitialize]
        public void Init()
        {
            _any = new Any
            {
                Strings = new List<string> { "aaa", "bbb" },
                Text = "abc",
                Anys = new List<Any> {
                    new Any { Text = "aa" },
                    new Any { Text = "bb" }
                },
                AnysSimple = new AnyList
                {
                    new Any { Text = "aa" },
                    new Any { Text = "bb" }
                },
                AnysAuto = new AnyListAuto
                {
                    new Any { Text = "aa" },
                    new Any { Text = "bb" }
                },
                AnysDetail = new AnyList
                {
                    new Any { Text = "aa" },
                    new Any { Text = "bb" }
                },
                AnysArray = new Any[]
                {
                    new Any { Text = "aa" },
                    new Any { Text = "bb" }
                },

                PrimitiveDic = new Dictionary<int, string> { { 1, "aaa" }, { 2, "bbb" } },
                AnyDic  = new Dictionary<int, Any>
                {
                    { 1, new Any { Text ="aa"  } },
                    { 2, new Any { Text = "bb" } }
                }
            };

            _any.AnysSimple.Num = 11;
            _any.AnysDetail.Num = 12;
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

        [TestMethod]
        public void StringValueSerializingTest()
        {
            var value = PropValueSerializer.Serialize(_any.GetType().GetProperty(nameof(Any.Text)), _any.Text, XmlOutputType.Auto, true);
            Assert.AreEqual(_any.Text, value);
        }

        [TestMethod]
        public void ListPrimitiveValueSerializingTest()
        {
            var list = ((List<object>)PropValueSerializer.Serialize(_any.GetType().GetProperty(nameof(Any.Strings)), _any.Strings, XmlOutputType.Auto, true)).Select(o=>(XElement)o).ToList();
            //Console.WriteLine(xml.);

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(XmlIoTags.ITEM_TAG, list[0].Name.ToString());
            Assert.AreEqual("aaa", list[0].Value);
            Assert.AreEqual("bbb", list[1].Value);
        }

        [TestMethod]
        public void ItemsOnlyCollection()
        {
            var list = (List<object>)PropValueSerializer.Serialize(_any.GetType().GetProperty(nameof(Any.Anys)), _any.Anys, XmlOutputType.Auto, true);
            AssertAnyList(list);
        }

        [TestMethod]
        public void WithTypeByAttribute()
        {
            var listXml = (XElement)PropValueSerializer.Serialize(_any.GetType().GetProperty(nameof(Any.AnysSimple)), _any.AnysSimple, XmlOutputType.Auto, true);
            Assert.AreEqual(nameof(AnyList), listXml.Name.ToString());

            var list = listXml.Elements().ToList();
            AssertAnyList(list);
        }

        [TestMethod]
        public void WithTypeAuto()
        {
            var listXml = (XElement)PropValueSerializer.Serialize(_any.GetType().GetProperty(nameof(Any.AnysAuto)), _any.AnysAuto, XmlOutputType.Auto, true);
            Assert.AreEqual(nameof(AnyListAuto), listXml.Name.LocalName);           

            var list = listXml.Elements().ToList();
            AssertAnyList(list);
        }

        [TestMethod]
        public void DetailedCollection()
        {
            var listXml = (XElement)PropValueSerializer.Serialize(_any.GetType().GetProperty(nameof(Any.AnysDetail)), _any.AnysDetail, XmlOutputType.Auto, true);
            Console.WriteLine(listXml);
            Assert.AreEqual(nameof(AnyList), listXml.Name.ToString());
            Assert.AreEqual("12", listXml.Element(nameof(AnyList.Num)).Value);

            var list = listXml.Element("Items").Elements().ToList();
            AssertAnyList(list);
        }

        [TestMethod]
        public void CustomListXmlValueItemsSerializingTest()
        {
            var collectionXml = CollectionXmlCreator.Serialize(_any.GetType().GetProperty(nameof(Any.AnysSimple)), _any.AnysSimple, XmlOutputType.Auto, true);
            var list = collectionXml.XmlItems;
            AssertAnyList(list);

            var simple = collectionXml.Simple;
            Console.WriteLine(simple);
            Assert.AreEqual(nameof(AnyList), simple.Name.ToString());
            AssertAnyList(simple.Elements().ToList());

            var detailed = collectionXml.Detailed;
            Console.WriteLine(detailed);
            Assert.AreEqual(nameof(AnyList), detailed.Name.ToString());
            Assert.AreEqual("11", detailed.Element(nameof(AnyList.Num)).Value);
            AssertAnyList(detailed.Element("Items").Elements().ToList());
        }

        [TestMethod]
        public void Array()
        {
            var list = (List<object>)PropValueSerializer.Serialize(_any.GetType().GetProperty(nameof(Any.AnysArray)), _any.AnysArray, XmlOutputType.Auto, true);
            AssertAnyList(list);
        }

        [TestMethod]
        public void PrimitiveDicSerializingTest()
        {
            var list = ((List<object>)PropValueSerializer.Serialize(_any.GetType().GetProperty(nameof(Any.PrimitiveDic)), _any.PrimitiveDic, XmlOutputType.Auto, true)).Select(o => (XElement)o).ToList();

            Assert.AreEqual(2, list.Count);
            Console.WriteLine(list[0]);
            Console.WriteLine(list[1]);

            Assert.AreEqual(XmlIoTags.ITEM_TAG, list[0].Name.LocalName);
            Assert.AreEqual("1", list[0].Element(XmlIoTags.KEY_TAG).Value);
            Assert.AreEqual("aaa", list[0].Element(XmlIoTags.VALUE_TAG).Value);
            Assert.AreEqual("2", list[1].Element(XmlIoTags.KEY_TAG).Value);
            Assert.AreEqual("bbb", list[1].Element(XmlIoTags.VALUE_TAG).Value);
        }

        [TestMethod]
        public void ClassDicSerializingTest()
        {
            var list = ((List<object>)PropValueSerializer.Serialize(_any.GetType().GetProperty(nameof(Any.AnyDic)), _any.AnyDic, XmlOutputType.Auto, true)).Select(o => (XElement)o).ToList();

            Assert.AreEqual(2, list.Count);
            Console.WriteLine(list[0]);
            Console.WriteLine(list[1]);

            Assert.AreEqual(XmlIoTags.ITEM_TAG, list[0].Name.LocalName);
            Assert.AreEqual(XmlIoTags.ITEM_TAG, list[1].Name.LocalName);

            // assert keys
            Assert.AreEqual("1", list[0].Element(XmlIoTags.KEY_TAG).Value);
            Assert.AreEqual("2", list[1].Element(XmlIoTags.KEY_TAG).Value);

            // assert values
            var anylist = new List<object>
            {
                list[0].Element(XmlIoTags.VALUE_TAG).Elements().Single(),
                list[1].Element(XmlIoTags.VALUE_TAG).Elements().Single(),
            };
            AssertAnyList(anylist);

        }

        class List2<T> : List<T>
        {

        }

        [TestMethod]
        public void SerializeIheritedList()
        {
            var list = new List2<int> { 1, 2, 3 };
            var xml = XmlValueSerializer.Serialize(list, XmlOutputType.Auto, true);
            Console.WriteLine(xml);
        }



    }
}
