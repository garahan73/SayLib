using System;
using System.Collections.Generic;
using System.Text;

namespace Say32.DB.Core.Exceptions
{

    [Serializable]
    public class SayDbCreationException : Exception
    {
        public SayDbCreationException() { }
        public SayDbCreationException( string message ) : base(message) { }
        public SayDbCreationException( string message, Exception inner ) : base(message, inner) { }
        protected SayDbCreationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context ) : base(info, context) { }
    }
}
