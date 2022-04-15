using System;

namespace Say32.DB.Core.Exceptions
{
    public class SayDBIsolatedStorageException : SayDBException
    {
        public SayDBIsolatedStorageException(Exception ex) : base(string.Format(ExceptionMessageFormat.SayDBIsolatedStorageException,ex.Message), ex)
        {
            
        }
    }
}
