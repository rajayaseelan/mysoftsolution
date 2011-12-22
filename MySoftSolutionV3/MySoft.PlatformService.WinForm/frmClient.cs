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

namespace MySoft.PlatformService.WinForm
{
    public delegate void CallbackEventHandler(string[] apps);

    public partial class frmClient : Form
    {
        public event CallbackEventHandler OnCallback;

        private IStatusService service;
        private IList<string> apps;
        public frmClient(IStatusService service, IList<string> apps)
        {
            InitializeComponent();

            this.service = service;
            this.apps = apps;
        }

        private void frmAppClient_Load(object sender, EventArgs e)
        {
            checkedListBox1.Items.Clear();

            try
            {
                var appClients = service.GetAppClients().OrderBy(p => p.AppName).ToList();
                foreach (var app in appClients)
                {
                    //存在应用则跳过
                    if (apps.Contains(app.AppName)) continue;

                    checkedListBox1.Items.Add(new CheckedListBoxItem
                    {
                        Text = string.Format("【{0}】 => {1}[{2}]", app.AppName, app.IPAddress, app.HostName),
                        Value = app
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public class CheckedListBoxItem
        {
            /// <summary>
            /// 内容
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// 对象值
            /// </summary>
            public object Value { get; set; }

            public override string ToString()
            {
                return this.Text;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //添加应用入口
            if (OnCallback != null)
            {
                var apps = new List<string>();
                foreach (var item in checkedListBox1.CheckedItems)
                {
                    var app = item as CheckedListBoxItem;
                    apps.Add((app.Value as AppClient).AppName);
                }

                OnCallback(apps.ToArray());
            }

            this.Close();
        }
    }
}
