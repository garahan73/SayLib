using Microsoft.VisualStudio.TestTools.UnitTesting;
using Say32;    
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Say32
{
    [TestClass]
    public class SayTest1
    {

        [TestMethod]
        public void SimpleTimerTest()
        {
            int i = 0;

            var timer = new BasicTimeout();
            timer.Start(20, () => i = 1);
            Thread.Sleep(50);
            Assert.AreEqual(1, i);

            timer.Start(20, () => i = 2);
            timer.Stop();
            Thread.Sleep(30);
            Assert.AreEqual(1, i);
        }

        [TestMethod]
        public void LazyValueTest1()
        {
            LazyStructValue<int> lz = (Func<int>)(() => 1);
            Assert.AreEqual(1, lz.Value);

            lz = 2;
            Assert.AreEqual(2, lz.Value);
        }

        [TestMethod]
        public void LazyValueTest2()
        {
            LazyValue<object> lz = (Func<object>)(() => 1);
            Assert.AreEqual(1, lz.Value);

            lz = 2;
            Assert.AreEqual(2, lz.Value);
        }

        [TestMethod]
        public void ToBitsTest()
        {
            var bytes = new byte[] { 1 };
            var bits = bytes.ToBits();

            Assert.AreEqual(8, bits.Count);
            Assert.AreEqual<int>(1, bits[7]);

            bytes = new byte[] { 6 };
            bits = bytes.ToBits();

            Assert.AreEqual(8, bits.Count);
            Assert.AreEqual<int>(1, bits[6]);
            Assert.AreEqual<int>(1, bits[5]);

            bytes = new byte[] { 6, 1 };
            bits = bytes.ToBits();

            Assert.AreEqual(16, bits.Count);
            Assert.AreEqual<int>(1, bits[6]);
            Assert.AreEqual<int>(1, bits[5]);
            Assert.AreEqual<int>(1, bits[15]);
        }

        [TestMethod]
        public void FromBitsTest()
        {
            var bits = new List<Bit> { 1, 1, 0, 0 };
            var bytes = bits.ToBytes();

            Console.WriteLine(bytes.ToStringExt());
            Assert.AreEqual(1, bytes.Count);
            Assert.AreEqual<byte>((1 << 7) | (1 << 6), bytes[0]);

            bits = new List<Bit> { 1, 1, 0, 0, 00, 0, 0, 1 };
            bytes = bits.ToBytes();

            Console.WriteLine(bytes.ToStringExt());
            Assert.AreEqual(1, bytes.Count);
            Assert.AreEqual<byte>((1 << 7) | (1 << 6) | 1, bytes[0]);

            bits = new List<Bit> { 1, 1, 0, 0, 00, 0, 0, 1, 1 };
            bytes = bits.ToBytes();

            Assert.AreEqual(2, bytes.Count);
            Assert.AreEqual<byte>((1 << 7) | (1 << 6) | 1, bytes[0]);
            Assert.AreEqual<byte>(1 << 7, bytes[1]);

        }

    }
}
