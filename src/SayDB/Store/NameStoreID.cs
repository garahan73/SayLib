using System;
using System.Collections.Generic;
using System.Text;

namespace Say32.DB.Store
{
    internal class NameStoreID : DataStoreID
    {
        private readonly string _name;

        public NameStoreID(string name)
        {
            if (name == null)
                throw new SayDbNameStoreIdException("Name value is null");

            _name = name;
        }

        public override string KeyName => _name;

        public override int GetHashCode()
        {
            return KeyName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj != null &&
                obj is NameStoreID tid &&
                KeyName == tid.KeyName;
        }      
        
    }


    [Serializable]
    public class SayDbNameStoreIdException : Exception
    {
        public SayDbNameStoreIdException() { }
        public SayDbNameStoreIdException( string message ) : base(message) { }
        public SayDbNameStoreIdException( string message, Exception inner ) : base(message, inner) { }
        protected SayDbNameStoreIdException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context ) : base(info, context) { }
    }
}
