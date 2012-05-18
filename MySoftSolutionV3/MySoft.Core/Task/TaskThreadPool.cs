using System.Collections.Generic;
using System.Threading;
using MySoft.Task.Configuration;

namespace MySoft.Task
{
    /// <summary>
    /// 任务线程池
    /// </summary>
    public class TaskThreadPool
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly TaskThreadPool Instance = new TaskThreadPool();

        private Dictionary<string, Thread> _Threads;

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, Thread> Threads
        {
            get { return _Threads; }
            set { _Threads = value; }
        }

        Dictionary<string, Job> jobs = TaskConfiguration.GetConfig().Jobs;

        private TaskThreadPool()
        {
            _Threads = new Dictionary<string, Thread>();

            Thread thread = new Thread(RunTask);
            thread.Start();
        }

        void RunTask()
        {
            //检查线程状态
            foreach (KeyValuePair<string, Job> kvp in jobs)
            {
                Job job = kvp.Value;

                if (_Threads.ContainsKey(job.Name))
                {
                    Thread thread = _Threads[job.Name];

                    if (thread.ThreadState == ThreadState.Stopped && job.State == JobState.Running)
                    {
                        thread.Start();
                    }
                }
            }

            //检查线程池，删除无用线程
            foreach (KeyValuePair<string, Thread> kvp in _Threads)
            {
                if (!jobs.ContainsKey(kvp.Key))
                {
                    _Threads.Remove(kvp.Key);
                }
            }
        }
    }
}
