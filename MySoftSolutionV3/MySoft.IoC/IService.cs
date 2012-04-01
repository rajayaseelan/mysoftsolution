using MySoft.IoC.Messages;
using System;
using MySoft.Communication.Scs.Server;
using MySoft.Communication.Scs.Communication.Messages;
using MySoft.IoC.Services;

namespace MySoft.IoC
{
    /// <summary>
    /// interface of all services.
    /// </summary>
    public interface IService : IDisposable
    {
        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <value>The name of the service.</value>
        string ServiceName { get; }
        /// <summary>
        /// Calls the service.
        /// </summary>
        /// <param name="reqMsg">The MSG.</param>
        /// <returns>The result.</returns>
        ResponseMessage CallService(RequestMessage reqMsg);
    }
}
