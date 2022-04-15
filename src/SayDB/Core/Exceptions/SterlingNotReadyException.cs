namespace Say32.DB.Core.Exceptions
{
    public class SayDBNotReadyException : SayDBException
    {
        public SayDBNotReadyException() : base(ExceptionMessageFormat.SayDBNotReadyException)
        {
            
        }
    }
}
