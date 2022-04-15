namespace Say32.DB.Core.Exceptions
{
    public class SayDBNoTableDefinitionsException : SayDBException 
    {
        public SayDBNoTableDefinitionsException() : base(ExceptionMessageFormat.SayDBNoTableDefinitionsException)
        {
            
        }
    }
}
