using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC
{
    /// <summary>
    /// 容器接口
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        /// Releases the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        void Release(object obj);
        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
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
        /// Resolve local service
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        TService Resolve<TService>();

        /// <summary>
        /// Resolve local service
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        TService Resolve<TService>(string key);

        /// <summary>
        /// Register ocal service
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="classType"></param>
        void Register(Type serviceType, Type classType);

        /// <summary>
        /// Register ocal service
        /// </summary>
        /// <param name="key"></param>
        /// <param name="serviceType"></param>
        /// <param name="instance"></param>
        void Register(string key, Type serviceType, Type classType);

        /// <summary>
        /// Register ocal service
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="classType"></param>
        void Register(Type serviceType, object instance);

        /// <summary>
        /// Register ocal service
        /// </summary>
        /// <param name="key"></param>
        /// <param name="serviceType"></param>
        /// <param name="instance"></param>
        void Register(string key, Type serviceType, object instance);
    }
}
