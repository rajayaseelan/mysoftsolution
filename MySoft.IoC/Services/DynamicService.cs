using System;
using System.Collections.Generic;
using System.Linq;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// The dynamic service.
    /// </summary>
    public class DynamicService : BaseService
    {
        private IContainer container;
        private Type serviceType;
        private IDictionary<string, System.Reflection.MethodInfo> methods;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicService"/> class.
        /// </summary>
        /// <param name="serviceType">Type of the service interface.</param>
        public DynamicService(IContainer container, Type serviceType)
            : base(serviceType)
        {
            this.container = container;
            this.serviceType = serviceType;

            //Get method
            this.methods = CoreHelper.GetMethodsFromType(serviceType).ToDictionary(p => p.ToString());
        }

        /// <summary>
        /// Runs the specified MSG.
        /// </summary>
        /// <param name="reqMsg">The MSG.</param>
        /// <returns>The msg.</returns>
        protected override ResponseMessage Run(RequestMessage reqMsg)
        {
            #region 获取相应的方法

            //判断方法是否存在
            if (!methods.ContainsKey(reqMsg.MethodName))
            {
                string message = string.Format("The server【{2}({3})】not find matching method ({0}, {1})."
                    , reqMsg.ServiceName, reqMsg.MethodName, DnsHelper.GetHostName(), DnsHelper.GetIPAddress());

                throw new WarningException(message);
            }

            #endregion

            var callMethod = methods[reqMsg.MethodName];

            //解析参数
            ResolveParameters(callMethod, reqMsg);

            //调用方法
            var resMsg = InvokeMethod(callMethod, reqMsg);

            //处理消息
            HandleMessage(reqMsg, resMsg);

            return resMsg;
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="callMethod"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage InvokeMethod(System.Reflection.MethodInfo callMethod, RequestMessage reqMsg)
        {
            var resMsg = new ResponseMessage
            {
                TransactionId = reqMsg.TransactionId,
                ServiceName = reqMsg.ServiceName,
                MethodName = reqMsg.MethodName
            };

            //定义对象
            object instance = null;

            try
            {
                //解析服务
                instance = container.Resolve(serviceType);

                //参数赋值
                object[] parameters = IoCHelper.CreateParameters(callMethod, reqMsg.Parameters);

                //调用对应的服务
                resMsg.Value = callMethod.FastInvoke(instance, parameters);

                //处理返回参数
                IoCHelper.SetRefParameters(callMethod, parameters, resMsg.Parameters);

                return resMsg;
            }
            finally
            {
                //释放实例
                container.Release(instance);
            }
        }

        /// <summary>
        /// 解析参数
        /// </summary>
        /// <param name="callMethod"></param>
        /// <param name="reqMsg"></param>
        private void ResolveParameters(System.Reflection.MethodInfo callMethod, RequestMessage reqMsg)
        {
            if (reqMsg.InvokeMethod)
            {
                var objValue = reqMsg.Parameters["InvokeParameter"];

                if (objValue != null)
                {
                    //解析参数
                    reqMsg.Parameters = IoCHelper.CreateParameters(callMethod, objValue.ToString());
                }
            }
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="resMsg"></param>
        private void HandleMessage(RequestMessage reqMsg, ResponseMessage resMsg)
        {
            //返回结果数据
            if (reqMsg.InvokeMethod)
            {
                resMsg.Value = new InvokeData
                {
                    Value = SerializationManager.SerializeJson(resMsg.Value),
                    Count = resMsg.Count,
                    ElapsedTime = resMsg.ElapsedTime,
                    OutParameters = resMsg.Parameters.ToString()
                };

                //清除参数集合
                resMsg.Parameters.Clear();
            }
        }
    }
}
