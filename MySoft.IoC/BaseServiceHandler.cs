using MySoft.IoC.Configuration;
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
        private CastleFactoryConfiguration config;
        private IServiceContainer container;
        private IServiceCall call;
        private IService service;

        /// <summary>
        /// 实例化BaseServiceHandler
        /// </summary>
        /// <param name="config"></param>
        /// <param name="container"></param>
        /// <param name="call"></param>
        /// <param name="service"></param>
        public BaseServiceHandler(CastleFactoryConfiguration config, IServiceContainer container, IServiceCall call, IService service)
        {
            this.config = config;
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
            try
            {
                long elapsedTime = -1;

                //定义响应的消息
                var resMsg = GetResponse(reqMsg, ref elapsedTime);

                //抛出异常
                if (resMsg.IsError) throw resMsg.Error;

                if (resMsg is ResponseBuffer)
                {
                    //反序列化对象
                    var buffer = (resMsg as ResponseBuffer).Buffer;

                    resMsg = new ResponseMessage
                    {
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

                return resMsg;
            }
            catch (Exception ex)
            {
                //写错误日志
                WriteError(ex);

                throw;
            }
        }

        /// <summary>
        /// 写错误日志
        /// </summary>
        /// <param name="ex"></param>
        private void WriteError(Exception ex)
        {
            if (container != null)
            {
                container.WriteError(ex);
            }
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
                //写日志开始
                call.BeginCall(reqMsg);

                //获取响应
                var resMsg = GetResponse(reqMsg);

                elapsedTime = watch.ElapsedMilliseconds;

                //写日志结束
                call.EndCall(reqMsg, resMsg, elapsedTime);

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
        /// 获取响应信息
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetResponse(RequestMessage reqMsg)
        {
            //获取上下文
            var context = GetOperationContext(reqMsg);

            //设置上下文
            OperationContext.Current = context;

            try
            {
                //同步调用服务
                return service.CallService(reqMsg);
            }
            finally
            {
                OperationContext.Current = null;
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
