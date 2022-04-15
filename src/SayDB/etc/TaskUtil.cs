using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Say32.DB.etc
{
    public static class TaskUtil
    {

        public static Task<T> StartStaTask<T>(Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();
            Thread thread = new Thread(() =>
            {
                try
                {
                    tcs.SetResult(func());
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        public static Task StartStaTask(Action action)
        {
            return StartStaTask(() => { action(); return 1; });
        }

        public static async Task<bool> WaitAsync(this Task task, TimeSpan waitTime)
        {
            return await Task.WhenAny(task, Task.Delay(waitTime)) == task;
        }
    }
}
