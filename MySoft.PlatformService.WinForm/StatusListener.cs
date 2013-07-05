using ListControls;
using MySoft.IoC;
using MySoft.IoC.Messages;
using MySoft.Logger;
using System;
using System.Threading;
using System.Windows.Forms;

namespace MySoft.PlatformService.WinForm
{
    public class StatusListener : IStatusListener
    {
        public SynchronizationContext Context { get; set; }

        private TabControl control;
        private MessageListBox box1;
        private MessageListBox box2;
        private MessageListBox box3;
        private int rowCount;
        private int outCount;
        private int warningTimeout;
        private int timeout;
        private bool writeLog;

        public StatusListener(TabControl control, MessageListBox box1, MessageListBox box2, MessageListBox box3, int rowCount, int outCount, int warningTimeout, int timeout, bool writeLog)
        {
            this.control = control;
            this.box1 = box1;
            this.box2 = box2;
            this.box3 = box3;
            this.rowCount = rowCount;
            this.outCount = outCount;
            this.warningTimeout = warningTimeout;
            this.timeout = timeout;
            this.writeLog = writeLog;
        }

        #region IStatusListener 成员

        //服务端定时状态信息
        public void Push(ServerStatus serverStatus) { }

        public void Push(ConnectInfo connectInfo)
        {
            if (connectInfo == null) return;

            Context.Post(state =>
            {
                if (!Convert.ToBoolean(control.Tag)) return;

                if (box1.Items.Count >= rowCount)
                {
                    box1.Items.RemoveAt(box1.Items.Count - 1);
                }

                var msgType = ParseMessageType.Info;
                if (!connectInfo.Connected)
                {
                    msgType = ParseMessageType.Error;
                }

                box1.Items.Insert(0,
                    new ParseMessageEventArgs
                    {
                        MessageType = msgType,
                        LineHeader = string.Format("【{0}】 {1}:{2} {3}", connectInfo.ConnectTime, connectInfo.IPAddress, connectInfo.Port, connectInfo.Connected ? "连接" : "断开"),
                        MessageText = string.Format("{0}:{1} {4} {2}:{3}", connectInfo.IPAddress, connectInfo.Port, connectInfo.ServerIPAddress, connectInfo.ServerPort, connectInfo.Connected ? "Connect to" : "Disconnect from"),
                        Source = connectInfo
                    });

                box1.Invalidate();
                control.TabPages[1].Text = "连接信息(" + box1.Items.Count + ")";

                if (writeLog)
                {
                    var item = box1.Items[0];
                    var message = string.Format("{0}\r\n{1}", item.LineHeader, item.MessageText);
                    SimpleLog.Instance.WriteLogForDir("ConnectInfo", message);
                }
            }, null);
        }

        public void Change(string ipAddress, int port, AppClient appClient)
        {
            if (appClient == null) return;

            Context.Post(state =>
            {
                if (!Convert.ToBoolean(control.Tag)) return;

                for (int i = 0; i < box1.Items.Count; i++)
                {
                    var args = box1.Items[i];
                    if (args.Source == null) continue;

                    var connect = args.Source as ConnectInfo;
                    if (connect.AppName == null && connect.IPAddress == ipAddress && connect.Port == port)
                    {
                        connect.IPAddress = appClient.IPAddress;
                        connect.AppName = appClient.AppName;
                        connect.HostName = appClient.HostName;

                        args.LineHeader += string.Format("  【{0} <=> {1}】", appClient.AppName, appClient.HostName);
                        break;
                    }
                }

                box1.Invalidate();
            }, null);
        }

        public void Push(CallTimeout callTimeout)
        {
            if (callTimeout == null) return;

            Context.Post(state =>
            {
                if (!Convert.ToBoolean(control.Tag)) return;

                if (box2.Items.Count >= rowCount)
                {
                    box2.Items.RemoveAt(box2.Items.Count - 1);
                }

                var msgType = ParseMessageType.None;
                if (callTimeout.ElapsedTime >= timeout)
                    msgType = ParseMessageType.Error;
                else if (callTimeout.ElapsedTime >= warningTimeout)
                    msgType = ParseMessageType.Warning;
                else if (callTimeout.Count >= outCount)
                    msgType = ParseMessageType.Question;

                box2.Items.Insert(0,
                    new ParseMessageEventArgs
                    {
                        MessageType = msgType,
                        LineHeader = string.Format("【{0}】 [{3}] Timeout => ({1} rows)：{2} ms.", callTimeout.Caller.CallTime, callTimeout.Count, callTimeout.ElapsedTime, callTimeout.Caller.AppName),
                        MessageText = string.Format("{0},{1}", callTimeout.Caller.ServiceName, callTimeout.Caller.MethodName),
                        // + "\r\n" + callTimeout.Caller.Parameters
                        Source = callTimeout
                    });

                box2.Invalidate();
                control.TabPages[2].Text = "警告信息(" + box2.Items.Count + ")";

                if (writeLog && (msgType == ParseMessageType.Error || callTimeout.Count >= outCount))
                {
                    var item = box2.Items[0];
                    var message = string.Format("{0}\r\n{1}\r\n{2}", item.LineHeader, item.MessageText,
                        (item.Source as CallTimeout).Caller.Parameters);

                    if (callTimeout.Count >= outCount)
                        SimpleLog.Instance.WriteLogForDir("CallCount", message);

                    if (msgType == ParseMessageType.Error)
                        SimpleLog.Instance.WriteLogForDir("CallTimeout", message);
                }
            }, null);
        }

        public void Push(CallError callError)
        {
            if (callError == null) return;

            Context.Post(state =>
            {
                if (!Convert.ToBoolean(control.Tag)) return;

                if (box3.Items.Count >= rowCount)
                {
                    box3.Items.RemoveAt(box3.Items.Count - 1);
                }

                box3.Items.Insert(0,
                    new ParseMessageEventArgs
                    {
                        MessageType = ParseMessageType.Error,
                        LineHeader = string.Format("【{0}】 [{2}] Error => {1}", callError.Caller.CallTime, callError.Message, callError.Caller.AppName),
                        MessageText = string.Format("{0},{1}", callError.Caller.ServiceName, callError.Caller.MethodName),
                        //+ "\r\n" + callError.Caller.Parameters
                        Source = callError
                    });

                box3.Invalidate();
                control.TabPages[3].Text = "异常信息(" + box3.Items.Count + ")";

                if (writeLog)
                {
                    var item = box3.Items[0];
                    var message = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}", item.LineHeader, item.MessageText,
                        (item.Source as CallError).Caller.Parameters, (item.Source as CallError).Error);
                    SimpleLog.Instance.WriteLogForDir("CallError", message);
                }
            }, null);
        }

        #endregion
    }
}
