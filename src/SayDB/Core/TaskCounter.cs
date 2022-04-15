using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Say32.DB.Core
{
    class TaskCounter
    {
        private long _taskCount = 0;

        internal void Increase() => Interlocked.Increment(ref _taskCount);

        internal void Decrease() => Interlocked.Decrement(ref _taskCount);

        internal long Count => Interlocked.Read(ref _taskCount);

        void Reset() => Interlocked.Exchange(ref _taskCount, 0);
    }
}
