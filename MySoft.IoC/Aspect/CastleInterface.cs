using MySoft.IoC.Messages;
using Castle.Core;
using System.Collections.Generic;
using System;

namespace MySoft.IoC.Aspect
{
    /// <summary>
    /// Aspect调用接口
    /// </summary>
    public interface IInvocation : Castle.DynamicProxy.IInvocation
    {
        /// <summary>
        /// 参数集合信息
        /// </summary>
        ParameterCollection Parameters { get; }

        /// <summary>
        /// 操作描述信息
        /// </summary>
        string Description { get; }
    }
}

namespace MySoft.IoC
{
    /// <summary>
    /// 类型初始化时处理的实例
    /// </summary>
    public abstract class TypeInitializable : IInitializable, IStartable
    {
        private static IDictionary<Type, bool> initializeType = new Dictionary<Type, bool>();

        /// <summary>
        /// 初始化接口
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// 初始化接口
        /// </summary>
        void IInitializable.Initialize()
        {
            var type = this.GetType();

            if (!CheckInitType(type))
            {
                initializeType[type] = true;

                try
                {
                    Initialize();
                }
                catch (Exception ex)
                {
                    //TODO
                }
            }
        }

        /// <summary>
        /// 开始接口
        /// </summary>
        void IStartable.Start()
        {
            //TODO
        }

        /// <summary>
        /// 结束接口
        /// </summary>
        void IStartable.Stop()
        {
            //TODO
        }

        private bool CheckInitType(Type type)
        {
            lock (initializeType)
            {
                if (!initializeType.ContainsKey(type))
                {
                    initializeType[type] = false;
                }

                return initializeType[type];
            }
        }
    }
}
