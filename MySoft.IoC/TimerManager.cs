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
        public TimerManager(TimerCallback callback)
        {
            this.callback = callback;
        }

        /// <summary>
        /// TimerManager
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public TimerManager(TimerCallback callback, object state)
            : this(callback)
        {
            this.state = state;
        }

        /// <summary>
        /// 启动一个时间控件
        /// </summary>
        /// <param name="ts"></param>
        public void Start(TimeSpan ts)
        {
            if (timer != null)
            {
                timer.Start();
                return;
            }

            this.timer = new MySoft.IoC.Communication.Threading.Timer((int)ts.TotalMilliseconds);

            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        /// <summary>
        /// Timer stop.
        /// </summary>
        public void Stop()
        {
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
