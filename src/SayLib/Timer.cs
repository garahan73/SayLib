using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Say32
{
    public class TimerJob
    {
        public int Interval { get; private set; }

        private readonly Action _action;
        private bool _running;
        private Task? _task;

        public TimerJob(int interval, Action action )
        {
            Interval = interval;
            _action = action;
        }

        public void Start()
        {
            _running = true;

            _task = Task.Run(async ()=>
            {
                while (_running)
                {
                    _action();

                    await Task.Delay(Interval);
                }
            });
        }

        public async Task StopAsync()
        {
            _running = false;

            if (_task != null)
                await _task;
        }

        public void Stop() => StopAsync().Wait();
    }
}
