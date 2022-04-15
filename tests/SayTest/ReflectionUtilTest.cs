using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Say32
{
    [TestClass]
    public class ReflectionUtilTest
    {
        [TestMethod]
        public void IsSingleGenericInterfaceTypeTest()
        {
            var list = new List<int> { 1, 2, 3 };
            Assert.IsTrue(typeof(IList).IsAssignableFrom(list.GetType()));
            Assert.IsTrue(ReflectionUtil.IsSingleGenericInterfaceType(list.GetType(), typeof(IList<>)));
        }

        [TestMethod]
        public void GetSingleGenericInterfaceArgumentTest()
        {
            var list = new List<int> { 1, 2, 3 };
            Assert.AreEqual(typeof(int), list.GetType().GetGenericArguments()[0]);
            Assert.AreEqual(typeof(int), ReflectionUtil.GetSingleGenericInterfaceArgument(list.GetType(), typeof(IList<>)));
        }

        [TestMethod]
        public void GetBaseClassTest()
        {
            Assert.AreEqual(1, typeof(ListA).GetBaseClassesAndInterfaces(true, t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>)).Count());
        }

        class ListA : List<string>
        {
        }
    }
}
