using Say32.DB.Store;
using System;

namespace Say32.DB.Core.Database
{
    /// <summary>
    ///     Base for triggers
    /// </summary>
    /// <typeparam name="T">The type the trigger is for</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    public abstract class BaseDbTrigger<T,TKey> : ISayDBTrigger<T,TKey> where T: class//, new()
    {
        public bool BeforeSave(Type type, object instance)
        {
            return BeforeSave((T) instance);
        }

        public void AfterSave(Type type, object instance)
        {
            AfterSave((T) instance);
        }
       
        public bool BeforeDelete(DataStoreID id, object key)
        {
            return BeforeDelete((TKey) key);
        }

        public abstract bool BeforeSave(T instance);

        public abstract void AfterSave(T instance);

        public abstract bool BeforeDelete(TKey key);
        
    }
}