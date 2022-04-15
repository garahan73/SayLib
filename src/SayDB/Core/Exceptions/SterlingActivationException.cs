
namespace Say32.DB.Core.Exceptions
{
    public class SayDBActivationException : SayDBException 
    {
        public SayDBActivationException(string operation) : base(string.Format(ExceptionMessageFormat.SayDBActivationException, operation))
        {
            
        }
    }
}
