using System;

namespace Say32.DB.Core.Exceptions
{
    public class SayDBDuplicateDatabaseException : SayDBException
    {
        public SayDBDuplicateDatabaseException(SayDB instance) : base(
            string.Format(ExceptionMessageFormat.SayDBDuplicateDatabaseException, instance.GetType().FullName))
        {
        }

        public SayDBDuplicateDatabaseException(Type type)
            : base(
                string.Format(ExceptionMessageFormat.SayDBDuplicateDatabaseException, type.FullName))
        {
        }
    }
}