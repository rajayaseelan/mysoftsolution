using MySoft.IoC.Messages;
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
        public frmParameter(ParameterInfo parameter)
        {
            this.parameter = parameter;

            InitializeComponent();
        }

        private void frmParameter_Load(object sender, EventArgs e)
        {
            label1.Text = string.Format("请输入参数[{0}]的数据：", parameter.Name);

            if (parameter.SubParameters.Count == 0)
            {
                numericUpDown1.Value = 5;
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

            if (parameter.SubParameters.Count == 0)
            {
                var color = Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
                for (int i = 0; i < count; i++)
                {
                    var cp = CreateChildPanel(parameter.Name);
                    cp.BackColor = color;

                    //添加控件
                    panel1.Controls.Add(cp);
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
                            var cp = CreateChildPanel(_pp.Name);
                            cp.BackColor = color;

                            //添加控件
                            panel1.Controls.Add(cp);
                        }
                    }
                }
                else
                {
                    var color = Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
                    foreach (var _pp in parameter.SubParameters)
                    {
                        var cp = CreateChildPanel(_pp.Name);
                        cp.BackColor = color;

                        //添加控件
                        panel1.Controls.Add(cp);
                    }
                }
            }

            for (int i = panel1.Controls.Count - 1; i >= 0; i--)
            {
                panel1.Controls[i].SendToBack();
            }
        }

        /// <summary>
        /// 创建子Panel
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private Panel CreateChildPanel(string name)
        {
            var _p = new Panel();
            _p.Dock = DockStyle.Top;

            var _t = new TextBox();
            _t.Name = "txt_" + name;
            _t.Dock = DockStyle.Fill;

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
                    var text = txt.Controls.Cast<Control>().First(p => p is TextBox);
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
                        var text = txt.Controls.Cast<Control>().First(p => p is TextBox);

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
                        var text = txt.Controls.Cast<Control>().First(p => p is TextBox);
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
