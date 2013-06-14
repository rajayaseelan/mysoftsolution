using System;
using System.Diagnostics;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
using MySoft.IoC.Services;
using MySoft.Logger;

namespace MySoft.IoC
{
    /// <summary>
    /// 调用者
    /// </summary>
    internal class InvokeCaller : IDisposable
    {
        private CastleFactoryConfiguration config;
        private IContainer container;
        private IService service;
        private AsyncCaller caller;
        private IServiceCall call;
        private ILog logger;
        private string hostName;
        private string ipAddress;

        /// <summary>
        /// 实例化InvokeCaller
        /// </summary>
        /// <param name="config"></param>
        /// <param name="container"></param>
        /// <param name="service"></param>
        /// <param name="caller"></param>
        /// <param name="call"></param>
        /// <param name="logger"></param>
        public InvokeCaller(CastleFactoryConfiguration config, IContainer container, IService service, AsyncCaller caller, IServiceCall call, ILog logger)
        {
            this.config = config;
            this.container = container;
            this.service = service;
            this.call = call;
            this.caller = caller;
            this.logger = logger;

            this.hostName = DnsHelper.GetHostName();
            this.ipAddress = DnsHelper.GetIPAddress();
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public InvokeData InvokeResponse(InvokeMessage message)
        {
            #region 设置请求信息

            var reqMsg = new RequestMessage
            {
                InvokeMethod = true,
                AppVersion = ServiceConfig.CURRENT_FRAMEWORK_VERSION,       //版本号
                AppName = config.AppName,                                   //应用名称
                AppPath = AppDomain.CurrentDomain.BaseDirectory,            //应用路径
                HostName = hostName,                                        //客户端名称
                IPAddress = ipAddress,                                      //客户端IP地址
                ServiceName = message.ServiceName,                          //服务名称
                MethodName = message.MethodName,                            //方法名称
                EnableCache = config.EnableCache,                           //是否缓存
                CacheTime = message.CacheTime,                              //缓存时间
                TransactionId = Guid.NewGuid(),                             //Json字符串
                RespType = ResponseType.Json                                //数据类型
            };

            #endregion

            //给参数赋值
            reqMsg.Parameters["InvokeParameter"] = message.Parameters;

            //定义响应的消息
            var resMsg = GetResponse(reqMsg);

            //如果有异常，向外抛出
            if (resMsg.IsError)
            {
                if (logger != null) logger.WriteError(resMsg.Error);

                throw resMsg.Error;
            }

            return resMsg.Value as InvokeData;
        }

        /// <summary>
        /// 获取响应信息
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetResponse(RequestMessage reqMsg)
        {
            //开始一个记时器
            var watch = Stopwatch.StartNew();

            try
            {
                //写日志开始
                call.BeginCall(reqMsg);

                //获取上下文
                var context = GetOperationContext(reqMsg);

                //异步调用服务
                var item = caller.SyncRun(service, context, reqMsg);

                var resMsg = item.Message;

                if (item.Message == null)
                {
                    //反序列化对象
                    resMsg = IoCHelper.DeserializeObject(item.Buffer);
                }

                //设置耗时时间
                resMsg.ElapsedTime = Math.Min(resMsg.ElapsedTime, watch.ElapsedMilliseconds);

                //写日志结束
                call.EndCall(reqMsg, resMsg, watch.ElapsedMilliseconds);

                return resMsg;
            }
            finally
            {
                if (watch.IsRunning)
                {
                    watch.Stop();
                }
            }
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
                AppVersion = reqMsg.AppVersion,
                AppPath = reqMsg.AppPath,
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

        #region IDisposable 成员

        /// <summary>
        /// dispose resource.
        /// </summary>
        public void Dispose()
        {
            this.config = null;
            this.container = null;
            this.service = null;
            this.caller = null;
            this.call = null;
            this.logger = null;
        }

        #endregion
    }
}
