using Microsoft.VisualStudio.TestTools.UnitTesting;
using Say32;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Say32
{
    [TestClass]
    public class DelayedTest
    {
        [TestMethod]
        public void DelayedString()
        {
            LazyValue<string> delayed = "test";
            Assert.AreEqual<string>("test", delayed);

            Func<string> ftn = ()=> "test2";
            delayed = ftn;
            Assert.AreEqual<string>("test2", delayed);

            delayed = _f(() => "test3");
            Assert.AreEqual<string>("test3", delayed);

            Func<string> _f( Func<string> expr ) => expr;
        }


        
    }
}

