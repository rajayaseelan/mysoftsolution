using System;
using System.Collections;
using MySoft.IoC.Aspect;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// The dynamic service.
    /// </summary>
    public class DynamicService : BaseService
    {
        private static Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
        private IServiceContainer container;
        private Type serviceType;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicService"/> class.
        /// </summary>
        /// <param name="serviceType">Type of the service interface.</param>
        public DynamicService(IServiceContainer container, Type serviceType)
            : base(container, serviceType)
        {
            this.container = container;
            this.serviceType = serviceType;
        }

        /// <summary>
        /// Runs the specified MSG.
        /// </summary>
        /// <param name="reqMsg">The MSG.</param>
        /// <returns>The msg.</returns>
        protected override ResponseMessage Run(RequestMessage reqMsg)
        {
            var resMsg = new ResponseMessage
            {
                TransactionId = reqMsg.TransactionId,
                ReturnType = reqMsg.ReturnType,
                ServiceName = reqMsg.ServiceName,
                MethodName = reqMsg.MethodName
            };

            #region 获取相应的方法

            var methodKey = string.Format("{0}${1}", reqMsg.ServiceName, reqMsg.MethodName);
            if (!hashtable.ContainsKey(methodKey))
            {
                var m = CoreHelper.GetMethodFromType(serviceType, reqMsg.MethodName);
                if (m == null)
                {
                    string message = string.Format("The server【{2}({3})】not find matching method. ({0},{1})."
                        , reqMsg.ServiceName, reqMsg.MethodName, DnsHelper.GetHostName(), DnsHelper.GetIPAddress());

                    resMsg.Error = new WarningException(message);
                    return resMsg;
                }

                hashtable[methodKey] = m;
            }

            #endregion

            //容器实例对象
            object instance = null;

            try
            {
                //定义Method
                var method = hashtable[methodKey] as System.Reflection.MethodInfo;

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
                resMsg.Value = DynamicCalls.GetMethodInvoker(method).Invoke(service, parameters);

                //处理返回参数
                IoCHelper.SetRefParameters(method, resMsg.Parameters, parameters);

                //返回结果数据
                if (reqMsg.InvokeMethod)
                {
                    resMsg.Value = new InvokeData
                    {
                        Value = SerializationManager.SerializeJson(resMsg.Value),
                        Count = resMsg.Count,
                        OutParameters = resMsg.Parameters.ToString()
                    };

                    //清除参数集合
                    resMsg.Parameters.Clear();
                }
            }
            catch (Exception ex)
            {
                //捕获全局错误
                resMsg.Error = ex;
            }
            finally
            {
                //释放资源
                container.Release(instance);

                instance = null;
            }

            return resMsg;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public override void Dispose()
        {
            hashtable.Clear();
        }
    }
}
