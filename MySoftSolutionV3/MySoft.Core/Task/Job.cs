using System;
using System.Reflection;
using System.Threading;
using MySoft.Logger;

namespace MySoft.Task
{
    /// <summary>
    /// 任务实体
    /// </summary>
    [Serializable]
    public class Job : ILogable
    {
        /// <summary>
        /// 事件处理日志
        /// </summary>
        public event LogEventHandler OnLog;

        /// <summary>
        /// 是否注册了日志
        /// </summary>
        internal bool IsRegisterLog
        {
            get
            {
                return OnLog != null;
            }
        }

        private string _Name;

        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private DateTime _BeginDate;

        /// <summary>
        /// 任务开始日期
        /// </summary>
        public DateTime BeginDate
        {
            get { return _BeginDate; }
            set { _BeginDate = value; }
        }

        private DateTime _EndDate;

        /// <summary>
        /// 任务结束日期
        /// </summary>
        public DateTime EndDate
        {
            get { return _EndDate; }
            set { _EndDate = value; }
        }

        private string _BeginTime;

        /// <summary>
        /// 任务开始时间
        /// </summary>
        public string BeginTime
        {
            get { return _BeginTime; }
            set { _BeginTime = value; }
        }

        private string _EndTime;

        /// <summary>
        /// 任务结束时间
        /// </summary>
        public string EndTime
        {
            get { return _EndTime; }
            set { _EndTime = value; }
        }


        private int _Interval;

        /// <summary>
        /// 任务循环执行时间间隔（单位：毫秒）
        /// </summary>
        public int Interval
        {
            get { return _Interval; }
            set { _Interval = value; }
        }

        private string _AssemblyName;

        /// <summary>
        /// 程序集名称
        /// </summary>
        public string AssemblyName
        {
            get { return _AssemblyName; }
            set { _AssemblyName = value; }
        }

        private string _ClassName;

        /// <summary>
        /// 类名全称（任务执行入口方法在该类里面定义）
        /// </summary>
        public string ClassName
        {
            get { return _ClassName; }
            set { _ClassName = value; }
        }

        private JobState _State = JobState.Stop;

        /// <summary>
        /// 任务状态
        /// </summary>
        public JobState State
        {
            get { return _State; }
            set { _State = value; }
        }

        private string _LatestRunTime;

        /// <summary>
        /// 最近一次运行时间
        /// </summary>
        public string LatestRunTime
        {
            get { return _LatestRunTime; }
            set { _LatestRunTime = value; }
        }

        private bool _IsStopIfException = false;

        private MySoftException _LatestException = null;

        /// <summary>
        /// 任务运行时最近发生的异常
        /// </summary>
        public MySoftException LatestException
        {
            get { return _LatestException; }
            set { _LatestException = value; }
        }

        private int _ExceptionCount = 0;

        /// <summary>
        /// 异常计数
        /// </summary>
        public int ExceptionCount
        {
            get { return _ExceptionCount; }
            set { _ExceptionCount = value; }
        }

        /// <summary>
        /// 根据当前时间判断任务是否需要执行
        /// </summary>
        public bool IsRun()
        {
            DateTime now = DateTime.Now;
            TimeSpan nowTime = now.TimeOfDay;
            TimeSpan theTime = TimeSpan.Zero;

            if (now < _BeginDate)
            {
                return false;
            }

            if (now > _EndDate)
            {
                return false;
            }

            theTime = TimeSpan.Parse(_BeginTime);
            if (nowTime < theTime)
            {
                return false;
            }

            theTime = TimeSpan.Parse(_EndTime);
            if (nowTime > theTime)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        public void Execute()
        {
            while (true)
            {
                if (this.IsRun() && this._State == JobState.Running)
                {
                    //记录最近执行时间
                    _LatestRunTime = DateTime.Now.ToString();

                    try
                    {
                        WriteLog(string.Format("正在执行任务[{0}]......", this.Name), LogType.Information);

                        //执行任务
                        Type type = Type.GetType(string.Format("{0}, {1}", _ClassName, _AssemblyName));
                        object obj = Activator.CreateInstance(type);
                        ITask task = obj as ITask;
                        task.Run();

                        WriteLog(string.Format("执行任务[{0}]成功！", this.Name), LogType.Information);
                    }
                    catch (Exception ex)
                    {
                        if (_IsStopIfException)
                        {
                            _State = JobState.Stop;
                            TaskThreadPool.Instance.Threads[_Name].Abort();
                        }

                        _ExceptionCount = _ExceptionCount + 1;
                        _LatestException = new MySoftException(ExceptionType.TaskException, "Task任务执行失败！", ex);

                        WriteLog(string.Format("执行任务[{0}]失败，错误：{1}！", this.Name, ex.Message), LogType.Error);
                    }
                }

                Thread.Sleep(_Interval);
            }
        }

        void WriteLog(string log, LogType type)
        {
            if (OnLog != null)
            {
                OnLog(log, type);
            }
        }
    }
}
