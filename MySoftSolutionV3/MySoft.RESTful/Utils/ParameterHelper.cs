using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Reflection;

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
            foreach (ParameterInfo info in paramters)
            {
                var type = GetElementType(info.ParameterType);
                if (nvs[info.Name] != null)
                {
                    try
                    {
                        //获取Json值
                        var jsonValue = CoreHelper.ConvertJsonValue(type, nvs[info.Name]);
                        args.Add(jsonValue);
                    }
                    catch (Exception ex)
                    {
                        throw new RESTfulException((int)HttpStatusCode.BadRequest, string.Format("Parameter [{0}] did not match type [{1}].",
                            info.Name, info.ParameterType.FullName));
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
    }
}
