using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
using MySoft.Logger;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 服务代理
    /// </summary>
    public class RemoteProxy : IService, IDisposable
    {
        protected ILog logger;
        protected RemoteNode node;
        protected ServiceRequestPool reqPool;
        private Hashtable hashtable = Hashtable.Synchronized(new Hashtable());

        public RemoteProxy(RemoteNode node, ILog logger)
        {
            this.node = node;
            this.logger = logger;

            InitRequest();
        }

        /// <summary>
        /// 初始化请求
        /// </summary>
        protected virtual void InitRequest()
        {
            this.reqPool = new ServiceRequestPool(node.MaxPool);

            //服务请求池化
            for (int i = 0; i < node.MaxPool; i++)
            {
                var reqService = new ServiceRequest(node, logger, true);
                reqService.OnCallback += new EventHandler<ServiceMessageEventArgs>(reqService_OnCallback);

                this.reqPool.Push(reqService);
            }
        }

        /// <summary>
        /// 添加消息到队列
        /// </summary>
        /// <param name="resMsg"></param>
        protected void QueueMessage(ResponseMessage resMsg)
        {
            if (resMsg.Expiration > DateTime.Now)
            {
                hashtable[resMsg.TransactionId] = resMsg;
            }
        }

        void reqService_OnCallback(object sender, ServiceMessageEventArgs args)
        {
            if (args.Result is ResponseMessage)
            {
                var resMsg = args.Result as ResponseMessage;
                QueueMessage(resMsg);
            }

            args = null;
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="logTimeout"></param>
        /// <returns></returns>
        public ResponseMessage CallService(RequestMessage reqMsg, double logTimeout)
        {
            //如果池为空，则判断是否达到最大池
            if (reqPool.Count == 0)
            {
                throw new Exception("Service request pool is empty！");
            }

            //获取一个服务请求
            var reqService = reqPool.Pop();

            try
            {
                //设置过期时间
                reqMsg.Expiration = DateTime.Now.AddSeconds(node.Timeout);

                //发送消息
                reqService.SendMessage(reqMsg);

                Thread thread = null;

                //获取消息
                var caller = new AsyncMethodCaller<ResponseMessage, RequestMessage>(state =>
                {
                    thread = Thread.CurrentThread;

                    //启动线程来
                    while (true)
                    {
                        var retMsg = hashtable[state.TransactionId] as ResponseMessage;

                        //如果有数据返回，则响应
                        if (retMsg != null)
                        {
                            //用完后移除
                            hashtable.Remove(state.TransactionId);

                            return retMsg;
                        }

                        //防止cpu使用率过高
                        Thread.Sleep(1);
                    }
                });

                //开始调用
                IAsyncResult ar = caller.BeginInvoke(reqMsg, iar => { }, caller);

                //开始计时
                Stopwatch watch = Stopwatch.StartNew();

                //等待信号，客户端等待5分钟
                bool timeout = !ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(node.Timeout));

                watch.Stop();

                if (timeout)
                {
                    try
                    {
                        if (!ar.IsCompleted && thread != null)
                            thread.Abort();
                    }
                    catch (Exception ex)
                    {
                    }

                    string title = string.Format("Call ({0}:{1}) remote service ({2},{3}) timeout.", node.IP, node.Port, reqMsg.ServiceName, reqMsg.SubServiceName);
                    string body = string.Format("【{5}】Call ({0}:{1}) remote service ({2},{3}) timeout ({4} ms)！", node.IP, node.Port, reqMsg.ServiceName, reqMsg.SubServiceName, watch.ElapsedMilliseconds, reqMsg.TransactionId);
                    throw new WarningException(body)
                    {
                        ApplicationName = reqMsg.AppName,
                        ServiceName = reqMsg.ServiceName,
                        ExceptionHeader = string.Format("Application【{0}】call service timeout. ==> Comes from {1}({2}).", reqMsg.AppName, reqMsg.HostName, reqMsg.IPAddress)
                    };
                }

                //获取返回结果
                var resMsg = caller.EndInvoke(ar);

                //关闭句柄
                ar.AsyncWaitHandle.Close();

                //如果时间超过预定，则输出日志
                if (watch.ElapsedMilliseconds > logTimeout * 1000)
                {
                    //SerializationManager.Serialize(retMsg)
                    string log = string.Format("【{7}】Call ({0}:{1}) remote service ({2},{3}). {5}\r\nMessage ==> {6}\r\nParameters ==> {4}", node.IP, node.Port, resMsg.ServiceName, resMsg.SubServiceName, resMsg.Parameters, "Spent time: (" + watch.ElapsedMilliseconds + ") ms.", resMsg.Message, resMsg.TransactionId);
                    string title = string.Format("Elapsed time ({0}) ms more than ({1}) ms.", watch.ElapsedMilliseconds, logTimeout * 1000);
                    string body = string.Format("{0} {1}", title, log);
                    var exception = new WarningException(body)
                    {
                        ApplicationName = reqMsg.AppName,
                        ServiceName = reqMsg.ServiceName,
                        ExceptionHeader = string.Format("Application【{0}】occurs error. ==> Comes from {1}({2}).", reqMsg.AppName, reqMsg.HostName, reqMsg.IPAddress)
                    };

                    logger.WriteError(exception);
                }

                return resMsg;
            }
            finally
            {
                this.reqPool.Push(reqService);
            }
        }

        #region IService 成员

        /// <summary>
        /// 远程节点
        /// </summary>
        public RemoteNode Node
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
                return typeof(RemoteProxy).FullName;
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

            GC.SuppressFinalize(this);
        }
    }
}