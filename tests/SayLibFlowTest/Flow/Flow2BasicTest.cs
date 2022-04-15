using Microsoft.VisualStudio.TestTools.UnitTesting;
using Say32.Flows;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Say32.Lib.Flows
{
    [TestClass]
    public class Flow2Test
    {
        [TestMethod]
        public void FlowBasic()
        {
            var flow = Flow.Begin()
                        .And(() => 1)
                        .And(i => i + 10);
            var run = flow.Run();

            Console.WriteLine(run.Log);

            Assert.IsTrue(run.IsSuccess);
            Assert.AreEqual(11, run.ResultValue);
        }

        [TestMethod]
        public void FlowStartWithArg()
        {
            var flow = Flow.Begin().And((int i) => i + 1).And(i => i + 10);
            var run = flow.Run(1);
            Assert.IsTrue(run.IsSuccess);
            Assert.AreEqual(12, run.ResultValue);
        }

        [TestMethod]
        public void FlowArgTypeChange()
        {
            var flow = Flow.Begin().And((int i) => i + 1).And(() => { }).And(i => i + 10);
            var run = flow.Run(1);
            Assert.IsTrue(run.IsSuccess);
            Assert.AreEqual(12, run.ResultValue);
        }

        [TestMethod]
        public async Task FlowAwait()
        {
            var flow = Flow.Begin().And(() => 1).And(i => i + 10);
            var run = flow.Run();
            await run;
            Assert.IsTrue(run.IsSuccess);
            Assert.AreEqual(11, run.ResultValue);
        }

        [TestMethod]
        public void SubflowTest()
        {
            var flow = Flow.Begin("MAIN").And((int i) => i + 1)
                .And(Flow.Begin("SUB").And((int i) => i * 10).And(() => Thread.Sleep(100)).And(i => i + 1))
                .And((int i) => i + 10);
            var run = flow.Run(1).Wait();
            Assert.IsTrue(run.IsSuccess);
            Assert.AreEqual(31, run.ResultValue);

            Console.WriteLine(run.Log);
        }

        [TestMethod]
        public void SubflowTest2()
        {
            var flow = Flow.Begin("MAIN").And((int i) => i + 1)
                .And((FlowContext ctx) =>
                {
                    var sub = Flow.Begin("SUB").And((int i) => i * 10).And(() => Thread.Sleep(100)).And(i => i + 1);
                    return sub.Run(ctx.ParamValue, ctx).ResultValue;
                })
                .And((int i) => i + 10);

            var run = flow.Run(1).Wait();
            Assert.IsTrue(run.IsSuccess);
            Assert.AreEqual(31, run.ResultValue);

            Console.WriteLine(run.Log);
        }

        [TestMethod]
        public void SubflowTest2_1()
        {
            var flow = Flow.Begin("MAIN")
                .And((FlowContext ctx) =>
                {
                    var sub = Flow.Begin("SUB").And((int i) => i + 1);
                    return sub.Run(ctx.ParamValue, ctx).ResultValue;
                });


            var run = flow.Run(1).Wait();
            Assert.IsTrue(run.IsSuccess);
            Assert.AreEqual(2, run.ResultValue);

            Console.WriteLine(run.Log);
        }




        [TestMethod]
        public void AsyncStep()
        {
            Assert.IsTrue(typeof(Task).IsAssignableFrom(typeof(Task<int>)));

            var flow = Flow.Begin().And((int i) => i + 1)
                .And(async i => await Task.Run(() => { Thread.Sleep(100); return i * 10; }))
                .And((int i) => i + 10);
            var run = flow.Run(1).Wait();
            Assert.IsTrue(run.IsSuccess);
            Assert.AreEqual(30, run.ResultValue);
        }



        [TestMethod]
        public void NameTest()
        {
            var flow = Flow.Begin("flow1").And((int i) => i + 1);
            Assert.AreEqual("flow1", flow.Name);

            var run = flow.Run(1);
            Assert.AreEqual("flow1", run.Name);
        }

        [TestMethod]
        public void StepNameTest()
        {
            var flow = Flow.Begin().And((int i) => i + 1, "step1");

            var run = flow.Run(1);
            //Assert.AreEqual("step1", run[0].Name);
        }

        [TestMethod]
        public void CallHistoryTest()
        {
            var run = Flow.Begin("test flow")
                           .And((int i) => i + 1, "first")
                           .And(i => i + 1, "second")
                           .And(i => i + 1, "third")
                           .Run(0);

        }

        [TestMethod]
        public void CallHistoryWithSubflow()
        {
            var run = Flow.Begin("MAIN")
                           .And((int i) => i + 1, "first")
                           .And(i => i + 1, "second")
                           .And(Flow.Begin("SUB")
                                .And((int i) => i + 1)
                                .And(i => i + 1)
                                .And(i => i + 1)
                                )
                            .And((int i) => i + 1, "third")
                           .Run(0);

        }

        [TestMethod]
        public void OnErrorTest()
        {
            //const string EXCEPTION_MESSAGE = "(FLOW[0]) test";

            var i = 0;
            
            var flow = Flow.Begin()
                            .And(() => throw new Exception("test"))
                            .OnError(r =>
                            {
                                i = 1;
                                                                
                                Assert.AreEqual("test", r.Exception.GetRoot().Message);
                            });

            FlowRun run = null;
            Exception ex = null;

            var errorHandled = false;
            try
            {
                run = flow.Run();
            }
            catch (AggregateException ae)
            {
                Assert.IsNull(run);

                Flow.HandleAggregateException(ae, (r, e) =>
                { 
                    errorHandled = true;
                    run = r;
                    ex = e;
                });
            }

            Assert.IsTrue(errorHandled);

            Assert.IsFalse(run.IsSuccess);

            Assert.AreEqual(0, run.Exception.StepIndex);
            Assert.AreEqual("test", run.Exception.GetRoot().Message);
            Assert.AreEqual("test", run.Exception.GetRoot().Message);
            Assert.AreEqual("test", ex.Message);

            Assert.AreEqual(1, i);
        }

        [TestMethod]
        public void OnError_DoNotThrowsExceptionTest()
        {
            //const string EXCEPTION_MESSAGE = "(FLOW[0]) test";

            var flow = Flow.Begin()
                            .And(() => throw new Exception("test"))
                            .OnError(r =>
                            {
                                r.ThrowsException = false;
                            });

            var run = flow.Run();

            Assert.IsFalse(run.IsSuccess);
            Assert.AreEqual("test", run.Exception.GetRoot().Message);
        }

        [TestMethod]
        public void OnError_DoNotThrowsExceptionTest2()
        {
            //const string EXCEPTION_MESSAGE = "(FLOW[0]) test";

            var flow = Flow.Begin()
                            .And(() => throw new Exception("test"));

            var run = flow.Run(throwsException: false);

            Assert.IsFalse(run.IsSuccess);
            //Assert.AreEqual(EXCEPTION_MESSAGE, run.Exception.Message);
            Assert.AreEqual("test", run.Exception.GetRoot().Message);
        }

        [TestMethod]
        public void OnResultTest()
        {
            var num = 1;

            var flow = Flow.Begin()
                            .And((int i) => i + 1)
                            .OnResult((flowRun) =>
                            {
                                num = 2;
                            });

            var run = flow.Run(num);
            Assert.AreEqual(2, run.ResultValue);
            Assert.AreEqual(2, num);
        }


        [TestMethod]
        public async Task AsyncStep_ReturnsVoid()
        {
            Assert.IsTrue(typeof(Task).IsAssignableFrom(typeof(Task<int>)));

            var flow = Flow.Begin().And((int i) => i + 1)
                .And(async i => await Task.Run(() => Thread.Sleep(100)))
                .And((int i) => i + 10);

            var run = flow.Run(1);
            await run;

            Assert.IsTrue(run.IsSuccess);
            Assert.AreEqual(12, run.ResultValue);
        }


        [TestMethod]
        public void ArgTypeMismatchTest()
        {
            var flow = Flow.Begin()
                            .And((int i) => i + 1)
                            .OnError(r => { r.ThrowsException = false; });

            var run = flow.Run();

            Assert.IsFalse(run.IsSuccess);

            Assert.IsTrue(run.Exception.InnerException.InnerException is FlowParameterException fpe);
        }

        [TestMethod]
        public void FlowContextTest()
        {
            var flow = Flow.Begin().And(() => 1)
                            .And((FlowContext ctx) => ctx.ParamValue + 1);

            var run = flow.Run();
            Assert.AreEqual(2, run.ResultValue);
            Assert.AreEqual(2, run.Context.ParamValue);
        }
    }


}
