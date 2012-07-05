using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services.Tasks
{
    /// <summary>
    /// 调用参数
    /// </summary>
    internal class AsyncCallerArgs
    {
        /// <summary>
        /// Get request message
        /// </summary>
        public RequestMessage Request { get; set; }

        /// <summary>
        /// Get operation context
        /// </summary>
        public OperationContext Context { get; set; }
    }
}
