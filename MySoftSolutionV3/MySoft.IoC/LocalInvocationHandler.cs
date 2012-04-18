using System;
using System.Collections;
using System.Collections.Generic;
using MySoft.IoC.Aspect;
using MySoft.IoC.Cache;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 本地拦截器
    /// </summary>
    public class LocalInvocationHandler : ServiceInvocationHandler
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="ServiceInvocationHandler"/> class.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="container"></param>
        /// <param name="service"></param>
        /// <param name="serviceType"></param>
        /// <param name="cache"></param>
        public LocalInvocationHandler(CastleFactoryConfiguration config, IServiceContainer container, IService service, Type serviceType, IServiceCache cache, IServiceLog logger)
            : base(config, container, service, serviceType, cache, logger)
        {
            //TO DO
        }

        /// <summary>
        /// 重载调用服务
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        protected override ResponseMessage CallService(RequestMessage reqMsg)
        {
            var caller = CreateCaller(reqMsg);

            //上下文
            SetOperationContext(caller);

            //调用基类方法
            return base.CallService(reqMsg);
        }

        /// <summary>
        /// 创建AppCaller
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private AppCaller CreateCaller(RequestMessage reqMsg)
        {
            //创建AppCaller对象
            var caller = new AppCaller
            {
                AppPath = AppDomain.CurrentDomain.BaseDirectory,
                AppName = reqMsg.AppName,
                IPAddress = reqMsg.IPAddress,
                HostName = reqMsg.HostName,
                ServiceName = reqMsg.ServiceName,
                MethodName = reqMsg.MethodName,
                Parameters = reqMsg.Parameters.ToString(),
                CallTime = DateTime.Now
            };

            return caller;
        }

        /// <summary>
        /// 设置上下文
        /// </summary>
        /// <param name="caller"></param>
        private void SetOperationContext(AppCaller caller)
        {
            //初始化上下文
            OperationContext.Current = new OperationContext
            {
                Container = container,
                Caller = caller
            };
        }
    }
}
