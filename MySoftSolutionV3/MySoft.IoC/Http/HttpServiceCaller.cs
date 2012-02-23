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
        private IDictionary<string, HttpCallerInfo> callers;
        private int port;

        /// <summary>
        /// HttpServiceCaller初始化
        /// </summary>
        /// <param name="container"></param>
        /// <param name="httpAuth"></param>
        /// <param name="port"></param>
        public HttpServiceCaller(IServiceContainer container, int port)
        {
            this.container = container;
            this.port = port;
            this.callers = new Dictionary<string, HttpCallerInfo>();

            //初始化字典
            foreach (var serviceType in container.GetInterfaces<ServiceContractAttribute>())
            {
                var serviceAttr = CoreHelper.GetTypeAttribute<ServiceContractAttribute>(serviceType);
                var serviceName = serviceAttr.Name ?? serviceType.FullName;

                object instance = null;
                try { instance = container.Resolve(serviceType); }
                catch { }
                if (instance == null) continue;

                //添加方法
                foreach (var methodInfo in CoreHelper.GetMethodsFromType(serviceType))
                {
                    bool authorized = true;
                    string authParameter = null;
                    var description = serviceAttr.Description;
                    var methodAttr = CoreHelper.GetMemberAttribute<OperationContractAttribute>(methodInfo);

                    if (methodAttr != null && methodAttr.HttpGet)
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

                        authorized = methodAttr.Authorized;
                        authParameter = methodAttr.AuthParameter;

                        //创建一个新的Caller
                        CreateNewCaller(serviceType, methodInfo, instance, fullName, description, authorized, authParameter);
                    }
                }
            }
        }

        private void CreateNewCaller(Type serviceType, MethodInfo methodInfo, object instance,
            string fullName, string description, bool authorized, string authParameter)
        {
            //将方法添加到字典
            var callerInfo = new HttpCallerInfo
            {
                ServiceName = string.Format("【{0}】\r\n{1}", serviceType.FullName, methodInfo.ToString()),
                Method = methodInfo,
                Instance = instance,
                Authorized = authorized,
                AuthParameter = authParameter,
                Description = description,
                HttpMethod = HttpMethod.GET //默认为GET方式
            };

            var types = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
            if (!CoreHelper.CheckPrimitiveType(types))
            {
                callerInfo.HttpMethod = HttpMethod.POST;
            }

            if (callers.ContainsKey(fullName))
            {
                //处理重复的方法
                for (int i = 0; i < 10000; i++)
                {
                    var name = fullName + (i + 1);
                    if (!callers.ContainsKey(name))
                    {
                        fullName = name;
                        break;
                    }
                }
            }

            callers[fullName] = callerInfo;
        }

        /// <summary>
        /// 获取Http方法
        /// </summary>
        /// <returns></returns>
        public string GetDocument()
        {
            var doc = new HttpDocument(callers, port);
            return doc.MakeDocument();
        }

        /// <summary>
        /// 获取调用信息
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public HttpCallerInfo GetCaller(string name)
        {
            if (callers.ContainsKey(name))
            {
                return callers[name];
            }

            return null;
        }

        /// <summary>
        /// 调用服务，并返回字符串
        /// </summary>
        /// <param name="name"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public string CallMethod(string name, IDictionary<string, string> collection)
        {
            var caller = callers[name];
            var parameters = new object[0];
            if (caller.Method.GetParameters().Length > 0)
            {
                var jobject = ParameterHelper.Resolve(collection);
                parameters = ParameterHelper.Convert(jobject, caller.Method.GetParameters());
            }

            var retVal = DynamicCalls.GetMethodInvoker(caller.Method).Invoke(caller.Instance, parameters);

            //如果返回类型为字符串，直接返回值
            if (caller.Method.ReturnType == typeof(string))
            {
                return Convert.ToString(retVal);
            }
            else
            {
                if (retVal == null) return "{}";
                return SerializationManager.SerializeJson(retVal, new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter());
            }
        }
    }
}
