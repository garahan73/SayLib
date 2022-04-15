using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Say32
{
    public static class ExceptionExtensions
    {
        public static Exception GetRoot(this Exception ex)
        {
            Exception e = ex;
            while (e.InnerException != null) e = e.InnerException;
            return e;
        }

        public static string ToStringEx(this Exception ex)
        {
            var sb = new StringBuilder();
            sb.Append(ex.ToString());
            
            PrintData(ex);
            PrintData(ex.InnerException);

            return sb.ToString();

            void PrintData(Exception ex1)
            {
                if (ex1 != null && ex1.Data.Count != 0)
                {
                    sb.AppendLine("\n[Data]------------------------------------");
                    sb.AppendLine($"{ex.GetType().Name}: {ex.Message}");

                    foreach (var key in ex1.Data.Keys)
                    {
                        sb.AppendLine($"- {key} = {ex.Data[key]}");
                    }
                }
            }
        }

        public static Exception StripAggregate(this Exception exception)
        {
            while( exception is AggregateException)
            {
                exception = exception.InnerException;
            }
            return exception;
        }

        public static string GetTypeAndMessage(this Exception ex)
        {
            return $"{ex.GetType().Name}: {ex.Message}";
        }
    }

    public static class Ex
    {
        public static bool ShowOnConsole { get; set; }

        public static Exception CreateException<T>( params (string Key, object? Value)[] data ) where T : Exception 
            => CreateException<T>((string?)null, (Exception?)null, data);

        public static Exception CreateException<T>(string message, params (string Key, object? Value)[] data) where T : Exception 
            => CreateException<T>(message, null, data);

        public static Exception CreateException<T>(object obj, string? methodName, string? message=null, Exception? inner = null, params (string Key, object? Value)[] data) where T : Exception
            => CreateException<T>(obj.GetDegbugMessage(methodName, message), inner, data);

        

        public static Exception CreateException<T>(string? message, Exception? exception, params (string Key, object? Value)[] data) where T : Exception
        {
            Exception ex = CreateExceptionObject<T>(message, exception);

            if (data != null)
            {
                foreach (var pair in data)
                {
                    ex.Data.Add(pair.Key, pair.Value);
                }
            }

            if (ShowOnConsole) 
                Console.WriteLine(ex.ToStringEx());

            return ex;
        }        

        private static Exception CreateExceptionObject<T>(string? message, Exception? exception) where T : Exception
        {
            return CreateException(typeof(T), message, exception);
        }

        public static Exception CreateException( Type type, string? message, Exception? exception ) 
        {
            try
            {
                var ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(string), typeof(Exception) }, new ParameterModifier[] { });
                if (ctor != null)
                {
                    return (Exception)ctor.Invoke(new object?[] { message, exception });
                }

                ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(string), typeof(Exception), typeof((string Key, object? Value)[]) }, new ParameterModifier[] { });
                if (ctor != null)
                {
                    return (Exception)ctor.Invoke(new object?[] { message, exception, new (string Key, object? Value)[0] });
                }

                throw new Exception($"No proper constructor for exception type '{type.Name}'", exception);
            }
            catch (Exception ex)
            {
                return new ExceptionCreationFail($"Exception type='{type.Name}', getMessage='{message}'", ex);
            }
        }

        public static void Assert<TException>(LazyStructValue<bool> assertion, object obj, string methodName, Func<object> message, Exception? exception = null, params (string key, object? value)[] data)
            where TException : Exception
            => Assert<TException>(assertion, obj, methodName, (LazyValue<object>)message, exception, data);
        public static void Assert<TException>(LazyStructValue<bool> assertion, object obj, string methodName, LazyValue<object>? message = null, Exception? exception = null, params (string key, object? value)[] data)
            where TException : Exception
            => Assert<TException>(assertion, (Func<object>)(()=>obj.GetDegbugMessage(methodName, message)), exception, data);


        public static void Assert<TException>(LazyStructValue<bool> assertion, Func<object> message, Exception? exception = null, params (string key, object? value)[] data) where TException : Exception
            => Assert<TException>(assertion, (LazyValue<object>)message, exception, data);
        public static void Assert<TException>(LazyStructValue<bool> assertion, LazyValue<object>? message=null, Exception? exception = null, params (string key, object? value)[] data)
            where TException : Exception
        {
            if (!assertion)
            {
                var ex = Ex.CreateException<TException>(message?.ToString(), exception, data);
                Debug.WriteLine(ex.ToStringEx());
                throw ex;
            }
        }


        /*
        public static void AssertWhen<TException>(LazyValue<bool> when, LazyValue<bool> assertion, object obj, string methodName, Func<object> message, Exception exception = null, params (string key, object value)[] data)
            where TException : Exception
            => AssertWhen<TException>(when, assertion, obj, methodName, (LazyValue<object>)message, exception, data);
        public static void AssertWhen<TException>(LazyValue<bool> when, LazyValue<bool> assertion, object obj, string methodName, LazyValue<object> message=null, Exception exception = null, params (string key, object value)[] data)
            where TException : Exception
        {
            if (when)
            {
                Assert<TException>(assertion, obj, methodName, message, exception, data);
            }
        }

        public static void AssertWhen<TException>(LazyValue<bool> when, LazyValue<bool> assertion, Func<object> message, Exception exception = null, params (string key, object value)[] data)
            where TException : Exception
            => AssertWhen<TException>(when, assertion, (LazyValue<object>)message, exception, data);
        public static void AssertWhen<TException>(LazyValue<bool> when, LazyValue<bool> assertion, LazyValue<object> message=null, Exception exception = null, params (string key, object value)[] data)
            where TException : Exception
        {
            if (when)
            {
                Assert<TException>(assertion, message, exception, data);
            }
        }
        */
    }

    [Serializable]
    public class ExceptionCreationFail : Exception
    {
        public ExceptionCreationFail() { }
        public ExceptionCreationFail(string message) : base(message) { }
        public ExceptionCreationFail(string message, Exception inner) : base(message, inner) { }
        protected ExceptionCreationFail(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
