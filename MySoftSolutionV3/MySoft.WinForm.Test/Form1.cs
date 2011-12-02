using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.Remoting.Messaging;

namespace MySoft.WinForm.Test
{
    delegate int AddHandler(int a, int b);

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread thread = null;
            AddHandler add = new AddHandler((a, b) =>
            {
                thread = Thread.CurrentThread;
                System.Threading.Thread.Sleep(10000);
                return a + b;
            });
            var ar = add.BeginInvoke(1, 2, iar => { }, add);

            if (!ar.AsyncWaitHandle.WaitOne(1000, true))
            {
                thread.Abort();
                //ar.AsyncWaitHandle.Close();
                textBox1.Text = "timeout!";
            }
            else
            {
                textBox1.Text = add.EndInvoke(ar).ToString();
                ar.AsyncWaitHandle.Close();
            }
        }
    }
}
