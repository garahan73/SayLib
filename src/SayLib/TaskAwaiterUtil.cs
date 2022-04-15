using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Say32
{
    public static class TaskAwaiterUtil
    {
        public static TaskAwaiter GetAwaiter( this TimeSpan timeSpan )
        {
            return Task.Delay(timeSpan).GetAwaiter();
        }

        //public static TaskAwaiter GetAwaiter( this Int32 millisecondsDue )
        //{
        //    return TimeSpan.FromMilliseconds(millisecondsDue).GetAwaiter();
        //}

        public static TaskAwaiter GetAwaiter( this DateTime dateTime )
        {
            return (dateTime - DateTime.Now).GetAwaiter();
        }

        public static TaskAwaiter GetAwaiter( this IEnumerable<Task> tasks )
        {
            return Task.WhenAll(tasks).GetAwaiter();
        }

        public static CultureAwaiter WithCurrentCulture( this Task task )
        {
            return new CultureAwaiter(task);
        }

        public class CultureAwaiter : INotifyCompletion
        {
            private readonly TaskAwaiter m_awaiter;
            private CultureInfo? m_culture;

            public CultureAwaiter( Task task )
            {
                if (task == null) throw new ArgumentNullException("task");
                m_awaiter = task.GetAwaiter();
            }

            public CultureAwaiter GetAwaiter() { return this; }

            public bool IsCompleted { get { return m_awaiter.IsCompleted; } }

            public void OnCompleted( Action continuation )
            {
                m_culture = CultureInfo.CurrentCulture;
                m_awaiter.OnCompleted(continuation);
            }

            public void GetResult()
            {
                if (m_culture != null) Thread.CurrentThread.CurrentCulture = m_culture;

                m_awaiter.GetResult();
            }
        }


    }
}
