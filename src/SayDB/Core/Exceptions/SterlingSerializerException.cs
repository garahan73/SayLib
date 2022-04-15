
using System;
using Say32.DB.Core.Serialization;

namespace Say32.DB.Core.Exceptions
{
    public class SayDBSerializerException : SayDBException 
    {
        public SayDBSerializerException(IDbSerializer serializer, Type targetType) : 
            base(string.Format(ExceptionMessageFormat.SayDBSerializerException, serializer.GetType().FullName, targetType.FullName))
        {
            
        }
    }
}
