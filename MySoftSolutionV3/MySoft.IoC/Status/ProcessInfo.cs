namespace MySoft.IoC.Status
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// 进程信息
    /// </summary>
    [Serializable]
    public class ProcessInfo
    {
        #region Fields

        /// <summary>
        /// cpu使用率
        /// </summary>
        public int CpuUsage { get; set; }

        /// <summary>
        /// 进程I描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 进程ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 进程名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 进程路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 进程标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 进程使用内存
        /// </summary>
        public long WorkingSet { get; set; }

        /// <summary>
        /// 线程数
        /// </summary>
        public int ThreadCount { get; set; }

        #endregion Fields
    }
}