using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Say32
{
    public class Wait
    {
        private const int CheckIntervalInMilliSecDefault = 50;

        public static void Until( Func<bool> exitCondition, Timeout timeout = default, int checkIntervalInMilliSec = CheckIntervalInMilliSecDefault, CancelToken? cancelToken = null )
        {
            if (!UntilSafe(exitCondition, timeout, checkIntervalInMilliSec, cancelToken))
            {
                throw new WaitTimeoutException((int)timeout.Value.TotalMilliseconds);
            }
        }

        public static bool UntilSafe( Func<bool> exitCondition, Timeout timeout = default, int checkIntervalInMilliSec = CheckIntervalInMilliSecDefault, CancelToken? cancelToken = null )
        {
            var run = timeout.Start();

            while (!(cancelToken?.IsCanceled ?? false))
            {
                // check timeout
                if (run)
                    return false;

                // check exit condition
                if (exitCondition())
                    return true;

                Thread.Sleep(checkIntervalInMilliSec);
            }

            return false;
        }

        public static async Task UntilAsync( Func<bool> exitCondition, Timeout timeout = default, int checkIntervalInMilliSec = CheckIntervalInMilliSecDefault, CancelToken? cancelToken = null )
        {
            await Task.Run(() =>
            {
                try
                {
                    Until(exitCondition, timeout, checkIntervalInMilliSec, cancelToken);
                }
                catch (Exception ex)
                {
                    throw new WaitException($"{nameof(Wait)} fail", ex);
                }
            });
        }

        public static async Task<bool> UntilSafeAsync( Func<bool> exitCondition, Timeout timeout = default, int checkIntervalInMilliSec = CheckIntervalInMilliSecDefault, CancelToken? cancelToken = null )
        {
            return await Task.Run(() =>
            {
                try
                {
                    return UntilSafe(exitCondition, timeout, checkIntervalInMilliSec, cancelToken);
                }
                catch (Exception ex)
                {
                    throw new WaitException($"{nameof(Wait)} fail", ex);
                }
            });
        }

        public static async Task UntilAsync( Func<Task<bool>> exitConditionAsync, Timeout timeout = default, int checkIntervalInMilliSec = CheckIntervalInMilliSecDefault, CancelToken? cancelToken = null )
        {
            if (!await UntilSafeAsync(exitConditionAsync, timeout, checkIntervalInMilliSec, cancelToken))
            {
                throw new WaitTimeoutException((int)timeout.Value.TotalMilliseconds);
            }
        }

        public static async Task<bool> UntilSafeAsync( Func<Task<bool>> exitConditionAsync, Timeout timeout = default, int checkIntervalInMilliSec = CheckIntervalInMilliSecDefault, CancelToken? cancelToken = null )
        {
            var run = timeout.Start();

            while (!(cancelToken?.IsCanceled ?? false))
            {
                // check timeout
                if (run)
                    return false;

                // check exit condition
                if (await exitConditionAsync())
                    return true;

                await Task.Delay(checkIntervalInMilliSec);
            }

            return false;
        }
    }



    [Serializable]
    public class WaitException : Exception
    {
        public WaitException() { }
        public WaitException( string message ) : base(message) { }
        public WaitException( string message, Exception inner ) : base(message, inner) { }
        protected WaitException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context ) : base(info, context) { }
    }


    [Serializable]
    public class WaitCancelationException : Exception
    {
        public WaitCancelationException() { }
        public WaitCancelationException( string message ) : base(message) { }
        public WaitCancelationException( string message, Exception inner ) : base(message, inner) { }
        protected WaitCancelationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context ) : base(info, context) { }
    }

    [Serializable]
    public class WaitTimeoutException : Exception
    {
        public int TimeoutInMilliSec { get; }

        public WaitTimeoutException( int timeoutInMilliSec, Exception? inner=null ) : base($"Timeout = {timeoutInMilliSec} milli sconds", inner) 
        { 
            TimeoutInMilliSec = timeoutInMilliSec; 
        }

        protected WaitTimeoutException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context ) : base(info, context) 
        {
            TimeoutInMilliSec = (int) info.GetValue(nameof(TimeoutInMilliSec), typeof(int));
        }
    }
}
