
using Say32.DB.Store;
using System;

namespace Say32.DB.Core.Exceptions
{
    public class DbDuplicateTypeException : SayDBException 
    {
        public DbDuplicateTypeException(DataStoreID storeID, string databaseName) :
            base(string.Format(ExceptionMessageFormat.SayDBDuplicateTypeException, storeID, databaseName))
        {
            
        }
    }
}
