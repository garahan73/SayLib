using System;

namespace Say32.DB.Core.Database
{
    public abstract class BaseDbByteInterceptor : ISayDBByteInterceptor
    {
        virtual public byte[] Save(byte[] sourceStream)
        {
            throw new NotImplementedException();
        }

        virtual public byte[] Load(byte[] sourceStream)
        {
            throw new NotImplementedException();
        }
    }

}
