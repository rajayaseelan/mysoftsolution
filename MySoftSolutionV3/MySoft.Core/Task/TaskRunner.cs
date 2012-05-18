using System;
using System.Collections.Generic;
using System.Threading;
using MySoft.Logger;
using MySoft.Task.Configuration;

namespace MySoft.Task
{
    /// <summary>
    /// 计划任务执行者
    /// </summary>
    public class TaskRunner : MarshalByRefObject, ILogable
    {
        private TaskConfiguration cfg;

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, Job> JobList
        {
            get { return cfg.Jobs; }
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="job"></param>
        public void AddJob(Job job)
        {
            if (!job.IsRegisterLog) job.OnLog += OnLog;
            cfg.Jobs.Add(job.Name, job);
        }

        /// <summary>
        /// 实例化TaskRunner
        /// </summary>
        public TaskRunner()
        {
            this.cfg = new TaskConfiguration();
        }

        /// <summary>
        /// 实例化TaskRunner
        /// </summary>
        public TaskRunner(TaskConfiguration cfg)
        {
            this.cfg = cfg;
        }

        /// <summary>
        /// 执行计划任务
        /// </summary>
        public void RunSchemeTask()
        {
            if (cfg != null)
            {
                Dictionary<string, Job> jobs = cfg.Jobs;
                Dictionary<string, Thread> threads = TaskThreadPool.Instance.Threads;

                foreach (KeyValuePair<string, Job> kvp in jobs)
                {
                    Job job = kvp.Value;

                    if (!job.IsRegisterLog) job.OnLog += OnLog;

                    if (!threads.ContainsKey(job.Name))
                    {
                        job.State = JobState.Running;
                        Thread thread = new Thread(job.Execute);
                        thread.IsBackground = true;
                        threads[job.Name] = thread;
                        thread.Start();

                        WriteLog(string.Format("计划任务[{0}]已启动，服务类名：{1}，程序集：{2}", job.Name, job.ClassName, job.AssemblyName), LogType.Information);
                    }
                }
            }
            else
            {
                WriteLog("当前配置文件没有设置要执行的计划任务！", LogType.Warning);
            }
        }

        /// <summary>
        /// 启动指定任务
        /// </summary>
        /// <param name="jobName"></param>
        public void Start(string jobName)
        {
            if (cfg == null) return;
            if (cfg.Jobs == null) return;
            if (cfg.Jobs.Count == 0) return;

            if (cfg.Jobs.ContainsKey(jobName))
            {
                Job job = cfg.Jobs[jobName];

                if (job.State == JobState.Stop)
                {
                    job.State = JobState.Running;

                    Thread thread = TaskThreadPool.Instance.Threads[job.Name];
                    thread = new Thread((job.Execute));
                    thread.IsBackground = true;
                    thread.Start();

                    WriteLog(string.Format("计划任务[{0}]已启动，服务类名：{1}，程序集：{2}", job.Name, job.ClassName, job.AssemblyName), LogType.Information);
                }
            }
        }

        /// <summary>
        /// 停止指定任务
        /// </summary>
        /// <param name="jobName"></param>
        public void Stop(string jobName)
        {
            if (cfg == null) return;
            if (cfg.Jobs == null) return;
            if (cfg.Jobs.Count == 0) return;

            if (cfg.Jobs.ContainsKey(jobName))
            {
                Job job = cfg.Jobs[jobName];

                if (job.State == JobState.Running)
                {
                    job.State = JobState.Stop;

                    try
                    {
                        TaskThreadPool.Instance.Threads[job.Name].Abort();
                    }
                    catch { }

                    WriteLog(string.Format("计划任务[{0}]已停止，服务类名：{1}，程序集：{2}", job.Name, job.ClassName, job.AssemblyName), LogType.Warning);
                }
            }
        }

        /// <summary>
        /// 启动所有任务
        /// </summary>
        public void StartAll()
        {
            foreach (KeyValuePair<string, Job> kvp in cfg.Jobs)
            {
                Start(kvp.Key);
            }
        }

        /// <summary>
        /// 停止所有任务
        /// </summary>
        public void StopAll()
        {
            foreach (KeyValuePair<string, Job> kvp in cfg.Jobs)
            {
                Stop(kvp.Key);
            }
        }

        #region ILogable 成员

        /// <summary>
        /// 
        /// </summary>
        public event LogEventHandler OnLog;

        #endregion

        void WriteLog(string log, LogType type)
        {
            if (OnLog != null)
            {
                OnLog(log, type);
            }
        }
    }
}
