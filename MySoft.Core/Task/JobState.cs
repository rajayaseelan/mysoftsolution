using System;

namespace MySoft.Task
{
    /// <summary>
    /// 任务状态
    /// </summary>
    [Serializable]
    public enum JobState
    {
        /// <summary>
        /// 正在运行
        /// </summary>
        Running = 0,

        /// <summary>
        /// 停止
        /// </summary>
        Stop = 1
    }
}
