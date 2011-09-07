using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC.Status
{
    /// <summary>
    /// 性能监控进程项
    /// </summary>
    [Serializable]
    internal class PerfProcess
    {
        public PerfProcess(string name, int processId, string caption, string path)
        {
            this.name = name;
            this.processId = processId;
            this.caption = caption;
            this.path = path;
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private int processId;
        public int ProcessId
        {
            get { return processId; }
            set { processId = value; }
        }

        private string caption;
        public string Caption
        {
            get { return caption; }
            set { caption = value; }
        }

        private string path;
        public string Path
        {
            get { return path; }
            set { path = value; }
        }
    }
}
