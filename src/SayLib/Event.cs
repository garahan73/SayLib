using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Say32
{
    public class AsyncEvent<T>
    {
        private TaskCompletionSource<T>? _tcs = null;
        private readonly ThreadSafeEvent<T> _handlers = new ThreadSafeEvent<T>();

        private readonly AsyncActionQueue _actionQueue = new AsyncActionQueue();

        public bool IsSet => this.Lock(() => _tcs == null || _tcs.Task.IsCompleted);

        public async Task<T> WaitAsync(TimeoutRun? timeout = null)
        {
            //Debug.WriteLine($"==== {nameof(AsyncEvent)} Wait..");
            var tcs = GetFreshTcs();

            return await tcs.Task.WaitAsync(timeout);
        }

        public object? Wait(TimeoutRun? timeout = null)
        {
            return WaitAsync(timeout).WaitAndGetResult();
        }

        public void Set( T eventArg )
        {
            //Debug.WriteLine($"!!!! AsyncEvent SET: {eventArg}");
            _actionQueue.Add(() => InvokeEvent(eventArg));
        }

        private void InvokeEvent(T eventArg)
        {
            _handlers.Invoke(eventArg);

            var tcs = GetWaitingTcs();
            tcs?.SetResult(eventArg);
        }


        private TaskCompletionSource<T> GetFreshTcs()
        {
            return this.Lock(() =>
            {
                if (_tcs == null || _tcs.Task.IsCompleted)
                {
                    _tcs = new TaskCompletionSource<T>();
                    //Debug.WriteLine($"++++ AsyncEvent NEW TCS!!!");
                }

                return _tcs;
            });
        }

        private TaskCompletionSource<T>? GetWaitingTcs()
        {
            return this.Lock(() =>
            {
                if (_tcs != null && !_tcs.Task.IsCompleted)
                {
                    //Debug.WriteLine($"==== AsyncEvent INVOKE TCS!!!");
                    return _tcs;                    
                }

                return null;
            });
        }


        public void AddEventHandler(Action<T> eventHandler)
        {
            lock(this)
            {
                _handlers.Add(eventHandler);
            }
        }

        internal void RemoveEventHandler(Action<T> eventHandler)
        {
            lock(this)
            {
                _handlers.Remove(eventHandler);
            }
        }
    }

    public class AsyncEvent : AsyncEvent<object?>
    {
        
    }

    public class KeyEventArgs<T>
    {
        public KeyEventArgs(string key, T arg)
        {
            Key = key;
            Arg = arg;
        }
        public string Key { get; }
        public T Arg { get; }
    }

    public class KeyEvents<T>
    {
        internal readonly ConcurrentDictionary<string, AsyncEvent<T>> _map = new ConcurrentDictionary<string, AsyncEvent<T>>();
        //private readonly ConcurrentDictionary<string, Event<KeyEventArgs<T>>> _keyHeaderMap = new ConcurrentDictionary<string, Event<KeyEventArgs<T>>>();

        public bool AsyncCall { get; }

        public KeyEvents(bool asyncCall = false)
        {
            AsyncCall = asyncCall;
        }

        public event Action<string, T>? EventOccurred;

        public async Task<T> WaitAsync(Enum key, TimeoutRun? timeout = null) => await WaitAsync(key.ToString(), timeout);

        public async Task<T> WaitAsync(string key, TimeoutRun? timeout = null)
        {
            CreateEventIfNeeded(key);
            return await _map[key].WaitAsync(timeout);
        }

        public object? Wait(Enum key, Timeout timeout = default) => Wait(key.ToString(), timeout);

        public object? Wait(string key, Timeout timeout = default)
        {
            return WaitAsync(key, timeout).WaitAndGetResult();
        }

        public virtual void Set(string key, T eventArg, bool async = false )
        {
            //CreateEventIfNeeded(key);

            if (_map.ContainsKey(key))
                _map[key].Set(eventArg);

            if (EventOccurred != null)
            {
                Task.Run(() => EventOccurred.Invoke(key, eventArg));
            }
            

            // invoke events where key is matched to key header            
            //_keyHeaderMap.Keys.Where(kh => key.StartsWith(kh))
            //    .ForEach(kh => _keyHeaderMap[kh].Set(new KeyEventArgs<T>(key, eventArg) ));
        }

        public void Set(Enum key, T eventArg)
        {
            Set(key.ToString(), eventArg);
        }

        public void AddEventHandler(string key, Action<T> eventHandler)
        {
            CreateEventIfNeeded(key);
            _map[key].AddEventHandler(eventHandler);
        }

        //public void AddEventHandlerByKeyHeader( string keyHeader, Action<string, T> eventHandler )
        //{
        //    if (!_keyHeaderMap.ContainsKey(keyHeader))
        //    {
        //        _keyHeaderMap.TryAdd(keyHeader, new Event<KeyEventArgs<T>>());
        //    }

        //    _keyHeaderMap[keyHeader].AddEventHandler(args =>eventHandler(args.Key, args.Arg));
        //}


        public void AddEventHandler(Enum key, Action<T> eventHandler) => AddEventHandler(key.ToString(), eventHandler);

        public void RemoveEventHandler(string key, Action<T> eventHandler)
        {
            if (_map.ContainsKey(key))
            {
                _map[key].RemoveEventHandler(eventHandler);
            }
        }

        public void RemoveEventHandler(Enum key, Action<T> eventHandler) => RemoveEventHandler(key.ToString(), eventHandler);

        private void CreateEventIfNeeded(string key)
        {
            if (!_map.ContainsKey(key))
            {
                _map.TryAdd(key, new AsyncEvent<T>());
            }
        }

    }

    public class AsyncKeyEvents : KeyEvents<object?> { }


    public class ThreadSafeEvent<T>
    {
        private readonly ThreadSafeList<Action<T>> _actions = new ThreadSafeList<Action<T>>();


        public void Add( Action<T> action ) => _actions.Add(action);
        public void Remove( Action<T> action ) => _actions.Remove(action);

        public void Invoke( T arg )
        {
            if (_actions.Count == 0) return;

            _actions.ForEach(action => Run.Safely(()=>action(arg)));
        }


        public void InvokeAsync( T arg )
        {
            if (_actions.Count == 0) return;

            _actions.ForEach(action =>Task.Run(()=> action(arg)));
        }
    }

    public class ThreadSafeEvent<T1, T2>
    {
        private readonly ThreadSafeList<Action<T1, T2>> _actions = new ThreadSafeList<Action<T1, T2>>();

        public void Invoke( T1 arg1, T2 arg2 )
        {
            if (_actions.Count == 0) return;

            _actions.ForEach(action => action(arg1, arg2));
        }

        public void Add( Action<T1, T2> action ) => _actions.Add(action);
        public void Remove( Action<T1, T2> action ) => _actions.Remove(action);
    }
}


