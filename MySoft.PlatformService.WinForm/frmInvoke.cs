using MySoft.IoC;
using MySoft.IoC.Messages;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MySoft.PlatformService.WinForm
{
    public delegate void JsonCallback(string json);

    public partial class frmInvoke : Form
    {
        private bool isReflection;
        private ServiceInfo serviceInfo;
        private MethodInfo methodInfo;
        private string serviceName;
        private string methodName;
        private int cacheTime;
        private ServerNode node;
        private IList<ParameterInfo> parameters;
        private IDictionary<string, TextBox> txtParameters;
        private IDictionary<string, CheckBox> checkParameters;
        private string paramValue;
        private int timeout;

        public frmInvoke(bool isReflection, ServerNode node, ServiceInfo service, MethodInfo method)
        {
            InitializeComponent();

            this.isReflection = isReflection;
            this.node = node;
            this.timeout = node.Timeout;
            this.node.Timeout = 30;
            this.serviceInfo = service;
            this.methodInfo = method;
            this.serviceName = service.FullName;
            this.methodName = method.FullName;
            this.cacheTime = method.CacheTime;
            this.parameters = method.Parameters;
            this.txtParameters = new Dictionary<string, TextBox>();
            this.checkParameters = new Dictionary<string, CheckBox>();
        }

        public frmInvoke(bool isReflection, ServerNode node, ServiceInfo service, MethodInfo method, string paramValue)
            : this(isReflection, node, service, method)
        {
            this.paramValue = paramValue;
        }

        private void frmInvoke_Load(object sender, EventArgs e)
        {
            var nodes = CastleFactory.Create().GetServerNodes();
            var nodeKey = node.Key;

            comboBox1.DisplayMember = "Key";
            comboBox1.ValueMember = "Key";
            comboBox1.DataSource = nodes;
            comboBox1.SelectedIndex = nodes.ToList().FindIndex(p => p.Key == nodeKey);

            //自动生成列
            gridDataQuery.AutoGenerateColumns = true;
            webBrowser1.Navigate(new Uri("about:blank"));
            webBrowser1.ScriptErrorsSuppressed = true;

            lblServiceName.Text = serviceName;
            lblMethodName.Text = methodName;

            JObject obj = new JObject();
            if (!string.IsNullOrEmpty(paramValue))
            {
                obj = JObject.Parse(paramValue);
                if (obj.Count == 1 && obj["InvokeParameter"] != null)
                {
                    obj = JObject.Parse(obj["InvokeParameter"].ToString());
                }
            }

            if (parameters.Count > 0)
            {
                int index = 0;
                int countHeight = 0;
                foreach (var parameter in parameters)
                {
                    Panel p = new Panel();
                    p.Top = (index++) * 48 + countHeight;
                    p.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                    p.Width = plParameters.Width;
                    p.Height = 45;
                    //p.BorderStyle = BorderStyle.Fixed3D;

                    Label l = new Label();
                    l.Text = "  【" + parameter.Name + "】 => " + parameter.TypeName;
                    l.Dock = DockStyle.Top;
                    l.AutoSize = false;
                    //l.BorderStyle = BorderStyle.Fixed3D;
                    l.TextAlign = ContentAlignment.MiddleLeft;

                    if (!parameter.IsPrimitive || parameter.IsEnum)
                    {
                        var text = "Parameter【" + parameter.Name + "】type: " + parameter.TypeFullName;
                        if (parameter.IsEnum || parameter.SubParameters.Count > 0)
                        {
                            text += "\r\n\r\n";
                            text += GetParameterText(parameter, 0);
                        }
                        toolTip1.SetToolTip(l, text);
                        l.ForeColor = Color.Red;
                        l.Click += new EventHandler((s, ee) =>
                        {
                            richTextBox1.Text = text;
                        });
                    }

                    Control _c = l;

                    if (!parameter.IsPrimitive && !parameter.IsOut)
                    {
                        Panel _lp = new Panel();
                        //_lp.BorderStyle = BorderStyle.Fixed3D;
                        _lp.Dock = DockStyle.Top;
                        _lp.Height = l.Height;

                        LinkLabel _ll = new LinkLabel();
                        _ll.AutoSize = false;
                        _ll.Width = 60;
                        _ll.TextAlign = ContentAlignment.MiddleRight;
                        _ll.Dock = DockStyle.Right;
                        _ll.Text = "设置参数";
                        _ll.LinkClicked += (_sender, _e) =>
                        {
                            var jvalue = txtParameters[parameter.Name].Text.Trim();
                            frmParameter frm = new frmParameter(parameter, jvalue);
                            frm.OnCallback += (json) =>
                            {
                                txtParameters[parameter.Name].Text = json;
                            };
                            frm.ShowDialog();
                        };

                        l.Dock = DockStyle.Fill;
                        _lp.Controls.Add(_ll);
                        _lp.Controls.Add(l);

                        _c = _lp;

                        p.Controls.Add(_lp);
                    }
                    else
                    {
                        p.Controls.Add(l);
                    }

                    Panel tp = new Panel();
                    tp.Dock = DockStyle.Fill;

                    TextBox t = new TextBox();
                    t.Dock = DockStyle.Fill;

                    //给参数赋值
                    if (obj.Count > 0)
                    {
                        if (obj[parameter.Name] != null)
                        {
                            t.Text = obj[parameter.Name].Value<string>();
                        }
                    }

                    t.Tag = parameter;

                    CheckBox c = new CheckBox();
                    c.AutoSize = true;
                    c.Dock = DockStyle.Left;
                    c.Checked = true;
                    c.CheckedChanged += (_sender, _e) =>
                    {
                        t.Enabled = c.Checked;
                    };

                    tp.Controls.Add(c);
                    tp.Controls.Add(t);

                    if (parameter.IsByRef && parameter.IsOut)
                    {
                        c.Checked = false;
                        t.Text = "输出参数";
                        tp.Enabled = false;
                    }

                    p.Controls.Add(tp);

                    _c.SendToBack();
                    c.SendToBack();

                    if (!parameter.IsPrimitive && !parameter.IsOut)
                    {
                        var h = parameter.SubParameters.Count == 0 ? 60 : 100;

                        t.Multiline = true;
                        p.Height += h;

                        countHeight += h;
                    }

                    plParameters.Controls.Add(p);
                    txtParameters[parameter.Name] = t;
                    checkParameters[parameter.Name] = c;
                }
            }
            else
            {
                label3.Visible = false;
                linkLabel1.Visible = false;
            }
        }

        private string GetParameterText(ParameterInfo parameter, int index)
        {
            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < index * 4; i++) sb.Append(" ");
            sb.AppendLine("【" + parameter.Name + "】 => " + parameter.TypeName);

            if (parameter.IsEnum)
            {
                for (var i = 0; i < index * 4 + 1; i++) sb.Append(" ");
                sb.AppendLine("{");
                foreach (var p in parameter.EnumValue)
                {
                    for (var i = 0; i < (index + 1) * 4; i++) sb.Append(" ");
                    sb.AppendLine("【" + p.Name + "】 => " + p.Value);
                }
                for (var i = 0; i < index * 4 + 1; i++) sb.Append(" ");
                sb.AppendLine("}");
            }
            else if (parameter.SubParameters.Count > 0)
            {
                for (var i = 0; i < index * 4 + 1; i++) sb.Append(" ");
                sb.AppendLine("{");
                {
                    foreach (var p in parameter.SubParameters)
                    {
                        if (p.IsEnum || p.SubParameters.Count > 0)
                            sb.Append(GetParameterText(p, index + 1));
                        else
                        {
                            for (var i = 0; i < (index + 1) * 4; i++) sb.Append(" ");
                            sb.AppendLine("【" + p.Name + "】 => " + p.TypeName);
                        }
                    }
                }
                for (var i = 0; i < index * 4 + 1; i++) sb.Append(" ");
                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        protected override void OnShown(EventArgs e)
        {
            if (txtParameters.Count > 0)
            {
                var p = txtParameters.Values.FirstOrDefault();
                p.Focus();
            }
            else
            {
                button1.Focus();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex < 0) return;
            node = comboBox1.Items[comboBox1.SelectedIndex] as ServerNode;
            numericUpDown1_ValueChanged(null, null);

            this.Text = string.Format("分布式服务调用【当前服务器节点({0}:{1})】", node.IP, node.Port);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (txtParameters.Count > 0)
            {
                var list = new List<string>();
                foreach (var kvp in txtParameters)
                {
                    //检测数据是否有效
                    if (checkParameters[kvp.Key].Checked && string.IsNullOrEmpty(kvp.Value.Text.Trim()))
                    {
                        list.Add(kvp.Key);
                    }
                }

                if (list.Count > 0)
                {
                    MessageBox.Show(string.Format("参数【{0}】值不能为空！", string.Join("、", list.ToArray()))
                    , "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;
                }
            }

            button1.Enabled = false;

            label5.Text = "正在调用服务，请稍候...";
            label5.Refresh();

            var jValue = new JObject();
            if (txtParameters.Count > 0)
            {
                foreach (var kvp in txtParameters)
                {
                    if (!checkParameters[kvp.Key].Checked) continue;

                    var text = kvp.Value.Text.Trim();
                    var info = kvp.Value.Tag as ParameterInfo;
                    if (info.IsPrimitive)
                    {
                        //如果字符串包含引号，则不处理。
                        if (!(text.StartsWith("\"") && text.EndsWith("\"")))
                        {
                            text = string.Format("\"{0}\"", text);
                        }
                    }

                    try
                    {
                        jValue[kvp.Key] = JToken.Parse(text);
                    }
                    catch
                    {
                        jValue[kvp.Key] = text;
                    }
                }
            }

            if (isReflection)
            {
                //Bin方式调用 
                BinaryInvoke(jValue);
            }
            else
            {
                //Json方式调用
                JsonInvoke(jValue);
            }
        }

        #region binary方式调用

        /// <summary>
        /// Bin方式调用
        /// </summary>
        /// <param name="jValue"></param>
        private void BinaryInvoke(JObject jValue)
        {
            var info = new AppDomainSetup();
            info.LoaderOptimization = LoaderOptimization.SingleDomain;

            var domain = AppDomain.CreateDomain("Service Invoke", null, info);

            try
            {
                var assembly = domain.Load(serviceInfo.Assembly);
                var type = assembly.GetType(serviceInfo.FullName, true);

                //获取执行实例
                var instance = CastleFactory.Create() as object;
                var method = instance.GetType().GetMethod("GetChannel", new Type[] { typeof(ServerNode) });
                instance = method.MakeGenericMethod(type).Invoke(instance, new object[] { node });

                //执行方法
                method = CoreHelper.GetMethodFromType(type, methodInfo.FullName);
                var parameters = IoCHelper.CreateParameters(method, jValue.ToString());
                var values = parameters.Values.ToArray();

                //异步调用
                var func = new AsyncDoMethod(DoMethod);
                func.BeginInvoke(method, instance, values, DoMethodComplete, domain);
            }
            catch (Exception ex)
            {
                AppDomain.Unload(domain);

                //设置控件焦点
                SetControlFocus(false);

                var errMsg = ErrorHelper.GetInnerException(ex).Message;
                MessageBox.Show(errMsg, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="method"></param>
        /// <param name="instance"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private BinaryResponse DoMethod(System.Reflection.MethodInfo method, object instance, object[] parameters)
        {
            var watch = Stopwatch.StartNew();

            try
            {
                var value = method.Invoke(instance, parameters);

                //获取返回参数
                var collection = new ParameterCollection();
                IoCHelper.SetRefParameters(method, parameters, collection);

                return new BinaryResponse
                {
                    Value = value,
                    Count = GetCount(value),
                    OutParameters = collection.ToString(),
                    ElapsedTime = watch.ElapsedMilliseconds,
                    ElapsedMilliseconds = watch.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                return new BinaryResponse
                {
                    ElapsedTime = watch.ElapsedMilliseconds,
                    ElapsedMilliseconds = watch.ElapsedMilliseconds,
                    Exception = ErrorHelper.GetInnerException(ex)
                };
            }
            finally
            {
                if (watch.IsRunning)
                {
                    watch.Stop();
                }
            }
        }

        /// <summary>
        /// 获取记录数
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private int GetCount(object val)
        {
            if (val == null) return 0;

            try
            {
                if (val is ICollection)
                {
                    return (val as ICollection).Count;
                }
                else if (val is Array)
                {
                    return (val as Array).Length;
                }
                else if (val is DataTable)
                {
                    return (val as DataTable).Rows.Count;
                }
                else if (val is DataSet)
                {
                    var ds = val as DataSet;
                    if (ds.Tables.Count > 0)
                    {
                        int count = 0;
                        foreach (DataTable table in ds.Tables)
                        {
                            count += table.Rows.Count;
                        }
                        return count;
                    }
                }
                else if (val is InvokeData)
                {
                    return (val as InvokeData).Count;
                }

                return 1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        /// <summary>
        /// 服务调用完成
        /// </summary>
        /// <param name="ar"></param>
        private void DoMethodComplete(IAsyncResult ar)
        {
            var domain = ar.AsyncState as AppDomain;

            try
            {
                var _ar = ar as System.Runtime.Remoting.Messaging.AsyncResult;
                var func = _ar.AsyncDelegate as AsyncDoMethod;

                var value = func.EndInvoke(ar);

                var response = new InvokeResponse(value)
                {
                    Value = SerializationManager.SerializeJson(value.Value),
                    Exception = value.Exception,
                    ElapsedMilliseconds = value.ElapsedMilliseconds
                };

                if (!value.IsError)
                {
                    InvokeMethod(new Action<object>(table =>
                    {
                        if (table is IDictionary)
                        {
                            table = (table as IDictionary).Values;
                        }

                        gridDataQuery.DataSource = new BindingSource(table, null);
                        gridDataQuery.Refresh();

                    }), value.Value);

                    //获取DataView数据
                    InvokeMethod(new Action<string>(json =>
                    {
                        var token = JContainer.Parse(json);

                        if (!token.HasValues)
                        {
                            var html = token.ToString();
                            SetWebBrowser(html);
                        }
                        else
                        {
                            SetWebBrowser(string.Empty);
                        }
                    }), response.Value);
                }

                InvokeResponse(response);
            }
            finally
            {
                AppDomain.Unload(domain);

                ar.AsyncWaitHandle.Close();
            }
        }

        #endregion

        /// <summary>
        /// Json方式调用
        /// </summary>
        /// <param name="jValue"></param>
        private void JsonInvoke(JObject jValue)
        {
            //提交的参数信息
            string parameter = jValue.ToString(Newtonsoft.Json.Formatting.Indented);
            var message = new InvokeMessage
            {
                ServiceName = serviceName,
                MethodName = methodName,
                Parameters = parameter,
                CacheTime = cacheTime
            };

            //启用线程进行数据填充
            var caller = new AsyncMethodCaller(AsyncCaller);
            caller.BeginInvoke(message, AsyncComplete, caller);
        }

        private InvokeData AsyncCaller(InvokeMessage message)
        {
            //开始计时
            var watch = Stopwatch.StartNew();

            try
            {
                //调用服务
                var invokeData = CastleFactory.Create().Invoke(node, message);
                var data = new InvokeResponse(invokeData);
                data.ElapsedMilliseconds = watch.ElapsedMilliseconds;

                return data;
            }
            catch (Exception ex)
            {
                return new InvokeResponse
                {
                    ElapsedTime = watch.ElapsedMilliseconds,
                    ElapsedMilliseconds = watch.ElapsedMilliseconds,
                    Exception = ex
                };
            }
            finally
            {
                if (watch.IsRunning)
                {
                    watch.Stop();
                }
            }
        }

        private void AsyncComplete(IAsyncResult ar)
        {
            try
            {
                if (this.IsDisposed) return;

                var caller = ar.AsyncState as AsyncMethodCaller;
                var value = caller.EndInvoke(ar);
                var data = value as InvokeResponse;

                if (!data.IsError)
                {
                    InvokeTable(data.Value);
                }

                InvokeResponse(data);
            }
            catch (Exception ex) { }
            finally
            {
                ar.AsyncWaitHandle.Close();
            }
        }

        /// <summary>
        /// 填充DataTable
        /// </summary>
        /// <param name="json"></param>
        private void InvokeTable(string json)
        {
            var container = JContainer.Parse(json);

            //获取DataView数据
            InvokeMethod(new Action<JToken>(token =>
            {
                if (!token.HasValues)
                {
                    gridDataQuery.DataSource = null;
                    gridDataQuery.Refresh();
                    var html = token.ToString();
                    SetWebBrowser(html);
                }
                else
                {
                    //获取DataView数据
                    var table = GetDataTable(token);
                    gridDataQuery.DataSource = table;
                    gridDataQuery.Refresh();
                    SetWebBrowser(string.Empty);
                }
            }), container);
        }

        private void InvokeResponse(InvokeResponse data)
        {
            InvokeMethod(new Action<InvokeResponse>(response =>
            {
                if (response.IsError)
                {
                    richTextBox1.Text = string.Format("【Error】 =>\r\n{0}", response.Exception.Message);
                    richTextBox1.Refresh();
                }
                else
                {
                    richTextBox1.Text = string.Format("【InvokeValue】({0} rows) =>\r\n{1}\r\n\r\n【OutParameters】 =>\r\n{2}",
                                        response.Count, response.Value, response.OutParameters);
                    richTextBox1.Refresh();
                }

                //设置控件焦点
                SetControlFocus(true);

                if (response.ElapsedMilliseconds == response.ElapsedTime)
                {
                    label5.Text = string.Format("{0} ms.  Row(s): {1}.  Size: {2}.",
                                response.ElapsedTime, response.Count, GetDataSize(response.Value));
                }
                else
                {
                    label5.Text = string.Format("{0} / {1} ms.  Row(s): {2}.  Size: {3}.", response.ElapsedMilliseconds,
                                response.ElapsedTime, response.Count, GetDataSize(response.Value));
                }

                label5.Refresh();
            }), data);
        }

        /// <summary>
        /// 设置控件焦点
        /// </summary>
        private void SetControlFocus(bool success)
        {
            button1.Enabled = true;

            if (txtParameters.Count > 0)
            {
                var p = txtParameters.Values.FirstOrDefault();
                p.Focus();
            }
            else
            {
                button1.Focus();
            }

            if (success)
            {
                label5.Text = "接口调用已成功完成！";
                label5.Refresh();
            }
            else
            {
                label5.Text = "接口调用失败！";
                label5.Refresh();
            }
        }

        /// <summary>
        /// 获取数据大小
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetDataSize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "0 Byte";
            }

            var count = Encoding.UTF8.GetByteCount(value);

            if (count * 1.0 / (1024 * 1024) > 1)
            {
                return string.Format("{0} MB", Math.Round(count * 1.0 / (1024 * 1024), 2));
            }
            else if (count * 1.0 / 1024 > 1)
            {
                return string.Format("{0} KB", Math.Round(count * 1.0 / 1024, 2));
            }
            else
            {
                return string.Format("{0} Byte(s)", count);
            }
        }

        /// <summary>
        /// 设置浏览器
        /// </summary>
        /// <param name="html"></param>
        private void SetWebBrowser(string html)
        {
            if (this.IsDisposed) return;
            if (webBrowser1.IsBusy) return;

            try
            {
                webBrowser1.Navigate(new Uri("about:blank"));

                while (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
                { Application.DoEvents(); }

                webBrowser1.Document.Encoding = "utf-8";
                webBrowser1.Document.Write(html);
            }
            catch (Exception ex)
            {

            }
        }

        private void InvokeMethod<T>(Action<T> action, T state)
        {
            if (this.IsDisposed) return;

            try
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(action, state);
                }
                else
                {
                    action(state);
                }
            }
            catch
            {
                //TODO
            }
        }

        /// <summary>
        /// 生成DataTable
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        private DataTable GetDataTable(JToken container)
        {
            DataTable table = null;

            try
            {
                var tokens = GetFromJToken(container);

                if (tokens.Count > 0)
                {
                    table = new DataTable("TEMP_TABLE");
                    AddFromJToken(table, tokens.ToArray());
                }
            }
            catch
            {
                table = null;
                //TO DO
            }

            return table;
        }

        /// <summary>
        /// 添加到table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="tokens"></param>
        private void AddFromJToken(DataTable table, params JToken[] tokens)
        {
            for (int i = 0; i < tokens.Count(); i++)
            {
                if (tokens[i] is JObject)
                {
                    var value = tokens[i] as JObject;
                    if (i == 0)
                    {
                        foreach (var kv in value)
                        {
                            table.Columns.Add(kv.Key);
                        }
                    }

                    table.Rows.Add(value.Values().ToArray());
                }
                else
                {
                    if (i == 0)
                    {
                        table.Columns.Add("Temp_Column");
                    }

                    table.Rows.Add(tokens[i]);
                }
            }
        }

        /// <summary>
        /// 从对象添加
        /// </summary>
        /// <param name="value"></param>
        private IList<JToken> GetFromJToken(JToken value)
        {
            var list = new List<JToken>();

            if (value is JArray)
            {
                foreach (var jtoken in value.ToArray())
                {
                    list.AddRange(GetFromJToken(jtoken));
                }
            }
            else
            {
                var deepin = CheckJTokenDeep(value, -1);

                if (deepin <= 0)
                {
                    list.Add(value);
                }
                else if (deepin == 1)
                {
                    list.AddRange(value.Values().ToArray());
                }
                else if (deepin == 2)
                {
                    foreach (var jtoken in value.Values())
                    {
                        list.AddRange(GetFromJToken(jtoken));
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 检测深度
        /// </summary>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private int CheckJTokenDeep(JToken value, int index)
        {
            if (value is JObject)
            {
                return CheckJTokenDeep(value.Values().First(), index + 1);
            }
            else if (value is JArray)
            {
                return CheckJTokenDeep(value.First, index + 1);
            }
            else if (value is JValue)
            {
                return index;
            }

            return index;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmInvoke_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.node = null;
            this.parameters = null;
            this.txtParameters = null;
            this.webBrowser1.Dispose();
            this.webBrowser1 = null;

            this.Dispose();
        }

        /// <summary>
        /// 生成行号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridDataQuery_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            Color color = gridDataQuery.RowHeadersDefaultCellStyle.ForeColor;
            if (gridDataQuery.Rows[e.RowIndex].Selected)
                color = gridDataQuery.RowHeadersDefaultCellStyle.SelectionForeColor;
            else
                color = gridDataQuery.RowHeadersDefaultCellStyle.ForeColor;

            using (SolidBrush b = new SolidBrush(color))
            {
                e.Graphics.DrawString((e.RowIndex + 1).ToString(), e.InheritedRowStyle.Font, b, e.RowBounds.Location.X + 20, e.RowBounds.Location.Y + 6);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                int seconds = Convert.ToInt32(numericUpDown1.Value);
                node.Timeout = seconds;
            }
            else
            {
                node.Timeout = timeout;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            checkBox1_CheckedChanged(sender, e);
        }

        private void lblMethodName_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetData(DataFormats.StringFormat, lblMethodName.Text);
                MessageBox.Show("内容成功复制到剪切板！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
            }
        }

        private void lblServiceName_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetData(DataFormats.StringFormat, lblServiceName.Text);
                MessageBox.Show("内容成功复制到剪切板！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmJSON frm = new frmJSON();
            frm.OnCallback += (json) =>
            {
                JObject obj = new JObject();
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        obj = JObject.Parse(json);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("解析数据出错：" + ex.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        return;
                    }
                }

                foreach (var p in txtParameters)
                {
                    var _p = p.Value.Tag as ParameterInfo;
                    if (obj[_p.Name] != null)
                    {
                        if (_p.IsOut) continue;
                        p.Value.Text = obj[_p.Name].ToString();
                    }
                }
            };

            frm.ShowDialog();
        }
    }
}
