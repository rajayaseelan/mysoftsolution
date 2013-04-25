#region usings

using System;
using System.Collections.Generic;
using System.Reflection;

#endregion

namespace MySoft.Web.UI
{
    internal class AsyncMethodInfo
    {
        /// <summary>
        /// 是否异步
        /// </summary>
        public bool Async { get; set; }

        /// <summary>
        /// 调用方法
        /// </summary>
        public MethodInfo Method { get; set; }
    }

    internal class AjaxMethodHelper
    {
        private static IDictionary<Type, IDictionary<string, AsyncMethodInfo>> ajaxMethods;

        static AjaxMethodHelper()
        {
            ajaxMethods = new Dictionary<Type, IDictionary<string, AsyncMethodInfo>>();
        }

        /// <summary>
        /// 获取方法列表
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IDictionary<string, AsyncMethodInfo> GetAjaxMethods(Type t)
        {
            if (!ajaxMethods.ContainsKey(t))
            {
                lock (ajaxMethods)
                {
                    ajaxMethods[t] = InternalGetAjaxMethods(t);
                }
            }

            return ajaxMethods[t];
        }

        private static IDictionary<string, AsyncMethodInfo> InternalGetAjaxMethods(Type type)
        {
            var ret = new Dictionary<string, AsyncMethodInfo>();
            var mis = CoreHelper.GetMethodsFromType(type);
            foreach (MethodInfo mi in mis)
            {
                string methodName = mi.Name;
                var method = CoreHelper.GetMemberAttribute<AjaxMethodAttribute>(mi);
                if (method != null)
                {
                    if (!string.IsNullOrEmpty(method.Name))
                    {
                        methodName = method.Name;
                    }

                    if (!ret.ContainsKey(methodName))
                    {
                        AsyncMethodInfo asyncMethod = new AsyncMethodInfo();
                        asyncMethod.Method = mi;
                        asyncMethod.Async = method.Async;
                        ret[methodName] = asyncMethod;
                    }
                }
            }

            return ret;
        }
    }
}

