using Microsoft.VisualStudio.TestTools.UnitTesting;
using Say32.Xml.Serialize.Collection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Say32.Xml
{
    [TestClass]
    public class CollectionItemsXmlSerializerTest
    {
        [TestMethod]
        public void AsPrimitiveAuto()
        {
            var list = CollectionItemsSerializer.Serialize(new string[] { "aaa", "bbb" }, "item", XmlOutputType.Auto, true).Select(o=>(XElement)o).ToList();

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("item", list[0].Name.ToString());
            Assert.AreEqual("aaa", list[0].Value);
            Assert.AreEqual("bbb", list[1].Value);
        }

        [TestMethod]
        public void AsXmlAuto()
        {
            Console.WriteLine("start");
            var list = CollectionItemsSerializer.Serialize(new Any[] { new Any { Text = "aaa" }, new Any { Text = "bbb" } }, "item", XmlOutputType.Auto, true)
                        .Select(o => (XElement)o).ToList();

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("Any", list[0].Name.ToString());
            Console.WriteLine(list[0]);
            Assert.AreEqual("aaa", list[0].Element(nameof(Any.Text)).Value);
            Assert.AreEqual("bbb", list[1].Element(nameof(Any.Text)).Value);
        }

        [TestMethod]
        public void XmlItemsFromInheritedList()
        {
            var list = CollectionItemsSerializer.Serialize(new AnyList { new Any { Text = "aaa" }, new Any { Text = "bbb" } }, "item", XmlOutputType.Auto, true).
                            Select(o => (XElement)o).ToList();

            //var list = value.Elements(nameof(Any)).ToList();

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("Any", list[0].Name.ToString());
            Assert.AreEqual("aaa", list[0].Element(nameof(Any.Text)).Value);
            Assert.AreEqual("bbb", list[1].Element(nameof(Any.Text)).Value);
        }

        [TestMethod]
        public void SimpleCollectionXml()
        {
            var anylist = new AnyList { new Any { Text = "aaa" }, new Any { Text = "bbb" } };
            var collectionXml = CollectionXmlCreator.Serialize(null, anylist, XmlOutputType.Auto, true);                                    

            var list = collectionXml.XmlItems.Select(o=>(XElement)o).ToList();

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("Any", list[0].Name.ToString());
            Assert.AreEqual("aaa", list[0].Element(nameof(Any.Text)).Value);
            Assert.AreEqual("bbb", list[1].Element(nameof(Any.Text)).Value);
        }


        [TestMethod]
        public void DetailedCollectionXml()
        {
            var anyList = new AnyList { new Any { Text = "aaa" }, new Any { Text = "bbb" } };
            var collectionXml = CollectionXmlCreator.Serialize(null, anyList, XmlOutputType.Auto, true);
            Console.WriteLine(collectionXml.Detailed);

            Assert.AreEqual(nameof(AnyList), collectionXml.TypeName);

            var list = collectionXml.XmlItems.Select(o => (XElement)o).ToList();
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("Any", list[0].Name.ToString());
            Assert.AreEqual("aaa", list[0].Element(nameof(Any.Text)).Value);
            Assert.AreEqual("bbb", list[1].Element(nameof(Any.Text)).Value);

            list = collectionXml.Simple.Elements().ToList();
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("Any", list[0].Name.ToString());
            Assert.AreEqual("aaa", list[0].Element(nameof(Any.Text)).Value);
            Assert.AreEqual("bbb", list[1].Element(nameof(Any.Text)).Value);

            list = collectionXml.Detailed.Element(XmlIoTags.ITEMS_TAG).Elements().ToList();
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("Any", list[0].Name.ToString());
            Assert.AreEqual("aaa", list[0].Element(nameof(Any.Text)).Value);
            Assert.AreEqual("bbb", list[1].Element(nameof(Any.Text)).Value);
        }
    }

}
