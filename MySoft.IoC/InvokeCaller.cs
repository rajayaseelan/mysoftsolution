using System;
using MySoft.IoC.Messages;
using MySoft.IoC.Services;
using MySoft.Cache;

namespace MySoft.IoC
{
    /// <summary>
    /// 调用者
    /// </summary>
    public class InvokeCaller
    {
        private string appName;
        private IService service;
        private string hostName;
        private string ipAddress;
        private IServiceContainer container;
        private AsyncCaller asyncCaller;

        /// <summary>
        /// 实例化InvokeCaller
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="container"></param>
        /// <param name="service"></param>
        /// <param name="timeout"></param>
        public InvokeCaller(string appName, IServiceContainer container, IService service, TimeSpan timeout)
        {
            this.appName = appName;
            this.service = service;
            this.container = container;

            //实例化异步服务
            this.asyncCaller = new AsyncCaller(service, timeout, false);

            this.hostName = DnsHelper.GetHostName();
            this.ipAddress = DnsHelper.GetIPAddress();
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public InvokeData CallMethod(InvokeMessage message)
        {
            #region 设置请求信息

            var reqMsg = new RequestMessage
            {
                InvokeMethod = true,
                AppName = appName,                                      //应用名称
                HostName = hostName,                                    //客户端名称
                IPAddress = ipAddress,                                  //客户端IP地址
                ServiceName = message.ServiceName,                      //服务名称
                MethodName = message.MethodName,                        //方法名称
                ReturnType = typeof(string),                            //返回类型
                TransactionId = Guid.NewGuid(),
                RespType = ResponseType.Json                        //数据类型
            };

            #endregion

            //给参数赋值
            reqMsg.Parameters["InvokeParameter"] = message.Parameters;

            //获取上下文
            var context = GetOperationContext(reqMsg);

            //异步调用服务
            var resMsg = asyncCaller.Run(context, reqMsg);

            //如果有异常，向外抛出
            if (resMsg.IsError) throw resMsg.Error;

            //返回数据
            return resMsg.Value as InvokeData;
        }

        /// <summary>
        /// 获取上下文对象
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private OperationContext GetOperationContext(RequestMessage reqMsg)
        {
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

            return new OperationContext
            {
                Container = container,
                Caller = caller
            };
        }
    }
}
