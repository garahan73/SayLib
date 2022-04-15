using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Say32
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

        public static Task<T> StartStaTaskAsync<T>(Func<Task<T>> func)
        {
            var tcs = new TaskCompletionSource<T>();
            Thread thread = new Thread(async () =>
            {
                try
                {
                    tcs.SetResult(await func());
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

        //public static async Task<bool> WaitAsync(this Task task, Timeout timeout = default)
        //{
        //    if (timeout.IsEnabled)
        //        return await Task.WhenAny(task, Task.Delay(timeout.RemainingTime)) == task;
        //    else
        //    {
        //        await task;
        //        return true;
        //    }
        //}

        public static async Task<bool> WaitSafeAsync( this Task task, TimeoutRun? timeout = null )
        {
            if (timeout == null)
            {
                await task;
                return true;
            }
            else
            {
                return await timeout.WaitAsync(task);
            }            
        }

        public static async Task WaitAsync(this Task task, TimeoutRun? timeout = null)
        {
            if (!await WaitSafeAsync(task, timeout))
            {
                throw new TimeoutException($"Timeout = {timeout?.Value.TotalMilliseconds} millseconds");
            }
        }

        public static void Wait( this Task task, TimeoutRun? timeout = null ) => WaitAsync(task, timeout).Wait();
        public static bool WaitSafe( this Task task, TimeoutRun? timeout = null ) => WaitSafeAsync(task, timeout).WaitAndGetResult();

        public static async Task<T> WaitAsync<T>(this Task<T> task, TimeoutRun? timeout = null)
        {
            if (!await WaitSafeAsync(task, timeout))
            {
                throw new TimeoutException($"Timeout = {timeout?.Value.TotalMilliseconds} millseconds");
            }
            return task.Result;
        }


        public static void Wait(this Task task, TimeoutRun? timeout = null, bool stripAggregateException = true)
        {
            try
            {
                task.WaitAsync(timeout).Wait();
            }
            catch (AggregateException aex)
            {
                if (stripAggregateException)
                {
                    var ex = aex.StripAggregate();
                    Exception newEx;
                    try
                    {
                        newEx = Ex.CreateException(ex.GetType(), ex.Message, ex.InnerException);
                    }
                    catch
                    {
                        throw new Exception("Wait fail", ex);
                    }

                    throw newEx;
                }
                else
                    throw;
            }
        }

        public static T WaitAndGetResult<T>(this Task<T> task, TimeoutRun? timeout = null, bool stripAggregateException = true )
        {
            task.Wait(timeout, stripAggregateException);
            return task.Result;
        }


    }


    public class PermanentTaskCompletionSource<T>
    {
        private TaskCompletionSource<T> _tcs = new TaskCompletionSource<T>();

        private readonly AsyncLock _tcsLock = new AsyncLock();

        public void SetResult(T result)
        {            
            Run.Safely(()=>_tcs?.SetResult(result));
        }

        public void SetException(Exception exception)
        {
            Run.Safely(()=>_tcs?.SetException(exception));
        }

        public async Task<T> WaitAsync(TimeoutRun? timeout = null)
        {
            using(await _tcsLock.LockAsync())
            {
                if (_tcs == null || _tcs.Task.IsCompleted)
                {
                    _tcs = new TaskCompletionSource<T>();
                }

                return await _tcs.Task.WaitAsync(timeout);
            }
        }

        //public Task<T> Task => this.Lock(() => _tcs.Task);

        private TaskCompletionSource<T> SetNewAndGetOldTcs()
        {
            using (_tcsLock.Lock())
            {
                var old = _tcs;
                _tcs = new TaskCompletionSource<T>();
                return old;
            }
        }
    }

    public class TasksRunner
    {
        public static void Run<T>(int numberOfTasks, IEnumerable<T> items, Action<T> action)
        {
            var runner = new TasksRunner<T>(numberOfTasks, items, action);
            runner.Main();
        }

        public static Task RunAsync<T>(int numberOfTasks, IEnumerable<T> items, Action<T> action)
        {
            return Task.Run(()=>Run(numberOfTasks, items, action));
        }
    }

    internal class TasksRunner<T>
    {
        public TasksRunner(int numberOfTasks, IEnumerable<T> items, Action<T> action)
        {
            NumberOfTasks = numberOfTasks;
            Items = items;
            Action = action;
        }

        public int NumberOfTasks { get; }
        public IEnumerable<T> Items { get; }
        public Action<T> Action { get; }



        internal void Main()
        {
            var tasks = new List<Task>();

            foreach (var item in Items)
            {
                tasks.Add(Task.Run(() => Action(item)));

                if (tasks.Count < NumberOfTasks)
                {
                    continue;
                }

                var index = Task.WaitAny(tasks.ToArray());
                tasks.RemoveAt(index);
            }

            if (tasks.Count != 0)
            {
                Task.WaitAll(tasks.ToArray());
            }

        }
    }
}
