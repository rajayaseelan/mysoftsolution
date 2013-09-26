using Castle.Windsor;
using System;

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
        /// <param name="service"></param>
        /// <returns></returns>
        object Resolve(Type service);

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
        public ServiceContainer(IWindsorContainer container)
        {
            this.container = container;
        }

        #region IContainer 成员

        /// <summary>
        /// 解析服务
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public object Resolve(Type service)
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
