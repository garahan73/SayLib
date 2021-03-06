
using System;
using System.Collections;
using System.Collections.Generic;
using Nito.AsyncEx;
using Say32.DB.Core;

namespace Say32.DB.Server.FileSystem
{
    internal static class PathLock
    {
        private static readonly Dictionary<int, AsyncLock> _pathLocks = new Dictionary<int, AsyncLock>();

        public static AsyncLock GetLock( string path )
        {
            var hash = path.GetHashCode();

            lock ( _pathLocks )
            {
                AsyncLock aLock = null;

                if ( _pathLocks.TryGetValue( hash, out aLock ) == false )
                {
                    aLock = _pathLocks[ hash ] = new AsyncLock();
                }

                return aLock;
            }
        }
    }
}
