using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Say32
{
    [TestClass]
    public class TimeoutTest
    {


        [TestMethod]
        public void BasicTimeoutTest()
        {
            Timeout t = TimeSpan.FromMilliseconds(10);
            var run = t.Start();

            Assert.IsTrue(Task.Run(() => 1).WaitSafe(run));
            Assert.IsTrue((Task.Run(() => System.Threading.Thread.Sleep(run.RemainingTime - TimeSpan.FromMilliseconds(2))).WaitSafe(run)));
            Assert.IsFalse((Task.Run(() => System.Threading.Thread.Sleep(3)).WaitSafe(run)));
        }

        [TestMethod]
        public void DisabledTimeoutTest()
        {
            Timeout t = default;
            var run = t.Start();

            Assert.IsTrue(run.Wait(Task.Run(() => 1)));
            Assert.IsTrue(run.Wait(Task.Run(() => System.Threading.Thread.Sleep(5))));
        }

        [TestMethod]
        public void DefaultTimeoutTest()
        {
            Assert.AreNotEqual(default(TimeSpan), TimeSpan.MinValue);

            var t = new Timeout();
            Assert.IsFalse(t.IsEnabled);
            Assert.AreEqual(TimeSpan.FromMilliseconds(-1), t.Value);

            t = default;
            Assert.IsFalse(t.IsEnabled);
            Assert.AreEqual(TimeSpan.FromMilliseconds(-1), t.Value);

            Assert.IsFalse(t.Start().IsTimeout);
        }

    }
}
