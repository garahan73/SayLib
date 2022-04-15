using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Say32.DB.Core.Async
{
    public class EnumerableTasksAwaiter<T> : ICriticalNotifyCompletion
    {
        public IEnumerable<Task<T>> Tasks { get; private set; } 

        private readonly Task<IEnumerable<T>> _task;

        public bool IsCompleted => _task.IsCompleted;

        public IEnumerable<T> GetResult() => _task.Result;

        public void OnCompleted(Action continuation) => continuation();

        public void UnsafeOnCompleted(Action continuation) => continuation();

        internal EnumerableTasksAwaiter(IEnumerable<Task<T>> tasks)
        {
            Tasks = tasks;

            _task = Task.Run(() =>
            {
                Task.WaitAll(Tasks.ToArray());
                return Tasks.Select(t => t.Result);
            });
        }

    }
}
