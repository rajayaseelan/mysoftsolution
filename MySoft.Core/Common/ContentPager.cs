using System;
using System.Collections.Generic;
using System.Text;

namespace MySoft
{
    /// <summary>
    /// 内容分页器
    /// </summary>
    public class ContentPager
    {
        private string content;
        private int words;
        private bool isother;

        /// <summary>
        /// 内容分页处理
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="words">每页字数</param>
        public ContentPager(string content, int words)
        {
            this.content = content;
            this.words = words;
        }

        /// <summary>
        /// 总共页数
        /// </summary>
        public int PageCount
        {
            get
            {
                double count = GetLen(content) * 1.0d / words * 1.0d;
                return (int)Math.Ceiling(count);
            }
        }

        /// <summary>
        /// 获取分布的内容
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public string GetContent(int page)
        {
            if (page <= 0 || page > PageCount)
            {
                throw new MySoftException("传入的页码不正确！");
            }

            isother = false;
            return GetPage(content, page, words);
        }

        /// <summary>
        /// 获取剩余的内容
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public string GetContentOther(int page)
        {
            if (page <= 0 || page > PageCount)
            {
                throw new MySoftException("传入的页码不正确！");
            }

            isother = true;
            return GetPage(content, page + 1, words);
        }

        /// <summary>
        /// 返回字符串的真实长度，一个汉字字符相当于两个单位长度
        /// </summary>
        /// <param name="str">指定字符串</param>
        /// <returns></returns>
        private int GetLen(string str)
        {
            int intResult = 0;
            foreach (char Char in str)
            {
                if ((int)Char > 127)
                    intResult += 2;
                else
                    intResult++;
            }
            return intResult;
        }

        /// <summary>
        /// 按照字符串的实际长度截取指定长度的字符串
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="length">指定长度</param>
        /// <returns></returns>
        private string GetPage(string text, int page, int length)
        {
            if (text == null) return string.Empty;
            int start = words * (page - 1);
            int i = 0, j = 0;
            foreach (char Char in text)
            {
                if ((int)Char > 127)
                    i += 2;
                else
                    i++;

                if (i > length)
                {
                    if (start > 0)
                    {
                        if (isother || page >= this.PageCount)
                            text = text.Substring(j * (page - 1));
                        else
                            text = text.Substring(j * (page - 1), j);
                    }
                    else
                        text = text.Substring(0, j);

                    break;
                }
                j++;
            }
            return text;
        }
    }
}
