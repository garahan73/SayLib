using Say32.Flows.Run;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Say32.Flows
{
    public class FlowCallHistory
    {
        internal void AddStep(FlowStepCallLog flowStepCallLog)
        {

        }
    }

    public class FlowStepCallLog
    {
        //public string Arg => _stepRun.Arg.GetValueText();
        //public string Result => _stepRun.HasResultValue ? _stepRun.ResultValue?.ToString() : "";

        public string Body { get; }

        public FlowStepCallLog(FlowStepRun stepRun)
        {
            var flowRun = stepRun.Context.CurrentFlow;
            Body = $"{flowRun.Name ?? "FLOW"} [{flowRun.StepIndex}] {stepRun.Name ?? ""}";
        }
    }
}
