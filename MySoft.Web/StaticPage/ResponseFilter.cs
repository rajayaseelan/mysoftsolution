using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Linq;
using MySoft.Web.Configuration;
using System.Threading;
using System.Collections;
using MySoft.Threading;

namespace MySoft.Web
{
    /// <summary>
    /// 生成htm静态页面
    /// </summary>
    public class ResponseFilter : AspNetFilter
    {
        private string filePath = string.Empty;
        private string validateString = string.Empty;
        private StringBuilder pageContent = new StringBuilder();
        private Encoding encoding;

        public ResponseFilter(Stream sink, string filePath, string validateString, bool replace, string extension)
            : base(sink, replace, extension)
        {
            this.filePath = filePath;
            this.validateString = validateString;

            //获取当前流的编码
            this.encoding = Encoding.GetEncoding(HttpContext.Current.Response.Charset);
        }

        public override void WriteContent(string content)
        {
            pageContent.Append(content);
        }

        public override void WriteComplete()
        {
            //替换内容
            string content = ReplaceContext(pageContent.ToString());
            string path = HttpContext.Current.Request.Url.PathAndQuery;

            //启动生成线程
            ManagedThreadPool.QueueUserWorkItem(WriteFile, new ArrayList { content, path });
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="state"></param>
        private void WriteFile(object state)
        {
            ArrayList arr = state as ArrayList;
            string content = arr[0].ToString();

            //如果页面内容中包含指定的验证字符串则生成
            if (string.IsNullOrEmpty(validateString) || content.Contains(validateString))
            {
                //内容进行编码处理
                string dynamicurl = arr[1].ToString();
                string staticurl = filePath;

                string extension = Path.GetExtension(staticurl);
                if (extension != null && extension.ToLower() == ".js")
                {
                    //加入静态页生成元素
                    content = string.Format("{3}\r\n\r\n//<!-- 生成方式：被动生成 -->\r\n//<!-- 更新时间：{0} -->\r\n//<!-- 动态URL：{1} -->\r\n//<!-- 静态URL：{2} -->",
                                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), dynamicurl, RemoveRootPath(staticurl), content.Trim());
                }
                else
                {
                    //加入静态页生成元素
                    content = string.Format("{3}\r\n\r\n<!-- 生成方式：被动生成 -->\r\n<!-- 更新时间：{0} -->\r\n<!-- 动态URL：{1} -->\r\n<!-- 静态URL：{2} -->",
                                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), dynamicurl, RemoveRootPath(staticurl), content.Trim());
                }

                //将内容写入文件
                StaticPageManager.SaveFile(content, filePath, encoding);
            }
        }

        /// <summary>
        /// 去除根目录
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string RemoveRootPath(string path)
        {
            try
            {
                return path.Replace(AppDomain.CurrentDomain.BaseDirectory, "/").Replace("\\", "/").Replace("//", "/");
            }
            catch
            {
                return path;
            }
        }
    }
}
