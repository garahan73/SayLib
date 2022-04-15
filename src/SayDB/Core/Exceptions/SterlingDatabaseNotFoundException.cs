
namespace Say32.DB.Core.Exceptions
{
    public class SayDBDatabaseNotFoundException : SayDBException
    {
        public SayDBDatabaseNotFoundException(string databaseName)
            : base(string.Format(ExceptionMessageFormat.SayDBDatabaseNotFoundException, databaseName))
        {
        }
    }
}