using System;
using System.Collections;
using System.Linq;
using MySoft.IoC.Messages;
using MySoft.IoC.Services;

namespace MySoft.IoC.HttpServer
{
    /// <summary>
    /// Http服务调用
    /// </summary>
    public class HttpServiceCaller
    {
        private IServiceContainer container;
        private HttpCallerInfoCollection callers;
        private int port;

        /// <summary>
        /// HttpServiceCaller初始化
        /// </summary>
        /// <param name="container"></param>
        /// <param name="cache"></param>
        /// <param name="port"></param>
        public HttpServiceCaller(IServiceContainer container, int port)
        {
            this.container = container;
            this.port = port;
            this.callers = new HttpCallerInfoCollection();

            //获取拥有ServiceContract约束的服务
            var types = container.GetServiceTypes<ServiceContractAttribute>();

            //初始化字典
            foreach (var type in types)
            {
                //状态服务跳过
                if (type == typeof(IStatusService)) continue;

                //添加方法
                foreach (var method in CoreHelper.GetMethodsFromType(type))
                {
                    var httpInvoke = CoreHelper.GetMemberAttribute<HttpInvokeAttribute>(method);
                    if (httpInvoke == null) continue;

                    //创建一个新的Caller
                    CreateNewCaller(type, method, httpInvoke);
                }
            }
        }

        private void CreateNewCaller(Type serviceType, System.Reflection.MethodInfo methodInfo, HttpInvokeAttribute invoke)
        {
            //将方法添加到字典
            var callerInfo = new HttpCallerInfo
            {
                Service = serviceType,
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

            callerInfo.CallerName = fullName;
            callers[fullName] = callerInfo;
        }

        /// <summary>
        /// 获取Http方法
        /// </summary>
        /// <returns></returns>
        public string GetDocument(string name)
        {
            var dicCaller = new HttpCallerInfoCollection();
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
                    Name = kvp.Value.CallerName,
                    Authorized = kvp.Value.Authorized,
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
                //获取当前调用者信息
                var client = new AppClient
                {
                    AppName = "HttpServer",
                    HostName = DnsHelper.GetHostName(),
                    IPAddress = DnsHelper.GetIPAddress()
                };

                var caller = callers[name];
                var message = new InvokeMessage
                {
                    ServiceName = caller.Service.FullName,
                    MethodName = caller.Method.ToString(),
                    Parameters = parameters
                };

                //初始化调用者
                var appCaller = new AppCaller
                {
                    AppName = client.AppName,
                    HostName = client.HostName,
                    IPAddress = client.IPAddress,
                    ServiceName = message.ServiceName,
                    MethodName = message.MethodName,
                    Parameters = message.Parameters
                };

                //初始化上下文
                OperationContext.Current = new OperationContext
                {
                    Container = container,
                    Caller = appCaller
                };

                //处理数据返回InvokeData
                var service = container.Resolve<IService>("Service_" + caller.Service.FullName);
                var invoke = new InvokeCaller(client, service);
                var invokeData = invoke.CallMethod(message);
                if (invokeData != null)
                    return invokeData.Value;
            }

            return "{}";
        }
    }
}
