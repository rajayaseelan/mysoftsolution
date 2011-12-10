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
                if (button1.Tag == null)
                {
                    var listener = new StatusListener(tabControl1, listBox1, listBox2, listBox3);
                    service = CastleFactory.Create().GetChannel<IStatusService>(listener);

                    var options = new SubscibeOptions
                    {
                        PushCallError = checkBox1.Checked,
                        PushCallTimeout = checkBox2.Checked,
                        CallTimeout = Convert.ToDouble(numericUpDown1.Value) / 1000
                    };
                    service.Subscibe(options);
                    button1.Text = "停止监控";
                    button1.Tag = label1.Text;
                    label1.Text = "正在进行监控...";

                    checkBox1.Enabled = false;
                    checkBox2.Enabled = false;
                    //button1.Enabled = false;
                }
                else
                {
                    service.Unsubscibe();
                    label1.Text = button1.Tag.ToString();
                    button1.Text = "开始监控";
                    button1.Tag = null;
                    listBox1.Items.Clear();
                    listBox2.Items.Clear();
                    listBox3.Items.Clear();

                    tabControl1.TabPages[1].Text = "异常信息";
                    tabControl1.TabPages[2].Text = "超时信息";

                    checkBox1.Enabled = true;
                    checkBox2.Enabled = true;
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
        private TabControl control;
        private ListBox box1;
        private ListBox box2;
        private ListBox box3;
        public StatusListener(TabControl control, ListBox box1, ListBox box2, ListBox box3)
        {
            this.control = control;
            this.box1 = box1;
            this.box2 = box2;
            this.box3 = box3;
        }

        #region IStatusListener 成员

        public void Push(EndPoint endPoint, bool connected)
        {
            box1.BeginInvoke(new Action(() =>
            {
                var ip = endPoint as IPEndPoint;
                box1.Items.Add(string.Format("{0}:{1} => {2}", ip.Address, ip.Port, connected ? "连接" : "断开"));
                box1.SelectedIndex = box1.Items.Count - 1;
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

            box2.BeginInvoke(new Action(() =>
            {
                box2.Items.Add(string.Format("{0} => {1},{2}", callError.CallTime, callError.Caller.ServiceName, callError.Caller.SubServiceName));
                box2.Items.Add(string.Format("      Parameters: {0}", callError.Caller.Parameters));
                box2.Items.Add(string.Format("      Error：{0}", callError.Message));
                box2.SelectedIndex = box2.Items.Count - 1;

                control.TabPages[1].Text = "异常信息(" + box2.Items.Count / 3 + ")";
            }));
        }

        public void Push(CallTimeout callTimeout)
        {
            //throw new NotImplementedException();

            box3.BeginInvoke(new Action(() =>
            {
                box3.Items.Add(string.Format("{0} => {1},{2}", callTimeout.CallTime, callTimeout.Caller.ServiceName, callTimeout.Caller.SubServiceName));
                box3.Items.Add(string.Format("      Parameters: {0}", callTimeout.Caller.Parameters));
                box3.Items.Add(string.Format("      Timeout：{0} ms.", callTimeout.ElapsedTime));
                box3.SelectedIndex = box3.Items.Count - 1;

                control.TabPages[2].Text = "超时信息(" + box3.Items.Count / 3 + ")";
            }));
        }

        #endregion
    }
}
