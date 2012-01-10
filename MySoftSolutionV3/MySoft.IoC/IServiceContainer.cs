using System;
using System.Collections;
using Castle.MicroKernel;
using MySoft.Logger;

namespace MySoft.IoC
{
    /// <summary>
    /// The service container interface.
    /// </summary>
    public interface IServiceContainer : IService, IDisposable, ILog, ILogable, IErrorLogable
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
        void RegisterComponent(string key, object instance);
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
        /// 获取约束的接口
        /// </summary>
        /// <returns></returns>
        Type[] GetInterfaces<ContractType>();
        /// <summary>
        /// 是否包含服务
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        bool Contains<ContractType>(string serviceName);
    }
}
