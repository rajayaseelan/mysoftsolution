using System;
using System.Drawing;
using System.IO;

namespace MySoft
{
    /// <summary>
    /// 缩放格式
    /// </summary>
    public enum ImageZoomMode
    {
        /// <summary>
        /// 指定高宽缩放（可能变形）
        /// </summary>
        HW,
        /// <summary>
        /// 指定宽，高按比例
        /// </summary>
        W,
        /// <summary>
        /// 指定高，宽按比例
        /// </summary>
        H,
        /// <summary>
        /// 指定高宽裁减（不变形） 
        /// </summary>
        Cut,
        /// <summary>
        /// 自动模式
        /// </summary>
        Auto
    }

    /// <summary>
    /// 验证码类型
    /// </summary>
    public enum CodeType
    {
        /// <summary>
        /// 数字
        /// </summary>
        Number,
        /// <summary>
        /// 字符
        /// </summary>
        Char,
        /// <summary>
        /// 数字和字符
        /// </summary>
        NumberChar
    }

    /// <summary>
    /// ImageHelper 的摘要说明
    /// </summary>
    public static class ImageHelper
    {
        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="originalImagePath">源图路径（物理路径）</param>
        /// <param name="thumbnailPath">缩略图路径（物理路径）</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param>
        /// <param name="mode">生成缩略图的方式</param>    
        public static void MakeThumbnail(string originalImagePath, string thumbnailPath, int width, int height, ImageZoomMode mode)
        {
            System.Drawing.Image originalImage = System.Drawing.Image.FromFile(originalImagePath);

            int towidth = width;
            int toheight = height;

            int x = 0;
            int y = 0;
            int ow = originalImage.Width;
            int oh = originalImage.Height;

            if (mode == ImageZoomMode.Auto)
            {
                if (ow > oh) mode = ImageZoomMode.H;
                else if (oh > ow) mode = ImageZoomMode.W;
            }

            switch (mode)
            {
                case ImageZoomMode.HW://指定高宽缩放（可能变形）                
                    break;
                case ImageZoomMode.W://指定宽，高按比例                    
                    toheight = originalImage.Height * width / originalImage.Width;
                    break;
                case ImageZoomMode.H://指定高，宽按比例
                    towidth = originalImage.Width * height / originalImage.Height;
                    break;
                case ImageZoomMode.Cut://指定高宽裁减（不变形）                
                    if ((double)originalImage.Width / (double)originalImage.Height > (double)towidth / (double)toheight)
                    {
                        oh = originalImage.Height;
                        ow = originalImage.Height * towidth / toheight;
                        y = 0;
                        x = (originalImage.Width - ow) / 2;
                    }
                    else
                    {
                        ow = originalImage.Width;
                        oh = originalImage.Width * height / towidth;
                        x = 0;
                        y = (originalImage.Height - oh) / 2;
                    }
                    break;
                default:
                    break;
            }

            //新建一个bmp图片
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(towidth, toheight);

            //新建一个画板
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);

            //设置高质量插值法
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

            //设置高质量,低速度呈现平滑程度
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充
            g.Clear(System.Drawing.Color.Transparent);

            //在指定位置并且按指定大小绘制原图片的指定部分
            g.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, towidth, toheight),
                new System.Drawing.Rectangle(x, y, ow, oh),
                System.Drawing.GraphicsUnit.Pixel);

            try
            {
                //以jpg格式保存缩略图
                bitmap.Save(thumbnailPath);
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                g.Dispose();
            }
        }

        /**/
        /// <summary>
        /// 在图片上增加文字水印
        /// </summary>
        /// <param name="Path">原服务器图片路径</param>
        /// <param name="WatermarkPath">生成的带文字水印的图片路径</param>
        public static void AddWordWatermark(string Path, string WatermarkPath, string WatermarkText)
        {
            System.Drawing.Image image = System.Drawing.Image.FromFile(Path);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(image);
            g.DrawImage(image, 0, 0, image.Width, image.Height);
            System.Drawing.Font f = new System.Drawing.Font("Verdana", 16);
            System.Drawing.Brush b = new System.Drawing.SolidBrush(System.Drawing.Color.Black);

            g.DrawString(WatermarkText, f, b, 15, 15);
            g.Dispose();

            image.Save(WatermarkPath);
            image.Dispose();
        }

        /**/
        /// <summary>
        /// 在图片上生成图片水印
        /// </summary>
        /// <param name="Path">原服务器图片路径</param>
        /// <param name="WatermarkPath">生成的带图片水印的图片路径</param>
        /// <param name="WatermarkImage">水印图片路径</param>
        public static void AddImageWatermark(string Path, string WatermarkPath, Image WatermarkImage)
        {
            System.Drawing.Image image = System.Drawing.Image.FromFile(Path);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(image);

            g.DrawImage(WatermarkImage, new System.Drawing.Rectangle(image.Width - WatermarkImage.Width - 10, image.Height - WatermarkImage.Height - 10, WatermarkImage.Width, WatermarkImage.Height), 0, 0, WatermarkImage.Width, WatermarkImage.Height, System.Drawing.GraphicsUnit.Pixel);
            g.Dispose();

            image.Save(WatermarkPath);
            image.Dispose();
            WatermarkImage.Dispose();
        }

        /// <summary>
        /// 生成随机码
        /// </summary>
        /// <param name="length">随机码个数</param>
        /// <returns></returns>
        public static string CreateRandomCode(int length)
        {
            return CreateRandomCode(length, CodeType.NumberChar);
        }

        /// <summary>
        /// 生成随机码
        /// </summary>
        /// <param name="length">随机码个数</param>
        /// <param name="number">是否为全数字</param>
        /// <returns></returns>
        public static string CreateRandomCode(int length, CodeType type)
        {
            int rand;
            char code;
            string randomcode = String.Empty;

            //生成一定长度的验证码
            System.Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                rand = random.Next();

                if (type == CodeType.Char)
                {
                    code = (char)('A' + (char)(rand % 26));
                }
                else if (type == CodeType.Number)
                {
                    code = (char)('0' + (char)(rand % 10));
                }
                else
                {
                    if (rand % 3 == 0)
                    {
                        code = (char)('A' + (char)(rand % 26));
                    }
                    else
                    {
                        code = (char)('0' + (char)(rand % 10));
                    }
                }

                randomcode += code.ToString();
            }

            return randomcode;
        }

        /// <summary>
        /// 创建随机码图片
        /// </summary>
        /// <param name="randomcode">随机码</param>
        public static Bitmap CreateImage(string randomcode)
        {
            return Image.FromStream(CreateImageStream(randomcode)) as Bitmap;
        }

        /// <summary>
        /// 创建随机码图片
        /// </summary>
        /// <param name="randomcode">随机码</param>
        public static byte[] CreateImageBytes(string randomcode)
        {
            return CreateImageStream(randomcode).ToArray();
        }

        /// <summary>
        /// 创建字节的图像，一般用于传输
        /// </summary>
        /// <param name="randomcode">随机码</param>
        /// <returns></returns>
        private static MemoryStream CreateImageStream(string randomcode)
        {
            int randAngle = 45; //随机转动角度
            int mapwidth = (int)(randomcode.Length * 14) + (18 - randomcode.Length * 2);
            Bitmap map = new Bitmap(mapwidth, 22);//创建图片背景
            Graphics graph = Graphics.FromImage(map);
            graph.Clear(Color.AliceBlue);//清除画面，填充背景
            graph.DrawRectangle(new Pen(Color.Black, 0), 0, 0, map.Width - 1, map.Height - 1);//画一个边框
            //graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;//模式

            Random rand = new Random();

            //背景噪点生成
            Pen blackPen = new Pen(Color.LightGray, 0);
            for (int i = 0; i < randomcode.Length * 15; i++)
            {
                int x = rand.Next(1, map.Width - 2);
                int y = rand.Next(1, map.Height - 2);
                graph.DrawRectangle(blackPen, x, y, 1, 1);
            }

            //验证码旋转，防止机器识别
            char[] chars = randomcode.ToCharArray();//拆散字符串成单字符数组

            //文字距中
            StringFormat format = new StringFormat(StringFormatFlags.NoClip);
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            //定义颜色
            Color[] c = { Color.Black, Color.Red, Color.DarkBlue, Color.Green, Color.Orange, Color.Brown, Color.DarkCyan, Color.Purple };
            //定义字体
            string[] font = { "Verdana", "Microsoft Sans Serif", "Comic Sans MS", "Arial", "宋体", "微软雅黑" };

            for (int i = 0; i < chars.Length; i++)
            {
                int findex = rand.Next(5);

                Font f = new System.Drawing.Font(font[findex], 14, System.Drawing.FontStyle.Bold);//字体样式(参数2为字体大小)

                int cindex = rand.Next(7);
                Brush b = new System.Drawing.SolidBrush(c[cindex]);

                Point dot = new Point(14, 14);
                //graph.DrawString(dot.X.ToString(),fontstyle,new SolidBrush(Color.Black),10,150);//测试X坐标显示间距的
                float angle = rand.Next(-randAngle, randAngle);//转动的度数

                graph.TranslateTransform(dot.X, dot.Y);//移动光标到指定位置
                graph.RotateTransform(angle);
                graph.DrawString(chars[i].ToString(), f, b, 1, -2, format);
                //graph.DrawString(chars[i].ToString(),fontstyle,new SolidBrush(Color.Blue),1,1,format);
                graph.RotateTransform(-angle);//转回去
                graph.TranslateTransform(-2, -dot.Y);//移动光标到指定位置，每个字符紧凑显示，避免被软件识别
            }

            //生成图片
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            ms.Position = 0;

            graph.Dispose();
            map.Dispose();

            return ms;
        }
    }
}
