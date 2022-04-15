using Say32.DB.Core.Database;
using Say32.DB.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Say32.DB.Core
{
    class TriggerManager
    {
        internal DbCore Core { get; private set; }

        #region PRIVATE CORE DELEGATES
        private Task<IDisposable> LockAsync() => Core.LockAsync();

        private Dictionary<DataStoreID, IStoreDefinition> StoreDefinitions => Core.StoreDefinitions;

        private TaskCounter TaskCounter => Core.TaskCounter;

        #endregion

        private readonly Dictionary<DataStoreID, List<ISayDBTrigger>> _triggers =
            new Dictionary<DataStoreID, List<ISayDBTrigger>>();

        /// <summary>
        /// The byte stream interceptor list. 
        /// </summary>
        internal readonly List<BaseDbByteInterceptor> byteInterceptorList =
            new List<BaseDbByteInterceptor>();

        /// <summary>
        ///     Register a trigger
        /// </summary>
        /// <param name="trigger">The trigger</param>
        public void RegisterTrigger<T, TKey>(BaseDbTrigger<T, TKey> trigger) where T : class //, new()
        {
            using (LockAsync().Result)
            {
                var tableID = DataStoreID.FromType(typeof(T));

                if (!_triggers.ContainsKey(tableID))
                {
                    _triggers.Add(tableID, new List<ISayDBTrigger>());
                }

                _triggers[tableID].Add(trigger);
            }
        }

        /// <summary>
        ///     Fire the triggers for a type
        /// </summary>
        /// <param name="tableID">The target type</param>
        internal IEnumerable<ISayDBTrigger> GetStoreTriggers(DataStoreID store)
        {
            return _triggers.ContainsKey(store) ? _triggers[store] : new List<ISayDBTrigger>();
        }


        /// <summary>
        ///     Unregister the trigger
        /// </summary>
        /// <param name="trigger">The trigger</param>
        public void UnregisterTrigger<T, TKey>(BaseDbTrigger<T, TKey> trigger) where T : class //, new()
        {
            using (LockAsync().Result)
            {
                var tableID = DataStoreID.FromType(typeof(T));
                _triggers[tableID].Remove(trigger);
            }
        }




        /// <summary>
        /// Registers the BaseSayDBByteInterceptor
        /// </summary>
        /// <typeparam name="T">The type of the interceptor</typeparam>
        public void RegisterInterceptor<T>() where T : BaseDbByteInterceptor //, new()
        {
            using (LockAsync().Result)
            {
                byteInterceptorList.Add((T)Activator.CreateInstance(typeof(T)));
            }
        }

        public void UnRegisterInterceptor<T>() where T : BaseDbByteInterceptor //, new()
        {
            using (LockAsync().Result)
            {
                var interceptor = (from i
                                       in byteInterceptorList
                                   where i.GetType().Equals(typeof(T))
                                   select i).FirstOrDefault();

                if (interceptor != null)
                {
                    byteInterceptorList.Remove(interceptor);
                }
            }
        }

        /// <summary>
        /// Clears the _byteInterceptorList object
        /// </summary>
        public void UnRegisterInterceptors()
        {
            using (LockAsync().Result)
            {
                if (byteInterceptorList != null)
                {
                    byteInterceptorList.Clear();
                }
            }
        }



    }
}
