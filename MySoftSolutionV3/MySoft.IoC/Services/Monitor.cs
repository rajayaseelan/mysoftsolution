namespace MySoft.IoC.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Management;
    using System.Text;
    using System.Timers;
    using System.Linq;
    using MySoft.IoC.Status;

    internal class Monitor
    {
        #region Fields

        private Dictionary<int, TimeSpan> _CPULimitExceeded;
        private Dictionary<int, TimeSpan> _MemoryLimitExceeded;
        private Dictionary<int, ProcessInfo> _ProcessInfoMap;
        private ManagementObjectSearcher _Searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT Name, ProcessId, Caption, ExecutablePath FROM Win32_Process");

        private ManagementObjectSearcher _Searcher2 =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT IDProcess, Name, PercentProcessorTime, ThreadCount, Description, ThreadCount, WorkingSet FROM Win32_PerfFormattedData_PerfProc_Process");
        private Timer _Timer;

        #endregion Fields

        #region Delegates

        public delegate void ProcessUpdate(List<ProcessInfo> processes);

        #endregion Delegates

        #region Events

        public event ProcessUpdate Changed;

        #endregion Events

        #region Methods

        public void Start()
        {
            _CPULimitExceeded = new Dictionary<int, TimeSpan>();
            _MemoryLimitExceeded = new Dictionary<int, TimeSpan>();
            _ProcessInfoMap = new Dictionary<int, ProcessInfo>();

            _Timer = new Timer(5000);
            _Timer.Elapsed += new ElapsedEventHandler(_Timer_Elapsed);
            _Timer.Start();
        }

        public void Stop()
        {
            _Searcher.Dispose();
            _CPULimitExceeded.Clear();

            _Timer.Stop();
            _Timer.Dispose();
        }

        private List<ProcessInfo> GetUsage()
        {
            IDictionary<int, PerfProcess> processlist = new Dictionary<int, PerfProcess>();
            foreach (ManagementObject queryObj in _Searcher.Get())
            {
                string name = Convert.ToString(queryObj["Name"]);
                int Id = Convert.ToInt32(queryObj["ProcessId"]);
                string caption = Convert.ToString(queryObj["Caption"]);
                string path = Convert.ToString(queryObj["ExecutablePath"]);

                processlist[Id] = new PerfProcess(name, Id, caption, path);
            }

            var processes = new List<ProcessInfo>();
            foreach (ManagementObject queryObj in _Searcher2.Get())
            {
                var process = new ProcessInfo
                {
                    Id = Convert.ToInt32(queryObj["IDProcess"]),
                    Name = Convert.ToString(queryObj["Name"]),
                    CpuUsage = Convert.ToInt32(queryObj["PercentProcessorTime"]),
                    Description = Convert.ToString(queryObj["Description"]),
                    WorkingSet = Convert.ToInt64(queryObj["WorkingSet"]),
                    ThreadCount = Convert.ToInt32(queryObj["ThreadCount"])
                };

                if (processlist.ContainsKey(process.Id))
                {
                    process.Title = processlist[process.Id].Name;
                    process.Path = processlist[process.Id].Path;
                }

                if (process.Id > 0)
                    processes.Add(process);
            }

            return processes;
        }

        void _Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var processes = GetUsage();

                if (Changed != null)
                    Changed(processes);
            }
            catch (Exception ex)
            {

            }
        }

        #endregion Methods
    }
}