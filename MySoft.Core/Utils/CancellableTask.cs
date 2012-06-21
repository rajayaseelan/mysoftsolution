using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MySoft
{
    /// <summary>
    /// 支持Cancel的任务
    /// </summary>
    public class CancellableTask
    {
        public delegate object WorkCallback(object arg);
        public delegate void CancelCallback(object state);

        protected class TimeoutState
        {
            internal Thread thread;
            internal object state;

            public TimeoutState(Thread thread, object state)
            {
                this.thread = thread;
                this.state = state;
            }
        }

        protected WorkCallback workCallback;
        protected CancelCallback cancelCallback;
        protected WorkCallback wrapper;

        /// <summary>
        /// 实例化CancellableTask
        /// </summary>
        /// <param name="workCallback"></param>
        public CancellableTask(WorkCallback workCallback)
        {
            this.workCallback = workCallback;
        }

        /// <summary>
        /// 实例化CancellableTask
        /// </summary>
        /// <param name="workCallback"></param>
        /// <param name="cancelCallback"></param>
        public CancellableTask(WorkCallback workCallback, CancelCallback cancelCallback)
        {
            this.workCallback = workCallback;
            this.cancelCallback = cancelCallback;
        }

        /// <summary>
        /// 开始调用委托
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="asyncCallback"></param>
        /// <param name="state"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public IAsyncResult BeginInvoke(object arg, AsyncCallback asyncCallback, object state, int timeout)
        {
            wrapper = delegate(object argv)
            {
                AutoResetEvent e = new AutoResetEvent(false);

                try
                {
                    TimeoutState waitOrTimeoutState = new TimeoutState(Thread.CurrentThread, state);

                    ThreadPool.RegisterWaitForSingleObject(e, WaitOrTimeout, waitOrTimeoutState, timeout, true);

                    return workCallback(argv);
                }
                finally
                {
                    e.Set();
                }
            };

            IAsyncResult asyncResult = wrapper.BeginInvoke(arg, asyncCallback, state);

            return asyncResult;
        }

        /// <summary>
        /// 结束委托
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public object EndInvoke(IAsyncResult result)
        {
            return wrapper.EndInvoke(result);
        }

        /// <summary>
        /// 等待超时
        /// </summary>
        /// <param name="state"></param>
        /// <param name="isTimeout"></param>
        protected void WaitOrTimeout(object state, bool isTimeout)
        {
            try
            {
                if (isTimeout)
                {
                    TimeoutState waitOrTimeoutState = state as TimeoutState;

                    if (null != cancelCallback)
                    {
                        cancelCallback(waitOrTimeoutState.state);
                    }
                    else
                    {
                        waitOrTimeoutState.thread.Abort();
                    }
                }
            }
            catch { }
        }
    }
}
