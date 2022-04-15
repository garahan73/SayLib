using System;

namespace Say32
{
    public class Result
    {
        protected Result( bool success, string error )
        {
            Success = success;
            ErrorMessage = error;
            Exception = null;
        }

        protected Result( bool success, Exception exception )
        {
            Success = success;
            ErrorMessage = exception.Message;
            Exception = exception;
        }

        public bool Success { get; }
        public string ErrorMessage { get; }
        public Exception? Exception { get; }

        public bool Failure => !Success;

        public static Result Fail( string message )
        {
            return new Result(false, message);
        }

        public static Result Fail( Exception exception )
        {
            return new Result(false, exception);
        }

        public static Result<T> Fail<T>( string message )
        {
#pragma warning disable CS8604 // 가능한 null 참조 인수입니다.
            return new Result<T>(default, false, message);
#pragma warning restore CS8604 // 가능한 null 참조 인수입니다.
        }

        public static Result<T> Fail<T>( Exception exception )
        {
#pragma warning disable CS8604 // 가능한 null 참조 인수입니다.
            return new Result<T>(default, false, exception);
#pragma warning restore CS8604 // 가능한 null 참조 인수입니다.
        }


        public static Result Ok()
        {
            return new Result(true, string.Empty);
        }

        public static Result<T> Ok<T>( T value )
        {
            return new Result<T>(value, true, string.Empty);
        }

        public static Result Combine( params Result[] results )
        {
            foreach (Result result in results)
            {
                if (result.Failure)
                {
                    return result;
                }
            }

            return Ok();
        }
    }

    public class Result<T> : Result
    {
        protected internal Result( T value, bool success, string error )
            : base(success, error)
        {
            Value = value;
        }

        protected internal Result( T value, bool success, Exception exception )
            : base(success, exception)
        {
            Value = value;
        }

        public T Value { get; }
    }
}
