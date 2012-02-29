using System;
using System.Collections.Generic;
using System.Linq;
using MySoft.IoC.Messages;
using System.Collections;

namespace MySoft.IoC.HttpServer
{
    /// <summary>
    /// Http服务调用
    /// </summary>
    public class HttpServiceCaller
    {
        private IServiceContainer container;
        private Dictionary<string, HttpCallerInfo> callers;
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
                //添加方法
                foreach (var methodInfo in CoreHelper.GetMethodsFromType(serviceType))
                {
                    var httpInvoke = CoreHelper.GetMemberAttribute<HttpInvokeAttribute>(methodInfo);
                    if (httpInvoke != null)
                    {
                        //创建一个新的Caller
                        CreateNewCaller(serviceType, methodInfo, httpInvoke);
                    }
                }
            }
        }

        private void CreateNewCaller(Type serviceType, System.Reflection.MethodInfo methodInfo, HttpInvokeAttribute invoke)
        {
            //将方法添加到字典
            var callerInfo = new HttpCallerInfo
            {
                ServiceName = serviceType.FullName,
                Method = methodInfo,
                TypeString = methodInfo.ReturnType == typeof(string),
                Description = invoke.Description,
                Authorized = invoke.Authorized,
                AuthParameter = invoke.AuthParameter,
                HttpMethod = HttpMethod.GET //默认为GET方式
            };

            var types = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
            if (!CoreHelper.CheckPrimitiveType(types))
            {
                callerInfo.HttpMethod = HttpMethod.POST;
            }

            string fullName = invoke.Name;
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
        public string GetDocument(string name)
        {
            var dicCaller = new Dictionary<string, HttpCallerInfo>();
            if (!string.IsNullOrEmpty(name))
            {
                if (callers.ContainsKey(name))
                    dicCaller[name] = callers[name];
            }
            else
                dicCaller = callers;

            var doc = new HttpDocument(dicCaller, port);
            return doc.MakeDocument(name);
        }

        /// <summary>
        /// 获取API文档
        /// </summary>
        /// <returns></returns>
        public string GetAPIText()
        {
            var array = new ArrayList();
            foreach (var kvp in callers)
            {
                array.Add(new
                {
                    Name = kvp.Key,
                    Authorized = kvp.Value.Authorized,
                    AuthParameter = kvp.Value.AuthParameter,
                    TypeString = kvp.Value.TypeString
                });
            }

            //系列化json输出
            return SerializationManager.SerializeJson(array);
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
        /// <param name="parameters"></param>
        /// <returns></returns>
        public string CallMethod(string name, string parameters)
        {
            if (callers.ContainsKey(name))
            {
                var caller = callers[name];
                var service = container.Resolve<IService>("Service_" + caller.ServiceName);
                var invoke = new InvokeCaller("HttpCaller", service);
                var message = new InvokeMessage
                {
                    ServiceName = caller.ServiceName,
                    MethodName = caller.Method.ToString(),
                    Parameters = parameters
                };

                //处理数据返回InvokeData
                var invokeData = invoke.CallMethod(message) as InvokeData;
                if (invokeData != null)
                    return invokeData.Value;
            }

            return "{}";
        }
    }
}
