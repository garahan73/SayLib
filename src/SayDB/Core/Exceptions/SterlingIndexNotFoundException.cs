using Say32.DB.Store;
using System;

namespace Say32.DB.Core.Exceptions
{
    public class DbIndexNotFoundException : SayDBException 
    {
        public DbIndexNotFoundException(string indexName, DataStoreID id) : 
            base(string.Format(ExceptionMessageFormat.SayDBIndexNotFoundException, indexName, id))
        {
            
        }
    }
}
