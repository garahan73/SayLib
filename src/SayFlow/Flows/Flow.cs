using Say32.Xml;
using System;
using System.Threading.Tasks;

namespace Say32.Flows
{
    [XmlCustomSerializer(typeof(FlowXml), nameof(FlowXml.Serialize))]
    public partial class Flow
    {
        public string Name { get; set; }

        public FlowPaths Steps { get; private set; } = new FlowPaths();

        internal FlowParameterType _lastValidParamType;


        public FlowParameterType ArgType => Steps.First().ArgType;

        public Action<FlowRun> ErrorHandler { get; private set; }
        public Action<FlowRun> ResultHandler { get; private set; }


        public static Flow Begin(string name = null) => new Flow(name);

        public FlowRun Run(object arg = null, FlowContext context = null, bool throwsException = true, object contextData = null)
            => RunAsync(arg, context, throwsException, contextData).Wait();

        public FlowRun RunAsync(object arg = null, FlowContext context = null, bool throwsException = true, object contextData = null)
        {
            var run = new FlowRun(this, context) { ThrowsException = throwsException };

            if (contextData != null)
                run.Context.Data = contextData;

            run.StartAsync(arg);

            return run;
        }

        #region CTOR

        public virtual Flow Copy() => new Flow(this);

        private Flow(string name)
        {
            Name = name;
        }

        protected Flow(Flow flow)
        {
            Name = flow.Name;
            Steps = flow.Steps.Copy();
            _lastValidParamType = flow._lastValidParamType;
        }

        #endregion

        public Flow OnError(Action<FlowRun> errorHandler)
        {
            ErrorHandler = errorHandler;
            return this;
        }

        public Flow OnResult(Action<FlowRun> resultHandler)
        {
            ResultHandler = resultHandler;
            return this;
        }

        public static void HandleAggregateException(AggregateException ae, Action<FlowRun, Exception> exceptionHandler)
        {
            ae.Handle(ex =>
            {
                if (ex is FlowRunException fre)
                {
                    exceptionHandler(fre.FlowRun, fre.InnerException);
                    return true;
                }
                return false;
            });
        }


    }







}
