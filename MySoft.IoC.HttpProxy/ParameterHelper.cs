using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using MySoft.Logger;
using Newtonsoft.Json.Linq;

namespace MySoft.IoC.HttpProxy
{
    /// <summary>
    /// 参数服务
    /// </summary>
    public static class ParameterHelper
    {
        /// <summary>
        /// 转换成JObject
        /// </summary>
        /// <param name="nvget"></param>
        /// <param name="nvpost"></param>
        /// <returns></returns>
        public static JObject ConvertJObject(NameValueCollection nvget, NameValueCollection nvpost)
        {
            var obj = new JObject();
            if (nvget != null && nvget.Count > 0)
            {
                foreach (var key in nvget.AllKeys)
                {
                    obj[key] = UrlDecodeString(nvget[key]);
                }
            }

            if (nvpost != null && nvpost.Count > 0)
            {
                foreach (var key in nvpost.AllKeys)
                {
                    try
                    {
                        obj[key] = JContainer.Parse(UrlDecodeString(nvpost[key]));
                    }
                    catch
                    {
                        obj[key] = UrlDecodeString(nvpost[key]);
                    }
                }
            }

            //转换成Json对象
            return obj;
        }

        /// <summary>
        /// 转换成NameValueCollection
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static NameValueCollection ConvertCollection(string data)
        {
            if (string.IsNullOrEmpty(data)) return null;

            //处理成Form方式
            var values = HttpUtility.ParseQueryString(data, Encoding.UTF8);

            //为0表示为json方式
            if (values.Count == 0 || (values.Count == 1 && values.AllKeys[0] == null))
            {
                try
                {
                    //清除所的值
                    values.Clear();

                    //保持与Json兼容处理
                    var jobj = JObject.Parse(UrlDecodeString(data));
                    foreach (var kvp in jobj)
                    {
                        values[kvp.Key] = kvp.Value.ToString();
                    }
                }
                catch (Exception ex)
                {
                    //TODO 不做处理
                    SimpleLog.Instance.WriteLogForDir("ConvertData", ex);
                }
            }

            return values;
        }

        /// <summary>
        /// 反编码字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string UrlDecodeString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return HttpUtility.UrlDecode(value, Encoding.UTF8);
        }
    }
}
