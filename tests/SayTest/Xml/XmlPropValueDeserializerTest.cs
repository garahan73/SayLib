using Microsoft.VisualStudio.TestTools.UnitTesting;
using Say32.Xml.Serialize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Say32.Xml
{
    [TestClass]
    public partial class XmlPropValueDeserializerTest
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

        [TestMethod]
        public void DeserializeStringValue()
        {
            var xml = new XElement(nameof(Any), new XElement(nameof(Any.Text), _any.Text));
            XmlProperty prop = _any.GetType().GetProperty(nameof(Any.Text));
            AssertPropValue(xml, prop);
        }



        [TestMethod]
        public void DeserializeIntegerValue()
        {
            XmlProperty prop = _any.GetType().GetProperty(nameof(Any.Integer));
            var xml = new XElement(nameof(Any), new XElement(nameof(Any.Integer), _any.Integer));
            AssertPropValue(xml, prop);
        }

        [TestMethod]
        public void DeserializeFloatValue()
        {
            XmlProperty prop = _any.GetType().GetProperty(nameof(Any.Number));
            var xml = new XElement(nameof(Any), new XElement(nameof(Any.Number), _any.Number));
            AssertPropValue(xml, prop);
        }

        [TestMethod]
        public void DeserializeStringAttributeValue()
        {
            XmlProperty prop = _any.GetType().GetProperty(nameof(Any.AttribText));
            var xml = new XElement(nameof(Any), new XAttribute(nameof(Any.AttribText), _any.AttribText));
            AssertPropValue(xml, prop);
        }


        [TestMethod]
        public void DeserializeCamelNamedAttributeValue()
        {
            XmlProperty prop = _any.GetType().GetProperty(nameof(Any.AttribText));
            var xml = new XElement(nameof(Any), new XAttribute(nameof(Any.AttribText).ToCamelCase(), _any.AttribText));
            AssertPropValue(xml, prop);
        }


        [TestMethod]
        public void DeserializePropXmlValue()
        {
            XmlProperty prop = _any.GetType().GetProperty(nameof(Any.Sub));
            var xml = new XElement(nameof(Any),
                            new XElement(nameof(Any.Sub),
                                new XElement(nameof(Any),
                                    new XElement(nameof(Any.Text), "sub-text")
                                )
                            )
                        );
            var value = (Any)prop.DeserializePropValue(xml);
            Assert.AreEqual("sub-text", value.Text);
        }

        [TestMethod]
        public void DeserializeStringList()
        {
            XmlProperty prop = _any.GetType().GetProperty(nameof(Any.Strings));
            Assert.IsTrue(prop.IsCollection);

            var xml = _any.ToXElement();
            // new XElement(nameof(Any), new XElement(nameof(Any.Strings), _any.Strings.Select(s=> new XElement(XmlIoContext.DEFAULT_COLLECTION_ITEM_TAG, s))));
            Console.WriteLine(xml.Element(nameof(Any.Strings)));

            var list = (List<string>)prop.DeserializePropValue(xml);
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
            XmlProperty prop = _any.GetType().GetProperty(nameof(Any.Anys));
            Assert.IsTrue(prop.IsCollection);

            var xml = _any.ToXElement();
            Console.WriteLine(xml.Element(nameof(Any.Anys)));

            var list = (List<Any>)prop.DeserializePropValue(xml);
            //Console.WriteLine(xml.);

            AssertAnyList(list);
        }



        [TestMethod]
        public void WithTypeCollection()
        {
            XmlProperty prop = _any.GetType().GetProperty(nameof(Any.AnysSimple));
            Assert.IsTrue(prop.IsCollection);
            Console.WriteLine(prop.GetCustomAttributeFromPropertyAndValue<XmlCollectionAttribute>().XmlType);

            var xml = _any.ToXElement();
            Console.WriteLine(xml.Element(nameof(Any.AnysSimple)));

            var list = (AnyList)prop.DeserializePropValue(xml);
            //Console.WriteLine(xml.);

            AssertAnyList(list);

            Assert.AreEqual(default(int), list.Num);
        }

        [TestMethod]
        public void WithAllCollection()
        {
            XmlProperty prop = _any.GetType().GetProperty(nameof(Any.AnysDetail));
            Assert.IsTrue(prop.IsCollection);
            Console.WriteLine(prop.GetCustomAttributeFromPropertyAndValue<XmlCollectionAttribute>().XmlType);

            var xml = _any.ToXElement();
            Console.WriteLine(xml.Element(nameof(Any.AnysDetail)));

            var list = (AnyList)prop.DeserializePropValue(xml);
            //Console.WriteLine(xml.);

            AssertAnyList(list);

            Assert.AreEqual(_any.AnysDetail.Num, list.Num);
        }

        [TestMethod]
        public void ArrayCollection()
        {
            XmlProperty prop = _any.GetType().GetProperty(nameof(Any.AnysArray));
            Assert.IsTrue(prop.IsCollection);

            var xml = _any.ToXElement();
            Console.WriteLine(xml.Element(nameof(Any.AnysArray)));

            var list = (Any[])prop.DeserializePropValue(xml);
            //Console.WriteLine(xml.);

            AssertAnyList(list.ToList());
        }


        [TestMethod]
        public void PrimitiveDictionary()
        {
            XmlProperty prop = _any.GetType().GetProperty(nameof(Any.PrimitiveDic));
            Assert.IsTrue(prop.IsCollection);

            var xml = _any.ToXElement();
            Console.WriteLine(xml.Element(nameof(Any.PrimitiveDic)));

            var dic = (Dictionary<int, string>)prop.DeserializePropValue(xml);
            //Console.WriteLine(xml.);

            Assert.AreEqual(2, dic.Count);
            Console.WriteLine(dic[1]);
            Console.WriteLine(dic[2]);
            Assert.AreEqual(_any.PrimitiveDic[1], dic[1]);
            Assert.AreEqual(_any.PrimitiveDic[2], dic[2]);
        }

        [TestMethod]
        public void AnyDictionary()
        {
            XmlProperty prop = _any.GetType().GetProperty(nameof(Any.AnyDic));
            Assert.IsTrue(prop.IsCollection);

            var xml = _any.ToXElement();
            Console.WriteLine(xml.Element(nameof(Any.AnyDic)));

            var dic = (Dictionary<int, Any>)prop.DeserializePropValue(xml);
            //Console.WriteLine(xml.);

            Assert.AreEqual(2, dic.Count);
            Console.WriteLine(dic[1]);
            Console.WriteLine(dic[2]);
            Assert.AreEqual(_any.AnyDic[1].Text, dic[1].Text);
            Assert.AreEqual(_any.AnyDic[2].Text, dic[2].Text);
        }


        [TestMethod]
        public void DeserializeAttributeAutomatically()
        {
            var xml = XElement.Parse($"<{nameof(Any)} {nameof(Any.Number)}='1.23'/>");
            XmlProperty prop = typeof(Any).GetProperty(nameof(Any.Number));

            var number = (double)prop.DeserializePropValue(xml);
        }

        [TestMethod]
        public void DeserializeCamelCaseAttributeAutomatically()
        {
            var xml = XElement.Parse($"<{nameof(Any)} {nameof(Any.Number).ToCamelCase()}='1.23'/>");
            XmlProperty prop = typeof(Any).GetProperty(nameof(Any.Number));

            var number = (double)prop.DeserializePropValue(xml);
        }
    }
}
