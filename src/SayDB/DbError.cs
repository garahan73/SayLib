using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Say32.DB
{
    class DbError
    {
        internal static void Fail<TException>(string message) where TException : Exception => Assert<TException>(false, message);

        internal static void Assert<TException>(bool contract, string message) where TException:Exception
        {
            if (!contract)
            {
                var ex = CreateException<TException>(message);

                //Debug.WriteLine("************** EXCEPTION *******************");
                //Debug.WriteLine(ex);

                throw ex;
            }
        }

        public static Exception CreateException<TException>(string message ) where TException : Exception
        {
            var type = typeof(TException);

            var paramTypes = new Type[] { typeof(string) };

            var ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, paramTypes, null);
            if (ctor == null)
                ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, paramTypes, null);

            if (ctor == null)
            {            
                throw new SayDbException($"Failed to create exception. Type {type.FullName}, message: {message}");
            }
            else
                return (Exception)ctor.Invoke(new object[] { message });


        }
    }
}
