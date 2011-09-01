using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MySoft.IoC.Configuration;
using MySoft.IoC.Message;
using MySoft.IoC.Services;
using MySoft.Logger;
using MySoft.Communication.Scs.Server;
using MySoft.Communication.Scs.Communication.EndPoints.Tcp;
using MySoft.Communication.Scs.Communication.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// Castle服务
    /// </summary>
    public class CastleService : ServerMoniter, IDisposable
    {
        private IScsServer server;
        private string serverUrl;
        /// <summary>
        /// 服务容器
        /// </summary>
        public IServiceContainer Container
        {
            get { return container; }
        }

        /// <summary>
        /// 实例化CastleService
        /// </summary>
        /// <param name="config"></param>
        public CastleService(CastleServiceConfiguration config)
            : base(config)
        {
            //实例化Socket服务
            ScsTcpEndPoint endPoint = null;
            if (string.Compare(config.Host, "any", true) == 0)
            {
                config.Host = "127.0.0.1";
                endPoint = new ScsTcpEndPoint(config.Port);
            }
            else
                endPoint = new ScsTcpEndPoint(config.Host, config.Port);

            this.serverUrl = endPoint.ToString();
            this.server = ScsServerFactory.CreateServer(endPoint);
            this.server.ClientConnected += new EventHandler<ServerClientEventArgs>(server_ClientConnected);
            this.server.ClientDisconnected += new EventHandler<ServerClientEventArgs>(server_ClientDisconnected);

            Thread thread = new Thread(DoWork);
            thread.IsBackground = true;
            thread.Start();
        }

        #region 定时统计

        void DoWork()
        {
            while (true)
            {
                try
                {
                    //获取最后一秒状态
                    var status = statuslist.GetLast();

                    //计算时间
                    if (status.RequestCount > 0)
                    {
                        //处理最高值 
                        #region 处理最高值

                        //流量
                        if (status.DataFlow > highest.DataFlow)
                        {
                            highest.DataFlow = status.DataFlow;
                            highest.DataFlowCounterTime = status.CounterTime;
                        }

                        //成功
                        if (status.SuccessCount > highest.SuccessCount)
                        {
                            highest.SuccessCount = status.SuccessCount;
                            highest.SuccessCountCounterTime = status.CounterTime;
                        }

                        //失败
                        if (status.ErrorCount > highest.ErrorCount)
                        {
                            highest.ErrorCount = status.ErrorCount;
                            highest.ErrorCountCounterTime = status.CounterTime;
                        }

                        //请求总数
                        if (status.RequestCount > highest.RequestCount)
                        {
                            highest.RequestCount = status.RequestCount;
                            highest.RequestCountCounterTime = status.CounterTime;
                        }

                        //耗时
                        if (status.ElapsedTime > highest.ElapsedTime)
                        {
                            highest.ElapsedTime = status.ElapsedTime;
                            highest.ElapsedTimeCounterTime = status.CounterTime;
                        }

                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    //写错误日志
                    SimpleLog.Instance.WriteLog(ex);
                }

                //每1秒处理一次
                Thread.Sleep(1000);
            }
        }

        #endregion

        #region 启动停止服务

        /// <summary>
        /// 启用服务
        /// </summary>
        public void Start()
        {
            Start(false);
        }

        /// <summary>
        /// 启用服务
        /// </summary>
        /// <param name="isWriteLog"></param>
        public void Start(bool isWriteLog)
        {
            //写发布服务信息
            if (isWriteLog) Publish();

            //启动服务
            server.Start();
        }

        /// <summary>
        /// 发布服务
        /// </summary>
        private void Publish()
        {
            var list = this.GetServiceInfoList();

            string log = string.Format("此次发布的服务有{0}个，共有{1}个方法，详细信息如下：\r\n\r\n", list.Count, list.Sum(p => p.Methods.Count()));
            StringBuilder sb = new StringBuilder(log);

            int index = 0;
            foreach (var info in list)
            {
                sb.AppendFormat("{0}, {1}\r\n", info.Name, info.Assembly);
                sb.AppendLine("------------------------------------------------------------------------------------------------------------------------");
                foreach (var method in info.Methods)
                {
                    sb.AppendLine(method.ToString());
                }

                if (index < list.Count - 1)
                {
                    sb.AppendLine();
                    sb.AppendLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                    sb.AppendLine();
                }

                index++;
            }

            SimpleLog.Instance.WriteLog(sb.ToString());
        }

        /// <summary>
        /// 获取服务的ServerUrl地址
        /// </summary>
        public string ServerUrl
        {
            get { return serverUrl.ToLower(); }
        }

        /// <summary>
        /// 停止服务
        /// </summary>1
        public void Stop()
        {
            server.Stop();
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            server.Stop();
            server.Clients.ClearAll();

            server = null;
            statuslist = null;
            highest = null;

            GC.SuppressFinalize(this);
        }

        #endregion

        #region 侦听事件

        void server_ClientConnected(object sender, ServerClientEventArgs e)
        {
            var endPoint = (e.Client.RemoteEndPoint as ScsTcpEndPoint);
            container_OnLog(string.Format("User connection {0}:{1}！", endPoint.IpAddress, endPoint.TcpPort), LogType.Information);
            e.Client.MessageReceived += new EventHandler<MessageEventArgs>(Client_MessageReceived);
            e.Client.MessageSent += new EventHandler<MessageEventArgs>(Client_MessageSent);
        }

        void Client_MessageSent(object sender, MessageEventArgs e)
        {
            //暂不作处理
        }

        void server_ClientDisconnected(object sender, ServerClientEventArgs e)
        {
            var endPoint = (e.Client.RemoteEndPoint as ScsTcpEndPoint);
            container_OnLog(string.Format("User Disconnection {0}:{1}！", endPoint.IpAddress, endPoint.TcpPort), LogType.Error);
        }

        void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            //不是指定消息不处理
            if (e.Message is ScsRawDataMessage)
            {
                //获取client发送端
                var client = sender as IScsServerClient;

                try
                {
                    var data = e.Message as ScsRawDataMessage;
                    RequestMessage reqMsg = SerializationManager.DeserializeBin<RequestMessage>(data.MessageData);

                    //发送响应信息
                    GetSendResponse(client, reqMsg);
                }
                catch (Exception ex)
                {
                    container_OnError(ex);

                    var resMsg = new ResponseMessage
                    {
                        TransactionId = new Guid(e.Message.RepliedMessageId),
                        Expiration = DateTime.Now.AddMinutes(1),
                        Compress = false,
                        Encrypt = false,
                        ReturnType = ex.GetType(),
                        Exception = ex
                    };

                    //发送数据到服务端
                    var buffer = SerializationManager.SerializeBin(resMsg);
                    client.SendMessage(new ScsRawDataMessage(buffer, e.Message.RepliedMessageId));
                }
            }
        }

        /// <summary>
        /// 获取响应信息并发送
        /// </summary>
        /// <param name="client"></param>
        /// <param name="reqMsg"></param>
        private void GetSendResponse(IScsServerClient client, RequestMessage reqMsg)
        {
            //如果是状态请求，则直接返回数据
            if (!IsServiceCounter(reqMsg))
            {
                //调用请求方法
                var resMsg = CallMethod(reqMsg);

                //发送数据到服务端
                var buffer = SerializationManager.SerializeBin(resMsg);
                client.SendMessage(new ScsRawDataMessage(buffer, reqMsg.TransactionId.ToString()));
            }
            else
            {
                //获取或创建一个对象
                TimeStatus status = statuslist.GetOrCreate(DateTime.Now);

                //开始计时
                Stopwatch watch = Stopwatch.StartNew();

                //调用请求方法
                var resMsg = CallMethod(reqMsg);

                watch.Stop();

                //处理时间
                status.ElapsedTime += watch.ElapsedMilliseconds;

                if (resMsg != null)
                {
                    //请求数累计
                    status.RequestCount++;

                    //错误及成功计数
                    if (resMsg.Exception == null)
                        status.SuccessCount++;
                    else
                        status.ErrorCount++;

                    var buffer = SerializationManager.SerializeBin(resMsg);

                    //计算流量
                    status.DataFlow += buffer.Length;

                    client.SendMessage(new ScsRawDataMessage(buffer, reqMsg.TransactionId.ToString()));
                }
                else
                {
                    var ex = new NullReferenceException(string.Format("Call service ({0}, {1}) response is null！", reqMsg.ServiceName, reqMsg.SubServiceName));
                    container_OnError(ex);
                }
            }
        }

        /// <summary>
        /// 调用 方法
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage CallMethod(RequestMessage reqMsg)
        {
            //获取返回的消息
            ResponseMessage resMsg = null;

            try
            {
                //处理cacheKey信息
                string cacheKey = string.Format("{0}_{1}_{2}", reqMsg.ServiceName, reqMsg.SubServiceName, reqMsg.Parameters);
                resMsg = CacheHelper.Get<ResponseMessage>(cacheKey);

                if (resMsg == null)
                {
                    //生成一个异步调用委托
                    AsyncMethodCaller handler = new AsyncMethodCaller(p =>
                    {
                        return container.CallService(p, config.LogTime);
                    });

                    //开始异步调用
                    IAsyncResult ar = handler.BeginInvoke(reqMsg, r => { }, handler);

                    //等待信号，等待10分钟
                    if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMinutes(10)))
                    {
                        throw new WarningException(string.Format("Call service ({0},{1}) timeout 10 minutes！", reqMsg.ServiceName, reqMsg.SubServiceName));
                    }
                    else
                    {
                        resMsg = handler.EndInvoke(ar);

                        if (resMsg != null && resMsg.Data != null)
                        {
                            //以100为单位进行缓存处理
                            int times = resMsg.RowCount / 100;

                            //默认缓存5秒
                            if (times > 0)
                                CacheHelper.Insert(cacheKey, resMsg, times);
                            else
                                CacheHelper.Insert(cacheKey, resMsg, 1);
                        }
                    }
                }
                else
                {
                    //为了数据能返回，需要把过期时间与传输ID修改
                    resMsg.TransactionId = reqMsg.TransactionId;
                    resMsg.Expiration = reqMsg.Expiration;
                }
            }
            catch (Exception ex)
            {
                //抛出错误信息
                container_OnError(ex);

                resMsg = new ResponseMessage();
                resMsg.TransactionId = reqMsg.TransactionId;
                resMsg.ServiceName = reqMsg.ServiceName;
                resMsg.SubServiceName = reqMsg.SubServiceName;
                resMsg.Parameters = reqMsg.Parameters;
                resMsg.Expiration = reqMsg.Expiration;
                resMsg.Compress = reqMsg.Compress;
                resMsg.Encrypt = reqMsg.Encrypt;
                resMsg.Exception = ex;
            }

            return resMsg;
        }

        /// <summary>
        /// 判断是否需要计数
        /// </summary>
        /// <param name="request"></param>
        private bool IsServiceCounter(RequestMessage request)
        {
            if (request == null) return false;
            if (request.ServiceName == typeof(IStatusService).FullName) return false;

            return true;
        }

        /// <summary>
        /// 获取连接客户信息
        /// </summary>
        /// <returns></returns>
        public override IList<ConnectInfo> GetConnectInfoList()
        {
            try
            {
                var items = server.Clients.GetAllItems();

                //统计客户端数量
                return items.Select(p => p.RemoteEndPoint as ScsTcpEndPoint).GroupBy(p => p.IpAddress)
                     .Select(p => new ConnectInfo { IP = p.Key, Count = p.Count() })
                     .ToList();
            }
            catch (Exception ex)
            {
                container_OnError(ex);

                return new List<ConnectInfo>();
            }
        }

        #endregion
    }
}
