using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using MySoft.IoC;
using MySoft.Logger;
using MySoft.RESTful;

namespace MySoft.Tools.CastleConfig
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
        }

        void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            SimpleLog.Instance.WriteLog(e.Exception);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            SimpleLog.Instance.WriteLog(e.ExceptionObject as Exception);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var filePath = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("请选择文件或填入正确的文件路径！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!File.Exists(filePath))
            {
                MessageBox.Show("选择或填入的文件路径不存在！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (checkBox1.Checked)
            {
                if (CreateConfig<PublishKindAttribute>(filePath))
                {

                    MessageBox.Show("配置生成成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("配置生成失败！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                if (CreateConfig<ServiceContractAttribute>(filePath))
                {
                    if (!string.IsNullOrEmpty(textBox2.Text.Trim()))
                    {
                        var filePath1 = textBox2.Text.Trim();
                        if (File.Exists(filePath1))
                        {
                            try { File.Delete(filePath1); }
                            catch { }
                        }
                        SimpleLog.WriteFile(filePath1, richTextBox1.Text);
                    }

                    MessageBox.Show("配置生成成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("配置生成失败！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = textBox1.Text.Trim();
            openFileDialog1.Multiselect = false;
            openFileDialog1.Filter = ".NET程序集(*.dll)|*.dll";
            var result = openFileDialog1.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                var filePath = openFileDialog1.FileName;
                textBox1.Text = filePath;

                if (!checkBox1.Checked)
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    textBox2.Text = string.Format("{0}\\{1}.config", CoreHelper.GetFullPath("config"), fileName);
                }
                else
                {
                    textBox2.Text = string.Empty;
                }
            }
        }

        private bool CreateConfig<TAttribute>(string filePath)
        {
            var template = "\t\t<component id=\"{0}\" service=\"{1}, {2}\" type=\"{3}, {4}\"/>";

            try
            {
                Assembly ass = Assembly.LoadFrom(filePath);

                var list = new List<string>();
                foreach (var type in ass.GetTypes())
                {
                    if (!type.IsClass) continue;
                    var typeInterfaces = type.GetInterfaces();
                    if (typeInterfaces.Length == 0) continue;

                    var iface = typeInterfaces[0];
                    var contract = CoreHelper.GetMemberAttribute<TAttribute>(iface);
                    if (contract == null) continue;

                    var obsolete = CoreHelper.GetMemberAttribute<ObsoleteAttribute>(iface);
                    if (obsolete != null) continue;

                    var value = string.Format(template, iface.FullName, iface.FullName, iface.Assembly.FullName.Split(',')[0],
                        type.FullName, type.Assembly.FullName.Split(',')[0]);

                    list.Add(value);
                }

                if (list.Count > 0)
                {
                    var xml = "<?xml version=\"1.0\"?>\r\n<configuration>\r\n\t<components>\r\n{0}\r\n\t</components>\r\n</configuration>";
                    richTextBox1.Text = string.Format(xml, string.Join("\r\n", list.ToArray()));

                    return true;
                }
                else
                {
                    MessageBox.Show("没有找到任何匹配的服务接口！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox2.Enabled = false;
            }
            else
            {
                textBox2.Enabled = true;
            }
        }
    }
}
