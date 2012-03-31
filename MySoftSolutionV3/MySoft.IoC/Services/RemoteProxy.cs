using System;
using System.Collections;
using System.Threading;
using MySoft.IoC.Messages;
using MySoft.Logger;
using System.Collections.Generic;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 服务代理
    /// </summary>
    public class RemoteProxy : IService
    {
        protected ILog logger;
        protected ServerNode node;
        protected ServiceRequestPool reqPool;
        private WaitResultCollection hashtable;

        public RemoteProxy(ServerNode node, ILog logger)
        {
            this.node = node;
            this.logger = logger;
            this.hashtable = new WaitResultCollection();

            InitRequest();
        }

        /// <summary>
        /// 初始化请求
        /// </summary>
        protected virtual void InitRequest()
        {
            this.reqPool = new ServiceRequestPool(node.MaxPool);

            lock (this.reqPool)
            {
                //服务请求池化
                for (int i = 0; i < node.MaxPool; i++)
                {
                    var reqService = new ServiceRequest(node, logger, true);
                    reqService.OnCallback += reqService_OnCallback;
                    reqService.OnError += reqService_OnError;

                    this.reqPool.Push(reqService);
                }
            }
        }

        void reqService_OnError(object sender, ErrorMessageEventArgs e)
        {
            QueueError(e.Request, e.Error);
        }

        protected void QueueError(RequestMessage reqMsg, Exception error)
        {
            if (reqMsg != null)
            {
                var resMsg = new ResponseMessage
                {
                    TransactionId = reqMsg.TransactionId,
                    ServiceName = reqMsg.ServiceName,
                    MethodName = reqMsg.Message,
                    ReturnType = reqMsg.ReturnType,
                    Error = error
                };

                QueueMessage(resMsg);
            }
        }

        /// <summary>
        /// 添加消息到队列
        /// </summary>
        /// <param name="resMsg"></param>
        protected void QueueMessage(ResponseMessage resMsg)
        {
            if (hashtable.ContainsKey(resMsg.TransactionId))
            {
                var waitResult = hashtable[resMsg.TransactionId];

                //数据响应
                waitResult.Set(resMsg);
            }
        }

        void reqService_OnCallback(object sender, ServiceMessageEventArgs args)
        {
            if (args.Result is ResponseMessage)
            {
                var resMsg = args.Result as ResponseMessage;
                QueueMessage(resMsg);
            }
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public virtual ResponseMessage CallService(RequestMessage reqMsg)
        {
            //获取一个请求
            var reqProxy = reqPool.Pop();

            try
            {
                //处理数据
                var waitResult = new WaitResult();
                hashtable[reqMsg.TransactionId] = waitResult;

                //参数值
                var jsonString = reqMsg.Parameters.ToString();
                string queueKey = string.Format("{0}${1}${2}", reqMsg.ServiceName, reqMsg.MethodName, jsonString);
                queueKey = ServiceConfig.FormatJson(queueKey);

                //处理Queue

                //发送消息
                reqProxy.SendMessage(reqMsg);

                //等待信号响应
                var elapsedTime = TimeSpan.FromSeconds(node.Timeout);
                if (!waitResult.Wait(elapsedTime))
                {
                    throw new WarningException(string.Format("【{0}:{1}】 => Call service ({2}, {3}) timeout ({4}) ms.\r\nParameters => {5}"
                       , node.IP, node.Port, reqMsg.ServiceName, reqMsg.MethodName, (int)elapsedTime.TotalMilliseconds, jsonString));
                }

                return waitResult.Message;
            }
            finally
            {
                //加入队列
                reqPool.Push(reqProxy);

                //用完后移除
                hashtable.Remove(reqMsg.TransactionId);
            }
        }

        #region IService 成员

        /// <summary>
        /// 远程节点
        /// </summary>
        public ServerNode Node
        {
            get { return node; }
        }

        /// <summary>
        /// 服务名称
        /// </summary>
        public virtual string ServiceName
        {
            get
            {
                return string.Format("{0}_{1}", typeof(RemoteProxy).FullName, node.Key);
            }
        }

        #endregion

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                while (reqPool.Count > 0)
                {
                    var reqService = reqPool.Pop();
                    reqService.Dispose();
                }
            }
            catch (Exception)
            {
            }

            this.reqPool = null;
        }
    }
}