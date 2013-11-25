using MySoft.IoC.Messages;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MySoft.PlatformService.WinForm
{
    public partial class frmParameter : Form
    {
        public event JsonCallback OnCallback;

        private ParameterInfo parameter;
        private JToken token;
        public frmParameter(ParameterInfo parameter, string json)
        {
            this.parameter = parameter;

            try
            {
                if (!string.IsNullOrEmpty(json))
                {
                    if (parameter.SubParameters.Count == 0 || parameter.IsCollection)
                        token = JArray.Parse(json);
                    else
                        token = JObject.Parse(json);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("解析数据失败！" + ex.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            InitializeComponent();
        }

        private void frmParameter_Load(object sender, EventArgs e)
        {
            label1.Text = string.Format("请输入参数[{0}]的数据：", parameter.Name);

            if (parameter.SubParameters.Count == 0)
            {
                numericUpDown1.Value = 5;
                numericUpDown1.Maximum = 30;
                CreatePanel(5);
            }
            else
            {
                if (parameter.IsCollection)
                {
                    numericUpDown1.Value = 2;
                    CreatePanel(2);
                }
                else
                {
                    label2.Visible = false;
                    numericUpDown1.Visible = false;
                    CreatePanel(1);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var json = GetJsonValue();
            if (string.IsNullOrEmpty(json))
            {
                panel1.Focus();

                MessageBox.Show("请输入JSON数据。", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            if (OnCallback != null)
            {
                this.Close();

                OnCallback(json);
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            CreatePanel((int)numericUpDown1.Value);
            panel1.Focus();
        }

        private void CreatePanel(int count)
        {
            var random = new Random();
            panel1.Controls.Clear();

            var panels = new List<Panel>();
            if (parameter.SubParameters.Count == 0)
            {
                var color = Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
                for (int i = 0; i < count; i++)
                {
                    var value = string.Empty;
                    if (token != null)
                    {
                        var arr = token as JArray;
                        if (arr != null && arr.Count > i)
                        {
                            value = arr[i].Value<string>();
                        }
                    }

                    var cp = CreateChildPanel(parameter.Name, value);
                    cp.BackColor = color;

                    panels.Add(cp);
                }
            }
            else
            {
                if (parameter.IsCollection)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var color = Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
                        foreach (var _pp in parameter.SubParameters)
                        {
                            var value = string.Empty;
                            if (token != null)
                            {
                                var arr = token as JArray;
                                if (arr != null && arr.Count > i)
                                {
                                    if (arr[i][_pp.Name] != null)
                                    {
                                        value = arr[i][_pp.Name].Value<string>();
                                    }
                                }
                            }

                            var cp = CreateChildPanel(_pp.Name, value);
                            cp.BackColor = color;

                            panels.Add(cp);
                        }

                        if (i < count)
                        {
                            var _p = new Panel();
                            _p.Dock = DockStyle.Top;
                            _p.Height = 8;

                            panels.Add(_p);
                        }
                    }
                }
                else
                {
                    var color = Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
                    foreach (var _pp in parameter.SubParameters)
                    {
                        var value = string.Empty;
                        if (token != null && token[_pp.Name] != null)
                        {
                            value = token[_pp.Name].Value<string>();
                        }

                        var cp = CreateChildPanel(_pp.Name, value);
                        cp.BackColor = color;

                        panels.Add(cp);
                    }
                }
            }

            //添加控件
            panel1.SuspendLayout();
            panel1.Controls.AddRange(panels.ToArray());

            for (int i = panel1.Controls.Count - 1; i >= 0; i--)
            {
                panel1.Controls[i].SendToBack();
            }

            panel1.ResumeLayout();
        }

        /// <summary>
        /// 创建子Panel
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private Panel CreateChildPanel(string name, string value)
        {
            var _p = new Panel();
            _p.Dock = DockStyle.Top;

            var _t = new TextBox();
            _t.Name = "txt_" + name;
            _t.Dock = DockStyle.Fill;
            _t.Text = value;

            var _l = new Label();
            _l.Dock = DockStyle.Left;
            _l.AutoSize = true;
            _l.TextAlign = ContentAlignment.MiddleLeft;
            _l.Text = name + ":";
            _l.ForeColor = Color.White;

            _p.Height = _l.Height + 2;
            _p.Controls.Add(_l);
            _p.Controls.Add(_t);

            _l.SendToBack();

            return _p;
        }

        /// <summary>
        /// 获取Json值
        /// </summary>
        /// <returns></returns>
        private string GetJsonValue()
        {
            var txts = panel1.Controls.Cast<Control>().Where(p => p is Panel).Reverse();

            if (parameter.SubParameters.Count == 0)
            {
                var list = new List<string>();
                foreach (var txt in txts)
                {
                    var text = txt.Controls.Cast<Control>().FirstOrDefault(p => p is TextBox);
                    if (text == null) continue;

                    if (!string.IsNullOrEmpty(text.Text.Trim()))
                    {
                        list.Add(text.Text.Trim());
                    }
                }

                return SerializeJson(list, false);
            }
            else
            {
                if (parameter.IsCollection)
                {
                    var list = new List<Dictionary<string, string>>();
                    var dict = new Dictionary<string, string>();
                    list.Add(dict);

                    foreach (var txt in txts)
                    {
                        var text = txt.Controls.Cast<Control>().FirstOrDefault(p => p is TextBox);
                        if (text == null) continue;

                        var _key = text.Name.Replace("txt_", "");
                        if (dict.ContainsKey(_key))
                        {
                            dict = new Dictionary<string, string>();
                            list.Add(dict);
                        }

                        dict[_key] = text.Text.Trim();
                    }

                    return SerializeJson(list, true);
                }
                else
                {
                    var dict = new Dictionary<string, string>();
                    foreach (var txt in txts)
                    {
                        var text = txt.Controls.Cast<Control>().FirstOrDefault(p => p is TextBox);
                        if (text == null) continue;

                        var _key = text.Name.Replace("txt_", "");
                        dict[_key] = text.Text.Trim();
                    }

                    return SerializeJson(dict, true);
                }
            }
        }

        /// <summary>
        /// 系列化成JSON
        /// </summary>
        /// <param name="value"></param>
        /// <param name="indented"></param>
        /// <returns></returns>
        private string SerializeJson(object value, bool indented)
        {
            return SerializationManager.SerializeJson(value, indented);
        }
    }
}
