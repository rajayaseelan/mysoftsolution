using System;
using System.Collections.Generic;
using Castle.MicroKernel;
using MySoft.IoC.Communication;
using MySoft.Logger;

namespace MySoft.IoC
{
    /// <summary>
    /// The service container interface.
    /// </summary>
    public interface IServiceContainer : IContainer, ILog, ILogable, IErrorLogable, ITcpConnection
    {
        /// <summary>
        /// Gets the kernel.
        /// </summary>
        /// <value>The kernel.</value>
        IKernel Kernel { get; }
        /// <summary>
        /// Registers the component.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="instance">Type of the class.</param>
        void RegisterComponent(string key, Type serviceType, object instance);
        /// <summary>
        /// Registers the component.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="classType">Type of the class.</param>
        void RegisterComponent(string key, Type serviceType, Type classType);
        /// <summary>
        /// Registers the components.
        /// </summary>
        /// <param name="serviceKeyTypes">The service key types.</param>
        void RegisterComponents(IDictionary<Type, object> serviceKeyTypes);
        /// <summary>
        /// Get contract interface
        /// </summary>
        /// <returns></returns>
        Type[] GetServiceTypes<ContractType>();
        /// <summary>
        /// Contains service
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        bool Contains<ContractType>(string serviceName);

        /// <summary>
        /// OnWriteConnected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void SendConnected(object sender, ConnectEventArgs args);

        /// <summary>
        /// OnWriteDisconnected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void SendDisconnected(object sender, ConnectEventArgs args);
    }
}
