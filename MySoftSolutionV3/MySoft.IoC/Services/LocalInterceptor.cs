using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.IoC.Aspect;
using Castle.DynamicProxy;
using System.Collections;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 本地拦截器
    /// </summary>
    [Serializable]
    public class LocalInterceptor : StandardInterceptor
    {
        private IDictionary<string, int> cacheTimes;
        public LocalInterceptor(Type serviceType)
        {
            this.cacheTimes = new Dictionary<string, int>();
            var methods = CoreHelper.GetMethodsFromType(serviceType);
            foreach (var method in methods)
            {
                var contract = CoreHelper.GetMemberAttribute<OperationContractAttribute>(method);
                if (contract != null && contract.ServerCacheTime > 0)
                    cacheTimes[method.ToString()] = contract.ServerCacheTime;
            }
        }

        /// <summary>
        /// 处理前
        /// </summary>
        /// <param name="invocation"></param>
        protected override void PreProceed(IInvocation invocation)
        {
            base.PreProceed(invocation);

            //初始化上下文
            OperationContext.Current = new OperationContext();
        }

        protected override void PerformProceed(IInvocation invocation)
        {
            string cacheKey = null;
            if (cacheTimes.ContainsKey(invocation.Method.ToString()))
            {
                cacheKey = string.Format("LocalCache_{0}_{1}", invocation.Method, SerializationManager.SerializeJson(invocation.Arguments));
            }

            if (cacheKey != null)
            {
                object obj = null;
                if (OperationContext.Current.Cache != null)
                    obj = OperationContext.Current.Cache.GetCache<object>(cacheKey);
                else
                    obj = CacheHelper.Get(cacheKey);

                if (obj != null)
                {
                    var arr = obj as ArrayList;
                    var index = 0;
                    foreach (var val in arr[0] as object[])
                    {
                        invocation.SetArgumentValue(index, val);
                        index++;
                    }
                    invocation.ReturnValue = arr[1];
                    return;
                }
            }

            base.PerformProceed(invocation);

            if (cacheKey != null)
            {
                int cacheTime = cacheTimes[invocation.Method.ToString()];
                object value = new ArrayList { invocation.Arguments, invocation.ReturnValue };
                if (OperationContext.Current.Cache != null)
                    OperationContext.Current.Cache.AddCache(cacheKey, value, cacheTime);
                else
                    CacheHelper.Insert(cacheKey, value, cacheTime);
            }
        }
    }
}
