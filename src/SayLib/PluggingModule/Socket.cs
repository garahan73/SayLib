using System.Collections.Generic;

namespace Say32.PluggingModule
{
    public interface IModuleSocket<T>
    {
        void Plug(IPluggableModule<T> pluggable);
    }

    public class SocketHelper
    {
        public static List<IModuleSocket<T>> GetSockets<T>(IPluggableModule<T> pluggable)
        {
            var sockets = new List<IModuleSocket<T>>();

            var type = typeof(T);
            foreach (var prop in type.GetProperties())
            {
                if (typeof(IModuleSocket<T>).IsAssignableFrom(prop.PropertyType))
                {
                    var socket = (IModuleSocket<T>)prop.GetValue(pluggable);
                    if (socket == null) continue;

                    sockets.Add(socket);
                }
            }

            return sockets;
        }
    }

    public class Socket<T> : IModuleSocket<T>
    {
        public Socket()
        {
        }

        public void Plug(IPluggableModule<T> pluggable)
        {
        }
    }


}