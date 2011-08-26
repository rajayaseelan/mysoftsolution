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
using MySoft.Net.Server;
using MySoft.Net.Sockets;

namespace MySoft.IoC
{
    /// <summary>
    /// Castle服务
    /// </summary>
    public class CastleService : IStatusService, IDisposable, ILogable, IErrorLogable
    {
        private IServiceContainer container;
        private CastleServiceConfiguration config;
        private SocketServerManager manager;
        private IList<EndPoint> clients;
        private TimeStatusCollection statuslist;
        private HighestStatus highest;
        private DateTime startTime;

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
        {
            this.config = config;

            //注入内部的服务
            Hashtable hashTypes = new Hashtable();
            hashTypes[typeof(IStatusService)] = this;

            this.container = new SimpleServiceContainer(CastleFactoryType.Local, hashTypes);
            this.container.OnError += new ErrorLogEventHandler(container_OnError);
            this.container.OnLog += new LogEventHandler(container_OnLog);
            this.clients = new List<EndPoint>();
            this.statuslist = new TimeStatusCollection(config.Records);
            this.highest = new HighestStatus();
            this.startTime = DateTime.Now;

            //服务器配置
            SocketServerConfiguration ssc = new SocketServerConfiguration
            {
                Host = config.Host,
                Port = config.Port,
                MaxConnectCount = config.MaxConnect,
                MaxBufferSize = config.MaxBuffer
            };

            //实例化Socket服务
            manager = new SocketServerManager(ssc);
            manager.OnConnectFilter += new ConnectionFilterEventHandler(SocketServerManager_OnConnectFilter);
            manager.OnDisconnected += new DisconnectionEventHandler(SocketServerManager_OnDisconnected);
            manager.OnBinaryInput += new BinaryInputEventHandler(SocketServerManager_OnBinaryInput);
            manager.OnMessageOutput += new EventHandler<LogOutEventArgs>(SocketServerManager_OnMessageOutput);

            Thread thread = new Thread(() =>
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
            });

            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            this.Stop();
            manager = null;
            clients = null;
            statuslist = null;
            highest = null;

            GC.SuppressFinalize(this);
        }

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

            manager.Server.Start();
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
            get
            {
                return string.Format("{0}://{1}/", manager.Server.Socket.ProtocolType, manager.Server.Socket.LocalEndPoint).ToLower();
            }
        }

        /// <summary>
        /// 最大连接数
        /// </summary>
        public int MaxConnect
        {
            get
            {
                return config.MaxConnect;
            }
        }

        /// <summary>
        /// 最大缓冲区
        /// </summary>
        public int MaxBuffer
        {
            get
            {
                return config.MaxConnect * config.MaxBuffer;
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop()
        {
            manager.Server.Stop();
        }

        #endregion

        void container_OnLog(string log, LogType type)
        {
            try
            {
                if (OnLog != null) OnLog(log, type);
            }
            catch (Exception)
            {
            }
        }

        void container_OnError(Exception exception)
        {
            try
            {
                if (OnError != null) OnError(exception);
            }
            catch (Exception)
            {
            }
        }

        #region ILogable Members

        /// <summary>
        /// OnLog event.
        /// </summary>
        public event LogEventHandler OnLog;

        #endregion

        #region IErrorLogable Members

        /// <summary>
        /// OnError event.
        /// </summary>
        public event ErrorLogEventHandler OnError;

        #endregion

        #region 侦听事件

        bool SocketServerManager_OnConnectFilter(SocketAsyncEventArgs socketAsync)
        {
            if (OnLog != null) OnLog(string.Format("User connection {0}！", socketAsync.AcceptSocket.RemoteEndPoint), LogType.Information);

            //将地址加入到列表中
            lock (clients)
            {
                clients.Add(socketAsync.AcceptSocket.RemoteEndPoint);
            }

            return true;
        }

        void SocketServerManager_OnMessageOutput(object sender, LogOutEventArgs e)
        {
            container_OnLog(e.Message, e.Type);
        }

        void SocketServerManager_OnDisconnected(int error, SocketAsyncEventArgs socketAsync)
        {
            container_OnLog(string.Format("User Disconnect {0}！", socketAsync.AcceptSocket.RemoteEndPoint), LogType.Error);
            socketAsync.UserToken = null;

            //将地址从列表中移除
            lock (clients)
            {
                clients.Remove(socketAsync.AcceptSocket.RemoteEndPoint);
            }

            socketAsync.AcceptSocket.Close();
        }

        void SocketServerManager_OnBinaryInput(byte[] buffer, SocketAsyncEventArgs socketAsync)
        {
            using (BufferReader read = new BufferReader(buffer))
            {
                int length;
                int cmd;
                Guid pid;

                if (read.ReadInt32(out length) && read.ReadInt32(out cmd) && read.ReadGuid(out pid) && length == read.Length)
                {
                    if (cmd == -10000)//请求结果信息
                    {
                        try
                        {
                            RequestMessage reqMsg;
                            if (read.ReadObject(out reqMsg))
                            {
                                if (reqMsg != null)
                                {
                                    //发送响应信息
                                    GetSendResponse(socketAsync, reqMsg);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            container_OnError(ex);

                            var resMsg = new ResponseMessage
                            {
                                TransactionId = pid,
                                Expiration = DateTime.Now.AddMinutes(1),
                                Compress = false,
                                Encrypt = false,
                                ReturnType = ex.GetType(),
                                Exception = ex
                            };

                            DataPacket packet = new DataPacket
                            {
                                PacketID = resMsg.TransactionId,
                                PacketObject = resMsg
                            };

                            byte[] data = BufferFormat.FormatFCA(packet);

                            //发送数据到服务端
                            manager.Server.SendData(socketAsync.AcceptSocket, data);
                        }
                    }
                    else //现在还没登入 如果有其他命令的请求那么 断开连接
                    {
                        manager.Server.Disconnect(socketAsync.AcceptSocket);
                    }
                }
                else //无法读取数据包 断开连接
                {
                    manager.Server.Disconnect(socketAsync.AcceptSocket);
                }
            }
        }

        /// <summary>
        /// 获取响应信息并发送
        /// </summary>
        /// <param name="socketAsync"></param>
        /// <param name="reqMsg"></param>
        private void GetSendResponse(SocketAsyncEventArgs socketAsync, RequestMessage reqMsg)
        {
            //如果是状态请求，则直接返回数据
            if (!IsServiceCounter(reqMsg))
            {
                //调用请求方法
                var resMsg = CallMethod(reqMsg);

                if (resMsg != null)
                {
                    DataPacket packet = new DataPacket
                    {
                        PacketID = resMsg.TransactionId,
                        PacketObject = resMsg
                    };

                    //发送数据到服务端
                    manager.Server.SendData(socketAsync.AcceptSocket, BufferFormat.FormatFCA(packet));
                }
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

                    DataPacket packet = new DataPacket
                    {
                        PacketID = resMsg.TransactionId,
                        PacketObject = resMsg
                    };

                    byte[] data = BufferFormat.FormatFCA(packet);

                    //计算流量
                    status.DataFlow += data.Length;

                    //发送数据到服务端
                    manager.Server.SendData(socketAsync.AcceptSocket, data);
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
            ResponseMessage response = null;

            try
            {
                //生成一个异步调用委托
                AsyncMethodCaller caller = new AsyncMethodCaller(p =>
                {
                    return container.CallService(p, config.LogTime);
                });

                //开始异步调用
                IAsyncResult result = caller.BeginInvoke(reqMsg, null, null);

                //等待信号
                if (result.AsyncWaitHandle.WaitOne())
                {
                    response = caller.EndInvoke(result);
                }
                else
                {
                    result.AsyncWaitHandle.Close();
                    throw new NullReferenceException("Call service response is null！");
                }
            }
            catch (Exception ex)
            {
                //抛出错误信息
                container_OnError(ex);

                response = new ResponseMessage();
                response.TransactionId = reqMsg.TransactionId;
                response.ServiceName = reqMsg.ServiceName;
                response.SubServiceName = reqMsg.SubServiceName;
                response.Parameters = reqMsg.Parameters;
                response.Expiration = reqMsg.Expiration;
                response.Compress = reqMsg.Compress;
                response.Encrypt = reqMsg.Encrypt;
                response.Exception = ex;
            }

            return response;
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

        #endregion

        #region IStatusService 成员

        /// <summary>
        /// 获取服务信息列表
        /// </summary>
        /// <returns></returns>
        public IList<ServiceInfo> GetServiceInfoList()
        {
            var list = new List<ServiceInfo>();
            foreach (Type type in container.GetInterfaces<ServiceContractAttribute>())
            {
                var service = new ServiceInfo
                {
                    Assembly = type.Assembly.FullName,
                    Name = type.FullName,
                    Methods = CoreHelper.GetMethodsFromType(type)
                };

                list.Add(service);
            }
            return list.ToArray();
        }

        /// <summary>
        /// 清除所有服务器状态
        /// </summary>
        public void ClearStatus()
        {
            lock (statuslist)
            {
                statuslist.Clear();
                highest = new HighestStatus();
            }
        }

        /// <summary>
        /// 服务状态信息
        /// </summary>
        /// <returns></returns>
        public ServerStatus GetServerStatus()
        {
            ServerStatus status = new ServerStatus
            {
                StartDate = startTime,
                TotalSeconds = (int)DateTime.Now.Subtract(startTime).TotalSeconds,
                Highest = GetHighestStatus(),
                Latest = GetLatestStatus(),
                Summary = GetSummaryStatus()
            };

            return status;
        }

        /// <summary>
        /// 获取最后一次服务状态
        /// </summary>
        /// <returns></returns>
        public TimeStatus GetLatestStatus()
        {
            return statuslist.GetLast();
        }

        /// <summary>
        /// 获取最高状态信息
        /// </summary>
        /// <returns></returns>
        public HighestStatus GetHighestStatus()
        {
            return highest;
        }

        /// <summary>
        /// 汇总状态信息
        /// </summary>
        /// <returns></returns>
        public SummaryStatus GetSummaryStatus()
        {
            //获取状态列表
            var list = GetTimeStatusList();

            //统计状态信息
            SummaryStatus status = new SummaryStatus
            {
                RunningSeconds = list.Count,
                RequestCount = list.Sum(p => p.RequestCount),
                SuccessCount = list.Sum(p => p.SuccessCount),
                ErrorCount = list.Sum(p => p.ErrorCount),
                ElapsedTime = list.Sum(p => p.ElapsedTime),
                DataFlow = list.Sum(p => p.DataFlow),
            };

            return status;
        }

        /// <summary>
        /// 获取服务状态列表
        /// </summary>
        /// <returns></returns>
        public IList<TimeStatus> GetTimeStatusList()
        {
            return statuslist.ToList();
        }

        /// <summary>
        /// 获取连接客户信息
        /// </summary>
        /// <returns></returns>
        public IList<ConnectInfo> GetConnectInfoList()
        {
            lock (clients)
            {
                var dict = clients.ToLookup(p => p.ToString().Split(':')[0]);
                IList<ConnectInfo> list = new List<ConnectInfo>();
                foreach (var item in dict)
                {
                    list.Add(new ConnectInfo { IP = item.Key, Count = item.Count() });
                }

                return list;
            }
        }

        #endregion
    }
}
