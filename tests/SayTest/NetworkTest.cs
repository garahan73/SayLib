using Microsoft.VisualStudio.TestTools.UnitTesting;
using Say32.Network;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Say32
{
    [Ignore]
    [TestClass]
    public class NetworkTest
    {
        [TestMethod]
        public async Task ConnectionTestAsync()
        {
            Socket active, passive = null;

            _ = Task.Run(async () => { passive = await SocketUtil.ConnectAsync(false, "127.0.0.1", 6000, 1); });

            using (active = await SocketUtil.ConnectAsync(true, "127.0.0.1", 6000))
            {
                using (passive)
                {
                    Thread.Sleep(50);

                    Assert.IsNotNull(active);
                    Assert.IsNotNull(passive);

                    var buffer = new byte[1024];
                    Assert.AreEqual(4, await active.SendStringAsync("test", Encoding.ASCII));

                    var message = await passive.ReceiveStringAsync(buffer, Encoding.ASCII);
                    Assert.AreEqual("test", message);
                }
            }
        }

        [TestMethod]
        public async Task NamedPipeTestAsync()
        {
            var factory = new NamedPipeFactory("test", 2);

            var serverTask1 = factory.GetConnectedServerPipeAsync(1000);
            var serverTask2 = factory.GetConnectedServerPipeAsync(1000);

            using var client1 = await factory.GetConnectedClientPipeAsync(1000);
            using var client2 = await factory.GetConnectedClientPipeAsync(1000);

            using var server1 = await serverTask1;
            using var server2 = await serverTask2;

            // client -> server
            var task1 = server1.ReadLineAsync();
            var task2 = server2.ReadLineAsync();

            await client1.WriteLineAsync("client1");
            await client2.WriteLineAsync("client2");

            Assert.AreEqual("client1", task1.Result);
            Assert.AreEqual("client2", task2.Result);

            // sever -> client
            task1 = client1.ReadLineAsync();
            task2 = client2.ReadLineAsync();

            await server1.WriteLineAsync("server1");
            await server2.WriteLineAsync("server2");

            Assert.AreEqual("server1", task1.Result);
            Assert.AreEqual("server2", task2.Result);

        }
    }
}
