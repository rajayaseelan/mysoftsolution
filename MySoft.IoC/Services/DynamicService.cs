using System;
using System.Collections;
using MySoft.IoC.Aspect;
using MySoft.IoC.Messages;
using System.Collections.Generic;
using System.Linq;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// The dynamic service.
    /// </summary>
    public class DynamicService : BaseService
    {
        private IServiceContainer container;
        private Type serviceType;
        private IDictionary<string, System.Reflection.MethodInfo> methods;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicService"/> class.
        /// </summary>
        /// <param name="serviceType">Type of the service interface.</param>
        public DynamicService(IServiceContainer container, Type serviceType)
            : base(container, serviceType)
        {
            this.container = container;
            this.serviceType = serviceType;
            this.methods = CoreHelper.GetMethodsFromType(serviceType).ToDictionary(p => p.ToString());
        }

        /// <summary>
        /// Runs the specified MSG.
        /// </summary>
        /// <param name="reqMsg">The MSG.</param>
        /// <returns>The msg.</returns>
        protected override ResponseMessage Run(RequestMessage reqMsg)
        {
            var resMsg = IoCHelper.GetResponse(reqMsg);

            #region 获取相应的方法

            //判断方法是否存在
            if (!methods.ContainsKey(reqMsg.MethodName))
            {
                string message = string.Format("The server【{2}({3})】not find matching method. ({0},{1})."
                    , reqMsg.ServiceName, reqMsg.MethodName, DnsHelper.GetHostName(), DnsHelper.GetIPAddress());

                resMsg.Error = new WarningException(message);
                return resMsg;
            }

            #endregion

            //容器实例对象
            object instance = null;

            try
            {
                //定义Method
                var method = methods[reqMsg.MethodName];

                //解析服务
                instance = container.Resolve(serviceType);

                //返回拦截服务
                var service = AspectFactory.CreateProxyService(serviceType, instance);

                if (reqMsg.InvokeMethod)
                {
                    var objValue = reqMsg.Parameters["InvokeParameter"];
                    var jsonString = (objValue == null ? string.Empty : objValue.ToString());

                    //解析参数
                    reqMsg.Parameters = IoCHelper.CreateParameters(method, jsonString);
                }

                //参数赋值
                object[] parameters = IoCHelper.CreateParameterValues(method, reqMsg.Parameters);

                //调用对应的服务
                var value = DynamicCalls.GetMethodInvoker(method).Invoke(service, parameters);

                //处理返回参数
                IoCHelper.SetRefParameters(method, resMsg.Parameters, parameters);

                //返回结果数据
                if (reqMsg.InvokeMethod)
                {
                    resMsg.Value = new InvokeData
                    {
                        Value = SerializationManager.SerializeJson(value),
                        Count = resMsg.Count,
                        OutParameters = resMsg.Parameters.ToString()
                    };

                    //清除参数集合
                    resMsg.Parameters.Clear();
                }
                else
                {
                    resMsg.Value = value;
                }
            }
            catch (Exception ex)
            {
                resMsg.Value = null;

                //捕获全局错误
                resMsg.Error = ex;
            }
            finally
            {
                //释放资源
                container.Release(instance);
            }

            return resMsg;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public override void Dispose()
        {
            methods.Clear();
        }
    }
}
