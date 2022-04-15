using Microsoft.VisualStudio.TestTools.UnitTesting;
using Say32.Xml.Serialize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Say32.Xml
{
    [TestClass]
    public class ObjectXElementSerializerTest
    {
        class Simple
        {
            public int Num;
            public string Text { get; set; }
        }

        class Inherited : Simple
        {
            public string Text2 { get; set; } = "test";
        }


        private Any any = null;
        //private ValueXmlSerializer serilalizer;

        [TestInitialize]
        public void Init()
        {
            any = new Any
            {
                Strings = new List<string> { "aaa", "bbb" },
                Text = "abc",
                Anys = new List<Any> {
                    new Any{ Text = "a" },
                    new Any{ Text = "b" }
                }
            };
        }

        [TestMethod]
        public void PropertyExtractorBindingFlagTest()
        {
            var bindingFlags = PropertyExtractor.GetPropertiesBindingFlags(false);
            Assert.AreNotEqual(BindingFlags.Default, bindingFlags);

            bindingFlags = PropertyExtractor.GetPropertiesBindingFlags(true);
            Assert.AreNotEqual(BindingFlags.Default, bindingFlags);
        }

        [TestMethod]
        public void PropertyExtractorTest()
        {
            var properties = PropertyExtractor.GetObjectProperties(typeof(Simple), false).ToList();
            Assert.AreEqual(2, properties.Count);

            properties = PropertyExtractor.GetObjectProperties(typeof(Simple), true).ToList();
            Assert.AreEqual(2, properties.Count);

            properties = PropertyExtractor.GetObjectProperties(typeof(Inherited), false).ToList();
            Assert.AreEqual(3, properties.Count);

            properties = PropertyExtractor.GetObjectProperties(typeof(Inherited), true).ToList();
            Assert.AreEqual(1, properties.Count);
        }

        [TestMethod]
        public void NormalPropertyObjectTest()
        {
            var o = new Simple { Num = 11, Text = "aaa" };
            var xml = new ClassObjectSerializer(o, true).Serialize();
            Console.WriteLine(xml);

            Assert.AreEqual(nameof(Simple), xml.Name.ToString());
            Assert.AreEqual("11", xml.Element("Num").Value);
            Assert.AreEqual("aaa", xml.Element("Text").Value);
        }

    }
}
