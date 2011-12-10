using System;
using System.Net;
using System.Windows.Forms;
using MySoft.IoC;
using MySoft.IoC.Status;

namespace MySoft.PlatformService.WinForm
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private IStatusService service;
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //if (string.IsNullOrEmpty(textBox1.Text))
                //{
                //    textBox1.Focus();
                //    return;
                //}

                //var service = CastleFactory.Create().GetChannel<IUserService>();
                //service.SendMessage(textBox1.Text);

                //richTextBox1.AppendText(textBox1.Text + "\r\n");
                //textBox1.Text = string.Empty;

                //textBox1.Focus();

                if (button1.Tag == null)
                {
                    var listener = new StatusListener(listBox1);
                    service = CastleFactory.Create().GetChannel<IStatusService>(listener);
                    service.Subscibe(Convert.ToDouble(numericUpDown1.Value));
                    button1.Text = "停止监控";
                    button1.Tag = label1.Text;
                    label1.Text = "正在进行监控...";
                    //button1.Enabled = false;
                }
                else
                {
                    service.Unsubscibe();
                    label1.Text = button1.Tag.ToString();
                    button1.Text = "开始监控";
                    button1.Tag = null;
                    listBox1.Items.Clear();
                    //button1.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class StatusListener : IStatusListener
    {
        private ListBox box;
        public StatusListener(ListBox box)
        {
            this.box = box;
        }


        #region IStatusListener 成员

        public void Push(EndPoint endPoint, bool connected)
        {
            box.BeginInvoke(new Action(() =>
            {
                var ip = endPoint as IPEndPoint;
                box.Items.Add(string.Format("{0}:{1} => {2}", ip.Address, ip.Port, connected ? "连接" : "断开"));
                box.SelectedIndex = box.Items.Count - 1;
            }));
        }

        public void Push(EndPoint endPoint, AppClient appClient)
        {
            //throw new NotImplementedException();
        }

        public void Push(ServerStatus serverStatus)
        {
            //throw new NotImplementedException();
        }

        public void Push(CallError callError)
        {
            //throw new NotImplementedException();

            box.BeginInvoke(new Action(() =>
            {
                box.Items.Add(string.Format("Error => {0}", callError.Message));
                box.SelectedIndex = box.Items.Count - 1;
            }));
        }

        public void Push(CallTimeout callTimeout)
        {
            //throw new NotImplementedException();

            box.BeginInvoke(new Action(() =>
            {
                box.Items.Add(string.Format("Timeout => {0} ms.", callTimeout.ElapsedTime));
                box.SelectedIndex = box.Items.Count - 1;
            }));
        }

        #endregion
    }
}
