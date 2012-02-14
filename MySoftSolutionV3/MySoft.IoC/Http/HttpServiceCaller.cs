using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using MySoft.RESTful;
using MySoft.Net.HTTP;
using System.Collections.Specialized;

namespace MySoft.IoC.Http
{
    /// <summary>
    /// Http服务调用
    /// </summary>
    public class HttpServiceCaller
    {
        private IServiceContainer container;
        private IDictionary<string, CallerInfo> callers;
        private int port;

        /// <summary>
        /// HttpServiceCaller初始化
        /// </summary>
        /// <param name="container"></param>
        public HttpServiceCaller(IServiceContainer container, int port)
        {
            this.container = container;
            this.port = port;
            this.callers = new Dictionary<string, CallerInfo>();

            //初始化字典
            foreach (var serviceType in container.GetInterfaces<ServiceContractAttribute>())
            {
                var serviceAttr = CoreHelper.GetTypeAttribute<ServiceContractAttribute>(serviceType);
                var serviceName = serviceAttr.Name ?? serviceType.FullName;
                var description = serviceAttr.Description;

                //添加方法
                foreach (var methodInfo in CoreHelper.GetMethodsFromType(serviceType))
                {
                    var methodAttr = CoreHelper.GetMemberAttribute<OperationContractAttribute>(methodInfo);
                    if (methodAttr != null && methodAttr.HttpEnabled)
                    {
                        string methodName = methodAttr.Name ?? methodInfo.Name;
                        string fullName = string.Format("{0}.{1}", serviceName, methodName);
                        if (!string.IsNullOrEmpty(methodAttr.Description))
                        {
                            if (string.IsNullOrEmpty(description))
                                description = methodAttr.Description;
                            else
                                description += " - " + methodAttr.Description;
                        }

                        //将方法添加到字典
                        var callerInfo = new CallerInfo
                        {
                            Method = methodInfo,
                            Instance = container[serviceType],
                            Description = description
                        };
                        callers[fullName] = callerInfo;
                    }
                }
            }
        }

        /// <summary>
        /// 获取Http方法
        /// </summary>
        /// <returns></returns>
        public string GetDocument()
        {
            var doc = new APIDocument(callers, port);
            return doc.MakeDocument();
        }

        /// <summary>
        /// 调用服务，并返回字符串
        /// </summary>
        /// <param name="name"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public string CallMethod(string name, NameValueCollection collection)
        {
            if (callers.ContainsKey(name))
            {
                var caller = callers[name];
                var parameters = ParseParameters(collection, caller.Method);

                try
                {
                    var retVal = DynamicCalls.GetMethodInvoker(caller.Method).Invoke(caller.Instance, parameters);

                    if (retVal != null)
                        return SerializationManager.SerializeJson(retVal);
                    else
                        return null;
                }
                catch (Exception ex)
                {
                    throw new HTTPMessageException(ex.Message);
                }
            }
            else
            {
                throw new HTTPMessageException(string.Format("Not found method {0}!", name));
            }
        }

        /// <summary>
        /// 处理参数
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private object[] ParseParameters(NameValueCollection collection, MethodInfo method)
        {
            var jobject = ParameterHelper.Resolve(collection);

            //处理参数
            if (jobject.Count == 0)
                return null;
            else
                return ParameterHelper.Convert(jobject, method.GetParameters());
        }
    }

    /// <summary>
    /// 调用信息
    /// </summary>
    internal class CallerInfo
    {
        /// <summary>
        /// 调用方法
        /// </summary>
        public MethodInfo Method { get; set; }

        /// <summary>
        /// 调用实例
        /// </summary>
        public object Instance { get; set; }

        /// <summary>
        /// 方法描述
        /// </summary>
        public string Description { get; set; }
    }
}
