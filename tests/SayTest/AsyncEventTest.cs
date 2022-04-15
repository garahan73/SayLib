using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Say32
{
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class AsyncEventTest
    {

        [TestMethod]
        public void EventTest1()
        {
            var e = new AsyncEvent();

            var task = e.WaitAsync(1000);

            e.Set(1);
            Assert.AreEqual(1, task.WaitAndGetResult());
        }

        [TestMethod]
        public void EventTest2()
        {
            var e = new AsyncEvent();

            e.Set(1);

            Assert.ThrowsException<TimeoutException>(() => e.Wait(1));
         
        }

        [TestMethod]
        public void EventTest3()
        {
            var e = new AsyncEvent();

            e.Set(1);            
            Thread.Sleep(250);

            var task = e.WaitAsync(1000);

            e.Set(2);
            Assert.AreEqual(2, task.WaitAndGetResult());

            task = e.WaitAsync(1000);

            e.Set(3);
            Assert.AreEqual(3, task.WaitAndGetResult());
        }

        [TestMethod]
        public void MultipleClientsWait()
        {
            var e = new AsyncEvent();

            var task1 = e.WaitAsync(1000);
            var task2 = e.WaitAsync(1000);

            e.Set(1);
            Assert.AreEqual(1, task1.WaitAndGetResult());
            Assert.AreEqual(1, task2.WaitAndGetResult());
        }


        [TestMethod]
        public void KeyEventTest1()
        {
            var e = new AsyncKeyEvents();

            var task1 = e.WaitAsync("a", 1000);
            var task2 = e.WaitAsync("b", 1000);

            e.Set("a", 1);
            e.Set("b", 2);

            Assert.AreEqual(1, task1.WaitAndGetResult());
            Assert.AreEqual(2, task2.WaitAndGetResult());
        }

    }
}
