using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// interface of all services.
    /// </summary>
    public interface IService
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
