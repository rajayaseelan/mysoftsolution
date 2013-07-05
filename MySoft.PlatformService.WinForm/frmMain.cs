using ListControls;
using MySoft.IoC;
using MySoft.IoC.Messages;
using MySoft.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace MySoft.PlatformService.WinForm
{
    public partial class frmMain : Form
    {
        private ServerNode defaultNode;
        private WebBrowser webBrowser1, webBrowser2;
        private System.Windows.Forms.Timer formTimer;
        private readonly object _syncLock = new object();

        public frmMain()
        {
            InitializeComponent();

            CastleFactory.Create().OnError += new ErrorLogEventHandler(frmMain_OnError);
            CastleFactory.Create().OnDisconnected += new EventHandler<ConnectEventArgs>(frmMain_OnDisconnected);
        }

        void frmMain_OnDisconnected(object sender, ConnectEventArgs e)
        {
            if (e.Subscribed)
            {
                lock (_syncLock)
                {
                    this.Invoke(new Action(() =>
                    {
                        if (button1.Tag == null) return;

                        StartMonitor(true);

                        var args = new ParseMessageEventArgs
                        {
                            MessageType = ParseMessageType.Error,
                            LineHeader = string.Format("【{0}】 当前监控已经从服务器断开...", DateTime.Now),
                            MessageText = string.Format("({0}){1}", e.Error.ErrorCode, e.Error.Message)
                        };

                        listConnect.Items.Insert(0, args);

                        listConnect.Invalidate();

                        //发送错误邮件
                        string errorMessage = string.Format("{0} - {1}", e.Error.ErrorCode, e.Error.Message);
                        SendErrorMail(errorMessage, e.Error);

                        //显示错误
                        ShowError(new Exception(args.LineHeader));
                    }));
                }
            }
        }

        void frmMain_OnError(Exception error)
        {
            lock (_syncLock)
            {
                this.Invoke(new Action(() =>
                {
                    ShowError(error);
                }));
            }
        }

        /// <summary>
        /// 发送错误邮件
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        private void SendErrorMail(string message, Exception inner)
        {
            var appClient = new AppClient
            {
                AppName = "监控客户端",
                HostName = DnsHelper.GetHostName(),
                IPAddress = DnsHelper.GetIPAddress()
            };
            var ex = new IoCException(message, inner)
            {
                ApplicationName = appClient.AppName,
                ServiceName = "MySoft.PlatformService.WinForm",
                ErrorHeader = string.Format("Application【{0}】occurs error. ==> Comes from {1}({2}).", appClient.AppName, appClient.HostName, appClient.IPAddress)
            };
            SimpleLog.Instance.WriteLogWithSendMail(ex, emails);
        }

        private IStatusService service;
        private void button1_Click(object sender, EventArgs e)
        {
            StartMonitor(false);
        }

        private void StartMonitor(bool socketError)
        {
            try
            {
                if (button1.Tag == null)
                {
                    if (defaultNode == null)
                    {
                        MessageBox.Show("请选择监控节点！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (!socketError)
                    {
                        if (MessageBox.Show("确定开始监控吗？", "系统提示",
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel) return;
                    }

                    if (checkBox3.Checked)
                    {
                        var timer = Convert.ToInt32(numericUpDown2.Value);
                        var url = ConfigurationManager.AppSettings["ServerMonitorUrl"];

                        if (!string.IsNullOrEmpty(url))
                        {
                            if (url.IndexOf('?') > 0)
                                url = url + "&timer=" + timer;
                            else
                                url = url + "?timer=" + timer;
                            webBrowser2.Navigate(url);
                        }
                    }

                    var listener = new StatusListener(tabControl1, listConnect, listTimeout, listError,
                        Convert.ToInt32(numericUpDown3.Value), Convert.ToInt32(numericUpDown5.Value),
                        Convert.ToInt32(numericUpDown1.Value), Convert.ToInt32(numericUpDown4.Value), checkBox4.Checked);

                    listener.Context = SynchronizationContext.Current;

                    service = CastleFactory.Create().GetChannel<IStatusService>(defaultNode, listener);

                    //var services = service.GetServiceList();

                    var options = new SubscribeOptions
                    {
                        PushCallError = checkBox1.Checked,
                        PushCallTimeout = checkBox2.Checked,
                        PushServerStatus = checkBox3.Checked,
                        PushClientConnect = true,
                        CallTimeout = Convert.ToDouble(numericUpDown1.Value) / 1000,
                        CallRowCount = Convert.ToInt32(numericUpDown5.Value),
                        ServerStatusTimer = Convert.ToInt32(numericUpDown2.Value)
                    };

                    try
                    {
                        service.Subscribe(options);

                        tabControl1.Tag = true;
                        button1.Text = "停止监控";
                        button1.Tag = label1.Text;
                        label1.Text = "正在进行监控...";

                        checkBox1.Enabled = false;
                        checkBox2.Enabled = false;
                        checkBox3.Enabled = false;
                        comboBox1.Enabled = false;

                        numericUpDown1.Enabled = checkBox2.Enabled && checkBox2.Checked;
                        numericUpDown2.Enabled = checkBox3.Enabled && checkBox3.Checked;

                        checkBox4.Enabled = false;
                        numericUpDown3.Enabled = false;
                        numericUpDown4.Enabled = false;
                        numericUpDown5.Enabled = false;
                        //button1.Enabled = false;
                    }
                    catch (Exception ex)
                    {
                        SimpleLog.Instance.WriteLogForDir("Client", ex);
                        ShowError(ex);
                    }
                }
                else
                {
                    if (!socketError)
                    {
                        if (MessageBox.Show("确定停止监控吗？", "系统提示",
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel) return;

                        try
                        {
                            service.Unsubscribe();
                        }
                        catch (Exception ex)
                        {
                            SimpleLog.Instance.WriteLogForDir("Client", ex);
                            ShowError(ex);
                        }
                    }

                    checkedListBox1.Items.Clear();
                    checkedListBox2.Items.Clear();

                    tabControl1.Tag = false;
                    label1.Text = button1.Tag.ToString();
                    button1.Text = "开始监控";
                    button1.Tag = null;

                    //listConnect.Items.Clear();
                    //listError.Items.Clear();
                    //listTimeout.Items.Clear();

                    //listConnect.Invalidate();
                    //listError.Invalidate();
                    //listTimeout.Invalidate();

                    tabPage1.Text = "连接信息" + (listConnect.Items.Count == 0 ? "" : "(" + listConnect.Items.Count + ")");
                    tabPage2.Text = "警告信息" + (listTimeout.Items.Count == 0 ? "" : "(" + listTimeout.Items.Count + ")");
                    tabPage3.Text = "异常信息" + (listError.Items.Count == 0 ? "" : "(" + listError.Items.Count + ")");

                    checkBox1.Enabled = true;
                    checkBox2.Enabled = true;
                    checkBox3.Enabled = true;
                    comboBox1.Enabled = true;

                    numericUpDown1.Enabled = checkBox2.Enabled && checkBox2.Checked;
                    numericUpDown2.Enabled = checkBox3.Enabled && checkBox3.Checked;

                    try
                    {
                        webBrowser1.Document.GetElementsByTagName("body")[0].InnerHtml = string.Empty;
                    }
                    catch
                    {

                    }

                    checkBox4.Enabled = true;
                    numericUpDown3.Enabled = true;
                    numericUpDown4.Enabled = true;
                    numericUpDown5.Enabled = true;
                    //button1.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("Client", ex);
                ShowError(ex);
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

        private void listBox3_Click(object sender, EventArgs e)
        {
            if (listTimeout.SelectedIndex < 0)
            {
                richTextBox1.Text = string.Empty;
                return;
            }

            var args = listTimeout.SelectedItem as ParseMessageEventArgs;
            var source = args.Source as CallTimeout;
            AppendText(richTextBox1, source.Caller);
        }

        private void listTotal_Click(object sender, EventArgs e)
        {
            if (listTotal.SelectedIndex < 0) return;

            listTimeout.SelectedIndex = -1;
            var args = listTotal.SelectedItem as ParseMessageEventArgs;
            var source = args.Source as TotalInfo;
            for (var i = 0; i < listTimeout.Items.Count; i++)
            {
                var item = listTimeout.Items[i] as ParseMessageEventArgs;
                var caller = (item.Source as CallTimeout).Caller;

                if (caller.AppName == source.AppName
                    && caller.ServiceName == source.ServiceName
                    && caller.MethodName == source.MethodName)
                {
                    listTimeout.SelectedIndex = i;
                }
            }

            listTimeout.Invalidate();
        }

        private void listBox2_Click(object sender, EventArgs e)
        {
            if (listError.SelectedIndex < 0)
            {
                try
                {
                    webBrowser1.Document.GetElementsByTagName("body")[0].InnerHtml = string.Empty;
                }
                catch
                {

                }
                return;
            }

            var args = listError.SelectedItem as ParseMessageEventArgs;
            var source = args.Source as CallError;
            try
            {
                webBrowser1.Document.GetElementsByTagName("body")[0].InnerHtml = string.Empty;
            }
            catch
            {

            }
            webBrowser1.Document.Write(source.HtmlError);
            AppendText(richTextBox2, source.Caller);
        }

        private void AppendText(RichTextBox rich, AppCaller caller)
        {
            rich.Clear();

            rich.SelectionIndent = 0;
            rich.SelectionColor = Color.Blue;
            rich.AppendText("AppPath:\r\n");
            rich.SelectionColor = Color.Black;
            rich.SelectionIndent = 20;
            rich.AppendText(caller.AppPath ?? "未知路径");
            rich.AppendText("\r\n\r\n");

            rich.SelectionIndent = 0;
            rich.SelectionColor = Color.Blue;
            rich.AppendText("CallTime:\r\n");
            rich.SelectionColor = Color.Black;
            rich.SelectionIndent = 20;
            rich.AppendText(caller.CallTime.ToString());
            rich.AppendText("\r\n\r\n");

            rich.SelectionIndent = 0;
            rich.SelectionColor = Color.Blue;
            rich.AppendText("AppName:\r\n");
            rich.SelectionColor = Color.Black;
            rich.SelectionIndent = 20;
            rich.AppendText(caller.AppName ?? "未知应用");
            rich.AppendText("\r\n\r\n");

            rich.SelectionIndent = 0;
            rich.SelectionColor = Color.Blue;
            rich.AppendText("HostName:\r\n");
            rich.SelectionColor = Color.Black;
            rich.SelectionIndent = 20;
            rich.AppendText(caller.IPAddress + " => " + caller.HostName);
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

        private string[] emails;
        private void frmMain_Load(object sender, EventArgs e)
        {
            listAssembly.SelectedIndexChanged += new EventHandler(listAssembly_SelectedIndexChanged);
            listService.SelectedIndexChanged += new EventHandler(listService_SelectedIndexChanged);
            listMethod.SelectedIndexChanged += new EventHandler(listMethod_SelectedIndexChanged);
            listMethod.MouseDoubleClick += new MouseEventHandler(listMethod_MouseDoubleClick);

            listTimeout.MouseDoubleClick += new MouseEventHandler(listTimeout_MouseDoubleClick);
            listTimeout.Items.OnItemInserted += new InsertEventHandler(TimeoutItems_OnItemInserted);
            listError.MouseDoubleClick += new MouseEventHandler(listError_MouseDoubleClick);
            listError.Items.OnItemInserted += new InsertEventHandler(Items_OnItemInserted);
            listTotal.MouseDoubleClick += new MouseEventHandler(listTotal_MouseDoubleClick);

            checkedListBox1.Items.Clear();
            checkedListBox2.Items.Clear();

            comboBox1.DisplayMember = "Key";
            comboBox1.DataSource = CastleFactory.Create().GetServerNodes();
            autoCompleteTextbox1.KeyPress += new KeyPressEventHandler(autoCompleteTextbox1_KeyPress);

            //解析邮件地址
            var receivedEmail = ConfigurationManager.AppSettings["ReceivedEmail"];
            if (string.IsNullOrEmpty(receivedEmail)) receivedEmail = "test@163.com";
            emails = receivedEmail.Split('|', ',', ';');

            formTimer = new System.Windows.Forms.Timer();
            formTimer.Interval = (int)TimeSpan.FromMinutes(10).TotalMilliseconds;
            formTimer.Tick += new EventHandler(timer_Tick);
            formTimer.Start();

            InitBrowser();
        }

        void autoCompleteTextbox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            //按回车
            if (e.KeyChar == 13)
            {
                var key = autoCompleteTextbox1.Text.Trim();
                if (methods.ContainsKey(key))
                {
                    var method = methods[key];
                    SingletonMul.Show(string.Format("FORM_{0}_{1}_{2}", method.ServiceName, method.Method.FullName, defaultNode), () =>
                    {
                        frmInvoke frm = new frmInvoke(defaultNode, method.ServiceName, method.Method);
                        return frm;
                    });
                }
                else
                {
                    MessageBox.Show("查找的方法【" + key + "】不存在！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        void Items_OnItemInserted(int index)
        {
            lblError.Text = listError.Items.Count + " times.";
        }

        void TimeoutItems_OnItemInserted(int index)
        {
            //统计
            this.Invoke(new Action(() =>
            {
                var timeOuts = new List<CallTimeout>();
                var totalCount = Convert.ToInt32(numericUpDown6.Value);
                if (totalCount > listTimeout.Items.Count)
                    totalCount = listTimeout.Items.Count;

                for (int i = 0; i < totalCount; i++)
                {
                    var item = listTimeout.Items[i];
                    timeOuts.Add(item.Source as CallTimeout);
                }

                var groups = timeOuts.GroupBy(p => new { p.Caller.AppName, p.Caller.ServiceName, p.Caller.MethodName })
                                    .Select(p => new TotalInfo
                                    {
                                        AppName = p.Key.AppName,
                                        ServiceName = p.Key.ServiceName,
                                        MethodName = p.Key.MethodName,
                                        ElapsedTime = p.Sum(c => c.ElapsedTime),
                                        Count = p.Sum(c => c.Count),
                                        Times = p.Count()
                                    });

                lblTimeout.Text = groups.Sum(p => p.ElapsedTime) + " ms.";
                listTotal.Items.Clear();
                var items = groups.OrderByDescending(p => new OrderTotalInfo
                {
                    Times = p.Times,
                    ElapsedTime = p.ElapsedTime,
                    Count = p.Count
                });

                var warningTimeout = Convert.ToInt32(numericUpDown1.Value);
                var timeout = Convert.ToInt32(numericUpDown4.Value);
                var count = Convert.ToInt32(numericUpDown5.Value);
                //var total = Convert.ToInt32(numericUpDown6.Value);

                foreach (var item in items)
                {
                    ParseMessageType msgType = ParseMessageType.None;
                    if (item.ElapsedTime > timeout * item.Times)
                        msgType = ParseMessageType.Error;
                    else if (item.ElapsedTime > warningTimeout * item.Times)
                        msgType = ParseMessageType.Warning;
                    else if (item.Count > count * item.Times)
                        msgType = ParseMessageType.Question;

                    if (msgType != ParseMessageType.None)
                    {
                        listTotal.Items.Add(
                            new ParseMessageEventArgs
                            {
                                MessageType = msgType,
                                LineHeader = string.Format("【{3}】 Total => Call ({0}) Times , ElapsedTime ({1}) ms, Count ({2}) rows.", item.Times, item.ElapsedTime, item.Count, item.AppName),
                                MessageText = string.Format("{0},{1}", item.ServiceName, item.MethodName),
                                Source = item
                            });
                    }
                }

                listTotal.Invalidate();
            }));
        }

        void listError_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listError.SelectedIndex < 0) return;
            var item = listError.Items[listError.SelectedIndex];
            PositionService((item.Source as CallError).Caller);
        }

        void listTimeout_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listTimeout.SelectedIndex < 0) return;
            var item = listTimeout.Items[listTimeout.SelectedIndex];
            PositionService((item.Source as CallTimeout).Caller);
        }

        void listTotal_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listTotal.SelectedIndex < 0) return;
            var item = listTotal.Items[listTotal.SelectedIndex];
            var total = item.Source as TotalInfo;
            PositionService(new AppCaller
            {
                ServiceName = total.ServiceName,
                MethodName = total.MethodName
            });
        }

        void PositionService(AppCaller caller)
        {
            listAssembly.SelectedIndex = -1;
            listService.SelectedIndex = -1;
            listMethod.SelectedIndex = -1;

            for (int index = 0; index < listAssembly.Items.Count; index++)
            {
                var item = listAssembly.Items[index];
                var services = item.Source as IList<ServiceInfo>;
                if (services.Any(p => p.FullName == caller.ServiceName))
                {
                    listAssembly.SelectedIndex = index;
                    break;
                }
            }

            for (int index = 0; index < listService.Items.Count; index++)
            {
                var item = listService.Items[index];
                var service = item.Source as ServiceInfo;
                if (service.FullName == caller.ServiceName)
                {
                    listService.SelectedIndex = index;
                    break;
                }
            }

            for (int index = 0; index < listMethod.Items.Count; index++)
            {
                var item = listMethod.Items[index];
                var method = item.Source as MethodInfo;
                if (method.FullName == caller.MethodName)
                {
                    listMethod.SelectedIndex = index;
                    break;
                }
            }

            //选中服务页
            tabControl1.SelectedIndex = 0;
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

                SingletonMul.Show(string.Format("FORM_{0}_{1}_{2}", service.FullName, method.FullName, defaultNode), () =>
                {
                    frmInvoke frm = new frmInvoke(defaultNode, service.FullName, method);
                    return frm;
                });
            }
        }

        /// <summary>
        /// 初始化浏览器
        /// </summary>
        void InitBrowser()
        {
            webBrowser1 = new WebBrowser();
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.Dock = DockStyle.Fill;
            splitContainer2.Panel2.Controls.Add(webBrowser1);

            webBrowser1.Navigate("about:blank");
            //webBrowser1.AllowNavigation = false;
            webBrowser1.IsWebBrowserContextMenuEnabled = false;

            var url1 = ConfigurationManager.AppSettings["ServerMonitorUrl"];
            var url2 = ConfigurationManager.AppSettings["ServerWebAPIUrl"];
            var url3 = ConfigurationManager.AppSettings["ServerOpenAPIUrl"];

            //处理Monitor
            if (string.IsNullOrEmpty(url1))
            {
                tabControl1.TabPages.Remove(tabPage4);
            }
            else
            {
                webBrowser2 = new WebBrowser();
                webBrowser2.ScriptErrorsSuppressed = true;
                webBrowser2.Dock = DockStyle.Fill;
                webBrowser2.Navigating += new WebBrowserNavigatingEventHandler(webBrowser2_Navigating);
                webBrowser2.Navigate(url1);
                tabPage4.Controls.Add(webBrowser2);
                //webBrowser2.AllowNavigation = false;
                //webBrowser2.IsWebBrowserContextMenuEnabled = false;
            }

            //处理WebAPI
            if (string.IsNullOrEmpty(url2))
            {
                tabControl1.TabPages.Remove(tabPage5);
            }
            else
            {
                var webBrowser = new WebBrowser();
                webBrowser.ScriptErrorsSuppressed = true;
                webBrowser.Dock = DockStyle.Fill;
                tabPage5.Controls.Add(webBrowser);
                webBrowser.Navigate(url2);
            }

            //处理OpenAPI
            if (string.IsNullOrEmpty(url3))
            {
                tabControl1.TabPages.Remove(tabPage6);
            }
            else
            {
                var webBrowser = new WebBrowser();
                webBrowser.ScriptErrorsSuppressed = true;
                webBrowser.Dock = DockStyle.Fill;
                tabPage6.Controls.Add(webBrowser);
                webBrowser.Navigate(url3);
            }
        }

        void webBrowser2_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            formTimer.Tag = e.Url;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            formTimer.Stop();

            var webBrowser = new WebBrowser();
            webBrowser.ScriptErrorsSuppressed = true;
            webBrowser.Dock = DockStyle.Fill;
            webBrowser.Navigating += new WebBrowserNavigatingEventHandler(webBrowser2_Navigating);
            webBrowser.Navigated += new WebBrowserNavigatedEventHandler(webBrowser2_Navigated);
            webBrowser.Navigate(formTimer.Tag as Uri);

            formTimer.Start();
        }

        void webBrowser2_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            webBrowser2.Dispose();
            webBrowser2 = sender as WebBrowser;

            tabPage4.Controls.Clear();
            tabPage4.Controls.Add(webBrowser2);
        }

        /// <summary>
        /// 服务信息
        /// </summary>
        private IList<ServiceInfo> services = new List<ServiceInfo>();
        private IDictionary<string, InvokeService> methods = new Dictionary<string, InvokeService>();

        private void InitService(ServerNode node, bool forceRefresh)
        {
            if (!forceRefresh && defaultNode != null)
            {
                if (node.Key == defaultNode.Key && listAssembly.Items.Count > 0)
                    return;
            }

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

            try
            {
                services = CastleFactory.Create().GetChannel<IStatusService>(node)
                    .GetServiceList().OrderBy(p => p.Name).ToList();

                methods.Clear();
                foreach (var s in services)
                {
                    foreach (var m in s.Methods)
                    {
                        var methodName = m.FullName;
                        var indexOf = m.FullName.IndexOf(' ');
                        if (indexOf >= 0)
                        {
                            methodName = m.FullName.Substring(indexOf + 1);
                        }

                        string key = string.Format("【{0}】{1}", s.FullName, methodName);
                        methods[key] = new InvokeService
                        {
                            ServiceName = s.FullName,
                            Method = m
                        };
                    }
                }

                //处理自动完成列表
                autoCompleteTextbox1.AutoCompleteList = methods.Select(p => p.Key).ToList();

                this.Text = string.Format("分布式服务监控 v2.0 【当前服务器节点({0}:{1}) 服务数:{2} 接口数:{3}】",
                    node.IP, node.Port, services.Count, services.Sum(p => p.Methods.Count));
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("Client", ex);
                ShowError(ex);
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

            defaultNode = node;
        }

        void InitAppTypes(IStatusService s)
        {
            checkedListBox1.Items.Clear();
            checkedListBox2.Items.Clear();

            var apps = s.GetSubscribeApps();
            foreach (var app in apps)
            {
                checkedListBox1.Items.Add(app);
            }
            var types = s.GetSubscribeTypes();
            foreach (var type in types)
            {
                checkedListBox2.Items.Add(type);
            }
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

        void listService_SelectedIndexChanged(object sender, EventArgs e)
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

        void listMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            richTextBox3.Clear();
            if (listMethod.SelectedIndex < 0) return;

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

        private void 订阅此服务SToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckMonitor()) return;

            if (listService.SelectedIndex < 0) return;

            try
            {
                var item = listService.Items[listService.SelectedIndex];
                service.SubscribeType((item.Source as ServiceInfo).FullName);

                InitAppTypes(service);
                MessageBox.Show("订阅成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("Client", ex);
                ShowError(ex);
            }
        }

        private void 退订此服务UToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckMonitor()) return;

            if (listService.SelectedIndex < 0) return;

            try
            {
                var item = listService.Items[listService.SelectedIndex];
                service.UnsubscribeType((item.Source as ServiceInfo).FullName);

                InitAppTypes(service);
                MessageBox.Show("退订成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("Client", ex);
                ShowError(ex);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定清除所有日志？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                listConnect.Items.Clear();
                listTimeout.Items.Clear();
                listError.Items.Clear();

                listConnect.Invalidate();
                listTimeout.Invalidate();
                listError.Invalidate();

                richTextBox1.Clear();
                richTextBox2.Clear();

                Items_OnItemInserted(0);
                TimeoutItems_OnItemInserted(0);

                tabPage1.Text = "连接信息";
                tabPage2.Text = "警告信息";
                tabPage3.Text = "异常信息";
            }
        }

        private void 刷新服务RToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (defaultNode == null)
            {
                MessageBox.Show("请选择监控节点！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            InitService(defaultNode, true);
        }

        private void 刷新WebAPI服务AToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (defaultNode == null)
            {
                MessageBox.Show("请选择监控节点！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("确定刷新WebAPI服务吗？\r\n\r\n注意：刷新WebAPI服务会将所有API服务接口进行重置后再重新加载！可能会对某些应用产生影响。", "系统提示",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel) return;

            try
            {
                CastleFactory.Create().GetChannel<IStatusService>(defaultNode).RefreshWebAPI();
                MessageBox.Show("刷新WebAPI服务成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("Client", ex);
                ShowError(ex);
            }
        }

        private void 调用此服务CToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listTimeout.SelectedIndex < 0) return;
            var item = listTimeout.Items[listTimeout.SelectedIndex];
            var caller = (item.Source as CallTimeout).Caller;
            ShowDialog(caller);
        }

        private void 调用此服务CToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listError.SelectedIndex < 0) return;
            var item = listError.Items[listError.SelectedIndex];
            var caller = (item.Source as CallError).Caller;
            ShowDialog(caller);
        }

        private void ShowDialog(AppCaller caller)
        {
            var service = services.First(p => p.FullName == caller.ServiceName);
            var method = service.Methods.First(p => p.FullName == caller.MethodName);

            SingletonMul.Show(string.Format("FORM_{0}_{1}_{2}", service.FullName, method.FullName, defaultNode), () =>
            {
                frmInvoke frm = new frmInvoke(defaultNode, service.FullName, method, caller.Parameters);
                return frm;
            });
        }

        private void 订阅此应用SToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listError.SelectedIndex < 0) return;
            var item = listError.Items[listError.SelectedIndex];
            var caller = (item.Source as CallError).Caller;
            PubSub(caller, true);
        }

        private void 退订此应用UToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listError.SelectedIndex < 0) return;
            var item = listError.Items[listError.SelectedIndex];
            var caller = (item.Source as CallError).Caller;
            PubSub(caller, false);
        }

        private void 订阅此应用SToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listTimeout.SelectedIndex < 0) return;
            var item = listTimeout.Items[listTimeout.SelectedIndex];
            var caller = (item.Source as CallTimeout).Caller;
            PubSub(caller, true);
        }

        private void 退订此应用UToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listTimeout.SelectedIndex < 0) return;
            var item = listTimeout.Items[listTimeout.SelectedIndex];
            var caller = (item.Source as CallTimeout).Caller;
            PubSub(caller, false);
        }

        private void PubSub(AppCaller caller, bool isSub)
        {
            if (!CheckMonitor()) return;

            try
            {
                if (isSub)
                {
                    service.SubscribeApp(caller.AppName);
                    InitAppTypes(service);
                    MessageBox.Show("订阅成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    service.UnsubscribeApp(caller.AppName);
                    InitAppTypes(service);
                    MessageBox.Show("退订成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("Client", ex);
                ShowError(ex);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex < 0) return;
            var node = comboBox1.Items[comboBox1.SelectedIndex] as ServerNode;

            InitService(node, false);
        }

        private void 全选AToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (checkedListBox1.Items.Count == 0) return;

            //应用全选
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, true);
            }
        }

        private void 退订此应用UToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!CheckMonitor()) return;

            if (checkedListBox1.CheckedItems.Count == 0) return;

            try
            {
                //应用退订
                foreach (var item in checkedListBox1.CheckedItems)
                {
                    try { service.UnsubscribeApp(item.ToString()); }
                    catch { }
                }

                InitAppTypes(service);
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("Client", ex);
                ShowError(ex);
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (checkedListBox2.Items.Count == 0) return;

            //服务全选
            for (int i = 0; i < checkedListBox2.Items.Count; i++)
            {
                checkedListBox2.SetItemChecked(i, true);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!CheckMonitor()) return;

            if (checkedListBox2.CheckedItems.Count == 0) return;

            try
            {
                //服务退订
                foreach (var item in checkedListBox2.CheckedItems)
                {
                    try { service.UnsubscribeType(item.ToString()); }
                    catch { }
                }

                InitAppTypes(service);
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("Client", ex);
                ShowError(ex);
            }
        }

        private void 订阅此服务SToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!CheckMonitor()) return;

            if (listTimeout.SelectedIndex < 0) return;

            try
            {
                var item = listTimeout.Items[listTimeout.SelectedIndex];
                service.SubscribeType((item.Source as CallTimeout).Caller.ServiceName);

                InitAppTypes(service);
                MessageBox.Show("订阅成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("Client", ex);
                ShowError(ex);
            }
        }

        private void 退订此服务UToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!CheckMonitor()) return;

            if (listTimeout.SelectedIndex < 0) return;

            try
            {
                var item = listTimeout.Items[listTimeout.SelectedIndex];
                service.UnsubscribeType((item.Source as CallTimeout).Caller.ServiceName);

                InitAppTypes(service);
                MessageBox.Show("退订成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("Client", ex);
                ShowError(ex);
            }
        }

        private void 订阅此服务SToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!CheckMonitor()) return;

            if (listError.SelectedIndex < 0) return;

            try
            {
                var item = listError.Items[listError.SelectedIndex];
                service.SubscribeType((item.Source as CallError).Caller.ServiceName);

                InitAppTypes(service);
                MessageBox.Show("订阅成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("Client", ex);
                ShowError(ex);
            }
        }

        private void 退订此服务UToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!CheckMonitor()) return;

            if (listError.SelectedIndex < 0) return;

            try
            {
                var item = listError.Items[listError.SelectedIndex];
                service.UnsubscribeType((item.Source as CallError).Caller.ServiceName);

                InitAppTypes(service);
                MessageBox.Show("退订成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("Client", ex);
                ShowError(ex);
            }
        }

        private void 添加应用AToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckMonitor()) return;

            Singleton.Show<frmClient>(() =>
            {
                frmClient frm = new frmClient(service, service.GetSubscribeApps());
                frm.OnCallback += new CallbackEventHandler(frm_OnCallback);
                return frm;
            });
        }

        void ShowError(Exception ex)
        {
            MessageBox.Show(ex.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        void frm_OnCallback(string[] apps)
        {
            foreach (var app in apps)
            {
                try { service.SubscribeApp(app); }
                catch (Exception ex)
                {
                    SimpleLog.Instance.WriteLogForDir("Client", ex);
                    ShowError(ex);
                }
            }

            InitAppTypes(service);
        }

        private bool CheckMonitor()
        {
            if (checkBox1.Enabled)
            {
                MessageBox.Show("请先启动监控！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            return true;
        }
    }
}
