using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Windsor;

namespace MySoft.RESTful
{
    /// <summary>
    /// 容器对象
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        /// 解析服务
        /// </summary>
        /// <returns></returns>
        object Resolve();

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="instance"></param>
        void Release(object instance);
    }

    /// <summary>
    /// 简单容器
    /// </summary>
    public sealed class ServiceContainer : IContainer
    {
        private IWindsorContainer container;
        private Type service;
        public ServiceContainer(IWindsorContainer container, Type service)
        {
            this.container = container;
            this.service = service;
        }

        #region IContainer 成员

        /// <summary>
        /// 解析服务
        /// </summary>
        /// <returns></returns>
        public object Resolve()
        {
            return container.Resolve(service);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="instance"></param>
        public void Release(object instance)
        {
            if (instance != null)
                container.Release(instance);
        }

        #endregion
    }
}
