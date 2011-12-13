using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ListControls;
using MySoft.IoC;
using MySoft.IoC.Status;
using MySoft.Logger;
using System.Net.Sockets;

namespace MySoft.PlatformService.WinForm
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
            CastleFactory.Create().OnError += new ErrorLogEventHandler(frmMain_OnError);
        }

        void frmMain_OnError(Exception error)
        {
            //发生错误SocketException为网络断开
            if (error is SocketException)
            {
                button1_Click(null, EventArgs.Empty);
            }
        }

        private IStatusService service;
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (button1.Tag == null)
                {
                    var listener = new StatusListener(tabControl1, listBox1, listBox2, listBox3,
                        Convert.ToInt32(numericUpDown3.Value), checkBox4.Checked);
                    service = CastleFactory.Create().GetChannel<IStatusService>(listener);

                    //var services = service.GetServiceList();

                    var options = new SubscribeOptions
                    {
                        PushCallError = checkBox1.Checked,
                        PushCallTimeout = checkBox2.Checked,
                        PushServerStatus = checkBox3.Checked,
                        CallTimeout = Convert.ToDouble(numericUpDown1.Value) / 1000,
                        StatusTimer = Convert.ToInt32(numericUpDown2.Value)
                    };
                    service.Subscribe(options);
                    button1.Text = "停止监控";
                    button1.Tag = label1.Text;
                    label1.Text = "正在进行监控...";

                    checkBox1.Enabled = false;
                    checkBox2.Enabled = false;
                    checkBox3.Enabled = false;

                    numericUpDown1.Enabled = checkBox2.Enabled && checkBox2.Checked;
                    numericUpDown2.Enabled = checkBox3.Enabled && checkBox3.Checked;

                    checkBox4.Enabled = false;
                    numericUpDown3.Enabled = false;
                    //button1.Enabled = false;
                }
                else
                {
                    if (sender != null) service.Unsubscribe();
                    label1.Text = button1.Tag.ToString();
                    button1.Text = "开始监控";
                    button1.Tag = null;
                    listBox1.Items.Clear();
                    listBox2.Items.Clear();
                    listBox3.Items.Clear();

                    listBox1.Invalidate();
                    listBox2.Invalidate();
                    listBox3.Invalidate();

                    tabControl1.TabPages[0].Text = "连接信息";
                    tabControl1.TabPages[2].Text = "异常信息";
                    tabControl1.TabPages[1].Text = "超时信息";

                    checkBox1.Enabled = true;
                    checkBox2.Enabled = true;
                    checkBox3.Enabled = true;

                    numericUpDown1.Enabled = checkBox2.Enabled && checkBox2.Checked;
                    numericUpDown2.Enabled = checkBox3.Enabled && checkBox3.Checked;
                    richTextBox1.Clear();
                    richTextBox2.Clear();

                    try { webBrowser1.Document.GetElementsByTagName("body")[0].InnerHtml = string.Empty; }
                    catch { }

                    checkBox4.Enabled = true;
                    numericUpDown3.Enabled = true;
                    //button1.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDown2.Enabled = checkBox3.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDown1.Enabled = checkBox2.Checked;
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox3.SelectedIndex < 0) richTextBox1.Text = string.Empty;

            var args = listBox3.SelectedItem as ParseMessageEventArgs;
            var source = args.Source as CallTimeout;
            AppendText(richTextBox1, source.Caller);
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex < 0) webBrowser1.Text = string.Empty;

            var args = listBox2.SelectedItem as ParseMessageEventArgs;
            var source = args.Source as CallError;
            try { webBrowser1.Document.GetElementsByTagName("body")[0].InnerHtml = string.Empty; }
            catch { }
            webBrowser1.Document.Write(source.Description);

            AppendText(richTextBox2, source.Caller);
        }

        private void AppendText(RichTextBox rich, AppCaller caller)
        {
            rich.Clear();
            rich.SelectionIndent = 0;
            rich.SelectionColor = Color.Blue;
            rich.AppendText("ServiceName:\r\n");
            rich.SelectionColor = Color.Black;
            rich.SelectionIndent = 20;
            rich.AppendText(caller.ServiceName);
            rich.AppendText("\r\n\r\n");
            rich.SelectionIndent = 0;
            rich.SelectionColor = Color.Blue;
            rich.AppendText("MethodName:\r\n");
            rich.SelectionColor = Color.Black;
            rich.SelectionIndent = 20;
            rich.AppendText(caller.MethodName);
            rich.AppendText("\r\n\r\n");
            rich.SelectionIndent = 0;
            rich.SelectionColor = Color.Blue;
            rich.AppendText("Parameters:\r\n");
            rich.SelectionColor = Color.Black;
            rich.SelectionIndent = 20;
            rich.AppendText(caller.Parameters);
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            InitBrowser();
        }

        /// <summary>
        /// 初始化浏览器
        /// </summary>
        void InitBrowser()
        {
            webBrowser1.Url = new Uri("about:blank");
            webBrowser1.AllowNavigation = false;
            webBrowser1.IsWebBrowserContextMenuEnabled = false;
        }
    }

    public class StatusListener : IStatusListener
    {
        private TabControl control;
        private MessageListBox box1;
        private MessageListBox box2;
        private MessageListBox box3;
        private int rowCount;
        private bool writeLog;
        public StatusListener(TabControl control, MessageListBox box1, MessageListBox box2, MessageListBox box3, int rowCount, bool writeLog)
        {
            this.control = control;
            this.box1 = box1;
            this.box2 = box2;
            this.box3 = box3;
            this.rowCount = rowCount;
            this.writeLog = writeLog;
        }

        #region IStatusListener 成员

        public void Push(IList<ClientInfo> clientInfos)
        {
            //throw new NotImplementedException();
        }

        public void Push(ConnectInfo connectInfo)
        {
            box1.BeginInvoke(new Action(() =>
            {
                if (box1.Items.Count >= rowCount)
                {
                    box1.Items.RemoveAt(box1.Items.Count - 1);
                }

                box1.Items.Insert(0,
                    new ParseMessageEventArgs
                    {
                        MessageType = ParseMessageType.Info,
                        LineHeader = string.Format("【{0}】 {1}:{2} {3}", connectInfo.ConnectTime, connectInfo.IPAddress, connectInfo.Port, connectInfo.Connected ? "连接" : "断开"),
                        MessageText = string.Format("{0}:{1} {4} {2}:{3}", connectInfo.IPAddress, connectInfo.Port, connectInfo.ServerIPAddress, connectInfo.ServerPort, connectInfo.Connected ? "Connect to" : "Disconnect from"),
                        Source = connectInfo
                    });

                box1.Invalidate();
                control.TabPages[0].Text = "连接信息(" + box1.Items.Count + ")";

                if (writeLog)
                {
                    var item = box1.Items[0];
                    var message = string.Format("{0}\r\n{1}", item.LineHeader, item.MessageText);
                    SimpleLog.Instance.WriteLogForDir("ConnectInfo", message);
                }
            }));
        }

        public void Push(string ipAddress, AppClient appClient)
        {
            box1.BeginInvoke(new Action(() =>
            {
                for (int i = 0; i < box1.Items.Count; i++)
                {
                    var args = box1.Items[i];
                    var connect = args.Source as ConnectInfo;
                    if (connect.AppName == null)
                    {
                        connect.AppName = appClient.AppName;
                        connect.HostName = appClient.HostName;
                        args.MessageText = string.Format("[{0}] ", appClient.AppName) + args.MessageText;
                    }
                }

                box1.Invalidate();
            }));
        }

        public void Push(ServerStatus serverStatus)
        {
            //服务端定时状态信息
        }

        public void Push(CallError callError)
        {
            box2.BeginInvoke(new Action(() =>
            {
                if (box2.Items.Count >= rowCount)
                {
                    box2.Items.RemoveAt(box2.Items.Count - 1);
                }

                box2.Items.Insert(0,
                    new ParseMessageEventArgs
                    {
                        MessageType = ParseMessageType.Error,
                        LineHeader = string.Format("【{0}】 [{2}] Error => {1}", callError.CallTime, callError.Message, callError.Caller.AppName),
                        MessageText = string.Format("{0},{1}", callError.Caller.ServiceName, callError.Caller.MethodName),
                        //+ "\r\n" + callError.Caller.Parameters
                        Source = callError
                    });

                box2.Invalidate();
                control.TabPages[2].Text = "异常信息(" + box2.Items.Count + ")";

                if (writeLog)
                {
                    var item = box2.Items[0];
                    var message = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}", item.LineHeader, item.MessageText,
                        (item.Source as CallError).Caller.Parameters, (item.Source as CallError).Description);
                    SimpleLog.Instance.WriteLogForDir("CallError", message);
                }
            }));
        }

        public void Push(CallTimeout callTimeout)
        {
            //throw new NotImplementedException();

            box3.BeginInvoke(new Action(() =>
            {
                if (box3.Items.Count >= rowCount)
                {
                    box3.Items.RemoveAt(box3.Items.Count - 1);
                }

                box3.Items.Insert(0,
                    new ParseMessageEventArgs
                    {
                        MessageType = ParseMessageType.Warning,
                        LineHeader = string.Format("【{0}】 [{3}] Timeout => ({1} rows)：{2} ms.", callTimeout.CallTime, callTimeout.Count, callTimeout.ElapsedTime, callTimeout.Caller.AppName),
                        MessageText = string.Format("{0},{1}", callTimeout.Caller.ServiceName, callTimeout.Caller.MethodName),
                        // + "\r\n" + callTimeout.Caller.Parameters
                        Source = callTimeout
                    });

                box3.Invalidate();
                control.TabPages[1].Text = "超时信息(" + box3.Items.Count + ")";

                if (writeLog)
                {
                    var item = box3.Items[0];
                    var message = string.Format("{0}\r\n{1}\r\n{2}", item.LineHeader, item.MessageText,
                        (item.Source as CallTimeout).Caller.Parameters);
                    SimpleLog.Instance.WriteLogForDir("CallTimeout", message);
                }
            }));
        }

        #endregion
    }
}
