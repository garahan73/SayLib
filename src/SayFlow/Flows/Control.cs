using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Say32.Flows
{
    public enum FlowControlType
    {
        Next, Retry, Path, EventWaiter, Pause
    }

    public delegate ref Action GetEventReference();
    public delegate ref Action<T> GetEventReference<T>();
    public delegate ref Action<T1, T2> GetEventReference<T1, T2>();

    public abstract class FlowControl
    {
        #region FACTORY

        public static FlowControl Next()
        {
            var ptype = FlowParameterType.Void;
            return new FlowControlNext(ptype, null);
        }

        public static FlowControl Next<T>(T arg)
        {
            var ptype = new FlowParameterType<T>();
            return new FlowControlNext(ptype, arg);
        }

        public static FlowControl Retry<T>(T arg)
        {
            var ptype = new FlowParameterType<T>();
            return new FlowControlRetry(ptype, arg);
        }

        public static FlowControl Path(string path)
        {
            var ptype = FlowParameterType.Void;
            return new FlowControlPath(path, ptype, null);
        }

        public static FlowControl Path<T>(string path, T arg)
        {
            var ptype = new FlowParameterType<T>();
            return new FlowControlPath(path, ptype, arg);
        }

        public static FlowControlEventWaiter WaitEvent(GetEventReference getEventRef, bool autoResetEventSubscription = true)
            => new FlowControlEventWaiter1(getEventRef, autoResetEventSubscription);

        public static FlowControlEventWaiter WaitEvent<T>(GetEventReference<T> getEventRef, bool autoResetEventSubscription = true)
            => new FlowControlEventWaiter<T>(getEventRef, autoResetEventSubscription);

        public static FlowControlEventWaiter WaitEvent<T1, T2, T>(GetEventReference<T1, T2> getEventRef, Func<T1, T2, T> resultConverter, bool autoResetEventSubscription = true)
            => new FlowControlEventWaiter<T1, T2, T>(getEventRef, resultConverter, autoResetEventSubscription);

        public static FlowControl Pause() => new FlowControlPause(true);
        internal static FlowControl PauseBySubFlow() => new FlowControlPause(false);

        #endregion


        public FlowControlType Type { get; private set; }

        public FlowParameterType ResultType { get; private set; }
        protected FlowParameter Parameter { get; private set; }

        public object ResultValue => Parameter.Value;

        protected FlowControl(FlowControlType type, FlowParameterType paramType, object value)
        {
            Type = type;
            ResultType = paramType;
            Parameter = paramType.CreateParameter(value);
        }

        public abstract FlowStep GetNextStep(FlowStepIterator iterator);

        public override string ToString() => $"{nameof(FlowControl)}.{Text}";
        public virtual string Text => $"{Type.ToString().ToUpper()}{(ResultType.IsVoid ? "" : $"({ResultValue})")}";

    }



    internal class FlowControlNext : FlowControl
    {
        public FlowControlNext(FlowParameterType paramType, object value) : base(FlowControlType.Next, paramType, value)
        {
        }

        public override FlowStep GetNextStep(FlowStepIterator iterator)
        {
            return iterator.MoveToNextInCurrentPath();
        }
    }

    internal class FlowControlRetry : FlowControl
    {
        public FlowControlRetry(FlowParameterType paramType, object value) : base(FlowControlType.Retry, paramType, value)
        {

        }

        public override FlowStep GetNextStep(FlowStepIterator iterator) => iterator.CurrentStep;
    }

    internal class FlowControlPath : FlowControl
    {
        public string PathName { get; private set; }

        public FlowControlPath(string path, FlowParameterType paramType, object value) : base(FlowControlType.Path, paramType, value)
        {
            PathName = path;
        }

        public override FlowStep GetNextStep(FlowStepIterator iterator)
        {
            if (iterator.HasPath(PathName))
            {
                iterator.SetPath(PathName);
                return iterator.CurrentStep;
            }
            else
                return null;
        }

        public override string ToString() => $"{nameof(FlowControl)}.{Type}({PathName})";
        public override string Text => $"{Type.ToString().ToUpper()}({PathName}{(ResultType.IsVoid ? "" : $":{ResultValue}")})";
    }

    public class FlowStepIterator
    {
        private FlowPaths _steps;

        public FlowStepIterator(FlowPaths steps)
        {
            _steps = steps;
        }

        public string PathName { get; private set; }
        public int Index { get; private set; }

        public List<FlowStep> CurrentPath => _steps[PathName];
        public FlowStep CurrentStep => CurrentPath[Index];

        internal FlowStep Begin()
        {
            PathName = FlowPath.Main;
            Index = 0;
            return _steps[PathName][Index];
        }

        internal bool HasPath(string pathName) => _steps.HasPath(pathName);

        internal FlowStep MoveToNextInCurrentPath()
        {
            Index++;
            return CurrentPath.Count > Index ? CurrentStep : null;
        }

        internal void SetPath(string path)
        {
            PathName = path;
            Index = 0;
        }
    }

    public abstract class FlowControlEventWaiter : FlowControl
    {
        protected readonly AutoResetEvent _are = new AutoResetEvent(false);
        //protected readonly bool _autoRemove;

        public Task Task { get; internal set; }

        protected FlowControlEventWaiter(FlowParameterType paramType, object value)
            : base(FlowControlType.EventWaiter, paramType, value)
        {
            //_autoRemove = autoRemove;
        }

        public override FlowStep GetNextStep(FlowStepIterator iterator)
        {
            return iterator.MoveToNextInCurrentPath();
        }
    }

    class FlowControlEventWaiter1 : FlowControlEventWaiter
    {
        public FlowControlEventWaiter1(GetEventReference getEventRef, bool autoResetEventSubscription)
            : base(FlowParameterType.Void, Void.Instance)
        {
            Action _eventHandler = () =>
            {
                _are.Set();
            };

            getEventRef() += _eventHandler;

            Task = Task.Run(() =>
            {
                _are.WaitOne();

                if (autoResetEventSubscription)
                    getEventRef() -= _eventHandler;
            });
        }
    }

    class FlowControlEventWaiter<T> : FlowControlEventWaiter
    {

        public FlowControlEventWaiter(GetEventReference<T> getEventRef, bool autoResetEventSubscription)
            : base(new FlowParameterType<T>(), null)
        {
            Action<T> eventHandler = (arg) =>
            {
                Parameter.Value = arg;
                _are.Set();
            };

            getEventRef() += eventHandler;

            Task = Task.Run(() =>
            {
                _are.WaitOne();

                if (autoResetEventSubscription)
                    getEventRef() -= eventHandler;
            });
        }
    }

    class FlowControlEventWaiter<T1, T2, T> : FlowControlEventWaiter
    {
        private Delegate _getEventRef;

        public FlowControlEventWaiter(GetEventReference<T1, T2> getEventRef, Func<T1, T2, T> resultConverter, bool autoResetEventSubscription)
            : base(new FlowParameterType<T>(), null)
        {
            Action<T1, T2> eventHandler = (arg1, arg2) =>
             {
                 Parameter.Value = resultConverter(arg1, arg2);
                 _are.Set();
             };
            _getEventRef = getEventRef;

            getEventRef() += eventHandler;

            Task = Task.Run(() =>
            {
                _are.WaitOne();

                if (autoResetEventSubscription)
                    getEventRef() -= eventHandler;
            });
        }
    }

    internal class FlowControlPause : FlowControl
    {
        public bool CanGoToNext { get; private set; }

        public FlowControlPause(bool canGoToNext) : base(FlowControlType.Pause, FlowParameterType.Void, Void.Instance)
        {
            CanGoToNext = canGoToNext;
        }

        public override FlowStep GetNextStep(FlowStepIterator iterator)
            => CanGoToNext ? iterator.MoveToNextInCurrentPath() : iterator.CurrentStep;
    }
}
