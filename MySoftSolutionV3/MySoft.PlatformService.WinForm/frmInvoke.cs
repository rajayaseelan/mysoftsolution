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

                    toolTip1.SetToolTip(l, "parameter type: " + parameter.TypeFullName);

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
                var txtValues = new Dictionary<string, string>();
                foreach (var p in txtParameters)
                {
                    txtValues[p.Key] = p.Value.Text.Trim();
                }

                //提交的参数信息
                string parameter = SerializationManager.SerializeJson(txtValues);

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
                richTextBox1.Text = ex.Message;
            }
            finally
            {
                label5.Text = watch.ElapsedMilliseconds + " ms";
            }
        }
    }
}
