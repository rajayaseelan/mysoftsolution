using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using MySoft.IoC.Status;
using MySoft.Logger;
using System.Net;

namespace MySoft.IoC
{
    /// <summary>
    /// 消息中心；
    /// </summary>
    class MessageCenter : IErrorLogable
    {
        #region MessageCenter 的单例实现

        //线程同步锁；
        private static readonly object _syncLock = new object();

        private static MessageCenter _instance;
        /// <summary>
        /// 返回 MessageCenter 的唯一实例；
        /// </summary>
        public static MessageCenter Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new MessageCenter();
                        }
                    }
                }
                return _instance;
            }
        }

        private IList<MessageListener> _listeners;

        /// <summary>
        /// 保证单例的私有构造函数；
        /// </summary>
        private MessageCenter()
        {
            _listeners = new List<MessageListener>();
        }

        #endregion

        /// <summary>
        /// 添加监听器
        /// </summary>
        /// <param name="listener"></param>
        public void AddListener(MessageListener listener)
        {
            lock (_syncLock)
            {
                if (_listeners.Contains(listener))
                {
                    throw new InvalidOperationException("Listeners have already registered.");
                }
                else
                {
                    _listeners.Add(listener);
                }
            }
        }

        /// <summary>
        /// 移除监听器
        /// </summary>
        /// <param name="listener"></param>
        public void RemoveListener(MessageListener listener)
        {
            lock (_syncLock)
            {
                if (_listeners.Contains(listener))
                {
                    this._listeners.Remove(listener);
                }
                else
                {
                    throw new InvalidOperationException("Listeners don't exist.");
                }
            }
        }

        /// <summary>
        /// 响应状态信息
        /// </summary>
        /// <param name="status"></param>
        public void Notify(ServerStatus status)
        {
            if (_listeners.Count == 0) return;

            MessageListener[] listeners = _listeners.ToArray();
            foreach (MessageListener lstn in listeners)
            {
                try
                {
                    var options = lstn.Options;

                    //如果设定的时间不正确，不进行推送
                    if (options.StatusTimer <= 0) continue;

                    //如果推送时间大于设定的时间，则进行推送
                    if (options.PushServerStatus
                        && DateTime.Now.Subtract(lstn.PushTime).TotalSeconds >= options.StatusTimer)
                    {
                        lstn.Notify(status);
                    }
                }
                catch (SocketException ex)
                {
                    _listeners.Remove(lstn);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.Message);
                    if (OnError != null) OnError(ex);
                }
            }
        }

        /// <summary>
        /// 调用事件信息
        /// </summary>
        /// <param name="callArgs"></param>
        public void Notify(CallEventArgs callArgs)
        {
            if (_listeners.Count == 0) return;

            MessageListener[] listeners = _listeners.ToArray();
            foreach (MessageListener lstn in listeners)
            {
                try
                {
                    if (lstn.SubscribeTypes.Count() == 0
                        || lstn.SubscribeTypes.Any(p => string.Compare(p, callArgs.Caller.ServiceName, true) == 0))
                    {
                        var options = lstn.Options;
                        if (options.PushCallError && callArgs.IsError)
                        {
                            //业务异常不进行推送
                            if (!callArgs.IsBusinessError)
                            {
                                var error = ErrorHelper.GetInnerException(callArgs.Error);
                                var callError = new CallError
                                {
                                    Caller = callArgs.Caller,
                                    CallTime = callArgs.CallTime,
                                    Type = error.GetType().FullName,
                                    Message = error.Message,
                                    Error = ErrorHelper.GetErrorWithoutHtml(callArgs.Error),
                                    HtmlError = ErrorHelper.GetHtmlError(callArgs.Error)
                                };
                                lstn.Notify(callError);
                            }
                        }

                        if (options.PushCallTimeout && !callArgs.IsError)
                        {
                            //如果设定的时间不正确，不进行推送
                            if (options.CallTimeout <= 0) continue;

                            if (callArgs.ElapsedTime > options.CallTimeout * 1000)
                            {
                                var callTimeout = new CallTimeout
                                {
                                    Caller = callArgs.Caller,
                                    CallTime = callArgs.CallTime,
                                    Count = callArgs.Count,
                                    ElapsedTime = callArgs.ElapsedTime
                                };
                                lstn.Notify(callTimeout);
                            }
                        }
                    }
                }
                catch (SocketException ex)
                {
                    _listeners.Remove(lstn);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.Message);
                    if (OnError != null) OnError(ex);
                }
            }
        }

        /// <summary>
        /// 通知消息
        /// </summary>
        /// <param name="connectInfo"></param>
        public void Notify(ConnectInfo connectInfo)
        {
            if (_listeners.Count == 0) return;

            MessageListener[] listeners = _listeners.ToArray();
            foreach (MessageListener lstn in listeners)
            {
                try
                {
                    var options = lstn.Options;
                    if (options.PushClientConnect)
                    {
                        lstn.Notify(connectInfo);
                    }
                }
                catch (SocketException ex)
                {
                    _listeners.Remove(lstn);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.Message);
                    if (OnError != null) OnError(ex);
                }
            }
        }

        /// <summary>
        /// 改变客户端信息
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="appClient"></param>
        public void Notify(string ipAddress, AppClient appClient)
        {
            if (_listeners.Count == 0) return;

            MessageListener[] listeners = _listeners.ToArray();
            foreach (MessageListener lstn in listeners)
            {
                try
                {
                    var options = lstn.Options;
                    if (options.PushClientConnect)
                    {
                        lstn.Notify(ipAddress, appClient);
                    }
                }
                catch (SocketException ex)
                {
                    _listeners.Remove(lstn);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.Message);
                    if (OnError != null) OnError(ex);
                }
            }
        }

        /// <summary>
        /// 推送客户端连接信息（只有第一次订阅的时候推送）
        /// </summary>
        /// <param name="clientInfos"></param>
        public void Push(IList<ClientInfo> clientInfos)
        {
            if (_listeners.Count == 0) return;

            MessageListener[] listeners = _listeners.ToArray();
            foreach (MessageListener lstn in listeners)
            {
                try
                {
                    var options = lstn.Options;
                    if (options.PushClientConnect)
                    {
                        lstn.Notify(clientInfos);
                    }
                }
                catch (SocketException ex)
                {
                    _listeners.Remove(lstn);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.Message);
                    if (OnError != null) OnError(ex);
                }
            }
        }

        #region IErrorLogable 成员

        /// <summary>
        /// 错误处理Handler
        /// </summary>
        public event ErrorLogEventHandler OnError;

        #endregion
    }
}
