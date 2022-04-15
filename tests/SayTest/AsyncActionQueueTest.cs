using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Say32
{
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class AsyncActionQueueTest
    {
        [TestMethod]
        public void AddPerformanceTest()
        {
            const int count = 1000;

            AsyncFunctionQueue fq = new AsyncFunctionQueue();

            var x = 0;

            var lap = new Timelap();

            for (int i = 0; i < count; i++)
            {
                var num = i;
                fq.Add(() =>
                {
                    return num;
                });
            }

            Console.WriteLine($"asyc functions add lap {lap}");

            fq.Wait();

            var aq = new AsyncActionQueue();

            lap = new Timelap();

            for (int i = 0; i < count; i++)
            {
                var num = i;
                aq.Add(() =>
                {
                    x = num;
                });
            }

            Console.WriteLine($"async actions add lap {lap}");

            fq.Wait();

            fq = new AsyncFunctionQueue();

            lap = new Timelap();

            for (int i = 0; i < count; i++)
            {
                var num = i;
                fq.Add(() =>
                {
                    return num;
                });
            }

            Console.WriteLine($"async functions add lap2 {lap}");

            var evt = new AsyncEvent();
            evt.AddEventHandler(o => Thread.Sleep(1));
            lap = new Timelap();

            for (int i = 0; i < count; i++)
            {
                evt.Set(i);
            }

            Console.WriteLine($"event set lap {lap}");

            

            var keyEvent = new KeyEvents<object>();
            keyEvent.AddEventHandler("key1", o => Thread.Sleep(1));

            lap = new Timelap();

            for (int i = 0; i < count; i++)
            {
                var num = i;
                keyEvent.Set("key1", i);
            }

            Console.WriteLine($"key event set lap {lap}");

        }

        [TestMethod]
        public async Task RunPerformanceTestAsync()
        {
            const int count = 1000;

            AsyncFunctionQueue fq = new AsyncFunctionQueue();

            var x = 0;

            var lap = new Timelap();

            for (int i = 0; i < count; i++)
            {
                var num = i;
                await fq.RunAsync(() =>
                {
                    return num;
                });
            }

            Console.WriteLine($"asyc functions add lap {lap}");

            var aq = new AsyncActionQueue();

            lap = new Timelap();

            for (int i = 0; i < count; i++)
            {
                var num = i;
                await aq.RunAsync(() =>
                {
                    x = num;
                });
            }

            Console.WriteLine($"async actions add lap {lap}");

            fq = new AsyncFunctionQueue();

            lap = new Timelap();

            for (int i = 0; i < count; i++)
            {
                var num = i;
                await fq.RunAsync(() =>
                {
                    return num;
                });
            }

            Console.WriteLine($"async functions add lap2 {lap}");

        }

        [TestMethod]
        public async Task BasicTestAsync()
        {
            var q = new AsyncFunctionQueue();

            var list = new List<int>();

            var task1 = q.RunAsync(() => { list.Add(1); return list.Last(); });
            var task2 = q.RunAsync(() => { list.Add(2); return list.Last(); });

            Assert.AreEqual(1, await task1);
            Assert.AreEqual(1, list[0]);

            Assert.AreEqual(2, await task2);
            Assert.AreEqual(2, list[1]);

            Assert.AreEqual(2, list.Count);

        }

        [TestMethod]
        public async Task ErrorDeliveryTestAsync()
        {
            var q = new AsyncFunctionQueue();

            try
            {
                await q.RunAsync(() => throw new Exception("test"));
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Assert.AreEqual("test", ex.Message);
                return;
            }
            Assert.Fail();
        }
    }
}
