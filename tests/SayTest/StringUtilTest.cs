using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Say32
{
    [TestClass]
    public class StringUtilTest
    {
        [TestMethod]
        public void SplitTest()
        {
            var r = "'a'b'".Split('\'');
            Console.WriteLine(r.ToStringExt());
            Assert.AreEqual(4, r.Length);

            r = StringUtil.Split("a'b", "\'");
            Console.WriteLine(r.ToStringExt());
            Assert.AreEqual(2, r.Length);
            Assert.AreEqual("a", r[0]);
            Assert.AreEqual("b", r[1]);

            r = StringUtil.Split("'a'b'", "\'");
            Console.WriteLine(r.ToStringExt());
            Assert.AreEqual(4, r.Length);

        }


        [TestMethod]
        public void SplitTest2()
        {
            var r = StringUtil.Split("item1 item2, item3", " ", ",");
            Console.WriteLine(r.ToStringExt());
            Assert.IsTrue(r.ContentsEquals(new string[] { "item1", "item2", "", "item3" }));
        }


        [TestMethod]
        public void StringGetEnclosedTest()
        {
            var r = "'a'b'".GetEnclosed("\'", "\'", out var remaining);
            Assert.AreEqual("a", r);
            Assert.AreEqual("b'", remaining);

            r = " 'abc' ".GetEnclosed("\'", "\'", out remaining);
            Assert.AreEqual("abc", r);
            Assert.AreEqual(" ", remaining);
        }

        [TestMethod]
        public void NameTest()
        {
            Name name = "root.ns1.name";
            Assert.AreEqual("root.ns1", name.NameSpace);
            Assert.AreEqual("name", name.NameOnly);
            Assert.AreEqual("root.ns1.name", name.FullName);
            Assert.AreEqual("root.ns1.name", name.ToString());
            Assert.AreEqual("root.ns1.name", (string)name);

            name = "name1";
            Assert.AreEqual("name1", name.FullName);
        }

        [TestMethod]
        public void NameEqualsTest()
        {
            Name name = "root.ns1.name";
            Assert.IsTrue(Equals(name, "root.ns1.name"));
            //Assert.IsTrue(Equals("root.ns1.name", name));
            Assert.IsTrue(name == "root.ns1.name");
            Assert.IsTrue("root.ns1.name" == name);
        }
    }
}
