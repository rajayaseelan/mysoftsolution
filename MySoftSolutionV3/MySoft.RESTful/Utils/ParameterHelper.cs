using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Net;

namespace MySoft.RESTful.Utils
{
    /// <summary>
    /// 参数处理
    /// </summary>
    public class ParameterHelper
    {
        /// <summary>
        /// 解析参数
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JObject Resolve(NameValueCollection parameters)
        {
            JObject obj = new JObject();
            foreach (var key in parameters.AllKeys)
            {
                obj.Add(key, parameters[key]);
            }
            return obj;
        }

        /// <summary>
        /// 参数解析
        /// </summary>
        /// <param name="paramters"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object[] Convert(ParameterInfo[] paramters, JObject obj)
        {
            try
            {
                List<object> args = new List<object>();
                foreach (ParameterInfo info in paramters)
                {
                    var property = obj.Properties().SingleOrDefault(p => string.Compare(p.Name, info.Name, true) == 0);
                    if (property != null)
                    {
                        //获取Json值
                        string value = property.Value.ToString(Newtonsoft.Json.Formatting.None);
                        object jsonValue = CoreHelper.ConvertValue(info.ParameterType, value);
                        args.Add(jsonValue);
                    }
                    else
                    {
                        //没有的参数使用默认值
                        var jsonValue = CoreHelper.GetTypeDefaultValue(GetElementType(info.ParameterType));
                        args.Add(jsonValue);
                    }
                }

                return args.ToArray();
            }
            catch (Exception ex)
            {
                throw new RESTfulException((int)HttpStatusCode.BadRequest, "Parameter type did not match. " + ex.Message);
            }
        }

        private static Type GetElementType(Type type)
        {
            if (type.IsByRef) type = type.GetElementType();
            return type;
        }
    }
}
