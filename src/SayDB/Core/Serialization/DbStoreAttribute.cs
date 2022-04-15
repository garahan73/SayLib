using System;

namespace Say32.DB.Core.Serialization
{
    /// <summary>
    ///     Attribute to tag a property, class, etc. that should not be serialized
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Enum | AttributeTargets.Struct, AllowMultiple = false)]
    public class DbStoreAttribute : Attribute
    {
        public string Name { get; set; }
        public Type Type { get; set; }
    }
}