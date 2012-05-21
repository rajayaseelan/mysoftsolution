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
        public static object[] Convert(ParameterInfo[] paramters, NameValueCollection nvs)
        {
            List<object> args = new List<object>();
            var obj = ConvertJsonObject(nvs);

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
                            info.Name, GetTypeName(type)));
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

        private static string GetTypeName(Type type)
        {
            string typeName = type.Name;
            if (type.IsGenericType) type = type.GetGenericArguments()[0];
            if (typeName.Contains("`1"))
            {
                typeName = typeName.Replace("`1", "&lt;" + type.Name + "&gt;");
            }
            return typeName;
        }

        /// <summary>
        /// 转换成JObject
        /// </summary>
        /// <param name="nvs"></param>
        /// <returns></returns>
        private static JObject ConvertJsonObject(NameValueCollection nvs)
        {
            var obj = new JObject();
            if (nvs.Count > 0)
            {
                foreach (var key in nvs.AllKeys)
                {
                    try
                    {
                        obj[key] = JContainer.Parse(nvs[key]);
                    }
                    catch
                    {
                        obj[key] = nvs[key];
                    }
                }
            }

            //转换成Json对象
            return obj;
        }
    }
}
