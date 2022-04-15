using System;
using Say32.DB.Core.Exceptions;

namespace Say32.DB.Server.FileSystem
{
    public class SayDBFileSystemException : SayDBException 
    {
        public SayDBFileSystemException(Exception ex) : base(string.Format("An exception occurred accessing the file system: {0}", ex), ex)
        {
            
        }
    }
}