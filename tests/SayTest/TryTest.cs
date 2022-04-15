using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Say32
{
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class TryTest
    {
        [TestMethod]
        public void TryActionSuccess()
        {
            var t = Try.Run(() => Console.WriteLine("test"));

            Assert.AreEqual(true, t.Success);
        }

        [TestMethod]
        public void TryActionFail()
        {
            var t = Try.Run(() => throw new Exception());

            Assert.AreEqual(false, t.Success);
        }

        [TestMethod]
        public async Task TryAsyncAction()
        {
            var t = await Try.RunAsync(() => Task.Run(() => Console.WriteLine("test")));

            Assert.AreEqual(true, t.Success);
        }

        [TestMethod]
        public async Task TryAsyncActionFail()
        {
            var t = await Try.RunAsync(() => Task.Run(() => throw new Exception()));

            Assert.AreEqual(false, t.Success);
        }

        [TestMethod]
        public async Task TryAsyncFunction()
        {
            var t = await Try.RunAsync(() => Task.Run(() => 1));

            Assert.AreEqual(true, t.Success);
            Assert.AreEqual(1, t.Result);
        }

        [TestMethod]
        public async Task TryAsyncFunctionFail()
        {
            var t = await Try.RunAsync(() => Task.Run(tempAsync));

            Assert.AreEqual(false, t.Success);

            int tempAsync() => throw new Exception();
        }
    }
}
