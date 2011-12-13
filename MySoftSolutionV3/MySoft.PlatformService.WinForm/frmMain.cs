using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ListControls;
using MySoft.IoC;
using MySoft.IoC.Status;
using MySoft.Logger;

namespace MySoft.PlatformService.WinForm
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
            CastleFactory.Create().OnError += new Logger.ErrorLogEventHandler(frmMain_OnError);
        }

        void frmMain_OnError(Exception error)
        {
            //发生错误SocketException为网络断开
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

                    var options = new SubscibeOptions
                    {
                        PushCallError = checkBox1.Checked,
                        PushCallTimeout = checkBox2.Checked,
                        PushServerStatus = checkBox3.Checked,
                        CallTimeout = Convert.ToDouble(numericUpDown1.Value) / 1000,
                        StatusTimer = Convert.ToInt32(numericUpDown2.Value)
                    };
                    service.Subscibe(options);
                    button1.Text = "停止监控";
                    button1.Tag = label1.Text;
                    label1.Text = "正在进行监控...";

                    checkBox1.Enabled = false;
                    checkBox2.Enabled = false;

                    numericUpDown1.Enabled = checkBox2.Enabled && checkBox2.Checked;
                    numericUpDown2.Enabled = checkBox3.Enabled && checkBox3.Checked;

                    checkBox4.Enabled = false;
                    numericUpDown3.Enabled = false;
                    //button1.Enabled = false;
                }
                else
                {
                    service.Unsubscibe();
                    label1.Text = button1.Tag.ToString();
                    button1.Text = "开始监控";
                    button1.Tag = null;
                    listBox1.Items.Clear();
                    listBox2.Items.Clear();
                    listBox3.Items.Clear();

                    tabControl1.TabPages[2].Text = "异常信息";
                    tabControl1.TabPages[1].Text = "超时信息";

                    checkBox1.Enabled = true;
                    checkBox2.Enabled = true;
                    numericUpDown1.Enabled = checkBox2.Enabled && checkBox2.Checked;
                    numericUpDown2.Enabled = checkBox3.Enabled && checkBox3.Checked;

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
            webBrowser1.Document.GetElementsByTagName("body")[0].InnerHtml = string.Empty;
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
        private bool sendMail;
        public StatusListener(TabControl control, MessageListBox box1, MessageListBox box2, MessageListBox box3, int rowCount, bool sendMail)
        {
            this.control = control;
            this.box1 = box1;
            this.box2 = box2;
            this.box3 = box3;
            this.rowCount = rowCount;
            this.sendMail = sendMail;
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
                        LineHeader = string.Format("【{0}】\t{1}:{2} => {3}:{4} ({5})",
                        connectInfo.ConnectTime, connectInfo.IPAddress, connectInfo.Port, connectInfo.ServerIPAddress, connectInfo.ServerPort, connectInfo.Connected ? "连接" : "断开"),
                        Source = connectInfo
                    });
                box1.SelectedIndex = box1.Items.Count - 1;
                box1.Invalidate();
            }));
        }

        public void Push(string ipAddress, AppClient appClient)
        {
            //throw new NotImplementedException();
        }

        public void Push(ServerStatus serverStatus)
        {
            //throw new NotImplementedException();
        }

        public void Push(CallError callError)
        {
            //throw new NotImplementedException();

            box2.BeginInvoke(new Action(() =>
            {
                if (box2.Items.Count >= rowCount)
                {
                    var item = box2.Items[box2.Items.Count - 1];
                    var message = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}", item.LineHeader, item.MessageText,
                        (item.Source as CallError).Caller.Parameters, (item.Source as CallError).Description);
                    SimpleLog.Instance.WriteLogForDir("CallError", message);
                    box2.Items.RemoveAt(box2.Items.Count - 1);
                }

                box2.Items.Insert(0,
                    new ParseMessageEventArgs
                    {
                        MessageType = ParseMessageType.Error,
                        LineHeader = string.Format("【{0}】\tError => {1}", callError.CallTime, callError.Message),
                        MessageText = string.Format("{0},{1}", callError.Caller.ServiceName, callError.Caller.MethodName),
                        //+ "\r\n" + callError.Caller.Parameters
                        Source = callError
                    });
                control.TabPages[2].Text = "异常信息(" + box2.Items.Count + ")";
                box2.Invalidate();
            }));
        }

        public void Push(CallTimeout callTimeout)
        {
            //throw new NotImplementedException();

            box3.BeginInvoke(new Action(() =>
            {
                if (box3.Items.Count >= rowCount)
                {
                    var item = box3.Items[box3.Items.Count - 1];
                    var message = string.Format("{0}\r\n{1}\r\n{2}", item.LineHeader, item.MessageText,
                        (item.Source as CallTimeout).Caller.Parameters);
                    SimpleLog.Instance.WriteLogForDir("CallTimeout", message);
                    box3.Items.RemoveAt(box3.Items.Count - 1);
                }

                box3.Items.Insert(0,
                    new ParseMessageEventArgs
                    {
                        MessageType = ParseMessageType.Warning,
                        LineHeader = string.Format("【{0}】\tTimeout => ({1} rows)：{2} ms.", callTimeout.CallTime, callTimeout.Count, callTimeout.ElapsedTime),
                        MessageText = string.Format("{0},{1}", callTimeout.Caller.ServiceName, callTimeout.Caller.MethodName),
                        // + "\r\n" + callTimeout.Caller.Parameters
                        Source = callTimeout
                    });
                control.TabPages[1].Text = "超时信息(" + box3.Items.Count + ")";
                box3.Invalidate();
            }));
        }

        #endregion
    }
}
