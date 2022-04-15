using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Say32.Flows
{

    public abstract class FlowParameterType
    {
        public abstract Type Type { get; }

        public static FlowParameterType Void { get; } = new FlowParameterType<Void>();
        public static FlowParameterType Unknown { get; } = new FlowParameterType<object>();

        public bool IsVoid => this == Void;

        internal abstract Flow CreateFlow(Flow flow);

        public abstract FlowParameter CreateParameter(object value = null);

        internal bool Is(Type type) => type == Type;
        internal abstract bool Is(object value);

        public bool IsTask => typeof(Task).IsAssignableFrom(Type);

        public bool IsContext => Type == typeof(FlowContext);

        public override string ToString() => Type.Name;
    }

    class FlowParameterType<T> : FlowParameterType
    {
        internal override Flow CreateFlow(Flow flow) => new Flow<T>(flow);

        public override FlowParameter CreateParameter(object value) => new FlowParameter<T>(value);

        public override Type Type => typeof(T);

        internal override bool Is(object value) => value == null ? Type.IsNullable() : value is T;
    }

    class Void
    {
        public static Void Instance { get; } = new Void();
        public override string ToString() => "VOID";
    }

    public abstract class FlowParameter
    {
        //public FlowParameterType Type { get; protected set; }
        public abstract object Value { get; set; }

        public static FlowParameter VOID { get; } = new FlowParameter<Void>(Void.Instance);
        public bool IsVoid => this == VOID;

        public object TriedValue { get; internal set; }

        public abstract Type Type { get; }

        public bool IsContext => Type == typeof(FlowContext);

        public string TriedValueDetail => $"{Type.Name}:{TriedValue?.ToString() ?? "NULL"}";

        public string GetValueText(bool indicateVoidType = false)
            => Value is Void ? indicateVoidType ? "VOID" : "" :
                        Value?.ToString() ?? "null";
    }

    public class FlowParameter<T> : FlowParameter
    {

        public FlowParameter(object value)
        {
            Value = value ?? default(T);
        }

        public override Type Type => typeof(T);

        private object _value;
        public override object Value
        {
            get => _value;
            set
            {
                TriedValue = value;

                if (typeof(T) == typeof(Void))
                    _value = Void.Instance;
                else if (value == null)
                {
                    if (CodeUtil.IsNullable<T>())
                        _value = null;
                    else
                        throw new FlowParameterException(this, $"Illegal null value. parameter type is {typeof(T).Name}");
                }
                else
                {
                    _value = (T)value;
                }
            }
        }




    }


}
