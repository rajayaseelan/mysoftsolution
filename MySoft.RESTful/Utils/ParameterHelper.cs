using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MySoft.RESTful.Utils
{
    /// <summary>
    /// 参数处理
    /// </summary>
    public class ParameterHelper
    {
        /// <summary>
        /// 参数解析
        /// </summary>
        /// <param name="paramters"></param>
        /// <param name="nvs"></param>
        /// <returns></returns>
        public static object[] Convert(ParameterInfo[] paramters, NameValueCollection nvget, NameValueCollection nvpost)
        {
            List<object> args = new List<object>();
            var obj = ConvertJObject(nvget, nvpost);

            foreach (ParameterInfo info in paramters)
            {
                var type = GetElementType(info.ParameterType);

                var property = obj.Properties().SingleOrDefault(p => string.Compare(p.Name, info.Name, true) == 0);
                if (property != null)
                {
                    try
                    {
                        //获取Json值
                        var jsonValue = CoreHelper.ConvertJsonValue(type, property.Value.ToString(Formatting.None));
                        args.Add(jsonValue);
                    }
                    catch (Exception ex)
                    {
                        throw new RESTfulException((int)HttpStatusCode.BadRequest, string.Format("Parameter [{0}] did not match type [{1}].",
                            info.Name, CoreHelper.GetTypeName(type)));
                    }
                }
                else
                {
                    throw new RESTfulException((int)HttpStatusCode.BadRequest, "Parameter [" + info.Name + "] is not found.");
                }
            }

            return args.ToArray();
        }

        private static Type GetElementType(Type type)
        {
            if (type.IsByRef) type = type.GetElementType();
            return type;
        }

        /// <summary>
        /// 转换成JObject
        /// </summary>
        /// <param name="get"></param>
        /// <param name="post"></param>
        /// <returns></returns>
        private static JObject ConvertJObject(NameValueCollection get, NameValueCollection post)
        {
            var obj = new JObject();
            if (get.Count > 0)
            {
                foreach (var key in get.AllKeys)
                {
                    obj[key] = get[key];
                }
            }

            if (post.Count > 0)
            {
                foreach (var key in get.AllKeys)
                {
                    try
                    {
                        obj[key] = JContainer.Parse(get[key]);
                    }
                    catch
                    {
                        obj[key] = get[key];
                    }
                }
            }

            //转换成Json对象
            return obj;
        }
    }
}
