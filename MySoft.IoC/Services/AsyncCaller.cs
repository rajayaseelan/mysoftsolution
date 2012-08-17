using System;
using System.Collections;
using System.Collections.Generic;
using MySoft.IoC.Messages;
using MySoft.Logger;
using MySoft.Threading;
using System.Threading;

namespace MySoft.IoC.Services
{
    internal class AsyncCaller
    {
        private ILog logger;
        private IService service;
        private IDictionary<string, Queue<WaitResult>> results;

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="service"></param>
        public AsyncCaller(ILog logger, IService service)
        {
            this.logger = logger;
            this.service = service;
            this.results = new Dictionary<string, Queue<WaitResult>>();
        }

        /// <summary>
        /// 同步调用
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage SyncCall(OperationContext context, RequestMessage reqMsg)
        {
            return GetResponse(service, context, reqMsg);
        }

        /// <summary>
        /// 异步调用服务
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage AsyncCall(OperationContext context, RequestMessage reqMsg)
        {
            using (var waitResult = new WaitResult(reqMsg))
            {
                lock (results)
                {
                    var caller = context.Caller;
                    var callKey = string.Format("Caller_{0}${1}${2}", caller.ServiceName, caller.MethodName, caller.Parameters);

                    if (!results.ContainsKey(callKey))
                    {
                        results[callKey] = new Queue<WaitResult>();
                        ManagedThreadPool.QueueUserWorkItem(GetResponse, new ArrayList { callKey, waitResult, context, reqMsg });
                    }
                    else
                    {
                        results[callKey].Enqueue(waitResult);
                    }
                }

                //定义响应消息
                ResponseMessage resMsg = null;

                //计算超时时间
                var elapsedTime = TimeSpan.FromSeconds(ServiceConfig.DEFAULT_CALL_TIMEOUT);

                if (!waitResult.Wait(elapsedTime))
                {
                    var body = string.Format("Remote client【{0}】call service ({1},{2}) timeout {4} ms.\r\nParameters => {3}",
                        reqMsg.Message, reqMsg.ServiceName, reqMsg.MethodName, reqMsg.Parameters.ToString(), (int)elapsedTime.TotalMilliseconds);

                    //获取异常
                    var error = IoCHelper.GetException(context, reqMsg, new TimeoutException(body));

                    logger.WriteError(error);

                    var title = string.Format("Call remote service ({0},{1}) timeout {2} ms.",
                                reqMsg.ServiceName, reqMsg.MethodName, (int)elapsedTime.TotalMilliseconds);

                    //处理异常
                    resMsg = IoCHelper.GetResponse(reqMsg, new TimeoutException(title));
                }
                else
                {
                    resMsg = waitResult.Message;
                }

                return resMsg;
            }
        }

        /// <summary>
        /// 设置响应
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="resMsg"></param>
        private void SetResponse(string callKey, ResponseMessage resMsg)
        {
            lock (results)
            {
                if (results.ContainsKey(callKey))
                {
                    var queue = results[callKey];

                    if (queue.Count > 0)
                    {
                        //输出队列信息
                        logger.WriteLog(string.Format("【Queues: {0}】{1}, {2}.", queue.Count, resMsg.ServiceName, resMsg.MethodName), LogType.Normal);

                        while (queue.Count > 0)
                        {
                            var item = queue.Dequeue();
                            item.Set(resMsg);
                        }
                    }

                    //移除指定的Key
                    results.Remove(callKey);
                }
            }
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="state"></param>
        private void GetResponse(object state)
        {
            var arr = state as ArrayList;

            var callKey = arr[0] as string;
            var wr = arr[1] as WaitResult;
            var context = arr[2] as OperationContext;
            var reqMsg = arr[3] as RequestMessage;

            //从缓存中获取数据
            var resMsg = CacheHelper.Get<ResponseMessage>(callKey);

            if (resMsg == null)
            {
                //获取响应的消息
                resMsg = GetResponse(service, context, reqMsg);

                var timeSpan = TimeSpan.FromSeconds(ServiceConfig.DEFAULT_SERVER_TIMEOUT);
                if (!resMsg.IsError && resMsg.Count > 0 && resMsg.ElapsedTime > timeSpan.TotalMilliseconds)
                {
                    //如果符合条件，则缓存30秒 【自动缓存功能】
                    CacheHelper.Insert(callKey, resMsg, ServiceConfig.DEFAULT_CALL_TIMEOUT);
                }
            }
            else
            {
                resMsg.TransactionId = reqMsg.TransactionId;
                resMsg.ElapsedTime = 0;
                if (resMsg.Value is InvokeData)
                {
                    (resMsg.Value as InvokeData).ElapsedTime = 0;
                }
            }

            wr.Set(resMsg);

            //设置响应
            SetResponse(callKey, resMsg);
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetResponse(IService service, OperationContext context, RequestMessage reqMsg)
        {
            //定义响应的消息
            ResponseMessage resMsg = null;

            try
            {
                OperationContext.Current = context;

                //响应结果，清理资源
                resMsg = service.CallService(reqMsg);
            }
            catch (Exception ex)
            {
                //将异常信息写出
                logger.WriteError(ex);

                //处理异常
                resMsg = IoCHelper.GetResponse(reqMsg, ex);
            }
            finally
            {
                OperationContext.Current = null;
            }

            return resMsg;
        }
    }
}
