using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Say32.Flows.Run
{

    public class FlowStepRun
    {
        public FlowContext Context { get; }

        internal FlowStep Step { get; private set; }

        //private readonly Action<FlowStepRun> _main;
        internal FlowLogger Logger => Context.Logger;

        internal FlowStepRun(FlowStep step, string flowName, /* int callIndex, */ FlowContext context)
        {
            //Debug.WriteLine($"[NEW STEP RUN] ({step.ArgType})=>{flowName ?? "STEP"}=>({step.ResultType})");

            Step = step;
            Context = context;

            Arg = step.ArgType.CreateParameter();

            FlowName = flowName;
        }

        public string Name => Step.Name;
        public string FlowName { get; private set; }

        public FlowParameter Arg { get; private set; }

        public FlowControl FlowControl { get; internal set; }

        //public FlowParameterType ResultType { get; private set; }
        public object ReturnValue { get; internal set; }

        public FlowRun SubflowRun { get; internal set; }

        private bool IsAction => Step.ResultType.IsVoid;

        internal object FunctionReturn { get; set; }
        public bool HasReturnValue => !(ReturnValue is Task) && !(ReturnValue is FlowControl) && ReturnValue != Void.Instance;

        public async Task Run()
        {
            Debug.WriteLine($"--> {FlowName}.{Name}[{Context.CurrentStepIndex}]: ({Arg.Value})=>");

            Step.Main(this);

            if (IsAction)
                await HandleActionResult();
            else
                await HandleFunctionResult();

            if (FlowControl == null)
                FlowControl = FlowControl.Next();

            Debug.WriteLine($"<-- {FlowName}.{Name}[{Context.CurrentStepIndex}]: ({Arg.Value})=>({ReturnValue}):{FlowControl.Type}");
        }



        private async Task HandleActionResult()
        {
            FunctionReturn = Void.Instance;

            if (Context.FlowControl != null)
            {
                await HandleFlowControlResult(Context.FlowControl);
                Context.FlowControl = null;
            }
        }

        private async Task HandleFunctionResult()
        {
            await HandleResultValue(FunctionReturn);
        }

        private async Task HandleResultValue(object value)
        {
            if (value is FlowControl flowControl)
            {
                await HandleFlowControlResult(flowControl);
                return;
            }

            switch (value)
            {
                case Task task:
                    await HandleTaskResult(task);
                    break;
                    
                case FlowRun flowRun:
                    await HandleSubflowRunResult(flowRun);
                    break;

                default:
                    ReturnValue = value;
                    break;
            }
        }


        private async Task HandleTaskResult(Task task)
        {
            await task;

            if (!task.HasResult())
            {
                ReturnValue = Void.Instance;
            }
            else
            {
                await HandleResultValue(((dynamic)task).Result);
            }
        }


        private async Task HandleFlowControlResult(FlowControl fc)
        {
            Debug.WriteLine($"- {fc}");
            
            FlowControl = fc;            

            switch (fc)
            {
                case FlowControlPath path:
                    Logger.Write($"PATH = {path.PathName}");
                    break;

                case FlowControlEventWaiter eventWaiter:
                    await eventWaiter.Task;
                    break;

                default:
                    break;
            }

            if (!IsAction)
                await HandleResultValue(fc.ResultValue);
        }

        private async Task HandleSubflowRunResult(FlowRun subflowRun)
        {
            // run subflow
            SubflowRun = subflowRun;
            await subflowRun;

            // handle pause
            if (subflowRun.State == FlowState.Paused)
            {
                await HandleResultValue(FlowControl.PauseBySubFlow());
                return;
            }

            // illegal end
            if (!subflowRun.IsFinished)
            {
                throw new InvalidOperationException($"Subflow should have been finished. subflow state = {subflowRun.State}");
                //return;
            }

            //stepRun.SubflowRun = null;

            // handle failed result
            if (subflowRun.State == FlowState.Fail)
            {
                throw subflowRun.Exception;
                //return;
            }

            // handle flow control PATH result
            if (subflowRun.Last.FlowControl.Type == FlowControlType.Path)
            {
                await HandleResultValue(subflowRun.Last.FlowControl);
                return;
            }

            // handle result value
            if (subflowRun.IsResultValid)
            {
                await HandleResultValue(subflowRun.ResultValue);
            }
        }
    }




}
