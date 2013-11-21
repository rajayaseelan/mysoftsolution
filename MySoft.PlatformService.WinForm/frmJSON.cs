using System;
using System.Windows.Forms;

namespace MySoft.PlatformService.WinForm
{
    public partial class frmJSON : Form
    {
        public event JsonCallback OnCallback;

        public frmJSON()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var json = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(json))
            {
                textBox1.Focus();

                MessageBox.Show("请输入JSON数据。", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            if (OnCallback != null)
            {
                this.Close();

                OnCallback(json);
            }
        }
    }
}
