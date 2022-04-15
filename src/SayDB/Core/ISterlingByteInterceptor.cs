using System;

namespace Say32.DB.Core
{
    /// <summary>
    /// Byte Interceptor interface
    /// </summary>
    public interface ISayDBByteInterceptor
    {
        byte[] Save(byte[] sourceStream);
        byte[] Load(byte[] sourceStream);
    }
}
