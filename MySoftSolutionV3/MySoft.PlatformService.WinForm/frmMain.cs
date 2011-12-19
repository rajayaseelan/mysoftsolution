using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ListControls;
using MySoft.IoC;
using MySoft.IoC.Status;
using MySoft.Logger;
using System.Net.Sockets;
using System.Linq;
using System.Threading;
using System.Configuration;

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
                //未连接状态，自动断开为SocketError.ConnectionReset
                if ((error as SocketException).SocketErrorCode == SocketError.NotConnected)
                {
                    button1_Click(null, EventArgs.Empty);

                    listConnect.Items.Insert(0,
                        new ParseMessageEventArgs
                        {
                            MessageType = ParseMessageType.Error,
                            LineHeader = string.Format("【{0}】 当前网络已经从服务器断开...", DateTime.Now),
                            MessageText = string.Format("({0}){1}", (error as SocketException).ErrorCode, (error as SocketException).Message)
                        });

                    listConnect.Invalidate();
                }
            }
        }

        private IStatusService service;
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (button1.Tag == null)
                {
                    var listener = new StatusListener(tabControl1, listConnect, listTimeout, listError,
                        Convert.ToInt32(numericUpDown3.Value), Convert.ToInt32(numericUpDown4.Value), checkBox4.Checked);
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
                    numericUpDown4.Enabled = false;
                    //button1.Enabled = false;
                }
                else
                {
                    if (sender != null) service.Unsubscribe();
                    label1.Text = button1.Tag.ToString();
                    button1.Text = "开始监控";
                    button1.Tag = null;
                    listConnect.Items.Clear();
                    listError.Items.Clear();
                    listTimeout.Items.Clear();

                    listConnect.Invalidate();
                    listError.Invalidate();
                    listTimeout.Invalidate();

                    tabPage1.Text = "连接信息";
                    tabPage2.Text = "超时信息";
                    tabPage3.Text = "异常信息";

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
                    numericUpDown4.Enabled = true;
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
            if (listTimeout.SelectedIndex < 0) richTextBox1.Text = string.Empty;

            var args = listTimeout.SelectedItem as ParseMessageEventArgs;
            var source = args.Source as CallTimeout;
            AppendText(richTextBox1, source.Caller);
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listError.SelectedIndex < 0) webBrowser1.Text = string.Empty;

            var args = listError.SelectedItem as ParseMessageEventArgs;
            var source = args.Source as CallError;
            try { webBrowser1.Document.GetElementsByTagName("body")[0].InnerHtml = string.Empty; }
            catch { }
            webBrowser1.Document.Write(source.HtmlError);
            AppendText(richTextBox2, source.Caller);
        }

        private void AppendText(RichTextBox rich, AppCaller caller)
        {
            rich.Clear();

            rich.SelectionIndent = 0;
            rich.SelectionColor = Color.Blue;
            rich.AppendText("ApplicationName:\r\n");
            rich.SelectionColor = Color.Black;
            rich.SelectionIndent = 20;
            rich.AppendText(caller.AppName);
            rich.AppendText("\r\n\r\n");

            rich.SelectionIndent = 0;
            rich.SelectionColor = Color.Blue;
            rich.AppendText("ServerName:\r\n");
            rich.SelectionColor = Color.Black;
            rich.SelectionIndent = 20;
            rich.AppendText(caller.IPAddress + "[" + caller.HostName + "]");
            rich.AppendText("\r\n\r\n");

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
            listAssembly.SelectedIndexChanged += new EventHandler(listAssembly_SelectedIndexChanged);
            listService.SelectedIndexChanged += new EventHandler(messageListBox1_SelectedIndexChanged);
            listMethod.SelectedIndexChanged += new EventHandler(messageListBox2_SelectedIndexChanged);
            listMethod.MouseDoubleClick += new MouseEventHandler(listMethod_MouseDoubleClick);

            InitBrowser();
            InitService();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!checkBox1.Enabled)
            {
                MessageBox.Show("请先停止监控！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
            }
        }

        void listMethod_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listService.SelectedIndex < 0) return;
            if (listMethod.SelectedIndex < 0) return;
            if (e.Button == MouseButtons.Left)
            {
                var item1 = listService.Items[listService.SelectedIndex];
                var item2 = listMethod.Items[listMethod.SelectedIndex];
                var service = item1.Source as ServiceInfo;
                var method = item2.Source as MethodInfo;

                SingletonMul.Show(string.Format("FORM_{0}_{1}", service.FullName, method.FullName), () =>
                {
                    frmInvoke frm = new frmInvoke(service.FullName, method.FullName, method.Parameters);
                    return frm;
                });
            }
        }

        /// <summary>
        /// 初始化浏览器
        /// </summary>
        void InitBrowser()
        {
            webBrowser1.Url = new Uri("about:blank");
            webBrowser1.AllowNavigation = false;
            webBrowser1.IsWebBrowserContextMenuEnabled = false;

            var url = ConfigurationManager.AppSettings["ServerMonitorUrl"];
            if (!string.IsNullOrEmpty(url))
                webBrowser2.Url = new Uri(url);
            else
                webBrowser2.Url = new Uri("about:blank");
            webBrowser2.AllowNavigation = false;
            webBrowser2.IsWebBrowserContextMenuEnabled = false;
        }

        private void InitService()
        {
            listAssembly.Items.Clear();
            listService.Items.Clear();
            listMethod.Items.Clear();
            richTextBox3.Clear();

            listAssembly.SelectedIndex = -1;
            listService.SelectedIndex = -1;
            listMethod.SelectedIndex = -1;

            listAssembly.Invalidate();
            listService.Invalidate();
            listMethod.Invalidate();

            tabPage0.Text = "服务信息";
            IList<ServiceInfo> services;
            try
            {
                services = CastleFactory.Create().GetChannel<IStatusService>()
                    .GetServiceList().OrderBy(p => p.Name).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var assemblies = services.GroupBy(prop => prop.Assembly)
                .Select(p => new { Name = p.Key.Split(',')[0], FullName = p.Key, Services = p.ToList() })
                .OrderBy(p => p.Name).ToList();
            tabPage0.Text = "服务信息(" + assemblies.Count + ")";
            listAssembly.Items.Clear();
            foreach (var assembly in assemblies)
            {
                listAssembly.Items.Add(
                    new ParseMessageEventArgs
                    {
                        MessageType = ParseMessageType.Info,
                        LineHeader = string.Format("{0} => ({1}) services", assembly.Name, assembly.Services.Count),
                        MessageText = string.Format("{0}", assembly.FullName),
                        Source = assembly.Services
                    });
            }

            listAssembly.Invalidate();
        }

        void listAssembly_SelectedIndexChanged(object sender, EventArgs e)
        {
            listService.Items.Clear();
            listMethod.Items.Clear();
            listService.SelectedIndex = -1;
            listMethod.SelectedIndex = -1;
            richTextBox3.Clear();

            if (listAssembly.SelectedIndex < 0)
            {
                listService.Invalidate();
                listMethod.Invalidate();
                return;
            }

            var item = listAssembly.Items[listAssembly.SelectedIndex];
            var services = (item.Source as IList<ServiceInfo>).OrderBy(p => p.Name).ToList();
            foreach (var service in services)
            {
                listService.Items.Add(
                    new ParseMessageEventArgs
                    {
                        MessageType = ParseMessageType.Info,
                        LineHeader = string.Format("{0} => ({1}) methods", service.Name, service.Methods.Count),
                        MessageText = string.Format("{0}", service.FullName),
                        Source = service
                    });
            }

            listService.Invalidate();
        }

        void messageListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listMethod.Items.Clear();
            listMethod.SelectedIndex = -1;
            richTextBox3.Clear();

            if (listService.SelectedIndex < 0)
            {
                listMethod.Invalidate();
                return;
            }

            var item = listService.Items[listService.SelectedIndex];
            var methods = (item.Source as ServiceInfo).Methods.OrderBy(p => p.Name).ToList();
            foreach (var method in methods)
            {
                listMethod.Items.Add(
                    new ParseMessageEventArgs
                    {
                        MessageType = ParseMessageType.Info,
                        LineHeader = string.Format("{0} => ({1}) parameters", method.Name, method.Parameters.Count),
                        MessageText = string.Format("{0}", method.FullName),
                        Source = method
                    });
            }

            listMethod.Invalidate();
        }

        void messageListBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            richTextBox3.Clear();
            if (listMethod.SelectedIndex < 0)
            {
                return;
            }

            var item = listMethod.Items[listMethod.SelectedIndex];
            var parameters = (item.Source as MethodInfo).Parameters;

            if (parameters.Count > 0)
            {
                richTextBox3.SelectionIndent = 0;
                richTextBox3.SelectionColor = Color.Blue;
                richTextBox3.AppendText("Parameters:");
                richTextBox3.AppendText("\r\n");
                foreach (var parameter in parameters)
                {
                    richTextBox3.SelectionColor = Color.Black;
                    richTextBox3.SelectionIndent = 20;
                    richTextBox3.AppendText("【" + parameter.Name + "】");
                    richTextBox3.AppendText(" => ");
                    richTextBox3.AppendText(parameter.TypeFullName);
                    richTextBox3.AppendText("\r\n");
                }
            }
        }

        /// <summary>
        /// 刷新服务信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 刷新服务信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InitService();
        }

        private void 添加此服务到监控AToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (checkBox1.Enabled)
            {
                MessageBox.Show("请先启动监控！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (listService.SelectedIndex < 0) return;

            try
            {
                var item = listService.Items[listService.SelectedIndex];
                service.SubscribeType((item.Source as ServiceInfo).FullName);

                MessageBox.Show("添加成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void 从监控中移除此服务OToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (checkBox1.Enabled)
            {
                MessageBox.Show("请先启动监控！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (listService.SelectedIndex < 0) return;

            try
            {
                var item = listService.Items[listService.SelectedIndex];
                service.UnsubscribeType((item.Source as ServiceInfo).FullName);

                MessageBox.Show("移除成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class StatusListener : IStatusListener
    {
        private TabControl control;
        private MessageListBox box1;
        private MessageListBox box2;
        private MessageListBox box3;
        private int rowCount;
        private int timeout;
        private bool writeLog;
        public StatusListener(TabControl control, MessageListBox box1, MessageListBox box2, MessageListBox box3, int rowCount, int timeout, bool writeLog)
        {
            this.control = control;
            this.box1 = box1;
            this.box2 = box2;
            this.box3 = box3;
            this.rowCount = rowCount;
            this.timeout = timeout;
            this.writeLog = writeLog;
        }

        #region IStatusListener 成员

        public void Push(IList<ClientInfo> clientInfos)
        {
            //客户端连接信息
        }


        public void Push(ServerStatus serverStatus)
        {
            //服务端定时状态信息
        }

        public void Push(ConnectInfo connectInfo)
        {
            box1.BeginInvoke(new Action(() =>
            {
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
            }));
        }

        public void Change(string ipAddress, int port, AppClient appClient)
        {
            box1.BeginInvoke(new Action(() =>
            {
                for (int i = 0; i < box1.Items.Count; i++)
                {
                    var args = box1.Items[i];
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
            }));
        }

        public void Push(CallTimeout callTimeout)
        {
            box2.BeginInvoke(new Action(() =>
            {
                if (box2.Items.Count >= rowCount)
                {
                    box2.Items.RemoveAt(box2.Items.Count - 1);
                }

                var msgType = ParseMessageType.Warning;
                if (callTimeout.ElapsedTime >= timeout)
                {
                    msgType = ParseMessageType.Error;
                }

                box2.Items.Insert(0,
                    new ParseMessageEventArgs
                    {
                        MessageType = msgType,
                        LineHeader = string.Format("【{0}】 [{3}] Timeout => ({1} rows)：{2} ms.", callTimeout.CallTime, callTimeout.Count, callTimeout.ElapsedTime, callTimeout.Caller.AppName),
                        MessageText = string.Format("{0},{1}", callTimeout.Caller.ServiceName, callTimeout.Caller.MethodName),
                        // + "\r\n" + callTimeout.Caller.Parameters
                        Source = callTimeout
                    });

                box2.Invalidate();
                control.TabPages[2].Text = "超时信息(" + box2.Items.Count + ")";

                if (writeLog && msgType == ParseMessageType.Error)
                {
                    var item = box2.Items[0];
                    var message = string.Format("{0}\r\n{1}\r\n{2}", item.LineHeader, item.MessageText,
                        (item.Source as CallTimeout).Caller.Parameters);
                    SimpleLog.Instance.WriteLogForDir("CallTimeout", message);
                }
            }));
        }

        public void Push(CallError callError)
        {
            box3.BeginInvoke(new Action(() =>
            {
                if (box3.Items.Count >= rowCount)
                {
                    box3.Items.RemoveAt(box3.Items.Count - 1);
                }

                box3.Items.Insert(0,
                    new ParseMessageEventArgs
                    {
                        MessageType = ParseMessageType.Error,
                        LineHeader = string.Format("【{0}】 [{2}] {1}", callError.CallTime, callError.Message, callError.Caller.AppName),
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
            }));
        }

        #endregion
    }
}
