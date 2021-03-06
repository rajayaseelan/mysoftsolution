﻿using System;
using System.Threading;
using MySoft.IoC.Messages;
using MySoft.Logger;
using MySoft.Threading;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 队列服务
    /// </summary>
    public class AsyncService : IService
    {
        private IWorkItemsGroup smart;
        private ILog logger;
        private IService service;
        private TimeSpan elapsedTime;

        /// <summary>
        /// 实例化QueueService
        /// </summary>
        /// <param name="smart"></param>
        /// <param name="logger"></param>
        /// <param name="service"></param>
        /// <param name="elapsedTime"></param>
        public AsyncService(IWorkItemsGroup group, ILog logger, IService service, TimeSpan elapsedTime)
        {
            this.smart = group;
            this.logger = logger;
            this.service = service;
            this.elapsedTime = elapsedTime;
        }

        /// <summary>
        /// 异步调用方法
        /// </summary>
        /// <param name="reqMsg"></param>
        public ResponseMessage CallService(RequestMessage reqMsg)
        {
            var context = OperationContext.Current;

            //实例化异步调用器
            var worker = smart.QueueWorkItem<OperationContext, RequestMessage, ResponseMessage>
                                            (GetResponse, context, reqMsg);

            //等待响应
            ResponseMessage resMsg = null;

            try
            {
                resMsg = worker.GetResult(elapsedTime, true);
            }
            catch (Exception ex)
            {
                //结束当前线程
                if (!worker.IsCompleted) worker.Cancel(true);

                var body = string.Format("Call service ({0}, {1}) timeout ({2}) ms. Error: {4}\r\nParameters => {3}"
                    , reqMsg.ServiceName, reqMsg.MethodName, (int)elapsedTime.TotalMilliseconds, reqMsg.Parameters.ToString(), ex.Message);

                //获取异常
                var error = IoCHelper.GetException(OperationContext.Current, reqMsg, body);

                //将异常信息写出
                logger.Write(error);

                //处理异常
                resMsg = new ResponseMessage
                {
                    TransactionId = reqMsg.TransactionId,
                    ReturnType = reqMsg.ReturnType,
                    ServiceName = reqMsg.ServiceName,
                    MethodName = reqMsg.MethodName,
                    Parameters = reqMsg.Parameters,
                    Error = error
                };
            }

            //返回响应的消息
            return resMsg;
        }

        /// <summary>
        /// 响应请求
        /// </summary>
        private ResponseMessage GetResponse(OperationContext context, RequestMessage reqMsg)
        {
            try
            {
                //设置上下文
                OperationContext.Current = context;

                //调用方法
                return service.CallService(reqMsg);
            }
            catch (Exception ex)
            {
                //出现异常时返回null
                return null;
            }
            finally
            {
                //初始化上下文
                OperationContext.Current = null;
            }
        }

        #region IService 成员

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName
        {
            get { return service.ServiceName; }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            service.Dispose();
        }

        #endregion
    }
}
