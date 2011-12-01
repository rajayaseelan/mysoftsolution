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
                AjaxMethodAttribute[] amas = (AjaxMethodAttribute[])mi.GetCustomAttributes(typeof(AjaxMethodAttribute), false);
                if (amas.Length == 1)
                {
                    if (amas[0].Name != null)
                    {
                        methodName = amas[0].Name;
                    }

                    if (!ret.ContainsKey(methodName))
                    {
                        AsyncMethodInfo asyncMethod = new AsyncMethodInfo();
                        asyncMethod.MethodInfo = mi;
                        asyncMethod.Async = amas[0].Async;
                        ret.Add(methodName, asyncMethod);
                    }
                }
            }
            return ret;
        }
    }
}

