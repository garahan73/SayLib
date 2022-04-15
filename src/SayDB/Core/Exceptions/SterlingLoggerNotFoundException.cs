
using System;

namespace Say32.DB.Core.Exceptions
{
    public class SayDBLoggerNotFoundException : SayDBException 
    {
        public SayDBLoggerNotFoundException(Guid guid) : base(string.Format(ExceptionMessageFormat.SayDBLoggerNotFoundException, guid))
        {
            
        }
    }
}
