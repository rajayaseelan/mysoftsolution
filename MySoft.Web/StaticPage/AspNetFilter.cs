using System.IO;
using System.Text;
using System.Web;

namespace MySoft.Web
{
    /// <summary>
    /// 字符串筛选器
    /// </summary>
    public class AspNetFilter : Stream
    {
        private Stream m_sink;
        private long m_position;
        private bool replace;
        private string extension;
        private Encoding encoding;

        public AspNetFilter(Stream sink, bool replace, string extension)
        {
            this.m_sink = sink;
            this.replace = replace;
            this.extension = extension;

            //获取当前流的编码
            this.encoding = Encoding.GetEncoding(HttpContext.Current.Response.Charset);
        }

        // The following members of Stream must be overriden.
        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { return 0; }
        }

        public override long Position
        {
            get { return m_position; }
            set { m_position = value; }
        }

        public override long Seek(long offset, System.IO.SeekOrigin direction)
        {
            return m_sink.Seek(offset, direction);
        }

        public override void SetLength(long length)
        {
            m_sink.SetLength(length);
        }

        public override void Close()
        {
            m_sink.Close();

            WriteComplete();
        }

        public override void Flush()
        {
            m_sink.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return m_sink.Read(buffer, offset, count);
        }

        /// <summary>
        /// 写内容
        /// </summary>
        /// <param name="content"></param>
        public virtual void WriteContent(string content)
        {
            //do someing
        }

        /// <summary>
        /// 写内容结束
        /// </summary>
        public virtual void WriteComplete()
        {
            //do someing
        }

        // Override the Write method to filter Response to a file.
        public override void Write(byte[] buffer, int offset, int count)
        {
            //首先判断有没有系统错误
            if (HttpContext.Current.Error == null)
            {
                //内容进行编码处理
                string content = encoding.GetString(buffer, offset, count);

                WriteContent(content);

                //如果是文本，则启用过滤
                if (HttpContext.Current.Response.ContentType.ToLower().Contains("text/html"))
                {
                    //处理内容
                    byte[] data = null;
                    if (replace)
                        data = encoding.GetBytes(ReplaceContext(content));
                    else
                        data = encoding.GetBytes(content);

                    //Write out the response to the browser.
                    m_sink.Write(data, 0, data.Length);
                }
                else
                {
                    m_sink.Write(buffer, offset, count);
                }
            }
            else
            {
                //Write out the response to the browser.
                m_sink.Write(buffer, offset, count);
            }
        }

        protected string ReplaceContext(string content)
        {
            return WebHelper.ReplaceContext(content, extension);
        }
    }
}
