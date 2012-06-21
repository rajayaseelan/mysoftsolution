using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MySoft.IoC;
using MySoft.IoC.Messages;
using Newtonsoft.Json.Linq;

namespace MySoft.PlatformService.WinForm
{
    public partial class frmInvoke : Form
    {
        private string serviceName;
        private string methodName;
        private ServerNode node;
        private IList<ParameterInfo> parameters;
        private IDictionary<string, TextBox> txtParameters;
        private string paramValue;
        private int timeout;

        public frmInvoke(ServerNode node, string serviceName, string methodName, IList<ParameterInfo> parameters)
        {
            InitializeComponent();

            this.node = node;
            this.timeout = node.Timeout;
            this.node.Timeout = 30;
            this.serviceName = serviceName;
            this.methodName = methodName;
            this.parameters = parameters;
            this.txtParameters = new Dictionary<string, TextBox>();
        }

        public frmInvoke(ServerNode node, string serviceName, string methodName, IList<ParameterInfo> parameters, string paramValue)
            : this(node, serviceName, methodName, parameters)
        {
            this.paramValue = paramValue;
        }

        private void frmInvoke_Load(object sender, EventArgs e)
        {
            //自动生成列
            gridDataQuery.AutoGenerateColumns = true;
            webBrowser1.Url = new Uri("about:blank");

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
                    if (parameter.IsByRef && parameter.IsOut) continue;

                    Panel p = new Panel();
                    p.Top = (index++) * 48 + countHeight;
                    p.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                    p.Width = plParameters.Width;
                    p.Height = 45;
                    //p.BorderStyle = BorderStyle.Fixed3D;

                    Label l = new Label();
                    l.Text = "【" + parameter.Name + "】 => " + parameter.TypeName;
                    l.Dock = DockStyle.Top;
                    l.AutoSize = false;
                    //l.BorderStyle = BorderStyle.Fixed3D;
                    l.TextAlign = ContentAlignment.MiddleLeft;
                    p.Controls.Add(l);

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
                    p.Controls.Add(t);

                    l.SendToBack();

                    if (!parameter.IsPrimitive)
                    {
                        t.Multiline = true;
                        p.Height += 50;

                        countHeight += 50;
                    }

                    plParameters.Controls.Add(p);
                    txtParameters[parameter.Name] = t;
                }
            }
            else
            {
                label3.Visible = false;
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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (txtParameters.Count > 0)
            {
                if (MessageBox.Show("请确认参数是否全部填写正确！", "系统提示",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
                    return;
            }

            button1.Enabled = false;

            label5.Text = "正在调用服务，请稍候...";
            label5.Refresh();

            var jValue = new JObject();
            if (txtParameters.Count > 0)
            {
                foreach (var p in txtParameters)
                {
                    var text = p.Value.Text.Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        var info = p.Value.Tag as ParameterInfo;

                        try
                        {
                            jValue[p.Key] = JToken.Parse(text);
                        }
                        catch
                        {
                            jValue[p.Key] = text;
                        }
                    }
                }
            }

            //提交的参数信息
            string parameter = jValue.ToString(Newtonsoft.Json.Formatting.None);
            var message = new InvokeMessage
            {
                ServiceName = serviceName,
                MethodName = methodName,
                Parameters = parameter
            };

            //启用线程进行数据填充
            var caller = new AsyncMethodCaller(AsyncCaller);
            var ar = caller.BeginInvoke(message, AsyncComplete, caller);
        }

        private InvokeData AsyncCaller(InvokeMessage message)
        {
            InvokeResponse data;

            //开始计时
            var watch = Stopwatch.StartNew();

            try
            {
                //调用服务
                var invokeData = CastleFactory.Create().Invoke(node, message);
                data = new InvokeResponse(invokeData);
            }
            catch (Exception ex)
            {
                data = new InvokeResponse { Exception = ex };
            }

            watch.Stop();
            data.ElapsedMilliseconds = watch.ElapsedMilliseconds;

            return data;
        }

        private void AsyncComplete(IAsyncResult ar)
        {
            if (this.IsDisposed) return;

            var caller = ar.AsyncState as AsyncMethodCaller;
            var value = caller.EndInvoke(ar);
            var data = value as InvokeResponse;

            if (!data.IsError)
            {
                InvokeMethod(new Action(() =>
                {
                    richTextBox1.Text = string.Format("【InvokeValue】({0} rows) =>\r\n{1}\r\n\r\n【OutParameters】 =>\r\n{2}",
                        data.Count, data.Value, data.OutParameters);
                    richTextBox1.Refresh();
                }));

                //启用线程来处理数据
                ThreadPool.QueueUserWorkItem(state =>
                {
                    var invokeData = state as InvokeData;
                    var container = JContainer.Parse(invokeData.Value);

                    //获取DataView数据
                    var table = GetDataTable(container);
                    if (table == null)
                    {
                        //写Document文档
                        InvokeMethod(new Action(() =>
                        {
                            gridDataQuery.DataSource = null;

                            var html = container.ToString();
                            webBrowser1.Url = new Uri("about:blank");
                            webBrowser1.DocumentCompleted += (sender, e) =>
                            {
                                if (this.IsDisposed) return;

                                webBrowser1.Document.GetElementsByTagName("body")[0].InnerHtml = string.Empty;
                                webBrowser1.Document.Write(html);
                            };
                        }));
                    }
                    else
                    {
                        InvokeMethod(new Action(() => gridDataQuery.DataSource = table));
                    }
                }, data);
            }
            else
            {
                InvokeMethod(new Action(() =>
                {
                    richTextBox1.Text = string.Format("【Error】 =>\r\n{0}", data.Exception.Message);
                }));
            }

            InvokeMethod(new Action(() =>
            {
                label5.Text = data.ElapsedMilliseconds + " ms";
                label5.Refresh();

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
            }));
        }

        private void InvokeMethod(Action action)
        {
            if (this.IsDisposed) return;

            try
            {
                if (this.InvokeRequired)
                {
                    var ar = this.BeginInvoke(action);
                    this.EndInvoke(ar);
                }
                else
                {
                    action();
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
                table = new DataTable("TEMP_TABLE");
                if (container is JArray)
                {
                    var jarray = container as JArray;
                    AddFromJArray(table, jarray);
                }
                else if (container is JObject)
                {
                    var value = container as JObject;
                    if (value.First.First is JObject)
                    {
                        var jarray = new JArray(value.Values().ToArray());
                        AddFromJArray(table, jarray);
                    }
                    else
                    {
                        AddFromJObject(table, value);
                    }
                }
                else
                {
                    table = null;
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
        /// 从对象添加
        /// </summary>
        /// <param name="table"></param>
        /// <param name="value"></param>
        private static void AddFromJObject(DataTable table, JObject value)
        {
            foreach (var kv in value)
            {
                table.Columns.Add(kv.Key);
            }

            table.Rows.Add(value.Values().ToArray());
        }

        /// <summary>
        /// 从数组添加
        /// </summary>
        /// <param name="table"></param>
        /// <param name="jarray"></param>
        private static void AddFromJArray(DataTable table, JArray jarray)
        {
            for (int i = 0; i < jarray.Count; i++)
            {
                var value = jarray[i] as JObject;
                if (i == 0)
                {
                    foreach (var kv in value)
                    {
                        table.Columns.Add(kv.Key);
                    }
                }

                table.Rows.Add(value.Values().ToArray());
            }
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
    }
}
