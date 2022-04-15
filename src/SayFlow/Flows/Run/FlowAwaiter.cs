using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Say32.Flows.Run
{
    public class FlowAwaiter : ICriticalNotifyCompletion
    {

        public FlowAwaiter(FlowRun flowRun)
        {
            _flowrun = flowRun;
        }

        private readonly FlowRun _flowrun;

        public bool IsCompleted => _flowrun.IsFinished;

        public object GetResult()
        {
            if (!_flowrun.IsFinished)
            {
                Thread.Yield();
                _flowrun.Wait();
            }
            return _flowrun.ResultValue;
        }

        public void OnCompleted(Action continuation)
        {
            continuation();
        }

        public void UnsafeOnCompleted(Action continuation) => continuation();
    }



}
