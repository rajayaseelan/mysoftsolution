using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.IO;
using System.Management;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;

namespace MySoft.Common
{
    /// <summary>
    /// Function ：功能函数类，字符串处理类等
    /// </summary>
    public abstract class FunctionHelper
    {
        #region 正则表达式的使用

        /// <summary>
        /// 判断输入的字符串是否完全匹配正则
        /// </summary>
        /// <param name="RegexExpression">正则表达式</param>
        /// <param name="str">待判断的字符串</param>
        /// <returns></returns>
        public static bool IsValiable(string RegexExpression, string str)
        {
            bool blResult = false;

            Regex rep = new Regex(RegexExpression, RegexOptions.IgnoreCase);

            //blResult = rep.IsMatch(str);
            Match mc = rep.Match(str);

            if (mc.Success)
            {
                if (mc.Value == str) blResult = true;
            }


            return blResult;
        }

        /// <summary>
        /// 转换代码中的URL路径为绝对URL路径
        /// </summary>
        /// <param name="sourceString">源代码</param>
        /// <param name="replaceURL">替换要添加的URL</param>
        /// <returns>string</returns>
        public static string ConvertURL(string sourceString, string replaceURL)
        {
            Regex rep = new Regex(" (src|href|background|value)=('|\"|)([^('|\"|)http://].*?)('|\"| |>)");
            sourceString = rep.Replace(sourceString, " $1=$2" + replaceURL + "$3$4");
            return sourceString;
        }

        /// <summary>
        /// 获取代码中所有图片的以HTTP开头的URL地址
        /// </summary>
        /// <param name="sourceString">代码内容</param>
        /// <returns>ArrayList</returns>
        public static ArrayList GetImgFileUrl(string sourceString)
        {
            ArrayList imgArray = new ArrayList();

            Regex r = new Regex("<IMG(.*?)src=('|\"|)(http://.*?)('|\"| |>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            MatchCollection mc = r.Matches(sourceString);
            for (int i = 0; i < mc.Count; i++)
            {
                if (!imgArray.Contains(mc[i].Result("$3")))
                {
                    imgArray.Add(mc[i].Result("$3"));
                }
            }

            return imgArray;
        }

        /// <summary>
        /// 获取代码中所有文件的以HTTP开头的URL地址
        /// </summary>
        /// <param name="sourceString">代码内容</param>
        /// <returns>ArrayList</returns>
        public static Hashtable GetFileUrlPath(string sourceString)
        {
            Hashtable url = new Hashtable();

            Regex r = new Regex(" (src|href|background|value)=('|\"|)(http://.*?)('|\"| |>)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            MatchCollection mc = r.Matches(sourceString);
            for (int i = 0; i < mc.Count; i++)
            {
                if (!url.ContainsValue(mc[i].Result("$3")))
                {
                    url.Add(i, mc[i].Result("$3"));
                }
            }

            return url;
        }

        /// <summary>
        /// 获取一条SQL语句中的所参数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns></returns>
        public static ArrayList SqlParame(string sql)
        {
            ArrayList list = new ArrayList();
            Regex r = new Regex(@"@(?<x>[0-9a-zA-Z]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            MatchCollection mc = r.Matches(sql);
            for (int i = 0; i < mc.Count; i++)
            {
                list.Add(mc[i].Result("$1"));
            }

            return list;
        }

        /// <summary>
        /// 获取一条SQL语句中的所参数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns></returns>
        public static ArrayList OracleParame(string sql)
        {
            ArrayList list = new ArrayList();
            Regex r = new Regex(@":(?<x>[0-9a-zA-Z]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            MatchCollection mc = r.Matches(sql);
            for (int i = 0; i < mc.Count; i++)
            {
                list.Add(mc[i].Result("$1"));
            }

            return list;
        }

        /// <summary>
        /// 将HTML代码转化成纯文本
        /// </summary>
        /// <param name="sourceHTML">HTML代码</param>
        /// <returns></returns>
        public static string ConvertText(string sourceHTML)
        {
            string strResult = "";
            Regex r = new Regex("<(.*?)>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            MatchCollection mc = r.Matches(sourceHTML);

            if (mc.Count == 0)
            {
                strResult = sourceHTML;
            }
            else
            {
                strResult = sourceHTML;

                for (int i = 0; i < mc.Count; i++)
                {
                    strResult = strResult.Replace(mc[i].ToString(), "");
                }
            }

            return strResult;
        }
        #endregion

        #region 自定义处理

        /// <summary>
        /// 获取 web.config 文件中指定 key 的值
        /// </summary>
        /// <param name="keyName">key名称</param>
        /// <returns></returns>
        public static string GetAppSettings(string keyName)
        {
            return ConfigurationManager.AppSettings[keyName];
        }


        /// <summary>
        /// 按照指定格式输出时间
        /// </summary>
        /// <param name="NowDate">时间</param>
        /// <param name="type">输出类型</param>
        /// <returns></returns>
        public static string WriteDate(string NowDate, int type)
        {
            double TimeZone = 0;
            DateTime NewDate = DateTime.Parse(NowDate).AddHours(TimeZone);
            string strResult = "";

            switch (type)
            {
                case 1:
                    strResult = NewDate.ToString();
                    break;
                case 2:
                    strResult = NewDate.ToShortDateString().ToString();
                    break;
                case 3:
                    strResult = NewDate.Year + "年" + NewDate.Month + "月" + NewDate.Day + "日 " + NewDate.Hour + "点" + NewDate.Minute + "分" + NewDate.Second + "秒";
                    break;
                case 4:
                    strResult = NewDate.Year + "年" + NewDate.Month + "月" + NewDate.Day + "日";
                    break;
                case 5:
                    strResult = NewDate.Year + "年" + NewDate.Month + "月" + NewDate.Day + "日 " + NewDate.Hour + "点" + NewDate.Minute + "分";
                    break;
                case 6:
                    strResult = NewDate.Year + "-" + NewDate.Month + "-" + NewDate.Day + "  " + NewDate.Hour + ":" + NewDate.Minute;
                    break;
                default:
                    strResult = NewDate.ToString();
                    break;
            }
            return strResult;
        }


        private static int Instr(string strA, string strB)
        {
            if (string.Compare(strA, strA.Replace(strB, "")) > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }


        /// <summary>
        /// 判断客户端操作系统和浏览器的配置
        /// </summary>
        /// <param name="Info">客户端返回的头信息(Request.UserAgent)</param>
        /// <param name="Type">获取类型：1为操作系统， 2为浏览器</param>
        /// <returns></returns>
        public static string GetInfo(string Info, int Type)
        {

            string GetInfo = "";
            switch (Type)
            {
                case 1:
                    if (Instr(Info, @"NT 5.1") > 0)
                    {
                        GetInfo = "操作系统：Windows XP";
                    }
                    else if (Instr(Info, @"Tel") > 0)
                    {
                        GetInfo = "操作系统：Telport";
                    }
                    else if (Instr(Info, @"webzip") > 0)
                    {
                        GetInfo = "操作系统：操作系统：webzip";
                    }
                    else if (Instr(Info, @"flashget") > 0)
                    {
                        GetInfo = "操作系统：flashget";
                    }
                    else if (Instr(Info, @"offline") > 0)
                    {
                        GetInfo = "操作系统：offline";
                    }
                    else if (Instr(Info, @"NT 5") > 0)
                    {
                        GetInfo = "操作系统：Windows 2000";
                    }
                    else if (Instr(Info, @"NT 4") > 0)
                    {
                        GetInfo = "操作系统：Windows NT4";
                    }
                    else if (Instr(Info, @"98") > 0)
                    {
                        GetInfo = "操作系统：Windows 98";
                    }
                    else if (Instr(Info, @"95") > 0)
                    {
                        GetInfo = "操作系统：Windows 95";
                    }
                    else
                    {
                        GetInfo = "操作系统：未知";
                    }
                    break;
                case 2:
                    if (Instr(Info, @"NetCaptor 6.5.0") > 0)
                    {
                        GetInfo = "浏 览 器：NetCaptor 6.5.0";
                    }
                    else if (Instr(Info, @"MyIe 3.1") > 0)
                    {
                        GetInfo = "浏 览 器：MyIe 3.1";
                    }
                    else if (Instr(Info, @"NetCaptor 6.5.0RC1") > 0)
                    {
                        GetInfo = "浏 览 器：NetCaptor 6.5.0RC1";
                    }
                    else if (Instr(Info, @"NetCaptor 6.5.PB1") > 0)
                    {
                        GetInfo = "浏 览 器：NetCaptor 6.5.PB1";
                    }
                    else if (Instr(Info, @"MSIE 6.0b") > 0)
                    {
                        GetInfo = "浏 览 器：Internet Explorer 6.0b";
                    }
                    else if (Instr(Info, @"MSIE 6.0") > 0)
                    {
                        GetInfo = "浏 览 器：Internet Explorer 6.0";
                    }
                    else if (Instr(Info, @"MSIE 5.5") > 0)
                    {
                        GetInfo = "浏 览 器：Internet Explorer 5.5";
                    }
                    else if (Instr(Info, @"MSIE 5.01") > 0)
                    {
                        GetInfo = "浏 览 器：Internet Explorer 5.01";
                    }
                    else if (Instr(Info, @"MSIE 5.0") > 0)
                    {
                        GetInfo = "浏 览 器：Internet Explorer 5.0";
                    }
                    else if (Instr(Info, @"MSIE 4.0") > 0)
                    {
                        GetInfo = "浏 览 器：Internet Explorer 4.0";
                    }
                    else
                    {
                        GetInfo = "浏 览 器：未知";
                    }
                    break;
            }
            return GetInfo;
        }


        /// <summary>
        /// 获取服务器本机的MAC地址
        /// </summary>
        /// <returns></returns>
        public static string GetMAC_Address()
        {
            string strResult = "";

            ManagementObjectSearcher query = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection queryCollection = query.Get();
            foreach (ManagementObject mo in queryCollection)
            {
                if (mo["IPEnabled"].ToString() == "True") strResult = mo["MacAddress"].ToString();
            }

            return strResult;
        }

        /// <summary>
        /// 转换文件路径中不规则字符
        /// </summary>
        /// <param name="path"></param>
        /// <returns>string</returns>
        public static string ConvertDirURL(string path)
        {
            return AddLast(path.Replace("/", "\\"), "\\");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns>string</returns>
        public static string ConvertXmlString(string str)
        {
            return "<![CDATA[" + str + "]]>";
        }

        /// <summary>
        /// 转换一个double型数字串为时间，起始 0 为 1970-01-01 08:00:00
        /// 原理就是，每过一秒就在这个数字串上累加一
        /// </summary>
        /// <param name="d">double 型数字</param>
        /// <returns>DateTime</returns>
        public static DateTime ConvertIntDateTime(double d)
        {
            DateTime time = DateTime.MinValue;

            DateTime startTime = DateTime.Parse("1970-01-01 08:00:00");

            time = startTime.AddSeconds(d);

            return time;
        }

        /// <summary>
        /// 转换时间为一个double型数字串，起始 0 为 1970-01-01 08:00:00
        /// 原理就是，每过一秒就在这个数字串上累加一
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns>double</returns>
        public static double ConvertDateTimeInt(DateTime time)
        {
            double intResult = 0;

            DateTime startTime = DateTime.Parse("1970-01-01 08:00:00");

            intResult = (time - startTime).TotalSeconds;

            return intResult;
        }

        /// <summary>
        /// 获取一个URL中引用的文件名称（包括后缀符）
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <returns>string</returns>
        public static string GetFileName(string url)
        {
            //string[] Name = FunctionHelper.SplitArray(url,'/');
            //return Name[Name.Length - 1];

            return System.IO.Path.GetFileName(url);
        }

        /// <summary>
        /// 检测某一字符串的第一个字符是否与指定的
        /// 字符一致，否则在该字符串前加上这个字符
        /// </summary>
        /// <param name="Strings">字符串</param>
        /// <param name="Str">字符</param>
        /// <returns>返回 string</returns>
        public static string AddFirst(string Strings, string Str)
        {
            string strResult = "";
            if (Strings.StartsWith(Str))
            {
                strResult = Strings;
            }
            else
            {
                strResult = String.Concat(Str, Strings);
            }
            return strResult;
        }


        /// <summary>
        /// 检测某一字符串的最后一个字符是否与指定的
        /// 字符一致，否则在该字符串末尾加上这个字符
        /// </summary>
        /// <param name="Strings">字符串</param>
        /// <param name="Str">字符</param>
        /// <returns>返回 string</returns>
        public static string AddLast(string Strings, string Str)
        {
            string strResult = "";
            if (Strings.EndsWith(Str))
            {
                strResult = Strings;
            }
            else
            {
                strResult = String.Concat(Strings, Str);
            }
            return strResult;
        }

        /// <summary>
        /// 检测某一字符串的第一个字符是否与指定的
        /// 字符一致，相同则去掉这个字符
        /// </summary>
        /// <param name="Strings">字符串</param>
        /// <param name="Str">字符</param>
        /// <returns>返回 string</returns>
        public static string DelFirst(string Strings, string Str)
        {
            string strResult = "";
            if (Strings.Length == 0) throw new Exception("原始字符串长度为零");

            if (Strings.StartsWith(Str))
            {
                strResult = Strings.Substring(Str.Length, Strings.Length - 1);
            }
            else
            {
                strResult = Strings;
            }

            return strResult;
        }

        /// <summary>
        /// 检测某一字符串的最后一个字符是否与指定的
        /// 字符一致，相同则去掉这个字符
        /// </summary>
        /// <param name="Strings">字符串</param>
        /// <param name="Str">字符</param>
        /// <returns>返回 string</returns>
        public static string DelLast(string Strings, string Str)
        {
            string strResult = "";

            if (Strings.EndsWith(Str))
            {
                strResult = Strings.Substring(0, Strings.Length - Str.Length);
            }
            else
            {
                strResult = Strings;
            }

            return strResult;
        }

        /// <summary>
        /// 获取一个目录的绝对路径（适用于WEB应用程序）
        /// </summary>
        /// <param name="folderPath">目录路径</param>
        /// <returns></returns>
        public static string GetRealPath(string folderPath)
        {
            string strResult = "";

            if (folderPath.IndexOf(":\\") > 0)
            {
                strResult = AddLast(folderPath, "\\");
            }
            else
            {
                if (folderPath.StartsWith("~/"))
                {
                    strResult = AddLast(System.Web.HttpContext.Current.Server.MapPath(folderPath), "\\");
                }
                else
                {
                    string webPath = System.Web.HttpContext.Current.Request.ApplicationPath + "/";
                    strResult = AddLast(System.Web.HttpContext.Current.Server.MapPath(webPath + folderPath), "\\");
                }
            }

            return strResult;
        }

        /// <summary>
        /// 获取一个文件的绝对路径（适用于WEB应用程序）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>string</returns>
        public static string GetRealFile(string filePath)
        {
            string strResult = "";

            //strResult = ((file.IndexOf(@":\") > 0 || file.IndexOf(":/") > 0) ? file : System.Web.HttpContext.Current.Server.MapPath(System.Web.HttpContext.Current.Request.ApplicationPath + "/" + file));
            strResult = ((filePath.IndexOf(":\\") > 0) ?
                filePath :
                System.Web.HttpContext.Current.Server.MapPath(filePath));

            return strResult;
        }

        /// <summary>
        /// 对字符串进行 HTML 编码操作
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static string HtmlEncode(string str)
        {
            str = str.Replace("&", "&amp;");
            str = str.Replace("'", "''");
            str = str.Replace("\"", "&quot;");
            str = str.Replace(" ", "&nbsp;");
            str = str.Replace("<", "&lt;");
            str = str.Replace(">", "&gt;");
            str = str.Replace("\n", "<br>");
            return str;
        }


        /// <summary>
        /// 对 HTML 字符串进行解码操作
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static string HtmlDecode(string str)
        {
            str = str.Replace("<br>", "\n");
            str = str.Replace("&gt;", ">");
            str = str.Replace("&lt;", "<");
            str = str.Replace("&nbsp;", " ");
            str = str.Replace("&quot;", "\"");
            return str;
        }

        /// <summary>
        /// 对脚本程序进行处理
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ConvertScript(string str)
        {
            string strResult = "";
            if (str != "")
            {
                StringReader sr = new StringReader(str);
                string rl;
                do
                {
                    strResult += sr.ReadLine();
                } while ((rl = sr.ReadLine()) != null);
            }

            strResult = strResult.Replace("\"", "&quot;");

            return strResult;
        }


        /// <summary>
        /// 将一个字符串以某一特定字符分割成字符串数组
        /// </summary>
        /// <param name="Strings">字符串</param>
        /// <param name="str">分割字符</param>
        /// <returns>string[]</returns>
        public static string[] SplitArray(string Strings, char str)
        {
            string[] strArray = Strings.Trim().Split(new char[] { str });

            return strArray;
        }

        /*
                /// <summary>
                /// 将一个字符串以某一字符分割成数组
                /// </summary>
                /// <param name="Strings">字符串</param>
                /// <param name="str">分割字符</param>
                /// <returns>string[]</returns>
                public static string[] SplitArray(string Strings, string str)
                {
                    Regex r = new Regex(str);
                    string[] strArray = r.Split(Strings.Trim());

                    return strArray;
                }

        */

        /// <summary>
        /// 检测一个字符串，是否存在于一个以固定分割符分割的字符串中
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="Strings">固定分割符分割的字符串</param>
        /// <param name="Str">分割符</param>
        /// <returns></returns>
        public static bool InArray(string str, string Strings, char Str)
        {
            bool blResult = false;

            string[] array = SplitArray(Strings, Str);
            for (int i = 0; i < array.Length; i++)
            {
                if (str == array[i])
                {
                    blResult = true;
                    break;
                }
            }

            return blResult;
        }

        /*
                /// <summary>
                /// 检测一个字符串，是否存在于一个以固定分割符分割的字符串中
                /// </summary>
                /// <param name="str">字符串</param>
                /// <param name="Strings">固定分割符分割的字符串</param>
                /// <param name="Str">分割符</param>
                /// <returns></returns>
                public static bool InArray(string str, string Strings, string Str)
                {
                    bool blResult = false;

                    string[] array = SplitArray(Strings, Str);
                    for(int i = 0; i < array.Length; i++)
                    {
                        if(str == array[i])
                        {
                            blResult = true;
                            break;
                        }
                    }

                    return blResult;
                }
                */

        /// <summary>
        /// 检测一个字符串，是否存在于一个以固定分割符分割的字符串中
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="array">字符串数组</param>
        /// <returns></returns>
        public static bool InArray(string str, string[] array)
        {
            bool blResult = false;

            for (int i = 0; i < array.Length; i++)
            {
                if (str == array[i])
                {
                    blResult = true;
                    break;
                }
            }

            return blResult;
        }


        /// <summary>
        /// 检测值是否有效，为 null 或 "" 均为无效
        /// </summary>
        /// <param name="obj">要检测的值</param>
        /// <returns></returns>
        public static bool CheckValiable(object obj)
        {
            if (Object.Equals(obj, null) || Object.Equals(obj, string.Empty))
                return false;
            else
                return true;
        }
        #endregion

        #region 构造获取分页操作SQL语句

        #region ConstructSplitSQL
        /// <summary>
        /// 获取分页操作SQL语句(对于排序的字段必须建立索引，优化分页提取方式)
        /// </summary>
        /// <param name="tblName">操作表名称</param>
        /// <param name="fldName">排序的索引字段</param>
        /// <param name="PageIndex">当前页</param>
        /// <param name="PageSize">每页显示记录数</param>
        /// <param name="totalRecord">总记录数</param>
        /// <param name="OrderType">排序方式(0升序，1为降序)</param>
        /// <param name="strWhere">检索的条件语句，不需要再加WHERE关键字</param>
        /// <returns></returns>
        public static string ConstructSplitSQL(string tblName,
                                                string fldName,
                                                int PageIndex,
                                                int PageSize,
                                                int totalRecord,
                                                int OrderType,
                                                string strWhere)
        {
            string strSQL = "";
            string strOldWhere = "";
            string rtnFields = "*";

            // 构造检索条件语句字符串
            if (strWhere != "")
            {
                // 去除不合法的字符，防止SQL注入式攻击
                strWhere = strWhere.Replace("'", "''");
                strWhere = strWhere.Replace("--", "");
                strWhere = strWhere.Replace(";", "");

                strOldWhere = " AND " + strWhere + " ";

                strWhere = " WHERE " + strWhere + " ";
            }

            // 升序操作
            if (OrderType == 0)
            {
                if (PageIndex == 1)
                {
                    strSQL += "SELECT TOP " + PageSize + " " + rtnFields + " FROM " + tblName + " ";

                    //strSQL += "WHERE (" + fldName + " >= ( SELECT MAX(" + fldName + ") FROM (SELECT TOP 1 " + fldName + " FROM " + tblName + strWhere + " ORDER BY " + fldName + " ASC ) AS T )) ";

                    //strSQL += strOldWhere + "ORDER BY " + fldName + " ASC";
                    strSQL += strWhere + "ORDER BY " + fldName + " ASC";
                }
                else
                {
                    strSQL += "SELECT TOP " + PageSize + " " + rtnFields + " FROM " + tblName + " ";

                    strSQL += "WHERE (" + fldName + " > ( SELECT MAX(" + fldName + ") FROM (SELECT TOP " + ((PageIndex - 1) * PageSize) + " " + fldName + " FROM " + tblName + strWhere + " ORDER BY " + fldName + " ASC ) AS T )) ";

                    strSQL += strOldWhere + "ORDER BY " + fldName + " ASC";
                }
            }
            // 降序操作
            else if (OrderType == 1)
            {
                if (PageIndex == 1)
                {
                    strSQL += "SELECT TOP " + PageSize + " " + rtnFields + " FROM " + tblName + " ";

                    //strSQL += "WHERE (" + fldName + " <= ( SELECT MIN(" + fldName + ") FROM (SELECT TOP 1 " + fldName + " FROM " + tblName + strWhere + " ORDER BY " + fldName + " DESC ) AS T )) ";

                    //strSQL += strOldWhere + "ORDER BY " + fldName + " DESC";
                    strSQL += strWhere + "ORDER BY " + fldName + " DESC";
                }
                else
                {
                    strSQL += "SELECT TOP " + PageSize + " " + rtnFields + " FROM " + tblName + " ";

                    strSQL += "WHERE (" + fldName + " < ( SELECT MIN(" + fldName + ") FROM (SELECT TOP " + ((PageIndex - 1) * PageSize) + " " + fldName + " FROM " + tblName + strWhere + " ORDER BY " + fldName + " DESC ) AS T )) ";

                    strSQL += strOldWhere + "ORDER BY " + fldName + " DESC";
                }
            }
            else // 异常处理
            {
                throw new DataException("未指定任何排序类型。0升序，1为降序");
            }

            return strSQL;
        }


        /// <summary>
        /// 获取分页操作SQL语句(对于排序的字段必须建立索引)
        /// </summary>
        /// <param name="tblName">操作表名</param>
        /// <param name="fldName">操作索引字段名称</param>
        /// <param name="PageIndex">当前页</param>
        /// <param name="PageSize">每页显示记录数</param>
        /// <param name="rtnFields">返回字段集合，中间用逗号格开。返回全部用“*”</param>
        /// <param name="OrderType">排序方式(0升序，1为降序)</param>
        /// <param name="strWhere">检索的条件语句，不需要再加WHERE关键字</param>
        /// <returns></returns>
        public static string ConstructSplitSQL(string tblName,
                                                string fldName,
                                                int PageIndex,
                                                int PageSize,
                                                string rtnFields,
                                                int OrderType,
                                                string strWhere)
        {
            string strSQL = "";
            string strOldWhere = "";

            // 构造检索条件语句字符串
            if (strWhere != "")
            {
                // 去除不合法的字符，防止SQL注入式攻击
                strWhere = strWhere.Replace("'", "''");
                strWhere = strWhere.Replace("--", "");
                strWhere = strWhere.Replace(";", "");

                strOldWhere = " AND " + strWhere + " ";

                strWhere = " WHERE " + strWhere + " ";
            }

            // 升序操作
            if (OrderType == 0)
            {
                if (PageIndex == 1)
                {
                    strSQL += "SELECT TOP " + PageSize + " " + rtnFields + " FROM " + tblName + " ";

                    //strSQL += "WHERE (" + fldName + " >= ( SELECT MAX(" + fldName + ") FROM (SELECT TOP 1 " + fldName + " FROM " + tblName + strWhere + " ORDER BY " + fldName + " ASC ) AS T )) ";

                    //strSQL += strOldWhere + "ORDER BY " + fldName + " ASC";
                    strSQL += strWhere + "ORDER BY " + fldName + " ASC";
                }
                else
                {
                    strSQL += "SELECT TOP " + PageSize + " " + rtnFields + " FROM " + tblName + " ";

                    strSQL += "WHERE (" + fldName + " > ( SELECT MAX(" + fldName + ") FROM (SELECT TOP " + ((PageIndex - 1) * PageSize) + " " + fldName + " FROM " + tblName + strWhere + " ORDER BY " + fldName + " ASC ) AS T )) ";

                    strSQL += strOldWhere + "ORDER BY " + fldName + " ASC";
                }
            }
            // 降序操作
            else if (OrderType == 1)
            {
                if (PageIndex == 1)
                {
                    strSQL += "SELECT TOP " + PageSize + " " + rtnFields + " FROM " + tblName + " ";

                    //strSQL += "WHERE (" + fldName + " <= ( SELECT MIN(" + fldName + ") FROM (SELECT TOP 1 " + fldName + " FROM " + tblName + strWhere + " ORDER BY " + fldName + " DESC ) AS T )) ";

                    //strSQL += strOldWhere + "ORDER BY " + fldName + " DESC";
                    strSQL += strWhere + "ORDER BY " + fldName + " DESC";
                }
                else
                {
                    strSQL += "SELECT TOP " + PageSize + " " + rtnFields + " FROM " + tblName + " ";

                    strSQL += "WHERE (" + fldName + " < ( SELECT MIN(" + fldName + ") FROM (SELECT TOP " + ((PageIndex - 1) * PageSize) + " " + fldName + " FROM " + tblName + strWhere + " ORDER BY " + fldName + " DESC ) AS T )) ";

                    strSQL += strOldWhere + "ORDER BY " + fldName + " DESC";
                }
            }
            else // 异常处理
            {
                throw new DataException("未指定任何排序类型。0升序，1为降序");
            }

            return strSQL;
        }


        /// <summary>
        /// 获取分页操作SQL语句(对于排序的字段必须建立索引)
        /// </summary>
        /// <param name="tblName">操作表名</param>
        /// <param name="fldName">操作索引字段名称</param>
        /// <param name="unionCondition">用于连接的条件，例如: LEFT JOIN UserInfo u ON (u.UserID = b.UserID)</param>
        /// <param name="PageIndex">当前页</param>
        /// <param name="PageSize">每页显示记录数</param>
        /// <param name="rtnFields">返回字段集合，中间用逗号格开。返回全部用“*”</param>
        /// <param name="OrderType">排序方式，0升序，1为降序</param>
        /// <param name="strWhere">检索的条件语句，不需要再加WHERE关键字</param>
        /// <returns></returns>
        public static string ConstructSplitSQL(string tblName,
            string fldName,
            string unionCondition,
            int PageIndex,
            int PageSize,
            string rtnFields,
            int OrderType,
            string strWhere)
        {
            string strSQL = "";
            string strOldWhere = "";

            // 构造检索条件语句字符串
            if (strWhere != "")
            {
                // 去除不合法的字符，防止SQL注入式攻击
                strWhere = strWhere.Replace("'", "''");
                strWhere = strWhere.Replace("--", "");
                strWhere = strWhere.Replace(";", "");

                strOldWhere = " AND " + strWhere + " ";

                strWhere = " WHERE " + strWhere + " ";
            }

            // 升序操作
            if (OrderType == 0)
            {
                if (PageIndex == 1)
                {
                    strSQL += "SELECT TOP " + PageSize + " " + rtnFields + " FROM " + unionCondition + " ";

                    //strSQL += "WHERE (" + fldName + " >= ( SELECT MAX(" + fldName + ") FROM (SELECT TOP 1 " + fldName + " FROM " + tblName + strWhere + " ORDER BY " + fldName + " ASC ) AS T )) ";

                    //strSQL += strOldWhere + "ORDER BY " + fldName + " ASC";
                    strSQL += strWhere + "ORDER BY " + fldName + " ASC";
                }
                else
                {
                    strSQL += "SELECT TOP " + PageSize + " " + rtnFields + " FROM " + unionCondition + " ";

                    strSQL += "WHERE (" + fldName + " > ( SELECT MAX(" + fldName + ") FROM (SELECT TOP " + ((PageIndex - 1) * PageSize) + " " + fldName + " FROM " + tblName + strWhere + " ORDER BY " + fldName + " ASC ) AS T )) ";

                    strSQL += strOldWhere + "ORDER BY " + fldName + " ASC";
                }
            }
            // 降序操作
            else if (OrderType == 1)
            {
                if (PageIndex == 1)
                {
                    strSQL += "SELECT TOP " + PageSize + " " + rtnFields + " FROM " + unionCondition + " ";

                    //strSQL += "WHERE (" + fldName + " <= ( SELECT MIN(" + fldName + ") FROM (SELECT TOP 1 " + fldName + " FROM " + tblName + strWhere + " ORDER BY " + fldName + " DESC ) AS T )) ";

                    //strSQL += strOldWhere + "ORDER BY " + fldName + " DESC";
                    strSQL += strWhere + "ORDER BY " + fldName + " DESC";
                }
                else
                {
                    strSQL += "SELECT TOP " + PageSize + " " + rtnFields + " FROM " + unionCondition + " ";

                    strSQL += "WHERE (" + fldName + " < ( SELECT MIN(" + fldName + ") FROM (SELECT TOP " + ((PageIndex - 1) * PageSize) + " " + fldName + " FROM " + tblName + strWhere + " ORDER BY " + fldName + " DESC ) AS T )) ";

                    strSQL += strOldWhere + "ORDER BY " + fldName + " DESC";
                }
            }
            else // 异常处理
            {
                throw new DataException("未指定任何排序类型。0升序，1为降序");
            }

            return strSQL;
        }
        #endregion


        #region ConstructSplitSQL_TOP


        /// <summary>
        /// 获取分页操作SQL语句(对于排序的字段必须建立索引)
        /// </summary>
        /// <param name="tblName">操作表名</param>
        /// <param name="fldName">操作索引字段名称</param>
        /// <param name="PageIndex">当前页</param>
        /// <param name="PageSize">每页显示记录数</param>
        /// <param name="rtnFields">返回字段集合，中间用逗号格开。返回全部用“*”</param>
        /// <param name="OrderType">排序方式(0升序，1为降序)</param>
        /// <param name="strWhere">检索的条件语句，不需要再加WHERE关键字</param>
        /// <returns></returns>
        public static string ConstructSplitSQL_TOP(string tblName,
                                                    string fldName,
                                                    int PageIndex,
                                                    int PageSize,
                                                    string rtnFields,
                                                    int OrderType,
                                                    string strWhere)
        {
            string strSQL = "";
            string strOldWhere = "";

            // 构造检索条件语句字符串
            if (strWhere != "")
            {
                // 去除不合法的字符，防止SQL注入式攻击
                strWhere = strWhere.Replace("'", "''");
                strWhere = strWhere.Replace("--", "");
                strWhere = strWhere.Replace(";", "");

                strOldWhere = " AND " + strWhere + " ";

                strWhere = " WHERE " + strWhere + " ";
            }

            // 升序操作
            if (OrderType == 0)
            {
                if (PageIndex == 1)
                {
                    strSQL += "SELECT TOP " + PageSize + " " + rtnFields + " FROM " + tblName + " ";

                    strSQL += strWhere + " ORDER BY " + fldName + " ASC";
                }
                else
                {
                    strSQL += "SELECT TOP " + PageSize + " " + rtnFields + " FROM " + tblName + " ";

                    strSQL += "WHERE (" + fldName + " > ( SELECT MAX(" + fldName + ") FROM (SELECT TOP " + ((PageIndex - 1) * PageSize) + " " + fldName + " FROM " + tblName + strWhere + " ORDER BY " + fldName + " ASC ) AS T )) ";

                    strSQL += strOldWhere + "ORDER BY " + fldName + " ASC";
                }
            }
            // 降序操作
            else if (OrderType == 1)
            {
                if (PageIndex == 1)
                {
                    strSQL += "SELECT TOP " + PageSize + " " + rtnFields + " FROM " + tblName + " ";

                    strSQL += strWhere + " ORDER BY " + fldName + " DESC";
                }
                else
                {
                    strSQL += "SELECT TOP " + PageSize + " " + rtnFields + " FROM " + tblName + " ";

                    strSQL += "WHERE (" + fldName + " < ( SELECT MIN(" + fldName + ") FROM (SELECT TOP " + ((PageIndex - 1) * PageSize) + " " + fldName + " FROM " + tblName + strWhere + " ORDER BY " + fldName + " DESC ) AS T )) ";

                    strSQL += strOldWhere + "ORDER BY " + fldName + " DESC";
                }
            }
            else // 异常处理
            {
                throw new DataException("未指定任何排序类型。0升序，1为降序");
            }

            return strSQL;
        }

        #endregion


        #region ConstructSplitSQL_sort(指定排序的表达式)

        /// <summary>
        /// 获取分页操作SQL语句(对于排序的字段必须建立索引)
        /// </summary>
        /// <param name="tblName">操作表名</param>
        /// <param name="fldName">操作索引字段名称</param>
        /// <param name="PageIndex">当前页</param>
        /// <param name="PageSize">每页显示记录数</param>
        /// <param name="rtnFields">返回字段集合，中间用逗号格开。返回全部用“*”</param>
        /// <param name="OrderType">排序方式(0升序，1为降序)</param>
        /// <param name="sort">排序表达式</param>
        /// <param name="strWhere">检索的条件语句，不需要再加WHERE关键字</param>
        /// <returns></returns>
        public static string ConstructSplitSQL_sort(string tblName,
            string fldName,
            int PageIndex,
            int PageSize,
            string rtnFields,
            int OrderType,
            string sort,
            string strWhere)
        {
            string strSQL = "";
            string strOldWhere = "";

            // 构造检索条件语句字符串
            if (strWhere != "")
            {
                // 去除不合法的字符，防止SQL注入式攻击
                strWhere = strWhere.Replace("'", "''");
                strWhere = strWhere.Replace("--", "");
                strWhere = strWhere.Replace(";", "");

                strOldWhere = " AND " + strWhere + " ";

                strWhere = " WHERE " + strWhere + " ";
            }

            if (sort != "") sort = " ORDER BY " + sort;

            // 升序操作
            if (OrderType == 0)
            {
                if (PageIndex == 1)
                {
                    strSQL += "SELECT TOP " + PageSize + " " + rtnFields + " FROM " + tblName + " ";

                    //strSQL += "WHERE (" + fldName + " >= ( SELECT MAX(" + fldName + ") FROM (SELECT TOP 1 " + fldName + " FROM " + tblName + strWhere + " ORDER BY " + fldName + " ASC ) AS T )) ";

                    //strSQL += strOldWhere + "ORDER BY " + fldName + " ASC";
                    strSQL += strWhere + sort;
                }
                else
                {
                    strSQL += "SELECT TOP " + PageSize + " " + rtnFields + " FROM " + tblName + " ";

                    strSQL += "WHERE (" + fldName + " > ( SELECT MAX(" + fldName + ") FROM (SELECT TOP " + ((PageIndex - 1) * PageSize) + " " + fldName + " FROM " + tblName + strWhere + sort + " ) AS T )) ";

                    strSQL += strOldWhere + sort;
                }
            }
            // 降序操作
            else if (OrderType == 1)
            {
                if (PageIndex == 1)
                {
                    strSQL += "SELECT TOP " + PageSize + " " + rtnFields + " FROM " + tblName + " ";

                    //strSQL += "WHERE (" + fldName + " <= ( SELECT MIN(" + fldName + ") FROM (SELECT TOP 1 " + fldName + " FROM " + tblName + strWhere + " ORDER BY " + fldName + " DESC ) AS T )) ";

                    //strSQL += strOldWhere + "ORDER BY " + fldName + " DESC";
                    strSQL += strWhere + sort;
                }
                else
                {
                    strSQL += "SELECT TOP " + PageSize + " " + rtnFields + " FROM " + tblName + " ";

                    strSQL += "WHERE (" + fldName + " < ( SELECT MIN(" + fldName + ") FROM (SELECT TOP " + ((PageIndex - 1) * PageSize) + " " + fldName + " FROM " + tblName + strWhere + sort + " ) AS T )) ";

                    strSQL += strOldWhere + sort;
                }
            }
            else // 异常处理
            {
                throw new DataException("未指定主索引排序类型。0升序，1为降序");
            }

            return strSQL;
        }

        #endregion

        #endregion
    }
}
