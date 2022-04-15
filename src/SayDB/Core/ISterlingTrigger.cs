using Say32.DB.Store;
using System;

namespace Say32.DB.Core
{
    /// <summary>
    ///     Interface for a SayDB trigger
    /// </summary>
    internal interface ISayDBTrigger
    {
        bool BeforeSave(Type type, object instance);
        void AfterSave(Type type, object instance);
        bool BeforeDelete(DataStoreID id, object key);
    }

    /// <summary>
    ///     Trigger for SayDB
    /// </summary>
    /// <typeparam name="T">The type it supports</typeparam>
    /// <typeparam name="TKey">The key</typeparam>
    internal interface ISayDBTrigger<T, TKey> : ISayDBTrigger where T: class//, new() 
    {
        bool BeforeSave(T instance);
        void AfterSave(T instance);
        bool BeforeDelete(TKey key);
    }
}