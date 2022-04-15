using Say32.DB.Core;
using Say32.DB.Core.Serialization;
using Say32.DB.Core.TypeSupport;
using System;
using System.Threading.Tasks;

namespace Say32.DB.Store.IO
{

    public class IoContext
    {
        public const int DEFATULT_IO_TIMEOUT_IN_SEC = 10;

        //external
        public int IoTimeoutinSec { get; set; } = DEFATULT_IO_TIMEOUT_IN_SEC;

        //permanent
        public readonly SayDB Database;
        internal DbCore DbCore => Database.Core;

        internal TypeManager TypeManager => DbCore.TypeManager;

        public IDbSerializer Serializer => DbWorkSpace.Serializers;

        // per IO instance
        public readonly IoCache Cache = new IoCache();

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dbCore">Database this is a helper for</param>
        /// <param name="serializer">The serializer</param>
        /// <param name="logManager">The logger</param>
        /// <param name="typeResolver"></param>
        /// <param name="typeIndexer"></param>
        /// <param name="platform"></param>
        internal IoContext(SayDB db)
        {
            Database = db;
        }

        private bool _alreadWaited = false;

        public async Task AwaitAllTasks()
        {
            if (_alreadWaited)
                return;

            _alreadWaited = true;

            bool waitTaskResult = await Cache.WaitTasksAsync(TimeSpan.FromSeconds(IoTimeoutinSec));
            DbError.Assert<TimeoutException>(waitTaskResult, $"DB IO timeout in {IoTimeoutinSec} seconds");
        }

        public async Task<Type> GetType(IoHeader header)
        {
            Type typeResolved = null;
            if (header.HasTypeInfo)
            {
                typeResolved = await TypeManager.GetType(header.TypeCode);
            }

            DbError.Assert<SayDbTypeException>(typeResolved != null, $"{nameof(typeResolved)} can't get type from {nameof(IoHeader)} code!  type='{header.TypeCode}'");
            return typeResolved;
        }

    }


    [Serializable]
    public class SayDbTypeException : Exception
    {
        public SayDbTypeException() { }
        public SayDbTypeException(string message) : base(message) { }
        public SayDbTypeException(string message, Exception inner) : base(message, inner) { }
        protected SayDbTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


}