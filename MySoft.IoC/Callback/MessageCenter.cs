using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;
using MySoft.Logger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MySoft.IoC.Callback
{
    /// <summary>
    /// 消息中心；
    /// </summary>
    internal class MessageCenter : ILogable, IErrorLogable
    {
        #region MessageCenter 的单例实现

        //线程同步锁；
        private static readonly object syncRoot = new object();
        private static MessageCenter instance;
        private IDictionary<string, MessageListener> listeners = new Dictionary<string, MessageListener>();

        /// <summary>
        /// 返回 MessageCenter 的唯一实例；
        /// </summary>
        public static MessageCenter Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new MessageCenter();
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// 监听数
        /// </summary>
        public int Count
        {
            get
            {
                lock (listeners)
                {
                    return listeners.Count;
                }
            }
        }

        #endregion

        /// <summary>
        /// 获取监控器
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public MessageListener GetListener(IScsServerClient channel)
        {
            if (listeners.Count == 0) return null;

            var listenerKey = channel.RemoteEndPoint.ToString();

            lock (listeners)
            {
                if (listeners.ContainsKey(listenerKey))
                {
                    return listeners[listenerKey];
                }
            }

            return null;
        }

        /// <summary>
        /// 添加监听器
        /// </summary>
        /// <param name="listener"></param>
        public void AddListener(MessageListener listener)
        {
            var listenerKey = listener.Channel.RemoteEndPoint.ToString();

            lock (listeners)
            {
                if (listeners.ContainsKey(listenerKey))
                {
                    throw new InvalidOperationException("Listeners have already registered.");
                }
                else
                {
                    if (OnLog != null) OnLog(string.Format("Add listener ({0}).", listenerKey), LogType.Warning);
                    listeners[listenerKey] = listener;
                }
            }
        }

        /// <summary>
        /// 移除监听器
        /// </summary>
        /// <param name="listener"></param>
        public void RemoveListener(MessageListener listener)
        {
            var listenerKey = listener.Channel.RemoteEndPoint.ToString();

            lock (listeners)
            {
                if (listeners.ContainsKey(listenerKey))
                {
                    if (OnLog != null) OnLog(string.Format("Remove listener ({0}).", listenerKey), LogType.Error);
                    listeners.Remove(listenerKey);
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
            if (listeners.Count == 0) return;

            IList<MessageListener> _listeners = null;

            lock (listeners)
            {
                _listeners = listeners.Values.ToList();
            }

            foreach (MessageListener lstn in _listeners)
            {
                try
                {
                    var options = lstn.Options;

                    //如果设定的时间不正确，不进行推送
                    if (options.ServerStatusTimer <= 0) continue;

                    //如果推送时间大于设定的时间，则进行推送
                    if (options.PushServerStatus
                        && DateTime.Now.Subtract(lstn.PushTime).TotalSeconds >= options.ServerStatusTimer)
                    {
                        lstn.Notify(status);
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        WriteError(lstn, ex);

                        RemoveListener(lstn);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// 调用事件信息
        /// </summary>
        /// <param name="callArgs"></param>
        public void Notify(CallEventArgs callArgs)
        {
            if (listeners.Count == 0) return;

            IList<MessageListener> _listeners = null;

            lock (listeners)
            {
                _listeners = listeners.Values.ToList();
            }

            foreach (MessageListener lstn in _listeners)
            {
                try
                {
                    if ((lstn.Apps.Count == 0 || lstn.Apps.Any(p => string.Compare(p, callArgs.Caller.AppName, true) == 0)) &&
                        (lstn.Types.Count == 0 || lstn.Types.Any(p => string.Compare(p, callArgs.Caller.ServiceName, true) == 0)))
                    {
                        var options = lstn.Options;
                        if (options.PushCallError && callArgs.IsError)
                        {
                            var callError = new CallError
                            {
                                Caller = callArgs.Caller,
                                Message = callArgs.Error.Message,
                                Error = ErrorHelper.GetErrorWithoutHtml(callArgs.Error),
                                HtmlError = ErrorHelper.GetHtmlError(callArgs.Error)
                            };

                            lstn.Notify(callError);
                        }

                        if (options.PushCallTimeout && !callArgs.IsError)
                        {
                            //如果设定的时间不正确，不进行推送
                            if (options.CallTimeout <= 0
                                && options.CallRowCount <= 0) continue;

                            if (callArgs.ElapsedTime > options.CallTimeout * 1000
                                || callArgs.Count > options.CallRowCount)
                            {
                                var callTimeout = new CallTimeout
                                {
                                    Caller = callArgs.Caller,
                                    Count = callArgs.Count,
                                    ElapsedTime = callArgs.ElapsedTime
                                };

                                lstn.Notify(callTimeout);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        WriteError(lstn, ex);

                        RemoveListener(lstn);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// 通知消息
        /// </summary>
        /// <param name="connectInfo"></param>
        public void Notify(ConnectInfo connectInfo)
        {
            if (listeners.Count == 0) return;

            IList<MessageListener> _listeners = null;

            lock (listeners)
            {
                _listeners = listeners.Values.ToList();
            }

            foreach (MessageListener lstn in _listeners)
            {
                try
                {
                    var options = lstn.Options;
                    if (options.PushClientConnect)
                    {
                        lstn.Notify(connectInfo);
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        WriteError(lstn, ex);

                        RemoveListener(lstn);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// 改变客户端信息
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <param name="appClient"></param>
        public void Notify(string ipAddress, int port, AppClient appClient)
        {
            if (listeners.Count == 0) return;

            IList<MessageListener> _listeners = null;

            lock (listeners)
            {
                _listeners = listeners.Values.ToList();
            }

            foreach (MessageListener lstn in _listeners)
            {
                try
                {
                    var options = lstn.Options;
                    if (options.PushClientConnect)
                    {
                        lstn.Notify(ipAddress, port, appClient);
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        WriteError(lstn, ex);

                        RemoveListener(lstn);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// 输出异常
        /// </summary>
        /// <param name="lstn"></param>
        /// <param name="ex"></param>
        private void WriteError(MessageListener lstn, Exception ex)
        {
            if (OnError != null)
            {
                var listenerKey = lstn.Channel.RemoteEndPoint.ToString();
                var error = new ApplicationException(string.Format("Notify listener ({0}) error.", listenerKey), ex);
                OnError(error);
            }
        }

        #region ILogable 成员

        /// <summary>
        /// 日志处理
        /// </summary>
        public event LogEventHandler OnLog;

        #endregion

        #region IErrorLogable 成员

        public event ErrorLogEventHandler OnError;

        #endregion
    }
}
