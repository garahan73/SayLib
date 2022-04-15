using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Say32.Flows
{
    public class FlowLogger
    {
        public bool EnableDebugTrace = true;

        private readonly StringBuilder _sb = new StringBuilder();

        //public FlowLogKey Key { get; private set; }

        //internal FlowLogger Parent { get; set; }

        //public string FlowName => Context.CurrentRun.Name;
        //public string StepName => Context.CurrentStep.Name;
        //public int CallIndex => Context.StepIndex;

        //public string CallDepth => $"( {FlowName ?? "FLOW"} [{CallIndex}]{(StepName == null ? "" : " " + StepName)} )";

        public static event Action<FlowLogKey, string, Exception> NewLogEvent;
        public FlowContext Context { get; private set; }

        public FlowLogger(FlowContext context)
        {
            Context = context;
            //Key = parentLogger?.Key ?? new FlowLogKey(flowName);
        }

        internal string Write(string text, Exception ex = null)
        {
            text = $"{ Context.CallHistory } {text}";

            var full = text + (ex == null ? "" : "\n" + ex);

            _sb.AppendLine(full);
            Debug.WriteLineIf(EnableDebugTrace, full);
            //Debug.WriteLineIf(ex != null, full);

            NewLogEvent?.Invoke(Context.RootRun.LogKey, text, ex);

            return text;
        }

        public override string ToString() => _sb.ToString();
    }

    public class FlowLogKey
    {
        public FlowLogKey(string name)
        {
            Time = DateTime.Now;
            FlowName = name;
        }

        public DateTime Time { get; private set; }
        public string FlowName { get; private set; }

        public override int GetHashCode() => $"{Time.Ticks}{FlowName}".GetHashCode();

        public override bool Equals(object obj) 
            => obj is FlowLogKey id && id.FlowName == FlowName && id.Time == Time;
    }
}
