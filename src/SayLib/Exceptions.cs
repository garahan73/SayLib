using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Say32
{

    [Serializable]
    public class TypeMismatchException : Exception
    {
        public TypeMismatchException(Type expected, Type actual, Exception? inner = null) : this(expected.FullName, actual.FullName, inner) { }
        public TypeMismatchException(object expected, object? actual, Exception? inner = null) : this($"Expected: {getTypeName(expected)}, Actual: {getTypeName(actual)}", inner) { }
        public TypeMismatchException() { }
        public TypeMismatchException(string message) : base(message) { }
        public TypeMismatchException(string message, Exception? inner) : base(message, inner) { }
        protected TypeMismatchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        private static object getTypeName( object? obj ) => obj is Type || obj is string ? obj : obj?.GetType().Name ?? "null";
    }


    [Serializable]
    public class NoDelegateException : Exception
    {
        public NoDelegateException(Type type, Exception? inner=null) : this($"Type = {type.FullName}", inner) { }
        public NoDelegateException() { }
        public NoDelegateException(string message) : base(message) { }
        public NoDelegateException(string message, Exception? inner) : base(message, inner) { }
        protected NoDelegateException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class MissingTypeException : Exception
    {
        public MissingTypeException() { }
        public MissingTypeException(string message) : base(message) { }
        public MissingTypeException(string message, Exception inner) : base(message, inner) { }
        protected MissingTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class IllegalStateException : Exception
    {
        public IllegalStateException() { }
        public IllegalStateException( string message ) : base(message) { }
        public IllegalStateException( string message, Exception inner ) : base(message, inner) { }
        protected IllegalStateException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context ) : base(info, context) { }
    }

    [Serializable]
    public class StringToLiteralConversionException : Exception
    {
        public StringToLiteralConversionException() { }
        public StringToLiteralConversionException( string message ) : base(message) { }
        public StringToLiteralConversionException( string message, Exception inner ) : base(message, inner) { }
        protected StringToLiteralConversionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context ) : base(info, context) { }
    }


    [Serializable]
    public class TestException : Exception
    {
        public TestException() { }
        public TestException( string message ) : base(message) { }
        public TestException( string message, Exception inner ) : base(message, inner) { }
        protected TestException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context ) : base(info, context) { }
    }


    [Serializable]
    public class ErrorHandlingException : Exception
    {
        public Exception OriginalException { get; }

        public ErrorHandlingException( string message, Exception inner, Exception original ) : base(message, inner) { OriginalException = original; }
        protected ErrorHandlingException(
          SerializationInfo info,
          StreamingContext context ) : base(info, context) 
        {
            OriginalException = (Exception)info.GetValue(nameof(OriginalException), typeof(Exception));
        }

        public override string ToString() => $"{base.ToString()}\r\n\r\n[Original Exception]\r\n{OriginalException}";
    }



    [Serializable]
    public class CancelException : Exception
    {
        public CancelException() { }
        public CancelException( string message ) : base(message) { }
        public CancelException( string message, Exception inner ) : base(message, inner) { }
        protected CancelException(
          SerializationInfo info,
          StreamingContext context ) : base(info, context) { }
    }
}
