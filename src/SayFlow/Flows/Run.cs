using Say32.Flows.Run;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Say32.Flows
{

    public enum FlowState
    {
        Ready, Running, Success, Fail, Paused
    }

    public partial class FlowRun
    {
        internal Flow Flow { get; }
        internal FlowPaths Steps { get; }
        internal List<FlowStepRun> StepRuns { get; } = new List<FlowStepRun>();

        internal FlowRunSnapshot Snapshot { get; set; }

        public FlowContext Context { get; private set; }

        public string Name => Flow.Name;
        public DateTime Time { get; } = DateTime.Now;

        public FlowState State { get; internal set; } = FlowState.Ready;

        public dynamic ResultValue => StepRuns.Last().ReturnValue;

        public FlowRunException Exception { get; internal set; }
        public bool ThrowsException { get; set; } = false;

        public bool IsRunning => State == FlowState.Running;

        public bool IsSuccess => State == FlowState.Success;
        public bool IsFinished => State == FlowState.Success || State == FlowState.Fail;

        public bool IsPaused => State == FlowState.Paused;

        public bool IsResultValid => IsSuccess && !StepRuns.Last().Step.ResultType.IsVoid;

        internal FlowStepRun this[int index] => StepRuns[index];
        internal FlowStepRun Last => StepRuns.Last();

        internal FlowLogger Logger => Context.Logger;

        public string Log => Logger.ToString();

        //public Task Task { get; internal set; }

        public FlowLogKey LogKey { get; }

        public string CallTag => $"[ {Name ?? "FLOW"}({StepIndex}){StepRun.Name.ToString(" - '{0}'")} ]";

        internal TaskCompletionSource<FlowRun> Tcs { get; } = new TaskCompletionSource<FlowRun>();

        public FlowRun(Flow flow, FlowContext context)
        {
            Flow = flow;
            Steps = flow.Steps;

            Context = context ?? new FlowContext(this);

            LogKey = new FlowLogKey(Name);
        }

        public FlowAwaiter GetAwaiter() => new FlowAwaiter(this);

        internal void StartAsync(object arg)
        {
            Context.FirstParameter = arg;
            Context.ParamValue = arg;
            _ = ResumeAsync();
        }

        public Task ResumeAsync()
        {
            return Task.Run(()=>new FlowStepRunner(this).RunAsync());
        }


        public FlowStepRun StepRun { get; internal set; }
        public int StepIndex => StepRuns.Count - 1;


        public FlowRun Wait(Timeout timeout = default) => WaitAsync(timeout).WaitAndGetResult();

        public async Task<FlowRun> WaitAsync(Timeout timeout = default)
        {
            return await Tcs.Task.WaitAsync(timeout);
        }

    }



}
