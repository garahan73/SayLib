using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Say32
{
    public class Try
    {
        public static TryingAction Run( Action method )
        {
            var t = new TryingAction(method);
            t.Run();
            return t;
        }

        public static TryingFunction<T> Run<T>( Func<T> method )
        {
            var t = new TryingFunction<T>(method);
            t.Run();
            return t;
        }

        public static bool Run(Action method, out Exception? exception)
        {
            var t = new TryingAction(method);
            t.Run();
            exception = t.Exception;
            return t.Success;
        }

        public static bool Run<T>( Func<T> method, out T result, out Exception? exception )
        {
            var t = new TryingFunction<T>(method);
            t.Run();
            result = t.Result;
            exception = t.Exception;
            return t.Success;
        }

        public static async Task<TryingAction> RunAsync( Func<Task> method )
        {
            var t = new TryAsync(method);            
            await t.RunAsync();
            return t;
        }

        public static async Task<TryingFunction<T>> RunAsync<T>( Func<Task<T>> method )
        {
            var t = new TryAsync<T>(method);
            await t.RunAsync();
            return t;
        }

        public static async Task<Exception?> RunActionAsync( Func<Task> method )
        {
            var t = new TryAsync(method);
            await t.RunAsync();
            return t.Exception;
        }

        public static async Task<(T result, Exception? exception)> RunFunctionAsync<T>( Func<Task<T>> method )
        {
            var t = new TryAsync<T>(method);
            await t.RunAsync();
            return (t.Result, t.Exception);
        }

    }

    public class TryingAction
    {
        public bool Success => Exception == null;
        public bool Fail => !Success;

        protected object? ResultInternal { get; set; }

        internal TryingAction( Delegate method ) => Method = method;
    
        public Delegate Method { get; }
        
        public Exception? Exception { get; protected set; }

        public void Run()
        {
            try
            {
                ResultInternal = Method.DynamicInvoke();
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
        }

        public static implicit operator bool (TryingAction trying) => trying.Success;
    }

    public class TryingFunction<T> : TryingAction
    {
        public TryingFunction( Func<T> func ) : base(func)
        {
        }


        public T Result
        {
            get
            {
#pragma warning disable CS8603 // 가능한 null 참조 반환입니다.
                try { return (T)ResultInternal; }
                catch { return default(T); }
#pragma warning restore CS8603 // 가능한 null 참조 반환입니다.
            }
        }
        
        public void Deconstruct(out bool success, out object? result)
        {
            success = Success;
            result = Result;
        }

        public void Deconstruct( out object? result, out Exception? exception )
        {
            result = Result;
            exception = Exception;
        }

    }

    public class TryAsync : TryingAction
    {
        private readonly Func<Task> _action;

        public TryAsync( Func<Task> action ) : base(action)
        {
            _action = action;
        }

        public async Task<bool> RunAsync()
        {
            try
            {
                await _action();
                return true;
            }
            catch (Exception ex)
            {
                Exception = ex;
                return false;
            }
        }
    }


    public class TryAsync<T> : TryingFunction<T>
    {
        private readonly Func<Task<T>> _func;

#pragma warning disable CS8603 // 가능한 null 참조 반환입니다.
        public TryAsync( Func<Task<T>> func ) : base(()=>default)
#pragma warning restore CS8603 // 가능한 null 참조 반환입니다.
        {
            _func = func;
        }

        public async Task<bool> RunAsync()
        {
            try
            {
                ResultInternal = await _func();
                return true;
            }
            catch (Exception ex)
            {
                Exception = ex;                
                return false;
            }
        }
    }






}
