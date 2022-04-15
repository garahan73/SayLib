
using System;

namespace Say32.DB.Core.Exceptions
{
    public class SayDBNullException : SayDBException 
    {
        public SayDBNullException(string property, Type type) : base(string.Format(ExceptionMessageFormat.SayDBNullException, property, type.FullName))
        {
            
        }
    }
}
