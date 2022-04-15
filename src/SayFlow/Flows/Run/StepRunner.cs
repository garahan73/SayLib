using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Say32.Flows.Run
{
    class FlowStepRunner
    {
        private readonly FlowRun _run;
        public FlowStepRunner(FlowRun run) => _run = run;


        private FlowStepRun StepRun { get => _run.StepRun; set => _run.StepRun = value; }

        FlowContext Context => _run.Context;


        FlowState State { get => _run.State; set => _run.State = value; }
        FlowRunException Exception { get => _run.Exception; set => _run.Exception = value; }

        FlowLogger Logger => _run.Logger;

        internal FlowRunSnapshot Snapshot { get => _run.Snapshot; set => _run.Snapshot = value; }

        internal async Task RunAsync()
        {
            try
            {
                Context.EnterFlow(_run);

                State = FlowState.Running;

                CreateSnapShotIfNeededOrHandleResume();

                // loop: run each step
                while (Snapshot.MoveToNextStep(StepRun) != null)
                {
                    PrepareStepRun();

                    SetStepRunArgument();

                    await RunStep();

                    UpdateContextWithReturnValue();

                    LogStepRunResult();

                    if (ExitLoopIfPaused())
                        break;
                }


                if (State == FlowState.Running)
                {
                    State = FlowState.Success;
                    _run.Flow.ResultHandler?.Invoke(_run);
                }

                if (State == FlowState.Success || State == FlowState.Paused)
                {   
                    _run.Tcs.SetResult(_run);
                }

            }
            catch (Exception ex)
            {
                State = FlowState.Fail;
                Exception = new FlowRunException(_run, ex);

                //var text = $"{ParentCallDepth ?? ""}{_stepRun.CallDepth}: ({_stepRun?.Arg.Value})=>";
                var text = $"({StepRun.Arg.TriedValueDetail})=>";

                Logger.Write(text, _run.Exception);

                if (_run.Flow.ErrorHandler != null)
                {
                    try
                    {
                        _run.Flow.ErrorHandler.Invoke(_run);                        
                    }
                    catch (Exception erhEx)
                    {
                        Exception = new FlowRunException(_run, erhEx);
                    }
                }
                _run.Tcs.SetException(Exception);

                //if (_run.ThrowsException)
                //    throw _run.Exception;
            }
            finally
            {
                Context.ExitFlow(_run);
            }
        }

        private void CreateSnapShotIfNeededOrHandleResume()
        {
            if (Snapshot == null)
            {
                Snapshot = new FlowRunSnapshot(_run.Steps);
            }
            // resume
            if (Snapshot.IsPaused)
            {
                Snapshot.IsPaused = false;
                Logger.Write("RESUME");
            }
        }


        private void LogStepRunResult()
        {
            if (StepRun.FlowControl.Type != FlowControlType.Retry)
            {
                var text = $"({StepRun.Arg.GetValueText()})=>{(StepRun.FlowControl.Type == FlowControlType.Next ? $"({(StepRun.HasReturnValue ? StepRun.ReturnValue : "")})" : StepRun.FlowControl.Text)}";
                Logger.Write(text);
            }
        }

        private void PrepareStepRun()
        {
            if (Snapshot.NewStep != Snapshot.CurrentStep)
            {
                Snapshot.CurrentStep = Snapshot.NewStep;
                StepRun = new FlowStepRun(Snapshot.CurrentStep, _run.Name, Context);
                _run.StepRuns.Add(StepRun);
            }

            // flow control should be reset (in resume case)
            StepRun.FlowControl = null;

            // update context
            Context.CurrentStep = StepRun;
        }

        private void SetStepRunArgument()
        {
            try
            {
                StepRun.Arg.Value = StepRun.Arg.IsContext ? Context : Context.ParamValue;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to set flow step[{_run.StepIndex}] argument. required type:{Snapshot.CurrentStep.ArgType}, actual value={Context.ParamValue?.ToString() ?? "NULL"}", ex);
            }
        }

        private async Task RunStep() => await StepRun.Run();

        private void UpdateContextWithReturnValue()
        {
            if (StepRun.HasReturnValue)
            {
                Context.ParamValue = StepRun.ReturnValue;
            }
        }

        private bool ExitLoopIfPaused()
        {
            if (StepRun.FlowControl.Type == FlowControlType.Pause)
            {
                State = FlowState.Paused;
                Snapshot.IsPaused = true;
                return true;
            }
            return false;
        }

        private string PathName => StepRun.FlowControl.Type == FlowControlType.Path ? $"<{(StepRun.FlowControl as FlowControlPath).PathName}>" : "";



    }

    class FlowRunSnapshot
    {
        public FlowStepIterator StepIterator;
        public FlowStep NewStep;
        public FlowStep CurrentStep;
        public bool IsPaused;

        public FlowRunSnapshot(FlowPaths steps)
        {
            StepIterator = new FlowStepIterator(steps);
            CurrentStep = null;
            IsPaused = false;
        }

        public FlowStep MoveToNextStep(FlowStepRun stepRun)
        {
            return NewStep = NewStep == null ? StepIterator.Begin() : stepRun.FlowControl.GetNextStep(StepIterator);
        }


    }
}
