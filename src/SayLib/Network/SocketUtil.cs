using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Say32.Network
{

    public static class SocketUtil
    {
        public static async Task<Socket> ConnectAsync( bool isActive, string ip, int port, int listenBacklog = 100 )
        {
            var endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            return await ConnectAsync(isActive, endPoint, listenBacklog);
        }

        public static async Task<Socket> ConnectAsync( bool isActive, IPEndPoint endPoint, int listenBacklog = 100 )
        {
            return isActive ? await ActiveConnectAsync(endPoint) : await PassiveConnectAsync(endPoint, listenBacklog);
        }

        public static async Task<Socket> PassiveConnectAsync( string ip, int port)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            return await PassiveConnectAsync(endPoint);
        }

        public static async Task<Socket> PassiveConnectAsync(this IPEndPoint endPoint, int listenBacklog = 10 )
        {
            Socket? serverSocket = null;

            //m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);       // Working
            try
            {
                serverSocket = CreateServerSocket(endPoint, listenBacklog);

                try
                {
                    return await serverSocket.AcceptAsync();
                }
                catch (SocketException se)
                {
                    Debug.WriteLine("Socket Accept Error: " + se.Message);
                    throw new SocketListenAndAcceptFail($"IP='{endPoint.Address}', Port={endPoint.Port}", se);
                }

            }
            finally
            {
                if (serverSocket != null)
                {
                    Run.Safely(() =>
                    {
                        serverSocket.LingerState = new LingerOption(false, 0);
                        Run.Safely(()=>serverSocket.Shutdown(SocketShutdown.Both));
                        Run.Safely(()=>serverSocket.Disconnect(false));
                        Run.Safely(()=>serverSocket.Close());
                    });
                }                
            }
        }

        public static Socket CreateServerSocket(this IPEndPoint endPoint, int listenBacklog = 10)
        {
            Socket? serverSocket = null;

            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                serverSocket.Bind(endPoint);
            }
            catch (SocketException se)
            {
                Debug.WriteLine("Socket Bind Error: " + se.Message);
                throw new SocketBindFail($"IP='{endPoint.Address}', Port={endPoint.Port}", se);
            }

            try
            {
                serverSocket.Listen(listenBacklog);       // Modified 2012/08/18
            }
            catch (SocketException se)
            {
                Debug.WriteLine("Socket Listen Error: " + se.Message);
                throw new SocketListenAndAcceptFail($"IP='{endPoint.Address}', Port={endPoint.Port}", se);
            }

            return serverSocket;
        }

        public static async Task<Socket> ActiveConnectAsync( string ip, int port )
        {
            var endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            return await ActiveConnectAsync(endPoint);
        }


        public static async Task<Socket> ActiveConnectAsync(this IPEndPoint endPoint )
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                await socket.ConnectAsync(endPoint);
            }
            catch (Exception ex)
            {
                CodeUtil.SafeRun(() => socket.Close());
                throw ex;
            }
            return socket;
        }

        public static async Task<int> SendStringAsync( this Socket socket, string msg, Encoding? encoding = null )
        {
            try
            {
                encoding ??= Encoding.Default;
                byte[] data = encoding.GetBytes(msg);
                return await SendAsync(socket, data);
            }
            catch (Exception ex)
            {
                throw new SocketSendStringError($"string = '{msg}'", ex);
            }
        }

        private static Task<int> SendAsync( this Socket socket, byte[] data )
        {
            return Task.Run(() => socket.Send(data));
        }

        public static async Task<Result<Socket>> AcceptAsyncSafe( this Socket socket )
        {
            Socket transferSocket;
            try
            {
                transferSocket = await AcceptAsync(socket).ConfigureAwait(false);
            }
            catch (SocketException ex)
            {
                return Result.Fail<Socket>($"{ex.Message} ({ex.GetType()})");
            }
            catch (InvalidOperationException ex)
            {
                return Result.Fail<Socket>($"{ex.Message} ({ex.GetType()})");
            }

            return Result.Ok(transferSocket);
        }

        public static async Task<Socket> AcceptAsync(Socket socket)
        {
            var acceptTask = Task<Socket>.Factory.FromAsync(socket.BeginAccept, socket.EndAccept, null);
            return await acceptTask.ConfigureAwait(false);
        }

        public static async Task<Result<int>> ReceiveWithTimeoutAsync(
            this Socket socket,
            byte[] buffer,
            int offset,
            int size,
            SocketFlags socketFlags,
            int timeoutMs )
        {
            int bytesReceived;
            try
            {
                var asyncResult = socket.BeginReceive(buffer, offset, size, socketFlags, null, null);
                var receiveTask = Task<int>.Factory.FromAsync(asyncResult, _ => socket.EndReceive(asyncResult));

                if (receiveTask == await Task.WhenAny(receiveTask, Task.Delay(timeoutMs)).ConfigureAwait(false))
                {
                    bytesReceived = await receiveTask.ConfigureAwait(false);
                }
                else
                {
                    throw new TimeoutException();
                }
            }
            catch (SocketException ex)
            {
                return Result.Fail<int>($"{ex.Message} ({ex.GetType()})");
            }
            catch (TimeoutException ex)
            {
                return Result.Fail<int>($"{ex.Message} ({ex.GetType()})");
            }

            return Result.Ok(bytesReceived);
        }

        public static async Task<int> ReceiveAsync(
            this Socket socket,
            byte[] buffer,
            int offset,
            int size,
            SocketFlags socketFlags = SocketFlags.None )
        {

            var asyncResult = socket.BeginReceive(buffer, offset, size, socketFlags, null, null);
            return await Task<int>.Factory.FromAsync(asyncResult, _ => socket.EndReceive(asyncResult));
        }

        public static async Task<int> ReceiveAsync(
            this Socket socket,
            byte[] buffer,
            SocketFlags socketFlags = SocketFlags.None )
        {
            return await ReceiveAsync(socket, buffer, 0, buffer.Length, socketFlags);
        }


        public static async Task<string> ReceiveStringAsync( this Socket socket, byte[] buffer, Encoding? encoding = null )
        {
            var size = await socket.ReceiveAsync(buffer);
            encoding ??= Encoding.Default;

            return encoding.GetString(buffer, 0, size);
        }

        public static async Task<Result<int>> SendWithTimeoutAsync(
            this Socket socket,
            byte[] buffer,
            int offset,
            int size,
            SocketFlags socketFlags,
            int timeoutMs )
        {
            int bytesSent;
            try
            {
                var asyncResult = socket.BeginSend(buffer, offset, size, socketFlags, null, null);
                var sendBytesTask = Task<int>.Factory.FromAsync(asyncResult, _ => socket.EndSend(asyncResult));

                if (sendBytesTask != await Task.WhenAny(sendBytesTask, Task.Delay(timeoutMs)).ConfigureAwait(false))
                {
                    throw new TimeoutException();
                }

                bytesSent = await sendBytesTask;
            }
            catch (SocketException ex)
            {
                return Result.Fail<int>($"{ex.Message} ({ex.GetType()})");
            }
            catch (TimeoutException ex)
            {
                return Result.Fail<int>($"{ex.Message} ({ex.GetType()})");
            }

            return Result.Ok(bytesSent);
        }

    }

    public class TcpUtil
    { 
        public static bool IsPortFree( int port )
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == port)
                {
                    return false;
                }
            }

            return true;
        }
    }


    [Serializable]
    public class SocketBindFail : Exception
    {
        public SocketBindFail() { }
        public SocketBindFail( string message ) : base(message) { }
        public SocketBindFail( string message, Exception inner ) : base(message, inner) { }
        protected SocketBindFail(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context ) : base(info, context) { }
    }


    [Serializable]
    public class SocketListenAndAcceptFail : Exception
    {
        public SocketListenAndAcceptFail() { }
        public SocketListenAndAcceptFail( string message ) : base(message) { }
        public SocketListenAndAcceptFail( string message, Exception inner ) : base(message, inner) { }
        protected SocketListenAndAcceptFail(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context ) : base(info, context) { }
    }


    [Serializable]
    public class SocketSendStringError : Exception
    {
        public SocketSendStringError() { }
        public SocketSendStringError( string message ) : base(message) { }
        public SocketSendStringError( string message, Exception inner ) : base(message, inner) { }
        protected SocketSendStringError(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context ) : base(info, context) { }
    }
}
