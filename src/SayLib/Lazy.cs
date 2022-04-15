using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;

namespace Say32
{
    public abstract class LazyValueBase<T>
    {
        private T _value;
        private Func<T>? _func;

        public LazyValueBase(T value)
        {
            _value = value;
        }

#pragma warning disable
        public LazyValueBase(Func<T> func)
        {
            _func = func;
            //_value = default;
#pragma warning restore
        }


        public T Value
        {
            get
            {
                if (_func != null)
                {
                    _value = _func.Invoke();
                    _func = null;
                }

                return _value;
            }
        }

        public override string? ToString() => Value?.ToStringExt();

    }

    public class LazyValue<T>: LazyValueBase<T> where T:class
    {
#pragma warning disable CS8603, CS8604, CS8620, 8634 // 가능한 null 참조 반환입니다.

        public LazyValue(T? value) : base(value) { }

        public LazyValue(Func<T?> func) : base(func) { }


        public static implicit operator LazyValue<T>(T? value) => new LazyValue<T>(value);
        public static implicit operator LazyValue<T>(Func<T?>? func) => new LazyValue<T>(func);
        public static implicit operator T?(LazyValue<T?> lazy) => lazy.Value;

#pragma warning restore
    }

    public class LazyStructValue<T> : LazyValueBase<T> where T : struct
    {
#pragma warning disable CS8603, CS8604, CS8620 // 가능한 null 참조 반환입니다.

        public LazyStructValue(T value) : base(value) { }

        public LazyStructValue(Func<T> func) : base(func) { }


#pragma warning restore

        public static implicit operator LazyStructValue<T>(T value) => new LazyStructValue<T>(value);
        public static implicit operator LazyStructValue<T>(Func<T> func) => new LazyStructValue<T>(func);
        public static implicit operator T(LazyStructValue<T> lazy) => lazy.Value;
    }

}
