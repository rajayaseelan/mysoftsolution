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
        public delegate void UpdateMessage(string msg);
        public frmMain()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CastleFactory.Create().OnError += new ErrorLogEventHandler(frmMain_OnError);
            IUserService service = CastleFactory.Create().GetService<IUserService>();

            int count = (int)numericUpDown1.Value;

            for (int i = 0; i < count; i++)
            {
                Thread thread = new Thread(DoWork);
                thread.Name = string.Format("Thread-->{0}", i);
                thread.IsBackground = true;
                thread.Start(service);
            }
        }

        void frmMain_OnError(Exception exception)
        {
            WriteMessage(exception.Message);
        }

        private void DoWork(object value)
        {
            IUserService service = value as IUserService;
            while (true)
            {
                Stopwatch watch = Stopwatch.StartNew();
                try
                {
                    UserInfo info = service.GetUserInfo("maoyong_" + new Random().Next(10000000));

                    string msg = string.Format("线程：{0} 耗时：{1} ms 数据：{2}", Thread.CurrentThread.Name, watch.ElapsedMilliseconds, info.Description);
                    WriteMessage(msg);
                }
                catch (Exception ex)
                {
                    string msg = string.Format("线程：{0} 耗时：{1} ms 异常：{2}", Thread.CurrentThread.Name, watch.ElapsedMilliseconds, ex.Message);
                    WriteMessage(msg);
                }

                Thread.Sleep(10);
            }
        }

        private void WriteMessage(string msg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateMessage(p =>
                {
                    richTextBox1.AppendText(p + "\r\n");
                    richTextBox1.Select(richTextBox1.TextLength, 0);
                    richTextBox1.ScrollToCaret();
                }), msg);
            }
            else
            {
                richTextBox1.AppendText(msg + "\r\n");
                richTextBox1.Select(richTextBox1.TextLength, 0);
                richTextBox1.ScrollToCaret();
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            var code = ImageHelper.CreateRandomCode(4);
            var image = ImageHelper.CreateImage(code);
            pictureBox1.Width = image.Width;
            pictureBox1.Height = image.Height;
            pictureBox1.Image = image;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            var code = ImageHelper.CreateRandomCode(new Random().Next(1, 15), CodeType.Char);
            //var code = "1234";
            //code = "15924186386";
            var image = ImageHelper.CreateImage(code);
            pictureBox1.Width = image.Width;
            pictureBox1.Height = image.Height;
            pictureBox1.Image = image;

            //MessageBox.Show(GetCodeText(image));
        }

        /// <summary>  
        /// 获取验证码  
        /// </summary>  
        /// <param name="p_Bitmap">图形 http://www.fjjj.gov.cn/Article/getcode.asp</param>  
        /// <returns>数值</returns>  
        public string GetCodeText(Bitmap p_Bitmap)
        {
            int _Width = p_Bitmap.Width / 4;
            int _Height = p_Bitmap.Height;

            Bitmap[] _Bitmap = new Bitmap[4];
            Rectangle _Rectangle = new Rectangle();
            _Rectangle.Width = _Width;
            _Rectangle.Height = _Height;
            for (int i = 0; i != _Bitmap.Length; i++)
            {
                _Bitmap[i] = p_Bitmap.Clone(_Rectangle, p_Bitmap.PixelFormat);
                _Rectangle.X += _Width;
            }
            int _Value1 = Array.IndexOf(_TextBytes, GetImageText(_Bitmap[0]));
            int _Value2 = Array.IndexOf(_TextBytes, GetImageText(_Bitmap[1]));
            int _Value3 = Array.IndexOf(_TextBytes, GetImageText(_Bitmap[2]));
            int _Value4 = Array.IndexOf(_TextBytes, GetImageText(_Bitmap[3]));

            string _Value = _Value1 == -1 ? "?" : _Value1.ToString();
            _Value += _Value2 == -1 ? "?" : _Value2.ToString();
            _Value += _Value3 == -1 ? "?" : _Value3.ToString();
            _Value += _Value4 == -1 ? "?" : _Value4.ToString();
            return _Value;

        }

        private string[] _TextBytes = new string[]  
        {  
            "E17BEFBDF7DE7BEFBDF7DE87FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",  
            "FBE3BFFFFEFBEFBFFFFEFB83FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",  
            "E17BFFFDF7EFDFBF7FFFFE03FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",  
            "E17BFFFDF7E37FFFFDF7DE87FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",  
            "EF9FBFFEFAEDBB0FFCFBEF1FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",  
            "C0FBEFBFFFE07FFFFDF7DE87FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",  
            "E3F7EFBFFFE273EFBDF7DE87FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",  
            "C07BFFFEFBF7DFBFFFFEFDF7FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",  
            "E17BEFBDF7E17BEFBDF7DE87FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",  
            "E17BEFBDF7CE47FFFDF7EFC7FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF"      
        };


        /// <summary>  
        /// 获取二值化数据  
        /// </summary>  
        /// <param name="p_Bitmap">图形</param>  
        /// <returns>二值化数据</returns>  
        public string GetImageText(Bitmap p_Bitmap)
        {
            int _Width = p_Bitmap.Width;
            int _Height = p_Bitmap.Height;
            BitmapData _Data = p_Bitmap.LockBits(new Rectangle(0, 0, _Width, _Height), ImageLockMode.ReadOnly, p_Bitmap.PixelFormat);

            byte[] _DataByte = new byte[_Data.Stride * _Height];

            Marshal.Copy(_Data.Scan0, _DataByte, 0, _DataByte.Length);

            BitArray _Bitarray = new BitArray(_DataByte.Length, true);

            int _Index = 0;
            for (int i = 0; i != _Height; i++)
            {
                int _WidthStar = i * _Data.Stride;
                for (int z = 0; z != _Width; z++)
                {
                    if (_DataByte[_WidthStar + (z * 3)] == 238 && _DataByte[_WidthStar + (z * 3) + 1] == 238 && _DataByte[_WidthStar + (z * 3) + 2] == 238)
                    {
                        _Bitarray[_Index] = true;
                    }
                    else
                    {
                        _Bitarray[_Index] = false;
                    }
                    _Index++;
                }
            }
            p_Bitmap.UnlockBits(_Data);

            int _ByteIndex = _Bitarray.Count / 8;
            if (_Bitarray.Count % 8 != 0) _ByteIndex++;

            byte[] _Temp = new byte[_ByteIndex];
            _Bitarray.CopyTo(_Temp, 0);

            return BitConverter.ToString(_Temp).Replace("-", "");
        }
    }
}
