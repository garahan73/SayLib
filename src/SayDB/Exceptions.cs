using System;
using System.Collections.Generic;
using System.Text;

namespace Say32.DB
{

    [Serializable]
    public class SayDbException : Exception
    {
        public SayDbException() { }
        public SayDbException(string message) : base(message) { }
        public SayDbException(string message, Exception inner) : base(message, inner) { }
        protected SayDbException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
