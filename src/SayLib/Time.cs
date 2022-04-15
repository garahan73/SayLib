
using Say32.Xml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Say32
{
    public class Timelap
    {
        public DateTime StartTime { get; private set; }

        public Timelap()
        {
            Start();
        }

        public void Start()
        {
            StartTime = DateTime.Now;
            StopTime = DateTime.MinValue;
        }

        public TimeSpan ElapsedTime => (StopTime == DateTime.MinValue ? DateTime.Now : StopTime) - StartTime;
        public DateTime StopTime { get; private set; } = DateTime.MinValue;


        public bool IsTimeOver( int timetoutInMilliSec )
        {
            return ElapsedTime >= TimeSpan.FromMilliseconds(timetoutInMilliSec);
        }

        public bool IsTimeOver( TimeSpan timetout )
        {
            return ElapsedTime >= timetout;
        }

        public override string ToString()
        {
            return TimeString;
        }

        public string TimeString
        {
            get
            {
                var lap = ElapsedTime;
                if (lap.TotalSeconds > 1)
                    return $"{lap.TotalSeconds:F2} seconds";

                var msec = lap.TotalMilliseconds;
                return msec < 1 ? $"{msec:F2} milliseconds" : $"{msec:F0} milliseconds";
            }
        }

        public void Stop() => StopTime = DateTime.Now;
    }



    public class QuickTimeout
    {   
        public static CancelToken Start( Timeout timeout, Action timeoutHandler )
        {
            var cancelToken = new CancelToken();

            if (timeout.IsEnabled)
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(timeout.Value);

                    if (!cancelToken.IsCanceled)
                    {
                        timeoutHandler.Invoke();
                    }
                });
            }

            return cancelToken;
        }

    }

    public class BasicTimeout
    {
        private bool _running = false;

        public async void Start( int timeoutInMilliSec, Action timeoutHandler )
        {
            if (_running)
                throw new InvalidOperationException($"{nameof(BasicTimeout)} is already running. Can't start again.");

            _running = true;

            await Task.Delay(timeoutInMilliSec);

            if (_running)
            {
                Stop();
                timeoutHandler.Invoke();
            }

        }

        public void Stop()
        {
            _running = false;
        }
    }

    public class CancelToken
    {
        public bool IsCanceled { get; private set; } = false;
        public event Action? Triggered;

        public void Cancel()
        {
            if (IsCanceled) return;

            IsCanceled = true;
            Triggered?.Invoke();
        }

        public void Reset()
        {
            IsCanceled = false;
        }
    }


    public struct Timeout
    {
        public Timeout( int timeoutInMilliSec )
            : this(TimeSpan.FromMilliseconds(timeoutInMilliSec))            
        {            
        }

        public Timeout( TimeSpan value )
        {
            var totMsec = value.TotalMilliseconds;
            _value = totMsec <= 0 ? default : value;
        }

        private TimeSpan _value;

        public TimeSpan Value => IsEnabled ? _value : TimeSpan.FromMilliseconds(-1);

        public bool IsEnabled => _value != default;
        
        public static implicit operator Timeout(int timeoutInMilliSec) => new Timeout(timeoutInMilliSec);
        public static implicit operator Timeout(TimeSpan timeout) => new Timeout(timeout);

        public static implicit operator int(Timeout timeout) => timeout.IsEnabled ? (int)timeout.Value.TotalMilliseconds : -1;
        public static implicit operator TimeSpan(Timeout timeout) => timeout.IsEnabled ? timeout.Value : TimeSpan.FromMilliseconds(-1);

        public static implicit operator Timeout(string timeout) => Timeout.Parse(timeout);

        public override string ToString()
        {
            return IsEnabled ? Value.TotalSeconds > 1 ? $"{Value.TotalSeconds:F0} seconds" : $"{Value.TotalMilliseconds} milliseconds" : "disabled timeout";
        }

        [XmlCustomDeserializer]
        private static Timeout ParseXmlValue(XText xtext) => Parse(xtext.Value);

        public static Timeout Parse(string text)
        {
            var org = text;

            try
            {
                text = text.Trim();

                if (text.EndsWith("ms"))
                {
                    text = text.TrimEnd("ms");
                    return (Timeout)int.Parse(text);
                }

                if (text.EndsWith("ms."))
                {
                    text = text.TrimEnd("ms.");
                    return (Timeout)int.Parse(text);
                }

                if (text.EndsWith("msec"))
                {
                    text = text.TrimEnd("msec");
                    return (Timeout)int.Parse(text);
                }

                if (text.EndsWith("msec."))
                {
                    text = text.TrimEnd("msec.");
                    return (Timeout)int.Parse(text);
                }

                if (text.EndsWith("milliseconds"))
                {
                    text = text.TrimEnd("milliseconds");
                    return (Timeout)int.Parse(text);
                }

                if (text.EndsWith("sec"))
                {
                    text = text.TrimEnd("sec");
                    return (Timeout)(int.Parse(text) * 1000);
                }

                if (text.EndsWith("sec."))
                {
                    text = text.TrimEnd("sec.");
                    return (Timeout)(int.Parse(text) * 1000);
                }

                if (text.EndsWith("seconds"))
                {
                    text = text.TrimEnd("seconds");
                    return (Timeout)(int.Parse(text) * 1000);
                }

                if (text.EndsWith("s"))
                {
                    text = text.TrimEnd("s");
                    return (Timeout)(int.Parse(text) * 1000);
                }

                if (text.EndsWith("s."))
                {
                    text = text.TrimEnd("s.");
                    return (Timeout)(int.Parse(text) * 1000);
                }

                return (Timeout)int.Parse(text);

            }
            catch (Exception ex)
            {
                throw new TimeoutParsingException($"Text = '{org}'", ex);
            }

        }

        public TimeoutRun Start() =>  new TimeoutRun(this);
        
    }

    public class TimeoutRun
    {
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }

        public TimeSpan Value => EndTime - StartTime;

        public virtual bool IsTimeout => Timeout.IsEnabled && DateTime.Now > EndTime;

        public TimeSpan ElapsedTime => DateTime.Now - StartTime;
        public TimeSpan RemainingTime => EndTime - DateTime.Now;

        public event Action<Timeout>? OnTimeout;

        public static implicit operator bool( TimeoutRun timeout ) => timeout.IsTimeout;

        public static implicit operator TimeoutRun(Timeout timeout) => new TimeoutRun(timeout);
        public static implicit operator TimeoutRun( int timeoutInMilliseconds ) => new TimeoutRun(timeoutInMilliseconds);
        public static implicit operator TimeoutRun( TimeSpan timeout ) => new TimeoutRun(timeout);

        internal TimeoutRun( Timeout timeout )
        {
            Timeout = timeout;

            if (Timeout.IsEnabled)
            {

                StartTime = DateTime.Now;
                EndTime = StartTime + timeout;
            }
            else // disabled
            {
                StartTime = DateTime.Now;
                EndTime = DateTime.MaxValue;
            }
        }

        public Timeout Timeout { get; }

        public override string ToString()
        {
            return $"{nameof(TimeoutRun)} Started at {StartTime}";
        }

        public virtual async Task<bool> WaitAsync( Task task )
        {
            if (IsTimeout) return false;

            if (task.IsCompleted)
                return true;

            if (!Timeout.IsEnabled)
            {
                await task;
                return true;
            }
            else
            {
                var success = await Task.WhenAny(task, Task.Delay(RemainingTime)) == task;

                Debug.WriteLine($"WaitTimeout {(success ? "success" : "fail")}");

                if (!success) // timeout
                {
                    Debug.WriteLine($"!! Timeout occurred. {Timeout.Value.TotalMilliseconds} msec.");
                    OnTimeout?.Invoke(Timeout);
                }
                else // no timeout
                {
                    if (task.Exception != null)
                    {
                        var ex = task.Exception.StripAggregate();
                        try
                        {
                            ex =  Ex.CreateException(ex.GetType(), ex.Message, ex);
                        }
                        catch
                        {
                            throw new TaskException("", ex);
                        }

                        throw ex;
                    }
                }

                return success;
            }
        }

        public bool Wait( Task task ) => WaitAsync(task).WaitAndGetResult();
    }



    public static class SayTimeUtil
    {
        public static DateTime Trim( this DateTime date, long roundTicks )
        {
            return new DateTime(date.Ticks - date.Ticks % roundTicks, date.Kind);
        }

        //public static bool IsEnabled( this Timeout? timeout ) => timeout?._isEnabled ?? false;
    }


    [Serializable]
    public class TaskException : Exception
    {
        public TaskException() { }
        public TaskException( string message ) : base(message) { }
        public TaskException( string message, Exception inner ) : base(message, inner) { }
        protected TaskException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context ) : base(info, context) { }
    }

    [Serializable]
    public class TimeoutParsingException : Exception
    {
        public TimeoutParsingException() { }
        public TimeoutParsingException( string message ) : base(message) { }
        public TimeoutParsingException( string message, Exception inner ) : base(message, inner) { }
        protected TimeoutParsingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context ) : base(info, context) { }
    }

}