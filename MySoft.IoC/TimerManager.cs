using MySoft.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// <param name="callback"></param>
        /// <param name="ts"></param>
        public TimerManager(TimerCallback callback, TimeSpan ts)
        {
            this.callback = callback;
            this.timer = new MySoft.IoC.Communication.Threading.Timer((int)ts.TotalMilliseconds);
            this.timer.Elapsed += timer_Elapsed;
        }

        /// <summary>
        /// TimerManager
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <param name="ts"></param>
        public TimerManager(TimerCallback callback, object state, TimeSpan ts)
            : this(callback, ts)
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
