using System;
using System.Collections;
using Castle.MicroKernel;
using MySoft.Logger;

namespace MySoft.IoC
{
    /// <summary>
    /// The service container interface.
    /// </summary>
    public interface IServiceContainer : IService, IContainer, IDisposable, ILog, ILogable, IErrorLogable
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
        void RegisterComponents(IDictionary serviceKeyTypes);
        /// <summary>
        /// Releases the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        void Release(object obj);
        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value></value>
        object this[string Key] { get; }
        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified service type.
        /// </summary>
        /// <value></value>
        object this[Type serviceType] { get; }
        /// <summary>
        /// Resolve local service
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        object Resolve(Type type);
        /// <summary>
        /// Resolve local service
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object Resolve(string key);
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
        /// Get service cache.
        /// </summary>
        IServiceCache ServiceCache { get; set; }
    }
}
