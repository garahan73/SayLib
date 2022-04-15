using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Say32.DB.Store.IO
{
    internal class IoHelper
    {
        public static object CreateObject(Type type)
        {
            try
            {
                var ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[] { }, new ParameterModifier[] { });
                if (ctor == null)
                    ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { }, new ParameterModifier[] { });

                if (ctor == null)
                {
                    return Activator.CreateInstance(type);
                    //throw new SayDbException($"Failed to create object during loading. Type {type.FullName} doesn't have constructor without parameter ");
                }
                else
                    return ctor.Invoke(new object[] { });
            }
            catch (Exception ex)
            {
                throw new SayDbObjectCreationException($"Failed to create object of type '{type.Name}'", ex);
            }
        }

        public static void SerializeNull(BinaryWriter bw, IoHeader header = null)
        {
            header = header ?? new IoHeader();

            header.IsNull = true;
            header.Serialize(bw);

            //Debug.WriteLine("[Save NULL]");
        }


        [Serializable]
        public class SayDbObjectCreationException : Exception
        {
            public SayDbObjectCreationException() { }
            public SayDbObjectCreationException(string message) : base(message) { }
            public SayDbObjectCreationException(string message, Exception inner) : base(message, inner) { }
            protected SayDbObjectCreationException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
    }
}
