
using System;

namespace Say32.DB.Core.Exceptions
{
    public class SayDBDuplicateIndexException : SayDBException 
    {
        public SayDBDuplicateIndexException(string indexName, Type type, string databaseName) : 
        base (string.Format(ExceptionMessageFormat.SayDBDuplicateIndexException, indexName, type.FullName, databaseName))
        {
            
        }        
    }
}
