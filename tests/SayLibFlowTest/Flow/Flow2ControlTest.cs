using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Say32.Flows;

namespace Say32.Lib.Flows
{
    [TestClass]
    public class Flow2ControlTest
    {
        [TestMethod]
        public async Task NextAndRetry()
        {
            var r = await Flow.Begin()
                        .And((int i) => i > 10 ? FlowControl.Next(i) : FlowControl.Retry(i + 1), "loop")
                        .And((int i)=>i+10, "next")
                        .Run(0);
            Assert.AreEqual(21, r);
        }

        [TestMethod]
        public void PathTest()
        {
            var flow = Flow.Begin()
                .And((int i) => i > 0 ? FlowControl.Path("patha", "aaa") : i < 0 ? FlowControl.Path("pathb", i) : FlowControl.Next(100))
                .Path("patha", (string s) => s+"bbb")
                .Path("pathb", (int i) => i + 1);

            Assert.AreEqual("aaabbb", flow.Run(1).ResultValue);
            Assert.AreEqual(0, flow.Run(-1).ResultValue);
            Assert.AreEqual(100, flow.Run(0).ResultValue);

            Console.WriteLine(flow.Run(1).Log);
            Console.WriteLine(flow.Run(-1).Log);
            Console.WriteLine(flow.Run(0).Log);
        }

        [TestMethod]
        public void PathAsSubflowTest()
        {
            var flow = Flow.Begin()
                .And((int i) => i > 0 ? FlowControl.Path("patha") : i < 0 ? FlowControl.Path("pathb", i) : FlowControl.Next(100))
                .Path("patha", () => "aaa")
                .Path("pathb", Flow.Begin().And((int i)=>i*10).And(i=>i+1));

            Assert.AreEqual("aaa", flow.Run(1).ResultValue);
            Assert.AreEqual(-9, flow.Run(-1).ResultValue);
            Assert.AreEqual(100, flow.Run(0).ResultValue);
        }

        [TestMethod]
        public void PathFromSubflowTest()
        {
            var flow = Flow.Begin()
                .And(Flow.Begin()
                        .And((int i) => i > 0 ? FlowControl.Path("patha") : 
                                i < 0 ? FlowControl.Path("pathb", i) : 
                                FlowControl.Next(100))
                        .And( (int i) => i + 1)
                )
                .Path("patha", () => "aaa")
                .Path("pathb", Flow.Begin().And((int i) => i * 10).And(i => i + 1));

            Assert.AreEqual("aaa", flow.Run(1).ResultValue);
            Assert.AreEqual(-9, flow.Run(-1).ResultValue);
            Assert.AreEqual(101, flow.Run(0).ResultValue);
        }

        Action _evt0 = null;

        [TestMethod]
        public async Task Event_WithoutArgument()
        {
            var flow = Flow.Begin()
                .And(() =>FlowControl.WaitEvent(() => ref _evt0))
                .And(()=>10);

            var run = flow.RunAsync();
            Assert.IsTrue(run.IsRunning);

            // invoke event
            _evt0();
            Assert.AreEqual(10, await run);

            // event link cleared
            Assert.IsNull(_evt0);
        }


        Action<int> _evt1 = null;

        [TestMethod]
        public async Task Event_WithArgument()
        {
            var flow = Flow.Begin()
                .And(() =>FlowControl.WaitEvent(() => ref _evt1))
                .And((int i) => i + 10);

            var run = flow.RunAsync();
            Assert.IsTrue(run.IsRunning);

            Assert.IsNotNull(_evt1);
            _evt1(1);
            Assert.AreEqual(11, await run);

            Assert.IsNull(_evt1);
        }

        Action<object, int> _evt2 = null;

        [TestMethod]
        public async Task Event_MergingMultipleArgumentsIntoOne()
        {
            var flow = Flow.Begin()
                .And(() => FlowControl.WaitEvent(() => ref _evt2, (sender, arg)=>arg))
                .And((int i) => i + 10);

            var run = flow.RunAsync();
            Assert.IsTrue(run.IsRunning);

            Assert.IsNotNull(_evt2);

            // invoke event
            _evt2(this, 1);
            Assert.AreEqual(11, await run);

            Assert.IsNull(_evt2);
        }


        [TestMethod]
        public void PauseTest1()
        {
            var flow = Flow.Begin().And(() => FlowControl.Pause()).And(() => 10);
            var run = flow.Run();
            Assert.IsFalse(run.IsFinished);
            Assert.IsFalse(run.IsRunning);
            Assert.AreEqual(FlowState.Paused, run.State);

            Console.WriteLine(run.ResultValue);

            run.ResumeAsync().Wait(); ;
            Assert.IsTrue(run.IsFinished);
            Assert.IsFalse(run.IsRunning);
            Assert.AreEqual(FlowState.Success, run.State);
            Assert.AreEqual(10, run.ResultValue);
        }



        [TestMethod]
        public void SubflowPauseTest()
        {

            var flow = Flow.Begin("MAIN")
                        .And(() => 1)
                        .And(Flow.Begin("SUB").And((int i) => i + 1).And(() => FlowControl.Pause()).And((int i) => i + 10))
                        .And((int i) => i + 100);

            var run = flow.Run();
            Console.WriteLine(run.Log);

            Assert.AreEqual(FlowState.Paused, run.State);
            Console.WriteLine(run.ResultValue);
            
            run.ResumeAsync().Wait();
            Assert.IsTrue(run.IsFinished);
            Assert.IsFalse(run.IsRunning);
            Assert.AreEqual(FlowState.Success, run.State);
            Assert.AreEqual(112, run.ResultValue);

            Console.WriteLine(run.Log);

        }

        [TestMethod]
        public void SubflowErrorPropagation()
        {
            var i = 0;
            var flow = Flow.Begin().And(() => i=1)
                            .And(Flow.Begin("SUB").And(() => throw new Exception("test")))
                            .And(() => i= 2);

            var run = flow.Run(throwsException: false);
            
            Console.WriteLine($"i={i}");
            Assert.AreEqual(1, i);

            Assert.IsTrue(run.IsFinished);
            Assert.IsFalse(run.IsSuccess);
            Assert.IsFalse(run.IsResultValid);

            Assert.IsNotNull(run.Exception);
            Console.WriteLine(run.Exception);

            Assert.IsTrue(run.Exception.InnerException is FlowRunException);
            Assert.AreEqual("test", run.Exception.GetRoot().Message);
        }

    }
}
