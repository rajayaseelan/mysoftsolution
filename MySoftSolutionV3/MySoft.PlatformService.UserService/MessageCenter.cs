using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace MySoft.PlatformService.UserService
{
    /// <summary>
    /// 消息中心；
    /// </summary>
    class MessageCenter
    {
        #region MessageCenter 的单例实现
        private static readonly object _syncLock = new object();//线程同步锁；
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

        /// <summary>
        /// 保证单例的私有构造函数；
        /// </summary>
        private MessageCenter() { }

        #endregion

        public event EventHandler<MessageListenerEventArgs> ListenerAdded;

        public event EventHandler<MessageListenerEventArgs> ListenerRemoved;

        public event EventHandler<MessageNotifyErrorEventArgs> NotifyError;

        private List<MessageListener> _listeners = new List<MessageListener>(0);

        public void AddListener(MessageListener listener)
        {
            lock (_syncLock)
            {
                if (_listeners.Contains(listener))
                {
                    throw new InvalidOperationException("重复注册相同的监听器！");
                }
                _listeners.Add(listener);
            }

            if (this.ListenerAdded != null)
            {
                this.ListenerAdded(this, new MessageListenerEventArgs(listener));
            }
        }

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
                    throw new InvalidOperationException("要移除的监听器不存在！");
                }
            }
            if (this.ListenerRemoved != null)
            {
                this.ListenerRemoved(this, new MessageListenerEventArgs(listener));
            }
        }

        public void NotifyMessage(string message)
        {
            MessageListener[] listeners = _listeners.ToArray();
            foreach (MessageListener lstn in listeners)
            {
                try
                {
                    lstn.Notify(message);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine(ex.Message);
                    _listeners.Remove(lstn);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    OnNotifyError(lstn, ex);
                }
            }
        }

        private void OnNotifyError(MessageListener listener, Exception error)
        {
            if (this.NotifyError == null)
            {
                return;
            }
            MessageNotifyErrorEventArgs args = new MessageNotifyErrorEventArgs(listener, error);
            ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                this.NotifyError(this, state as MessageNotifyErrorEventArgs);
            }, args);
        }
    }
}
