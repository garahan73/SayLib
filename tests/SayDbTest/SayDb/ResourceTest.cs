using System;
using System.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Say32.DB.Core.Exceptions;

namespace Say32.DB
{
    [TestClass]
    public class ResourceTest
    {
        [TestMethod]
        public void TestMethod1()
        {

            var rm = new ResourceManager(typeof(ExceptionMessageFormat));
            Console.WriteLine(rm.GetString("SterlingActivationException"));

            Console.WriteLine(ExceptionMessageFormat.SayDBActivationException);
        }
    }
}
