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
                    //这里要判断时间
                    var options = lstn.Options;
                    if (options.PushServerStatus)
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
                    if (lstn.SubscibeTypes.Count() == 0
                        || lstn.SubscibeTypes.Any(p => p.FullName == callArgs.Caller.ServiceName))
                    {
                        var options = lstn.Options;
                        if (options.PushCallError && callArgs.IsError)
                        {
                            var error = ErrorHelper.GetInnerException(callArgs.Error);
                            var callError = new CallError
                            {
                                Caller = callArgs.Caller,
                                CallTime = callArgs.CallTime,
                                Message = error.Message,
                                IsBusinessError = callArgs.IsBusinessError
                            };
                            lstn.Notify(callError);
                        }
                        else if (options.PushCallTimeout && callArgs.ElapsedTime > options.CallTimeout * 1000)
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
        /// <param name="endPoint"></param>
        /// <param name="connected"></param>
        public void Notify(EndPoint endPoint, bool connected)
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
                        lstn.Notify(endPoint, connected);
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
        /// <param name="endPoint"></param>
        /// <param name="appClient"></param>
        public void Notify(EndPoint endPoint, AppClient appClient)
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
                        lstn.Notify(endPoint, appClient);
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
