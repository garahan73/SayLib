using System;

namespace Say32.DB.Core.Exceptions
{
    /// <summary>
    ///     Base from which SayDB exceptions derived
    /// </summary>
    public class SayDBException : Exception
    {
        public SayDBException()
        {
            
        }

        public SayDBException(string message) : base(message)
        {
            
        }

        public SayDBException(string message, Exception innerException) : base(message, innerException)
        {
            
        }
    }
}