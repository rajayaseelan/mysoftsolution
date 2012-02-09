#region usings

using System;
using System.Collections.Generic;
using System.Reflection;

#endregion

namespace MySoft.Web.UI
{
    internal class AjaxMethodHelper
    {
        private static Dictionary<Type, Dictionary<string, AsyncMethodInfo>> AjaxMethodsMap = new Dictionary<Type, Dictionary<string, AsyncMethodInfo>>();

        /// <summary>
        /// 获取方法列表
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Dictionary<string, AsyncMethodInfo> GetAjaxMethods(Type t)
        {
            if (AjaxMethodsMap.ContainsKey(t))
            {
                return AjaxMethodsMap[t];
            }
            else
            {
                lock (AjaxMethodsMap)
                {
                    AjaxMethodsMap[t] = InternalGetAjaxMethods(t);
                    return AjaxMethodsMap[t];
                }
            }
        }

        private static Dictionary<string, AsyncMethodInfo> InternalGetAjaxMethods(Type type)
        {
            Dictionary<string, AsyncMethodInfo> ret = new Dictionary<string, AsyncMethodInfo>();
            MethodInfo[] mis = CoreHelper.GetMethodsFromType(type);
            foreach (MethodInfo mi in mis)
            {
                string methodName = mi.Name;
                AjaxMethodAttribute method = CoreHelper.GetTypeAttribute<AjaxMethodAttribute>(type);
                if (method != null)
                {
                    if (!string.IsNullOrEmpty(method.Name))
                    {
                        methodName = method.Name;
                    }

                    if (!ret.ContainsKey(methodName))
                    {
                        AsyncMethodInfo asyncMethod = new AsyncMethodInfo();
                        asyncMethod.MethodInfo = mi;
                        asyncMethod.Async = method.Async;
                        ret[methodName] = asyncMethod;
                    }
                }
            }
            return ret;
        }
    }
}

