using MySoft.Logger;
using System;
using System.Threading;

namespace MySoft.IoC
{
    /// <summary>
    /// Timer manager.
    /// </summary>
    public class TimerManager : IErrorLogable, IDisposable
    {
        public event ErrorLogEventHandler OnError;

        private MySoft.IoC.Communication.Threading.Timer timer;
        private TimerCallback callback;
        private object state;

        /// <summary>
        /// TimerManager
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="callback"></param>
        public TimerManager(TimeSpan ts, TimerCallback callback)
        {
            this.callback = callback;
            this.timer = new MySoft.IoC.Communication.Threading.Timer((int)ts.TotalMilliseconds);
            this.timer.Elapsed += timer_Elapsed;
        }

        /// <summary>
        /// TimerManager
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public TimerManager(TimeSpan ts, TimerCallback callback, object state)
            : this(ts, callback)
        {
            this.state = state;
        }

        /// <summary>
        /// 启动一个时间控件
        /// </summary>
        /// <param name="ts"></param>
        public void Start()
        {
            if (timer == null) return;

            timer.Start();
        }

        /// <summary>
        /// Timer stop.
        /// </summary>
        public void Stop()
        {
            if (timer == null) return;

            timer.Stop();
        }

        void timer_Elapsed(object sender, EventArgs e)
        {
            if (timer == null) return;

            timer.Stop();

            try
            {
                callback(state);
            }
            catch (Exception ex)
            {
                if (OnError != null) OnError(ex);
            }
            finally
            {
                timer.Start();
            }
        }

        /// <summary>
        /// Dispose timer.
        /// </summary>
        public void Dispose()
        {
            if (timer == null) return;

            timer.Stop();

            timer.Elapsed -= timer_Elapsed;
            timer = null;
        }
    }
}
