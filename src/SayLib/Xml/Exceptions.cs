using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Say32.Xml
{

    [Serializable]
    public class XmlNameMismatchException : Exception
    {
        public XmlNameMismatchException() { }
        public XmlNameMismatchException(string message) : base(message) { }
        public XmlNameMismatchException(string message, Exception inner) : base(message, inner) { }
        protected XmlNameMismatchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public static void Assert(string actual, string expected) => Ex.Assert<XmlNameMismatchException>(actual == expected, $"actual='{actual}', expected='{expected}'");
    }


    [Serializable]
    public class XmlLiteralValueConversionException : Exception
    {
        public XmlLiteralValueConversionException() { }
        public XmlLiteralValueConversionException(string message) : base(message) { }
        public XmlLiteralValueConversionException(string message, Exception inner) : base(message, inner) { }
        protected XmlLiteralValueConversionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class XmlDeserializationException : Exception
    {
        private static string? CreateMessage(string? message, XObject? xml, Exception? ex=null)
        {
            return xml == null || ex is XmlDeserializationException ? message : $"{message}, XML = {xml}";
        }

        public XmlDeserializationException(XObject? xml): base(CreateMessage(null, xml)) { }
        public XmlDeserializationException(string message, XObject? xml ) : base(CreateMessage(message, xml)) { }
        public XmlDeserializationException(string message, XObject? xml, Exception inner) : base(CreateMessage(message, xml, inner), inner) { }
        protected XmlDeserializationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class XmlValueElementCountNotOneException : Exception
    {
        public XmlValueElementCountNotOneException() { }
        public XmlValueElementCountNotOneException(string message) : base(message) { }
        public XmlValueElementCountNotOneException(string message, Exception inner) : base(message, inner) { }
        protected XmlValueElementCountNotOneException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class ObjectCreationException : Exception
    {
        public ObjectCreationException() { }
        public ObjectCreationException(string message) : base(message) { }
        public ObjectCreationException(string message, Exception inner) : base(message, inner) { }
        protected ObjectCreationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class CollectionXmlTypeMismatchException : Exception
    {
        public CollectionXmlTypeMismatchException() { }
        public CollectionXmlTypeMismatchException(string message) : base(message) { }
        public CollectionXmlTypeMismatchException(string message, Exception inner) : base(message, inner) { }
        protected CollectionXmlTypeMismatchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class XmlSerializationException : Exception
    {
        public XmlSerializationException() { }
        public XmlSerializationException( string message ) : base(message) { }
        public XmlSerializationException( string message, Exception inner ) : base(message, inner) { }
        protected XmlSerializationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context ) : base(info, context) { }
    }


    [Serializable]
    public class XmlUnknownPropertyException : Exception
    {
        public XmlUnknownPropertyException() { }
        public XmlUnknownPropertyException( string message ) : base(message) { }
        public XmlUnknownPropertyException( string message, Exception inner ) : base(message, inner) { }
        protected XmlUnknownPropertyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context ) : base(info, context) { }
    }


}
