using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySoft.IoC;
using MySoft.IoC.Messages;
using MySoft.IoC.Status;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace MySoft.PlatformService.WinForm
{
    public partial class frmInvoke : Form
    {
        private string serviceName;
        private string methodName;
        private IList<ParameterInfo> parameters;
        private IDictionary<string, TextBox> txtParameters;

        public frmInvoke(string serviceName, string methodName, IList<ParameterInfo> parameters)
        {
            InitializeComponent();

            this.serviceName = serviceName;
            this.methodName = methodName;
            this.parameters = parameters;
            this.txtParameters = new Dictionary<string, TextBox>();
        }

        private void frmInvoke_Load(object sender, EventArgs e)
        {
            lblServiceName.Text = serviceName;
            lblMethodName.Text = methodName;

            CastleFactory.Create().OnError += new Logger.ErrorLogEventHandler(frmInvoke_OnError);

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

                    var text = "Parameter【" + parameter.Name + "】type: " + parameter.TypeFullName;
                    if (parameter.IsEnum || parameter.SubParameters.Count > 0)
                    {
                        text += "\r\n\r\n";
                        text += GetParameterText(parameter, 0);
                    }
                    toolTip1.SetToolTip(l, text);
                    l.Click += new EventHandler((s, ee) =>
                    {
                        richTextBox1.Text = text;
                    });

                    TextBox t = new TextBox();
                    t.Dock = DockStyle.Fill;
                    p.Controls.Add(t);
                    l.SendToBack();

                    if (!parameter.IsPrimitive)
                    {
                        t.Multiline = true;
                        p.Height += 30;

                        countHeight += 30;
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

        void frmInvoke_OnError(Exception error)
        {
            richTextBox1.Text = string.Format("【Error】 =>\r\n{0}", error.Message);
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
                txtParameters.First().Value.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                var jValue = new JObject();
                foreach (var p in txtParameters)
                {
                    var text = p.Value.Text.Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        var pInfo = parameters.First(pi => pi.Name == p.Key);
                        if (pInfo.IsPrimitive)
                            jValue[p.Key] = text;
                        else
                            jValue[p.Key] = JToken.Parse(text);
                    }
                }

                //提交的参数信息
                string parameter = jValue.ToString(Newtonsoft.Json.Formatting.None);
                var data = CastleFactory.Create().Invoke(new InvokeMessage
                {
                    ServiceName = serviceName,
                    MethodName = methodName,
                    Parameter = parameter
                });

                watch.Stop();

                if (data != null)
                {
                    if (string.IsNullOrEmpty(data.Parameter))
                        richTextBox1.Text = string.Format("【InvokeValue】 =>\r\n{0}", data.Value);
                    else
                        richTextBox1.Text = string.Format("【InvokeValue】 =>\r\n{0}\r\n\r\n【OutParameter(s)】 =>\r\n{1}", data.Value, data.Parameter);
                }
            }
            catch (Exception ex)
            {
                watch.Stop();
                richTextBox1.Text = string.Format("【Error】 =>\r\n{0}", ex.Message);
            }
            finally
            {
                label5.Text = watch.ElapsedMilliseconds + " ms";
            }
        }
    }
}
