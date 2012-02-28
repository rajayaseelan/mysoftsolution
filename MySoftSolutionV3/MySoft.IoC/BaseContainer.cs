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
    }

    /// <summary>
    /// 容器基类
    /// </summary>
    public abstract class BaseContainer
    {
        /// <summary>
        /// 容器对象
        /// </summary>
        public IContainer Container
        {
            get;
            internal set;
        }
    }
}
