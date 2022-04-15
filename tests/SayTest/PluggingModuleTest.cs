using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Say32.PluggingModule;

namespace Say32
{
    [TestClass]
    public class PluggingTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var obj = new TestObject();
            
            obj.Modules.Plug(new Logger());
            obj.Modules.Plug(new Calculator());

            obj.Modules.GetModule<Calculator>().Add(1, 2);
        }
    }

    internal class TestObject : IModuleTarget
    {
        public TestObject()
        {
        }

        public IModuleSocket<TestObject> SocketA { get; } = new Socket<TestObject>();

        public ModuleSet Modules { get; } = new ModuleSet();
    }

    internal class Calculator : IPluggableModule<Calculator>
    {
        public ModuleSet Modules { get; set; }

        public Calculator()
        {
        }

        public int Add(int a, int b)
        {
            var result = a + b;
            var log = $"{a} + {b} = {a + b}";

            var logger = Modules.GetModule<Logger>();
            logger?.WriteLog(log);

            return result;
        }
    }

    internal class Logger : IPluggableModule<Logger>
    {
        public ModuleSet Modules { get; set; }

        public Logger()
        {
        }

        internal void WriteLog(string log)
        {
            Console.WriteLine(log);
        }
    }
}
