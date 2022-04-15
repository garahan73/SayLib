
using System;

namespace Say32.DB.Core.Exceptions
{
    public class SayDBTriggerException : SayDBException 
    {
        public SayDBTriggerException(string message, Type triggerType) :
            base(string.Format(ExceptionMessageFormat.SayDBTriggerException, triggerType.FullName,
                               message))
        {
            
        }
    }
}