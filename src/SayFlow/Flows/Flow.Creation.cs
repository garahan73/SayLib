using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Say32.Flows
{
    partial class Flow
    {
        #region CREATION

        public Flow<R> And<A, R>(string name, Func<A, R> method) => And(method, name);
        public Flow<R> And<A, R>(Func<A, R> method, string name = null) => Path(null, method, name);
        public Flow<R> Path<A, R>(string path, Func<A, R> method, string stepName = null)
        {
            Steps.AddStep(path, new FlowStep(stepName, Name).SetFunction(method));
            _lastValidParamType = new FlowParameterType<R>();
            return new Flow<R>(this);
        }

        public Flow<R> And<R>(string name, Func<R> method) => Path(null, method, name);
        public Flow<R> And<R>(Func<R> method, string name = null) => Path(null, method, name);
        public Flow<R> Path<R>(string path, Func<R> method, string name = null)
        {
            Steps.AddStep(path, new FlowStep(name, Name).SetFunction(method));
            _lastValidParamType = new FlowParameterType<R>();
            return new Flow<R>(this);
        }

        public Flow<R> And1<A, R>(string name, Func<A, Task<R>> method) => Path(null, method, name);
        public Flow<R> And<A, R>(Func<A, Task<R>> method, string name = null) => Path(null, method, name);
        public Flow<R> Path<A, R>(string path, Func<A, Task<R>> method, string name = null)
        {
            Steps.AddStep(path, new FlowStep(name, Name).SetFunction(method));
            _lastValidParamType = new FlowParameterType<R>();
            return new Flow<R>(this);
        }


        public Flow And<A>(string name, Action<A> method) => Path(null, method, name);
        public Flow And<A>(Action<A> method, string name = null) => Path(null, method, name);
        public Flow Path<A>(string path, Action<A> method, string name = null)
        {
            Steps.AddStep(path, new FlowStep(name, Name).SetAction(method));
            return _lastValidParamType != null ? _lastValidParamType.CreateFlow(this) : this;
        }

        public Flow And(string name, Action method) => Path(null, method, name);
        public Flow And(Action method, string name = null) => Path(null, method, name);
        public Flow Path(string path, Action method, string name = null)
        {
            Steps.AddStep(path, new FlowStep(name, Name).SetAction(method));
            return _lastValidParamType != null ? _lastValidParamType.CreateFlow(this) : this;
        }

        public Flow And(string name, Flow subflow) => Path(null, subflow, name);
        public Flow And(Flow subflow, string name = null) => Path(null, subflow, name);

        public Flow Path(string path, Flow subflow, string name = null)
        {
            Steps.AddStep(path, new FlowStep(name, Name).SetSubflow(subflow));
            return this;
        }

        #endregion
    }




    public class Flow<A> : Flow
    {
        internal Flow(Flow flow) : base(flow)
        {
        }

        public override Flow Copy() => new Flow<A>(this);

        public new Flow<R> And<R>(string name, Func<R> method) => Path(null, method, name);
        public new Flow<R> And<R>(Func<R> method, string name = null) => Path(null, method, name);
        public new Flow<R> Path<R>(string path, Func<R> method, string name = null)
        {
            Steps.AddStep(path, new FlowStep(name, Name).SetFunction(method));
            _lastValidParamType = new FlowParameterType<R>();
            return new Flow<R>(this);
        }

        public Flow<R> And<R>(string name, Func<A, R> method) => Path(null, method, name);
        public Flow<R> And<R>(Func<A, R> method, string name = null) => Path(null, method, name);
        public Flow<R> Path<R>(string path, Func<A, R> method, string name = null)
        {
            Steps.AddStep(path, new FlowStep(name, Name).SetFunction(method));
            _lastValidParamType = new FlowParameterType<R>();
            return new Flow<R>(this);
        }

        public Flow<A> And(string name, Action<A> method) => Path(null, method, name);
        public Flow<A> And(Action<A> method, string name = null) => Path(null, method, name);
        public Flow<A> Path(string path, Action<A> method, string name = null)
        {
            Steps.AddStep(path, new FlowStep(name, Name).SetAction(method));
            return this;
        }

        public new Flow<A> And(string name, Action method) => Path(null, method, name);
        public new Flow<A> And(Action method, string name = null) => Path(null, method, name);
        public new Flow<A> Path(string path, Action method, string name = null)
        {
            Steps.AddStep(path, new FlowStep(name, Name).SetAction(method));
            return this;
        }

        /*
        public new Flow And(Flow subflow) => Path(null, subflow);
        public new Flow Path(string path, Flow subflow)
        {            
            if (!subflow.ArgType.Is(typeof(A)))
                throw new FlowException(this, $"Can't add subflow step by argument type mismatch. required={typeof(A).Name}, subflow arg.={subflow.ArgType}");
                
            return base.Path(path, subflow);
        }
        */

    }
}
