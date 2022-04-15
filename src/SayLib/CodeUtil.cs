using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Say32
{
    public static class CodeUtil
    {
        public static Exception? SafeRun( Action action )
        {
            try 
            { 
                action();
                return null;
            } 
            catch(Exception ex) 
            {
                return ex;
            }
        }

        public static bool IsNull( this object o ) => o == null;

        public static bool IsNullable( this object o ) => o == null ? true : IsNullable(o.GetType());

        public static bool IsNullable<T>() => IsNullable(typeof(T));

        public static bool IsNullable( Type type )
            => type == null || !type.IsValueType ||
                    Nullable.GetUnderlyingType(type) != null;

        public static string PropertyList( this object obj )
        {
            var props = obj.GetType().GetProperties();
            var sb = new StringBuilder();
            foreach (var p in props)
            {
                sb.AppendLine(p.Name + ": " + p.GetValue(obj, null));
            }
            return sb.ToString();
        }

        public static bool DefaultStripAggregateException { get; set; } = true;

        public static void ExpectException( Action method, Action<Exception> exceptionHandler, bool? stripAggregateException = null )
        {
            try
            {
                method();
                throw new ExpectExceptionError();
            }
            catch (ExpectExceptionError epe)
            {
                throw epe;
            }
            catch (Exception ex)
            {
                ex = StripAggregateExcpetion(stripAggregateException, ex);
                exceptionHandler(ex);
            }
        }

        public static T Try_StripAggregateException<T>( Func<T> method, bool? stripAggregateException = null )
        {
            try
            {
                return method();
            }
            catch (Exception ex)
            {
                ex = StripAggregateExcpetion(stripAggregateException, ex);
                throw ex;
            }
        }

        public static async Task<T> Try_StripAggregateExceptionAsync<T>( Func<Task<T>> method, bool? stripAggregateException = null )
        {
            try
            {
                return await method();
            }
            catch (Exception ex)
            {
                ex = StripAggregateExcpetion(stripAggregateException, ex);
                throw ex;
            }
        }


        public static void Try_StripAggregateException( Action method, bool? stripAggregateException = null )
        {
            try
            {
                method();
            }
            catch (Exception ex)
            {
                ex = StripAggregateExcpetion(stripAggregateException, ex);
                throw ex;
            }
        }

        public static bool Try( Action action, out Exception? exception, bool? stripAggregateException = null )
        {
            try
            {
                action();
                exception = null;
                return true;
            }
            catch (Exception ex)
            {
                ex = StripAggregateExcpetion(stripAggregateException, ex);

                exception = ex;
                return false;
            }
        }

        public static void Try( Action method, Action<Exception> exceptionHandler, bool? stripAggregateException = null )
        {
            try
            {
                method();
            }
            catch (Exception ex)
            {
                ex = StripAggregateExcpetion(stripAggregateException, ex);

                exceptionHandler(ex);
            }
        }

        public static T Try<T>( Func<T> method, Func<Exception, T> exceptionHandler, bool? stripAggregateException = null )
        {
            try
            {
                return method();
            }
            catch (Exception ex)
            {
                ex = StripAggregateExcpetion(stripAggregateException, ex);

                return exceptionHandler(ex);
            }
        }

        public static async Task<T> TryAsync<T>(Func<Task<T>> asyncMethod, Func<Exception, T> exceptionHandler, bool? stripAggregateException = null)
        {
            try
            {
                return await asyncMethod();
            }
            catch (Exception ex)
            {
                ex = StripAggregateExcpetion(stripAggregateException, ex);

                return exceptionHandler(ex);
            }
        }

        public static async Task<T> TryAsync<T>(Func<Task<T>> asyncMethod, Action<Exception> exceptionHandler, bool? stripAggregateException = null)
        {
            try
            {
                return await asyncMethod();
            }
            catch (Exception ex)
            {
                ex = StripAggregateExcpetion(stripAggregateException, ex);

                exceptionHandler(ex);

#pragma warning disable CS8603 // 가능한 null 참조 반환입니다.
                return default;
#pragma warning restore CS8603 // 가능한 null 참조 반환입니다.
            }
        }

        public static async Task<(T result, Exception? exception)> TryAsync<T>(Func<Task<T>> asyncMethod, bool? stripAggregateException = null)
        {
            try
            {
                return ( await asyncMethod(), null);
            }
            catch (Exception ex)
            {
                ex = StripAggregateExcpetion(stripAggregateException, ex);

#pragma warning disable CS8619 // 값에 있는 참조 형식 Null 허용 여부가 대상 형식과 일치하지 않습니다.
                return (default, ex);
#pragma warning restore CS8619 // 값에 있는 참조 형식 Null 허용 여부가 대상 형식과 일치하지 않습니다.
            }
        }


        public static bool Try<T>( Func<T> method, out T result, out Exception? exception )
        {
            try
            {
                result = method();
                exception = null;
                return true;
            }
            catch (Exception ex)
            {
#pragma warning disable CS8601 // 가능한 null 참조 할당입니다.
                result = default;
#pragma warning restore CS8601 // 가능한 null 참조 할당입니다.
                exception = ex;
                return false;
            }
        }




        public static bool Try<T>( Func<T> func, out T result, out Exception? exception, bool? stripAggregateException = null )
        {
            try
            {
                result = func();
                exception = null;
                return true;
            }
            catch (Exception ex)
            {
                var ex2 = StripAggregateExcpetion(stripAggregateException, ex);

                exception = ex2;
#pragma warning disable CS8601 // 가능한 null 참조 할당입니다.
                result = default;
#pragma warning restore CS8601 // 가능한 null 참조 할당입니다.
                return false;
            }
        }

        public static T Try<T>( Func<T> func, out Exception? exception, bool? stripAggregateException = null )
        {
            exception = null;

            try
            {
                return func();
            }
            catch (Exception ex)
            {
                ex = StripAggregateExcpetion(stripAggregateException, ex);

#pragma warning disable CS8603 // 가능한 null 참조 반환입니다.
                return default;
#pragma warning restore CS8603 // 가능한 null 참조 반환입니다.
            }
        }



        private static Exception StripAggregateExcpetion( bool? stripAggregateException, Exception ex )
        {
            stripAggregateException ??= DefaultStripAggregateException;

            while (stripAggregateException == true && ex is AggregateException aex)
                ex = aex.InnerException;

            return ex;
        }

        //public static void Try( Action action, Func<Exception, Exception> exceptionModifier )
        //{
        //    try
        //    {
        //        action();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw exceptionModifier(ex);
        //    }
        //}

    }


    [Serializable]
    public class ExpectExceptionError : Exception
    {
        public ExpectExceptionError() { }
        public ExpectExceptionError( string message ) : base(message) { }
        public ExpectExceptionError( string message, Exception inner ) : base(message, inner) { }
        protected ExpectExceptionError(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context ) : base(info, context) { }
    }
}
