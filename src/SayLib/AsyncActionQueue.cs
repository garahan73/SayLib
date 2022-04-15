using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Say32
{
    public class AsyncActionQueue
    {
        private List<ActionJob> _jobs = new List<ActionJob>();

        private readonly object _queueLock = new object();
        private readonly AsyncLock _actionLock = new AsyncLock();


        
        //private ulong _count = 0;

        public void Add( Action action )
        {
            var job = new ActionJob(false) { Action = action };
            AddJobAndRunJobs(job);
        }

        public async Task RunAsync( Action action )
        {
            //var i = _count++;

            //var lap = new Timelap();
            var job = new ActionJob(true) { Action = action };
            //Debug.WriteLine($"[{nameof(AsyncActionQueue)}] {i} create job lap: {lap}");

            //lap.Start();

            //Debug.WriteLine($"[{nameof(AsyncActionQueue)}] {i} add job lap: {lap}");
            //Debug.WriteLine($"[{nameof(AsyncActionQueue)}] {i} jobs count = {_jobs.Count}");

            //lap.Start();
            await RunJobsAndWaitThisJobAsync(job);
            //Debug.WriteLine($"[{nameof(AsyncActionQueue)}] {i} job run lab: {lap}");
        }


        public async Task RunAsync( Func<Task> asyncAction )
        {
            var job = new ActionJob(true) { AsyncAction = asyncAction };

            await RunJobsAndWaitThisJobAsync(job);
        }

        //private ThreadSafeList<Task> _tasks = new ThreadSafeList<Task>();
        private Task? _runJobsTask;

        //private int _runCount = 0;

        protected void AddJobAndRunJobs( ActionJob job )
        {
            lock (_queueLock)
            {
                _jobs.Add(job);
            }

            Run.Safely(() =>
            {
                if (_runJobsTask?.IsCompleted ?? true)
                {
                    RunJobs();
                }
                else
                {
                    //Debug.WriteLine($"** Skip Run jobs - jobs already runing");
                }
            });
        }

        protected async Task RunJobsAndWaitThisJobAsync( ActionJob job )
        {
            AddJobAndRunJobs(job);

            if (job.CanWait)
                await job.WaitAsync();
        }

        private void RunJobs()
        {
            //Debug.WriteLine($"** Run jobs");

            _runJobsTask = Task.Run(async () =>
            {
                //Interlocked.Increment(ref _runCount);
                //Debug.WriteLine($"[{nameof(AsyncActionQueue)}] **** RUN COUNT = {_runCount}");

                using (await _actionLock.LockAsync())
                {
                    try
                    {
                        var jobs = _queueLock.Lock(() =>
                        {
                            var old = _jobs;
                            _jobs = new List<ActionJob>();
                            return old;
                        });

                        //Debug.WriteLine($"** - {jobs.Count} jobs found ");

                        foreach (var job in jobs)
                        {
                            await job.RunAsync();
                        }
                    }
                    catch { }
                    finally
                    {
                        _runJobsTask = null;
                        //Interlocked.Decrement(ref _runCount);
                    }
                }

                var count = _queueLock.Lock(() => _jobs.Count);
                if (count != 0)
                    RunJobs();
            });
        }


        public void Wait()
        {
            Run.Safely(() => _runJobsTask?.Wait());
        }

        protected class ActionJob
        {
            //private Task? _task;

            public Action? Action { get; set; }
            public Func<Task>? AsyncAction { get; set; }
            public Func<Task<object?>>? AsyncFunction { get; internal set; }
            public Func<object?>? Function { get; internal set; }

            public object? Result { get; private set; }

            private readonly TaskCompletionSource<object?>? _tcs;

            public bool CanWait => _tcs != null;

            public ActionJob(bool canWaitResult)
            {
                if (canWaitResult)
                    _tcs = new TaskCompletionSource<object?>();
            }

            public async Task RunAsync()
            {
                try
                {
                    if (Action != null)
                        Action.Invoke();
                    else if (AsyncAction != null)
                        await AsyncAction();
                    else if (Function != null)
                        Result = Function();
                    else if (AsyncFunction != null)
                        Result = await AsyncFunction();

                    Run.Safely(()=>_tcs?.SetResult(Result));
                }
                catch(Exception ex)
                {
                    Run.Safely(()=>_tcs?.SetException(ex));
                }
            }

            public async Task WaitAsync()
            {
                if(_tcs != null)
                    await _tcs.Task;
            }
        }
    }


    public class AsyncFunctionQueue : AsyncActionQueue
    {
        public void Add<T>( Func<T> func)
        {
            var job = new ActionJob(false) { Function = () => func() };
            AddJobAndRunJobs(job);
        }

        public async Task<T> RunAsync<T>( Func<T> func )
        {
            var job = new ActionJob(true) { Function = () => func() };

            await RunJobsAndWaitThisJobAsync(job);

#pragma warning disable CS8603 // 가능한 null 참조 반환입니다.
            return (T)job.Result;
#pragma warning restore CS8603 // 가능한 null 참조 반환입니다.
        }

        public async Task<T> RunAsync<T>( Func<Task<T>> asyncFunction )
        {
            var job = new ActionJob(true) { AsyncFunction = async () => await asyncFunction() };

            await RunJobsAndWaitThisJobAsync(job);

#pragma warning disable CS8603 // 가능한 null 참조 반환입니다.
            return (T)job.Result;
#pragma warning restore CS8603 // 가능한 null 참조 반환입니다.
        }
    }


    public abstract class AsyncActionQueueHandler
    {

        protected readonly AsyncActionQueue _actionQueue;

        public AsyncActionQueueHandler()
        {
            // by default, don't need to wait the result of each action (performance increase)
            _actionQueue = new AsyncActionQueue();
        }
    }
}
