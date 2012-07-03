using System;
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

            //Worker对象
            IWorkItemResult<ResponseMessage> worker = null;

            //等待响应
            ResponseMessage resMsg = null;

            try
            {
                //创建异步调用器
                worker = smart.QueueWorkItem<OperationContext, RequestMessage, ResponseMessage>(GetResponse, context, reqMsg);

                //获取结果
                resMsg = worker.GetResult(elapsedTime, true);
            }
            catch (Exception ex)
            {
                var body = string.Format("Call service ({0}, {1}) timeout ({2}) ms. error: {4}\r\nParameters => {3}"
                    , reqMsg.ServiceName, reqMsg.MethodName, (int)elapsedTime.TotalMilliseconds, reqMsg.Parameters.ToString(), ex.Message);

                //获取异常
                var error = IoCHelper.GetException(OperationContext.Current, reqMsg, body);

                //将异常信息写出
                logger.WriteError(error);

                //处理异常
                resMsg = IoCHelper.GetResponse(reqMsg, error);
            }
            finally
            {
                //将worker对象置null
                if (worker != null)
                {
                    //结束当前线程
                    if (!worker.IsCompleted)
                    {
                        worker.Cancel(true);
                    }

                    worker = null;
                }
            }

            //返回响应的消息
            return resMsg;
        }

        /// <summary>
        /// 响应请求
        /// </summary>
        private ResponseMessage GetResponse(OperationContext context, RequestMessage reqMsg)
        {
            //设置上下文
            OperationContext.Current = context;

            try
            {
                //调用方法
                return service.CallService(reqMsg);
            }
            catch (ThreadAbortException)
            {
                //线程异常不处理
                return null;
            }
            catch (Exception ex)
            {
                logger.WriteError(ex);

                //出现异常返回null
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
