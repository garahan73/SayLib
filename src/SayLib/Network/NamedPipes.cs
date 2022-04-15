using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Linq;

namespace Say32.Network
{
    public class NamedPipeFactory 
    {
        public NamedPipeFactory(string pipeName, int maxServerCount = 1)
        {
            PipeName = pipeName;
            MaxServerCount = maxServerCount;
        }

        public int MaxServerCount { get; set; } = 1;
        public string PipeName { get; }

        public async Task<NamedPipe> GetConnectedServerPipeAsync( Timeout timeout = default )
        {
            var pipe = GetServerPipe(timeout);
            await pipe.WaitConnectionAsync();
            return pipe;
        }

        public NamedPipe GetServerPipe( Timeout timeout = default ) => new ServerPipe(PipeName, MaxServerCount, timeout);

        public async Task<NamedPipe> GetConnectedClientPipeAsync( Timeout timeout = default )
        {
            var pipe = new ClientPipe(PipeName, timeout);
            await pipe.WaitConnectionAsync();
            return pipe;
        }
    }

    public abstract class NamedPipe : IDisposable
    {
        protected private StreamWriter? _writer;
        protected private StreamReader? _reader;
#pragma warning disable CS8618 // 생성자를 종료할 때 null을 허용하지 않는 필드에 null이 아닌 값을 포함해야 합니다. null 허용으로 선언해 보세요.
        protected private Task _task;
#pragma warning restore CS8618 // 생성자를 종료할 때 null을 허용하지 않는 필드에 null이 아닌 값을 포함해야 합니다. null 허용으로 선언해 보세요.

        protected abstract Task ConnectAsync( Timeout timeout );

        public async Task WaitConnectionAsync() => await _task;

        public virtual void Close()
        {
            _reader = null;
            _writer = null;
        }

        public void Dispose() => Close();

        public async Task<string> ReadLineAsync()
        {
            if (_reader == null) throw new Exception("PIPE is not ready yet.");

            return await _reader.ReadLineAsync();
        }

        public async Task<string> ReadAsync()
        {
            if (_reader == null) throw new Exception("PIPE is not ready yet.");

            var buffer = new char[255];
            var list = new List<char>();

            while(true)
            {
                var readCount = await _reader.ReadAsync(buffer, 0, buffer.Length);
                list.AddRange(buffer.Take(readCount));

                if (readCount != buffer.Length)
                    break;
            }

            return new string(list.ToArray());
        }

        public async Task WriteLineAsync( string line )
        {
            if (_writer == null) throw new Exception("PIPE is not ready yet.");

            await _writer.WriteLineAsync(line);
        }

        public async Task WriteAsync( string line )
        {
            if (_writer == null) throw new Exception("PIPE is not ready yet.");

            await _writer.WriteAsync(line);
        }

    }

    class ServerPipe : NamedPipe
    {
        private readonly NamedPipeServerStream _pipe;
        

        public ServerPipe( string pipeName, int maxServerCount, Timeout timeout )
        {
#if NET5_0 // .NET Core 3.1 does not support setting ACLs on named pipes.
            PipeSecurity security = new PipeSecurity();
            security.AddAccessRule(new PipeAccessRule($"{Environment.UserDomainName}\\{Environment.UserName}", PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow));

        NamedPipeServerStream serverPipe = NamedPipeServerStreamAcl.Create(pipeName, PipeDirection.InOut, maxServerCount, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 4096, 4096, security);
#else
            _pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, maxServerCount, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
#endif
            _task = ConnectAsync(timeout);

        }

        protected override async Task ConnectAsync( Timeout timeout)
        {
            await _pipe.WaitForConnectionAsync().WaitAsync(timeout);

            _writer = new StreamWriter(_pipe);
            _writer.AutoFlush = true;
            _reader = new StreamReader(_pipe);
        }

        

        public override void Close()
        {
            base.Close();
            _pipe?.Disconnect();
            _pipe?.Dispose();
        }
    }

    class ClientPipe : NamedPipe
    {
        private readonly NamedPipeClientStream _pipe;

        internal ClientPipe( string pipeName, Timeout timeout )
        {
            _pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            _task = ConnectAsync(timeout);
        }

        protected override async Task ConnectAsync( Timeout timeout )
        {
            if (timeout.IsEnabled)
                await _pipe.ConnectAsync(timeout);
            else
                await _pipe.ConnectAsync();

            _writer = new StreamWriter(_pipe);
            _writer.AutoFlush = true;
            _reader = new StreamReader(_pipe);
        }

        public override void Close()
        {
            base.Close();

            //_pipe?.WaitForPipeDrain();
            _pipe?.Close();
            _pipe?.Dispose();
        }
    }
}
