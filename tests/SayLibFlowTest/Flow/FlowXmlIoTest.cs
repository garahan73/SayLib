using Microsoft.VisualStudio.TestTools.UnitTesting;
using Say32.Flows;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Say32.Lib.Flows
{
    [TestClass]
    public class FlowXmlIoTest
    {
        [TestMethod]
        public void SerializeXml()
        {
            var flow = Flow.Begin("test")
                            .And("step1", () => { })
                            ;
            Console.WriteLine(flow.ToXElement());
        }
    }


}
