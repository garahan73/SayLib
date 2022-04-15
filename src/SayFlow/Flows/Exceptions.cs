using Say32.Flows.Run;
using System;

namespace Say32.Flows
{

    [Serializable]
    public class FlowException : Exception
    {
        public Flow Flow { get; private set; }

        public FlowException(Flow flow) { Flow = flow; }
        public FlowException(Flow flow, string message) : base(message) { Flow = flow; }
        public FlowException(Flow flow, string message, Exception inner) : base(message, inner) { Flow = flow; }
        protected FlowException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class FlowRunException : Exception
    {
        public FlowRun FlowRun { get; private set; }

        public int StepIndex => FlowRun.StepIndex;
        public FlowStepRun Step => FlowRun.StepRun;

        private string Header => $"{FlowRun.Name ?? "FLOW"}[{StepIndex}]{Step.Name}";

        public FlowRunException(FlowRun flowrun, Exception ex) : base(null, ex)
        {
            FlowRun = flowrun;
        }

        protected FlowRunException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public override string Message => $"{Header} ({Step.Arg.TriedValueDetail})=>";

    }

    [Serializable]
    public class FlowStepRunException : Exception
    {
        public FlowStepRun StepRun { get; private set; }

        public FlowStepRunException(FlowStepRun run) { StepRun = run; }
        public FlowStepRunException(FlowStepRun run, string message) : base(message) { StepRun = run; }
        public FlowStepRunException(FlowStepRun run, string message, Exception inner) : base(message, inner) { StepRun = run; }
        protected FlowStepRunException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class FlowParameterException : Exception
    {
        public FlowParameter Parameter { get; private set; }

        public FlowParameterException(FlowParameter parameter) { Parameter = parameter; }
        public FlowParameterException(FlowParameter parameter, string message) : base(message) { Parameter = parameter; }
        public FlowParameterException(FlowParameter parameter, string message, Exception inner) : base(message, inner) { Parameter = parameter; }
        protected FlowParameterException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
