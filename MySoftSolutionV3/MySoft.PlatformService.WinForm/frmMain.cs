using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using MySoft.Net.Sockets;
using MySoft.IoC;
using MySoft.PlatformService.UserService;
using MySoft.Logger;
using System.Drawing.Imaging;
using System.Collections;
using System.Runtime.InteropServices;

namespace MySoft.PlatformService.WinForm
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(textBox1.Text))
                {
                    textBox1.Focus();
                    return;
                }

                var service = CastleFactory.Create().CreateChannel<IUserService>();
                service.SendMessage(textBox1.Text);

                richTextBox1.AppendText(textBox1.Text + "\r\n");
                textBox1.Text = string.Empty;

                textBox1.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
