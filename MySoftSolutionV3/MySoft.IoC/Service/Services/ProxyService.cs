using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using MySoft.IoC.Configuration;
using MySoft.IoC.Message;
using MySoft.IoC.Services;
using MySoft.Logger;
using MySoft.Threading;
using MySoft.Net.Sockets;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务代理
    /// </summary>
    public class ProxyService : IService, IDisposable
    {
        private ILog logger;
        private RemoteNode node;
        private ServiceMessagePool reqPool;
        private Hashtable hashtable = Hashtable.Synchronized(new Hashtable());

        public ProxyService(ILog logger, RemoteNode node, int bufferSize)
        {
            this.logger = logger;
            this.node = node;

            #region socket通讯

            //实例化服务池
            reqPool = new ServiceMessagePool(node.MaxPool);
            for (int i = 0; i < node.MaxPool; i++)
            {
                var request = new ServiceMessage(node, logger, bufferSize);
                request.SendCallback += new ServiceMessageEventHandler(client_SendMessage);

                //请求端入栈
                reqPool.Push(request);
            }

            #endregion
        }

        void client_SendMessage(object sender, ServiceMessageEventArgs seviceMsg)
        {
            var resMsg = seviceMsg.Result;
            if (resMsg.Expiration > DateTime.Now)
            {
                //数据结果加入到集合中
                hashtable[resMsg.TransactionId] = resMsg;
            }

            seviceMsg = null;
        }

        /// <summary>
        /// 获取缓存的Key
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetMessageKey(RequestBase value)
        {
            return string.Format("Message_{0}", value.TransactionId);
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="logTimeout"></param>
        /// <returns></returns>
        public ResponseMessage CallService(RequestMessage reqMsg, double logTimeout)
        {
            //如果池为空
            if (reqPool.Count == 0)
            {
                throw new Exception("Service pool is empty！");
            }

            //从池中弹出一个可用请求
            var reqService = reqPool.Pop();

            try
            {
                DataPacket packet = new DataPacket
                {
                    PacketID = reqMsg.TransactionId,
                    PacketObject = reqMsg
                };

                //发送数据包到服务端
                reqService.Send(packet, TimeSpan.FromSeconds(reqMsg.Timeout));

                //开始计时
                Stopwatch watch = Stopwatch.StartNew();

                //获取消息
                AsyncMethodCaller caller = new AsyncMethodCaller(GetResponse);

                //异步调用
                IAsyncResult result = caller.BeginInvoke(reqMsg, null, null);

                ResponseMessage resMsg = null;

                // Wait for the WaitHandle to become signaled.
                if (!result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(reqMsg.Timeout)))
                {
                    result.AsyncWaitHandle.Close();
                    watch.Stop();

                    string title = string.Format("Call ({0}:{1}) remote service ({2},{3}) failure.", node.IP, node.Port, reqMsg.ServiceName, reqMsg.SubServiceName);
                    string body = string.Format("【{5}】Call ({0}:{1}) remote service ({2},{3}) failure. timeout ({4} ms)！", node.IP, node.Port, reqMsg.ServiceName, reqMsg.SubServiceName, watch.ElapsedMilliseconds, reqMsg.TransactionId);
                    throw new WarningException(body)
                    {
                        ApplicationName = reqMsg.AppName,
                        ExceptionHeader = string.Format("Application【{0}】occurs error. ==> Comes from {1}({2}).", reqMsg.AppName, reqMsg.HostName, reqMsg.IPAddress)
                    };
                }
                else
                {
                    // Perform additional processing here.
                    // Call EndInvoke to retrieve the results.
                    resMsg = caller.EndInvoke(result);
                }

                watch.Stop();

                //如果时间超过预定，则输出日志
                if (watch.ElapsedMilliseconds > logTimeout * 1000)
                {
                    //SerializationManager.Serialize(retMsg)
                    string log = string.Format("【{7}】Call ({0}:{1}) remote service ({2},{3}). {5}\r\nMessage ==> {6}\r\nParameters ==> {4}", node.IP, node.Port, resMsg.ServiceName, resMsg.SubServiceName, resMsg.Parameters.SerializedData, "Spent time: (" + watch.ElapsedMilliseconds + ") ms.", resMsg.Message, resMsg.TransactionId);
                    string title = string.Format("Elapsed time ({0}) ms more than ({1}) ms.", watch.ElapsedMilliseconds, logTimeout * 1000);
                    string body = string.Format("{0} {1}", title, log);
                    var exception = new WarningException(body)
                    {
                        ApplicationName = reqMsg.AppName,
                        ExceptionHeader = string.Format("Application【{0}】occurs error. ==> Comes from {1}({2}).", reqMsg.AppName, reqMsg.HostName, reqMsg.IPAddress)
                    };
                    logger.WriteError(exception);
                }

                return resMsg;
            }
            finally
            {
                //将SocketRequest入栈
                reqPool.Push(reqService);
            }
        }

        /// <summary>
        /// 获取响应的消息
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetResponse(RequestMessage reqMsg)
        {
            //启动线程来
            while (true)
            {
                var resMsg = hashtable[reqMsg.TransactionId] as ResponseMessage;

                //如果有数据返回，则响应
                if (resMsg != null)
                {
                    //用完后移除
                    hashtable.Remove(reqMsg.TransactionId);
                    return resMsg;
                }

                //防止cpu使用率过高
                Thread.Sleep(1);
            }
        }

        #region IService 成员

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName
        {
            get { return typeof(ProxyService).FullName; }
        }

        #endregion

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                for (int i = 0; i < reqPool.Count; i++)
                {
                    ServiceMessage args = reqPool.Pop();
                    args.Dispose();
                }
            }
            catch (Exception)
            {
            }

            GC.SuppressFinalize(this);
        }
    }
}