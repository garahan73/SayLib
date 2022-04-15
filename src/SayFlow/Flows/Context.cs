using Say32;
using Say32.Flows.Run;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Say32.Flows
{
    public class FlowContext
    {
        public FlowContext(FlowRun flowRun)
        {
            RootRun = flowRun;
            Logger = new FlowLogger(this);
        }

        private Stack<FlowRun> _flowStack = new Stack<FlowRun>();
        private FlowStepRun _currentStep;
        private FlowCallHistory _callHistory = new FlowCallHistory();

        public FlowRun RootRun { get; }

        public dynamic FirstParameter { get; internal set; }
        public dynamic ParamValue { get; internal set; }

        public FlowControl FlowControl { get; set; }

        public object Data { get; set; }

        public FlowLogger Logger { get; }

        public FlowRun CurrentFlow
        {
            get
            {
                lock (_flowStack)
                {
                    return _flowStack.Peek();
                }
            }
        }

        public FlowStepRun CurrentStep
        {
            get => _currentStep;
            internal set
            {
                _currentStep = value;

                if (_currentStep != null)
                {
                    var flow = CurrentFlow;
                    _callHistory.AddStep(new FlowStepCallLog(value));
                }
            }
        }

        public int CurrentStepIndex => CurrentFlow.StepRuns.Count - 1;

        public string CallHistory
        {
            get
            {
                var sb = new StringBuilder();

                foreach (var flow in _flowStack.Reverse())
                {
                    sb.Append($" {flow.CallTag} ");
                }

                return sb.ToString();
            }
        }

        

        internal void EnterFlow(FlowRun flowRun)
        {
            lock (_flowStack)
            {
                if (_flowStack.Count() == 0 || _flowStack.Peek() != flowRun)
                {
                    _flowStack.Push(flowRun);
                    CurrentStep = null;
                    //UpdateFlowHierachy();
                }
            }
        }

        internal void ExitFlow(FlowRun flowRun)
        {
            lock (_flowStack)
            {
                if (_flowStack.Count() != 0 && _flowStack.Peek() == flowRun)
                {
                    _flowStack.Pop();
                    CurrentStep = null;
                    //UpdateFlowHierachy();
                }
            }
        }

        /*
        private void UpdateFlowHierachy()
        {
            if (_currentStep == null) return;

            var sb = new StringBuilder();

            foreach (var flow in _flowStack.Reverse())
            {
                sb.Append($"( {flow.Name ?? "FLOW"} [{flow.StepIndex}] {(_currentStep.Name ?? "")} ) ");
            }

            FlowCallHistory = sb.ToString();
            Debug.WriteLine(FlowCallHistory);
        }
        */

        public override string ToString() => nameof(FlowContext);
    }
}
