using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MySoft.Common
{
    #region 字符串的处理函数集

    /// <summary>
    /// 字符串的处理函数集
    /// </summary>
    public abstract class StringHelper
    {
        /// <summary>
        /// 移除多余的空格，保留一个空格
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string RemoveSurplusSpaces(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            RegexOptions opt = RegexOptions.None;
            Regex regex = new Regex(@"[ ]{2,}", opt);
            string str = regex.Replace(value, " ").Trim();

            return str;
        }

        public static string GetSubString(string p_SrcString, int p_Length, string p_TailString)
        {
            string text = p_SrcString;
            if (p_Length < 0)
            {
                return text;
            }
            byte[] sourceArray = Encoding.Default.GetBytes(p_SrcString);
            if (sourceArray.Length <= p_Length)
            {
                return text;
            }
            int length = p_Length;
            int[] numArray = new int[p_Length];
            byte[] destinationArray = null;
            int num2 = 0;
            for (int i = 0; i < p_Length; i++)
            {
                if (sourceArray[i] > 0x7f)
                {
                    num2++;
                    if (num2 == 3)
                    {
                        num2 = 1;
                    }
                }
                else
                {
                    num2 = 0;
                }
                numArray[i] = num2;
            }
            if ((sourceArray[p_Length - 1] > 0x7f) && (numArray[p_Length - 1] == 1))
            {
                length = p_Length + 1;
            }
            destinationArray = new byte[length];
            Array.Copy(sourceArray, destinationArray, length);
            return (Encoding.Default.GetString(destinationArray) + p_TailString);
        }


        /// <summary>
        /// 将 Stream 转化成 string
        /// </summary>
        /// <param name="s">Stream流</param>
        /// <returns>string</returns>
        public static string ConvertStreamToString(Stream s)
        {
            string strResult = "";
            StreamReader sr = new StreamReader(s, Encoding.UTF8);

            Char[] read = new Char[256];

            // Read 256 charcters at a time.    
            int count = sr.Read(read, 0, 256);

            while (count > 0)
            {
                // Dump the 256 characters on a string and display the string onto the console.
                string str = new String(read, 0, count);
                strResult += str;
                count = sr.Read(read, 0, 256);
            }


            // 释放资源
            sr.Close();

            return strResult;
        }

        /// <summary>
        /// 对传递的参数字符串进行处理，防止注入式攻击
        /// </summary>
        /// <param name="str">传递的参数字符串</param>
        /// <returns>String</returns>
        public static string ConvertSql(string str)
        {
            str = str.Trim();
            str = str.Replace("'", "''");
            str = str.Replace(";--", "");
            str = str.Replace("=", "");
            str = str.Replace(" or ", "");
            str = str.Replace(" and ", "");

            return str;
        }


        /// <summary>
        /// 格式化占用空间大小的输出
        /// </summary>
        /// <param name="size">大小</param>
        /// <returns>返回 String</returns>
        public static string FormatNUM(long size)
        {
            decimal NUM;
            string strResult;

            if (size > 1073741824)
            {
                NUM = (Convert.ToDecimal(size) / Convert.ToDecimal(1073741824));
                strResult = NUM.ToString("N") + " G";
            }
            else if (size > 1048576)
            {
                NUM = (Convert.ToDecimal(size) / Convert.ToDecimal(1048576));
                strResult = NUM.ToString("N") + " M";
            }
            else if (size > 1024)
            {
                NUM = (Convert.ToDecimal(size) / Convert.ToDecimal(1024));
                strResult = NUM.ToString("N") + " KB";
            }
            else
            {
                strResult = size + " 字节";
            }

            return strResult;
        }

        /// <summary>
        /// 判断字符串是否为有效的邮件地址
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^.+\@(\[?)[a-zA-Z0-9\-\.]+\.([a-zA-Z]{2,3}|[0-9]{1,3})(\]?)$");
        }

        /// <summary>
        /// 判断字符串是否为有效的URL地址
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsValidURL(string url)
        {
            return Regex.IsMatch(url, @"^(http|https|ftp)\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&%\$#\=~])*[^\.\,\)\(\s]$");
        }

        /// <summary>
        /// 判断字符串是否为Int类型的
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool IsValidInt(string val)
        {
            return Regex.IsMatch(val, @"^[1-9]\d*\.?[0]*$");
        }

        /// <summary>
        /// 检测字符串是否全为正整数
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNum(string str)
        {
            bool blResult = true;//默认状态下是数字

            if (str == "")
                blResult = false;
            else
            {
                foreach (char Char in str)
                {
                    if (!char.IsNumber(Char))
                    {
                        blResult = false;
                        break;
                    }
                }
                if (blResult)
                {
                    if (int.Parse(str) == 0)
                        blResult = false;
                }
            }
            return blResult;
        }


        //得到根url
        public static string GetUrlRoot(System.Web.HttpRequest Request)
        {
            string curpath = Request.Url.AbsoluteUri;
            int ipos = curpath.LastIndexOf("/");
            return curpath.Substring(0, ipos + 1);

        }

        /// <summary>
        /// 检测字符串是否全为数字型
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDouble(string str)
        {
            bool blResult = true;//默认状态下是数字

            if (str == "")
                blResult = false;
            else
            {
                foreach (char Char in str)
                {
                    if (!char.IsNumber(Char) && Char.ToString() != "-")
                    {
                        blResult = false;
                        break;
                    }
                }
            }
            return blResult;
        }

        /// <summary>
        /// 输出由同一字符组成的指定长度的字符串
        /// </summary>
        /// <param name="Char">输出字符，如：A</param>
        /// <param name="i">指定长度</param>
        /// <returns></returns>
        public static string Strings(char Char, int i)
        {
            string strResult = null;

            for (int j = 0; j < i; j++)
            {
                strResult += Char;
            }
            return strResult;
        }


        /// <summary>
        /// 返回字符串的真实长度，一个汉字字符相当于两个单位长度
        /// </summary>
        /// <param name="str">指定字符串</param>
        /// <returns></returns>
        public static int Len(string str)
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
        /// 以日期为标准获得一个绝对的名称
        /// </summary>
        /// <returns>返回 String</returns>
        public static string MakeName()
        {
            /*
            string y = DateTime.Now.Year.ToString();
            string m = DateTime.Now.Month.ToString();
            string d = DateTime.Now.Day.ToString();
            string h = DateTime.Now.Hour.ToString();
            string n = DateTime.Now.Minute.ToString();
            string s = DateTime.Now.Second.ToString();
            return y + m + d + h + n + s;
            */

            return DateTime.Now.ToString("yyMMddHHmmss");
        }


        /// <summary>
        /// 返回字符串的真实长度，一个汉字字符相当于两个单位长度(使用Encoding类)
        /// </summary>
        /// <param name="str">指定字符串</param>
        /// <returns></returns>
        public static int GetLen(string str)
        {
            int intResult = 0;
            Encoding gb2312 = Encoding.GetEncoding("gb2312");
            byte[] bytes = gb2312.GetBytes(str);
            intResult = bytes.Length;
            return intResult;
        }


        /// <summary>
        /// 按照字符串的实际长度截取指定长度的字符串
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="Length">指定长度</param>
        /// <param name="showomit">显示省略号</param>
        /// <returns></returns>
        public static string CutLen(string text, int length, string cutText)
        {
            if (text == null) return string.Empty;
            int i = 0, j = 0;
            foreach (char Char in text)
            {
                if ((int)Char > 127)
                    i += 2;
                else
                    i++;

                if (i > length)
                {
                    text = text.Substring(0, j);
                    text += cutText;
                    break;
                }
                j++;
            }
            return text;
        }


        /// <summary>
        /// 获取指定长度的纯数字随机数字串
        /// </summary>
        /// <param name="intLong">数字串长度</param>
        /// <returns>字符串</returns>
        public static string RandomNUM(int intLong)
        {
            string strResult = "";

            Random r = new Random();
            for (int i = 0; i < intLong; i++)
            {
                strResult = strResult + r.Next(10);
            }

            return strResult;
        }

        /// <summary>
        /// 获取一个由26个小写字母组成的指定长度的随即字符串
        /// </summary>
        /// <param name="intLong">指定长度</param>
        /// <returns></returns>
        public static string RandomSTR(int intLong)
        {
            string strResult = "";
            string[] array = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

            Random r = new Random();

            for (int i = 0; i < intLong; i++)
            {
                strResult += array[r.Next(26)];
            }

            return strResult;
        }

        /// <summary>
        /// 获取一个由数字和26个小写字母组成的指定长度的随即字符串
        /// </summary>
        /// <param name="intLong">指定长度</param>
        /// <returns></returns>
        public static string RandomNUMSTR(int intLong)
        {
            string strResult = "";
            string[] array = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

            Random r = new Random();

            for (int i = 0; i < intLong; i++)
            {
                strResult += array[r.Next(36)];
            }

            return strResult;
        }

        /// <summary>
        /// 将指定字符串中的汉字转换为拼音首字母的缩写，其中非汉字保留为原字符
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ConvertSpellFirst(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            char pinyin;
            byte[] array;
            StringBuilder sb = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                pinyin = c;
                array = Encoding.Default.GetBytes(new char[] { c });

                if (array.Length == 2)
                {
                    int i = array[0] * 0x100 + array[1];

                    if (i < 0xB0A1) pinyin = c;
                    else
                        if (i < 0xB0C5) pinyin = 'a';
                        else
                            if (i < 0xB2C1) pinyin = 'b';
                            else
                                if (i < 0xB4EE) pinyin = 'c';
                                else
                                    if (i < 0xB6EA) pinyin = 'd';
                                    else
                                        if (i < 0xB7A2) pinyin = 'e';
                                        else
                                            if (i < 0xB8C1) pinyin = 'f';
                                            else
                                                if (i < 0xB9FE) pinyin = 'g';
                                                else
                                                    if (i < 0xBBF7) pinyin = 'h';
                                                    else
                                                        if (i < 0xBFA6) pinyin = 'g';
                                                        else
                                                            if (i < 0xC0AC) pinyin = 'k';
                                                            else
                                                                if (i < 0xC2E8) pinyin = 'l';
                                                                else
                                                                    if (i < 0xC4C3) pinyin = 'm';
                                                                    else
                                                                        if (i < 0xC5B6) pinyin = 'n';
                                                                        else
                                                                            if (i < 0xC5BE) pinyin = 'o';
                                                                            else
                                                                                if (i < 0xC6DA) pinyin = 'p';
                                                                                else
                                                                                    if (i < 0xC8BB) pinyin = 'q';
                                                                                    else
                                                                                        if (i < 0xC8F6) pinyin = 'r';
                                                                                        else
                                                                                            if (i < 0xCBFA) pinyin = 's';
                                                                                            else
                                                                                                if (i < 0xCDDA) pinyin = 't';
                                                                                                else
                                                                                                    if (i < 0xCEF4) pinyin = 'w';
                                                                                                    else
                                                                                                        if (i < 0xD1B9) pinyin = 'x';
                                                                                                        else
                                                                                                            if (i < 0xD4D1) pinyin = 'y';
                                                                                                            else
                                                                                                                if (i < 0xD7FA) pinyin = 'z';
                }

                sb.Append(pinyin);
            }

            return sb.ToString();
        }

        #region 初始化定义

        /**/
        /// <summary> 
        ///将汉字转换成为拼音 
        /// </summary> 
        private static int[] pyvalue = new int[]{-20319,-20317,-20304,-20295,-20292,-20283,-20265,-20257,-20242,-20230,-20051,-                                20036,-20032,-20026, 
                                                     -20002,-19990,-19986,-19982,-19976,-19805,-19784,-19775,-19774,-19763,-19756,-19751,-19746,-19741,-19739,-19728, 
                                                     -19725,-19715,-19540,-19531,-19525,-19515,-19500,-19484,-19479,-19467,-19289,-19288,-19281,-19275,-19270,-19263, 
                                                     -19261,-19249,-19243,-19242,-19238,-19235,-19227,-19224,-19218,-19212,-19038,-19023,-19018,-19006,-19003,-18996, 
                                                     -18977,-18961,-18952,-18783,-18774,-18773,-18763,-18756,-18741,-18735,-18731,-18722,-18710,-18697,-18696,-18526, 
                                                     -18518,-18501,-18490,-18478,-18463,-18448,-18447,-18446,-18239,-18237,-18231,-18220,-18211,-18201,-18184,-18183, 
                                                     -18181,-18012,-17997,-17988,-17970,-17964,-17961,-17950,-17947,-17931,-17928,-17922,-17759,-17752,-17733,-17730, 
                                                     -17721,-17703,-17701,-17697,-17692,-17683,-17676,-17496,-17487,-17482,-17468,-17454,-17433,-17427,-17417,-17202, 
                                                     -17185,-16983,-16970,-16942,-16915,-16733,-16708,-16706,-16689,-16664,-16657,-16647,-16474,-16470,-16465,-16459, 
                                                     -16452,-16448,-16433,-16429,-16427,-16423,-16419,-16412,-16407,-16403,-16401,-16393,-16220,-16216,-16212,-16205, 
                                                     -16202,-16187,-16180,-16171,-16169,-16158,-16155,-15959,-15958,-15944,-15933,-15920,-15915,-15903,-15889,-15878, 
                                                     -15707,-15701,-15681,-15667,-15661,-15659,-15652,-15640,-15631,-15625,-15454,-15448,-15436,-15435,-15419,-15416, 
                                                     -15408,-15394,-15385,-15377,-15375,-15369,-15363,-15362,-15183,-15180,-15165,-15158,-15153,-15150,-15149,-15144, 
                                                     -15143,-15141,-15140,-15139,-15128,-15121,-15119,-15117,-15110,-15109,-14941,-14937,-14933,-14930,-14929,-14928, 
                                                     -14926,-14922,-14921,-14914,-14908,-14902,-14894,-14889,-14882,-14873,-14871,-14857,-14678,-14674,-14670,-14668, 
                                                     -14663,-14654,-14645,-14630,-14594,-14429,-14407,-14399,-14384,-14379,-14368,-14355,-14353,-14345,-14170,-14159, 
                                                     -14151,-14149,-14145,-14140,-14137,-14135,-14125,-14123,-14122,-14112,-14109,-14099,-14097,-14094,-14092,-14090, 
                                                     -14087,-14083,-13917,-13914,-13910,-13907,-13906,-13905,-13896,-13894,-13878,-13870,-13859,-13847,-13831,-13658, 
                                                     -13611,-13601,-13406,-13404,-13400,-13398,-13395,-13391,-13387,-13383,-13367,-13359,-13356,-13343,-13340,-13329, 
                                                     -13326,-13318,-13147,-13138,-13120,-13107,-13096,-13095,-13091,-13076,-13068,-13063,-13060,-12888,-12875,-12871, 
                                                     -12860,-12858,-12852,-12849,-12838,-12831,-12829,-12812,-12802,-12607,-12597,-12594,-12585,-12556,-12359,-12346, 
                                                     -12320,-12300,-12120,-12099,-12089,-12074,-12067,-12058,-12039,-11867,-11861,-11847,-11831,-11798,-11781,-11604, 
                                                     -11589,-11536,-11358,-11340,-11339,-11324,-11303,-11097,-11077,-11067,-11055,-11052,-11045,-11041,-11038,-11024, 
                                                     -11020,-11019,-11018,-11014,-10838,-10832,-10815,-10800,-10790,-10780,-10764,-10587,-10544,-10533,-10519,-10331, 
                                                     -10329,-10328,-10322,-10315,-10309,-10307,-10296,-10281,-10274,-10270,-10262,-10260,-10256,-10254};
        private static string[] pystr = new string[]{"a","ai","an","ang","ao","ba","bai","ban","bang","bao","bei","ben","beng","bi","bian","biao", 
                                                        "bie","bin","bing","bo","bu","ca","cai","can","cang","cao","ce","ceng","cha","chai","chan","chang","chao","che","chen", 
                                                        "cheng","chi","chong","chou","chu","chuai","chuan","chuang","chui","chun","chuo","ci","cong","cou","cu","cuan","cui", 
                                                        "cun","cuo","da","dai","dan","dang","dao","de","deng","di","dian","diao","die","ding","diu","dong","dou","du","duan", 
                                                        "dui","dun","duo","e","en","er","fa","fan","fang","fei","fen","feng","fo","fou","fu","ga","gai","gan","gang","gao", 
                                                        "ge","gei","gen","geng","gong","gou","gu","gua","guai","guan","guang","gui","gun","guo","ha","hai","han","hang", 
                                                        "hao","he","hei","hen","heng","hong","hou","hu","hua","huai","huan","huang","hui","hun","huo","ji","jia","jian", 
                                                        "jiang","jiao","jie","jin","jing","jiong","jiu","ju","juan","jue","jun","ka","kai","kan","kang","kao","ke","ken", 
                                                        "keng","kong","kou","ku","kua","kuai","kuan","kuang","kui","kun","kuo","la","lai","lan","lang","lao","le","lei", 
                                                        "leng","li","lia","lian","liang","liao","lie","lin","ling","liu","long","lou","lu","lv","luan","lue","lun","luo", 
                                                        "ma","mai","man","mang","mao","me","mei","men","meng","mi","mian","miao","mie","min","ming","miu","mo","mou","mu", 
                                                        "na","nai","nan","nang","nao","ne","nei","nen","neng","ni","nian","niang","niao","nie","nin","ning","niu","nong", 
                                                        "nu","nv","nuan","nue","nuo","o","ou","pa","pai","pan","pang","pao","pei","pen","peng","pi","pian","piao","pie", 
                                                        "pin","ping","po","pu","qi","qia","qian","qiang","qiao","qie","qin","qing","qiong","qiu","qu","quan","que","qun", 
                                                        "ran","rang","rao","re","ren","reng","ri","rong","rou","ru","ruan","rui","run","ruo","sa","sai","san","sang", 
                                                        "sao","se","sen","seng","sha","shai","shan","shang","shao","she","shen","sheng","shi","shou","shu","shua", 
                                                        "shuai","shuan","shuang","shui","shun","shuo","si","song","sou","su","suan","sui","sun","suo","ta","tai", 
                                                        "tan","tang","tao","te","teng","ti","tian","tiao","tie","ting","tong","tou","tu","tuan","tui","tun","tuo", 
                                                        "wa","wai","wan","wang","wei","wen","weng","wo","wu","xi","xia","xian","xiang","xiao","xie","xin","xing", 
                                                        "xiong","xiu","xu","xuan","xue","xun","ya","yan","yang","yao","ye","yi","yin","ying","yo","yong","you", 
                                                        "yu","yuan","yue","yun","za","zai","zan","zang","zao","ze","zei","zen","zeng","zha","zhai","zhan","zhang", 
                                                        "zhao","zhe","zhen","zheng","zhi","zhong","zhou","zhu","zhua","zhuai","zhuan","zhuang","zhui","zhun","zhuo", 
                                                        "zi","zong","zou","zu","zuan","zui","zun","zuo"};

        #endregion

        /// <summary>
        /// 将指定字符串中的汉字转换为拼音字母，其中非汉字保留为原字符
        /// </summary>
        /// <param name="text">要转换的文本内容</param>
        /// <returns>string</returns>
        public static string ConvertSpellFull(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            byte[] array = new byte[2];
            string returnstr = "";
            int chrasc = 0;
            int i1 = 0;
            int i2 = 0;
            char[] nowchar = text.ToCharArray();
            for (int j = 0; j < nowchar.Length; j++)
            {
                array = System.Text.Encoding.Default.GetBytes(nowchar[j].ToString());

                if (array.Length == 1)
                {
                    returnstr += nowchar[j].ToString();
                    continue;
                }

                i1 = (short)(array[0]);
                i2 = (short)(array[1]);

                chrasc = i1 * 256 + i2 - 65536;
                if (chrasc > 0 && chrasc < 160)
                {
                    returnstr += nowchar[j];
                }
                else
                {
                    for (int i = (pyvalue.Length - 1); i >= 0; i--)
                    {
                        if (pyvalue[i] <= chrasc)
                        {
                            returnstr += pystr[i];
                            break;
                        }
                    }
                }
            }

            return returnstr;
        }
    }

    #endregion
}
