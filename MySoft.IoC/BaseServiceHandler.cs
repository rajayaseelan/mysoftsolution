using MySoft.IoC.Messages;
using MySoft.IoC.Services;
using System;
using System.Diagnostics;

namespace MySoft.IoC
{
    /// <summary>
    /// 基础服务处理类
    /// </summary>
    internal class BaseServiceHandler
    {
        private IServiceContainer container;
        private IServiceCall call;
        private IService service;

        /// <summary>
        /// 实例化BaseServiceHandler
        /// </summary>
        /// <param name="container"></param>
        /// <param name="call"></param>
        /// <param name="service"></param>
        public BaseServiceHandler(IServiceContainer container, IServiceCall call, IService service)
        {
            this.container = container;
            this.call = call;
            this.service = service;
        }

        /// <summary>
        /// 获取响应信息
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage CallService(RequestMessage reqMsg)
        {
            long elapsedTime = -1;

            //定义响应的消息
            var resMsg = GetResponse(reqMsg, ref elapsedTime);

            //如果有异常，向外抛出
            if (resMsg.IsError)
            {
                if (container != null) container.WriteError(resMsg.Error);

                throw resMsg.Error;
            }
            else
            {
                if (resMsg is ResponseBuffer)
                {
                    //反序列化对象
                    var buffer = (resMsg as ResponseBuffer).Buffer;

                    resMsg = new ResponseMessage
                    {
                        TransactionId = resMsg.TransactionId,
                        ServiceName = resMsg.ServiceName,
                        MethodName = resMsg.MethodName,
                        Parameters = resMsg.Parameters,
                        ElapsedTime = resMsg.ElapsedTime,
                        Error = resMsg.Error,
                        Value = IoCHelper.DeserializeObject(buffer)
                    };
                }

                //设置耗时时间
                resMsg.ElapsedTime = Math.Min(resMsg.ElapsedTime, elapsedTime);
            }

            return resMsg;
        }

        /// <summary>
        /// 获取响应信息
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="elapsedTime"></param>
        /// <returns></returns>
        private ResponseMessage GetResponse(RequestMessage reqMsg, ref long elapsedTime)
        {
            //开始一个记时器
            var watch = Stopwatch.StartNew();

            try
            {
                using (var caller = new AsyncCaller(service))
                {
                    //写日志开始
                    call.BeginCall(reqMsg);

                    //获取上下文
                    var context = GetOperationContext(reqMsg);

                    //同步调用服务
                    var resMsg = caller.SyncRun(context, reqMsg);

                    elapsedTime = watch.ElapsedMilliseconds;

                    //写日志结束
                    call.EndCall(reqMsg, resMsg, watch.ElapsedMilliseconds);

                    return resMsg;
                }
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
    }
}
