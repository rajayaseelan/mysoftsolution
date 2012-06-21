using System;
using System.Text;
using System.Text.RegularExpressions;

namespace MySoft.Web
{
    /// <summary>
    /// 分页样式
    /// </summary>
    public enum LinkStyle
    {
        /// <summary>
        /// Custom Style
        /// </summary>
        [EnumDescription("custom")]
        Custom,
        /// <summary>
        /// Digg Style
        /// </summary>
        [EnumDescription("digg")]
        Digg,
        /// <summary>
        /// Yahoo Style
        /// </summary>
        [EnumDescription("yahoo")]
        Yahoo,
        /// <summary>
        /// Meneame Style
        /// </summary>
        [EnumDescription("meneame")]
        Meneame,
        /// <summary>
        /// Flickr Style
        /// </summary>
        [EnumDescription("flickr")]
        Flickr,
        /// <summary>
        /// Sabrosus Style
        /// </summary>
        [EnumDescription("sabrosus")]
        Sabrosus,
        /// <summary>
        /// Pagination Style
        /// </summary>
        [EnumDescription("pagination")]
        Pagination,
        /// <summary>
        /// Scott Style
        /// </summary>
        [EnumDescription("scott")]
        Scott,
        /// <summary>
        /// Quotes Style
        /// </summary>
        [EnumDescription("quotes")]
        Quotes,
        /// <summary>
        /// Black Style
        /// </summary>
        [EnumDescription("black")]
        Black,
        /// <summary>
        /// Black2 Style
        /// </summary>
        [EnumDescription("black2")]
        Black2,
        /// <summary>
        /// BlackRed Style
        /// </summary>
        [EnumDescription("black-red")]
        BlackRed,
        /// <summary>
        /// Grayr Style
        /// </summary>
        [EnumDescription("grayr")]
        Grayr,
        /// <summary>
        /// Yellow Style
        /// </summary>
        [EnumDescription("yellow")]
        Yellow,
        /// <summary>
        /// Jogger Style
        /// </summary>
        [EnumDescription("jogger")]
        Jogger,
        /// <summary>
        /// Starcraft2 Style
        /// </summary>
        [EnumDescription("starcraft2")]
        Starcraft2,
        /// <summary>
        /// Tres Style
        /// </summary>
        [EnumDescription("tres")]
        Tres,
        /// <summary>
        /// Megas512 Style
        /// </summary>
        [EnumDescription("megas512")]
        Megas512,
        /// <summary>
        /// Technorati Style
        /// </summary>
        [EnumDescription("technorati")]
        Technorati,
        /// <summary>
        /// Youtube Style
        /// </summary>
        [EnumDescription("youtube")]
        Youtube,
        /// <summary>
        /// Msdn Style
        /// </summary>
        [EnumDescription("msdn")]
        Msdn,
        /// <summary>
        /// Badoo Style
        /// </summary>
        [EnumDescription("badoo")]
        Badoo,
        /// <summary>
        /// Manu Style
        /// </summary>
        [EnumDescription("manu")]
        Manu,
        /// <summary>
        /// GreenBlack Style
        /// </summary>
        [EnumDescription("green-black")]
        GreenBlack,
        /// <summary>
        /// Viciao Style
        /// </summary>
        [EnumDescription("viciao")]
        Viciao,
        /// <summary>
        /// Yahoo2 Style
        /// </summary>
        [EnumDescription("yahoo2")]
        Yahoo2
    }

    /// <summary>
    /// 链接样式
    /// </summary>
    public enum ButtonStyle
    {
        /// <summary>
        /// 链接
        /// </summary>
        Href,
        /// <summary>
        /// 按钮
        /// </summary>
        Button
    }

    /// <summary>
    /// 创建分页的html
    /// </summary>
    public class HtmlPager
    {
        /// <summary>
        /// 定义一个IDataPage属性
        /// </summary>
        private DataPage dataPage;

        private string firstUrl;
        /// <summary>
        /// 首页链接字符串
        /// </summary>
        public string FirstUrl
        {
            get { return firstUrl; }
            set { firstUrl = value; }
        }

        private string linkUrl;
        /// <summary>
        /// 翻页时链接字符串
        /// </summary>
        public string LinkUrl
        {
            get { return linkUrl; }
            set { linkUrl = value; }
        }

        private int linkSize;
        /// <summary>
        /// 每屏链接条数
        /// </summary>
        public int LinkSize
        {
            get { return linkSize; }
            set { linkSize = value; }
        }

        private LinkStyle lstyle;
        /// <summary>
        /// 链接样式
        /// </summary>
        public LinkStyle LinkStyle
        {
            get { return lstyle; }
            set { lstyle = value; }
        }

        private string linkCss;
        /// <summary>
        /// 分页样式
        /// </summary>
        public string LinkCss
        {
            set { linkCss = value; }
        }

        private ButtonStyle bstyle;
        /// <summary>
        /// 按钮样式
        /// </summary>
        public ButtonStyle ButtonStyle
        {
            get { return bstyle; }
            set { bstyle = value; }
        }

        private string pagerID = "$PageIndex";
        /// <summary>
        /// 页面参数ID
        /// </summary>
        public string PagerID
        {
            get { return pagerID; }
            set { pagerID = value; }
        }

        private string prevTitle = "上一页";
        /// <summary>
        /// 上一页标题
        /// </summary>
        public string PrevTitle
        {
            get { return prevTitle; }
            set { prevTitle = value; }
        }

        private string nextTitle = "下一页";
        /// <summary>
        /// 下一页标题
        /// </summary>
        public string NextTitle
        {
            get { return nextTitle; }
            set { nextTitle = value; }
        }

        private bool showBracket = false;
        /// <summary>
        /// 是否显示链接中的中括号
        /// </summary>
        public bool ShowBracket
        {
            get { return showBracket; }
            set { showBracket = value; }
        }

        private bool showGoto = true;
        /// <summary>
        /// 显示转到某页
        /// </summary>
        public bool ShowGoto
        {
            get { return showGoto; }
            set { showGoto = value; }
        }

        private bool showRecord = true;
        /// <summary>
        /// 显示记录信息
        /// </summary>
        public bool ShowRecord
        {
            get { return showRecord; }
            set { showRecord = value; }
        }

        /// <summary>
        /// 获取或设置DataPage
        /// </summary>
        public DataPage DataPage
        {
            get { return dataPage; }
            set { dataPage = value; }
        }

        /// <summary>
        /// 初始化HtmlPager对象
        /// </summary>
        /// <param name="dataPage"></param>
        public HtmlPager(DataPage dataPage)
        {
            this.dataPage = dataPage;
            this.linkSize = 10;
            this.lstyle = LinkStyle.Custom;
            this.bstyle = ButtonStyle.Href;
        }

        /// <summary>
        /// 初始化HtmlPager对象
        /// </summary>
        /// <param name="page">对应的数据源</param>
        /// <param name="linkUrl">对应的翻页格式必须设置，对应当前页</param>
        public HtmlPager(DataPage dataPage, string linkUrl)
            : this(dataPage)
        {
            this.linkUrl = linkUrl;
        }

        /// <summary>
        /// 初始化HtmlPager对象
        /// </summary>
        /// <param name="page">对应的数据源</param>
        /// <param name="linkUrl">对应的翻页格式必须设置，对应当前页</param>
        /// <param name="linkSize">每屏链接条数</param>
        public HtmlPager(DataPage dataPage, string linkUrl, int linkSize)
            : this(dataPage, linkUrl)
        {
            this.linkSize = linkSize;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (dataPage.CurrentPageIndex <= 0) dataPage.CurrentPageIndex = 1;
            if (dataPage.RowCount < 0) dataPage.RowCount = 0;

            int halfSize = Convert.ToInt32(Math.Floor(linkSize / 2.0));
            if (linkSize % 2 == 0) halfSize--;

            string html = string.Empty;

            //生成分页的html
            if (lstyle == LinkStyle.Custom)
            {
                sb.Append("<div id='htmlPager' class=\"" + (linkCss ?? EnumDescriptionAttribute.GetDescription(lstyle)) + "\">\n");
            }
            else
            {
                sb.Append("<div id='htmlPager' class=\"" + EnumDescriptionAttribute.GetDescription(lstyle) + "\">\n");
            }

            if (dataPage.PageCount == 0)
            {
                if (bstyle == ButtonStyle.Button)
                {
                    sb.Append("<input title=\"上一页\" type=\"button\" value=\"" + prevTitle + "\" disabled=\"disabled\" />\n");
                    sb.Append("<span class=\"current\">1</span>\n");
                    sb.Append("<input title=\"下一页\" type=\"button\" value=\"" + nextTitle + "\" disabled=\"disabled\" />\n");
                }
                else
                {
                    sb.Append("<span class=\"disabled\" title=\"上一页\">" + prevTitle + "</span>\n");
                    sb.Append("<span class=\"current\">1</span>\n");
                    sb.Append("<span class=\"disabled\" title=\"下一页\">" + nextTitle + "</span>\n");
                }
            }
            else
            {
                if (bstyle == ButtonStyle.Button)
                {
                    if (!dataPage.IsFirstPage)
                    {
                        sb.Append("<input title=\"上一页\" type=\"button\" onclick=\"" + GetButtonLink(dataPage.CurrentPageIndex - 1) + "\" value=\"" + prevTitle + "\" />\n");
                    }
                    else
                    {
                        sb.Append("<input title=\"上一页\" type=\"button\" value=\"" + prevTitle + "\" disabled=\"disabled\" />\n");
                    }
                }
                else
                {
                    if (!dataPage.IsFirstPage)
                    {
                        sb.Append("<a href=\"" + GetHtmlLink(dataPage.CurrentPageIndex - 1) + "\" title=\"上一页\">" + prevTitle + "</a>\n");
                    }
                    else
                    {
                        sb.Append("<span class=\"disabled\" title=\"上一页\">" + prevTitle + "</span>\n");
                    }
                }

                int startPage = dataPage.CurrentPageIndex;
                if (startPage <= halfSize || dataPage.PageCount <= linkSize) startPage = halfSize + 1;
                else if (startPage + halfSize >= dataPage.PageCount)
                {
                    startPage = dataPage.PageCount - halfSize;
                    if (linkSize % 2 == 0) startPage--;
                }

                int beginIndex = startPage - halfSize;
                int endIndex = startPage + halfSize;

                if (linkSize % 2 == 0) endIndex++;

                if (beginIndex - 1 > 0)
                {
                    if (beginIndex - 1 == 1)
                    {
                        sb.Append("<a href=\"" + GetHtmlLink(1) + "\" title=\"第1页\">[1]</a>\n");
                    }
                    else
                    {
                        sb.Append("<a href=\"" + GetHtmlLink(1) + "\" title=\"第1页\">[1]</a></span>...&nbsp;\n");
                    }
                }

                for (int index = beginIndex; index <= endIndex; index++)
                {
                    if (index > dataPage.PageCount) break;
                    if (index == dataPage.CurrentPageIndex)
                    {
                        sb.Append("<span class=\"current\">");
                        sb.Append(index);
                        sb.Append("</span>\n");
                    }
                    else
                    {
                        sb.Append("<a href=\"" + GetHtmlLink(index) + "\" title=\"第" + index + "页\">[" + index + "]</a>\n");
                    }
                }

                if (endIndex + 1 <= dataPage.PageCount)
                {
                    if (endIndex + 1 == dataPage.PageCount)
                    {
                        sb.Append("<a href=\"" + GetHtmlLink(endIndex + 1) + "\" title=\"第" + (endIndex + 1) + "页\">[" + (endIndex + 1) + "]</a>\n");
                    }
                    else
                    {
                        sb.Append("...&nbsp;<a href=\"" + GetHtmlLink(dataPage.PageCount) + "\" title=\"第" + dataPage.PageCount + "页\">[" + dataPage.PageCount + "]</a>\n");
                    }
                }

                if (bstyle == ButtonStyle.Button)
                {
                    if (!dataPage.IsLastPage)
                    {
                        sb.Append("<input title=\"下一页\" type=\"button\" onclick=\"" + GetButtonLink(dataPage.CurrentPageIndex + 1) + "\" value=\"" + nextTitle + "\" />\n");
                    }
                    else
                    {
                        sb.Append("<input title=\"下一页\" type=\"button\" value=\"" + nextTitle + "\" disabled=\"disabled\" />\n");
                    }
                }
                else
                {
                    if (!dataPage.IsLastPage)
                    {
                        sb.Append("<a href=\"" + GetHtmlLink(dataPage.CurrentPageIndex + 1) + "\" title=\"下一页\">" + nextTitle + "</a>\n");
                    }
                    else
                    {
                        sb.Append("<span class=\"disabled\" title=\"下一页\">" + nextTitle + "</span>\n");
                    }
                }
            }

            if (showGoto)
            {
                sb.Append("&nbsp;/&nbsp;第&nbsp;<select id=\"pageSelect\" onchange=\"" + GetHtmlLink("this.value") + "\">\n");
                if (dataPage.PageCount == 0)
                {
                    sb.Append("<option value=\"1\" selected=\"selected\">1</option>\n");
                }
                else
                {
                    for (int index = 1; index <= dataPage.PageCount; index++)
                    {
                        if (index == dataPage.CurrentPageIndex)
                        {
                            sb.Append("<option value=\"" + index + "\" selected=\"selected\">");
                            sb.Append(index);
                            sb.Append("</option>\n");
                        }
                        else
                        {
                            sb.Append("<option value=\"" + index + "\">");
                            sb.Append(index);
                            sb.Append("</option>\n");
                        }
                    }
                }
                sb.Append("</select>&nbsp;页&nbsp;\n");
            }

            if (showRecord)
            {
                sb.Append("&nbsp;共<span class=\"red\">" + dataPage.RowCount + "</span>条&nbsp;/&nbsp;每页<span class=\"red\">" + dataPage.PageSize + "</span>条\n");
            }

            sb.Append("<input type=\"hidden\" id=\"currentPage\" value=\"" + dataPage.CurrentPageIndex + "\"/>\n");
            sb.Append("</div>\n");

            html = sb.ToString();
            if (!showBracket)
            {
                Regex reg = new Regex(@"\[([\d]+)\]");
                html = reg.Replace(html, "$1");
            }

            return html;
        }

        private string GetHtmlLink(int value)
        {
            if (value == 1 && !string.IsNullOrEmpty(firstUrl))
                return firstUrl;
            else
                return linkUrl.Replace(pagerID, value.ToString());
        }

        private string GetHtmlLink(string value)
        {
            if (linkUrl.Contains("javascript:"))
            {
                string format = linkUrl.Replace("javascript:", string.Format("javascript:var selectValue={0};", value));
                value = "selectValue";
                return format.Replace(pagerID, value);
            }
            else
            {
                return string.Concat("javascript:location.href='", linkUrl.Replace(pagerID, ("'+" + value + "+'")), "';");
            }
        }

        /// <summary>
        /// 获取按钮事件
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetButtonLink(int value)
        {
            if (linkUrl.Contains("javascript:"))
            {
                return linkUrl.Replace(pagerID, value.ToString());
            }
            else
            {
                return string.Concat("javascript:location.href='", linkUrl.Replace(pagerID, value.ToString()), "';");
            }
        }
    }
}
