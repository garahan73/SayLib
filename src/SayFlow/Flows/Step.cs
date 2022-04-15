using Say32.Flows.Run;
using System;
using System.Xml.Serialization;

namespace Say32.Flows
{

    public class FlowStep
    {
        public FlowStep(string name, string flowName)
        {
            Name = name;
            FlowName = flowName;
        }

        [XmlIgnore] public string FlowName { get; private set; }
        [XmlAttribute] public string Name { get; private set; }

        [XmlIgnore] public FlowParameterType ArgType { get; protected set; }
        [XmlIgnore] public FlowParameterType ResultType { get; protected set; }

        [XmlIgnore] public Action<FlowStepRun> Main { get; private set; }

        internal FlowStep SetFunction<R>(Func<R> func)
        {
            ArgType = FlowParameterType.Void;
            ResultType = new FlowParameterType<R>();
            Main = stepRun => stepRun.FunctionReturn = func();

            return this;
        }

        internal FlowStep SetFunction<A, R>(Func<A, R> func)
        {
            ArgType = new FlowParameterType<A>();
            ResultType = new FlowParameterType<R>();
            Main = stepRun => stepRun.FunctionReturn = func((A)GetStepRunArg(stepRun));

            return this;
        }



        internal FlowStep SetAction(Action action)
        {
            ArgType = FlowParameterType.Void;
            ResultType = FlowParameterType.Void;
            Main = stepRun => action();

            return this;
        }

        internal FlowStep SetAction<A>(Action<A> action)
        {
            ArgType = new FlowParameterType<A>();
            ResultType = FlowParameterType.Void;
            Main = stepRun => action((A)GetStepRunArg(stepRun));

            return this;
        }

        internal FlowStep SetSubflow(Flow subflow)
        {
            ArgType = subflow.ArgType;
            ResultType = FlowParameterType.Unknown;
            Main = stepRun =>
            {
                // resume
                if (stepRun.SubflowRun != null)
                {
                    //stepRun.Logger.Add($"Resuming subflow ({stepRun.ArgValueText})=>");
                    stepRun.SubflowRun.ResumeAsync();
                }
                // first execution
                else
                {
                    var ctx = stepRun.Context;
                    stepRun.Logger.Write($"({stepRun.Arg.GetValueText()})=>");
                    stepRun.SubflowRun = subflow.RunAsync(ArgType == FlowParameterType.Void ? null :
                                                        GetStepRunArg(stepRun, ctx), ctx, throwsException: false);
                }

                stepRun.FunctionReturn = stepRun.SubflowRun;

            };

            return this;
        }

        private object GetStepRunArg(FlowStepRun stepRun) => GetStepRunArg(stepRun, stepRun.Context);

        private object GetStepRunArg(FlowStepRun stepRun, FlowContext ctx) => ArgType.IsContext ? ctx : stepRun.Arg.Value;
    }


}
