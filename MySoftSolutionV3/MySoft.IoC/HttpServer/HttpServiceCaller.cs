using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MySoft.IoC.Configuration;
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
        private CastleServiceConfiguration config;
        private HttpCallerInfoCollection callers;
        private IDictionary<string, int> callTimeouts;

        /// <summary>
        /// HttpServiceCaller初始化
        /// </summary>
        /// <param name="config"></param>
        /// <param name="container"></param>
        public HttpServiceCaller(CastleServiceConfiguration config, IServiceContainer container)
        {
            this.config = config;
            this.container = container;
            this.callers = new HttpCallerInfoCollection();
            this.callTimeouts = new Dictionary<string, int>();
        }

        /// <summary>
        /// 初始化Caller
        /// </summary>
        /// <param name="resolver"></param>
        public void InitCaller(IHttpApiResolver resolver)
        {
            //清理资源
            callTimeouts.Clear();
            callers.Clear();

            //获取拥有ServiceContract约束的服务
            var types = container.GetServiceTypes<ServiceContractAttribute>();

            //初始化字典
            foreach (var type in types)
            {
                //状态服务跳过
                if (type == typeof(IStatusService)) continue;

                var contract = CoreHelper.GetMemberAttribute<ServiceContractAttribute>(type);
                if (contract != null)
                {
                    if (contract.Timeout > 0)
                        callTimeouts[type.FullName] = contract.Timeout;
                }

                if (resolver != null)
                {
                    //添加方法
                    foreach (var httpApi in resolver.MethodResolver(type))
                    {
                        //添加一个新的Caller
                        AddNewCaller(type, httpApi);
                    }
                }
            }
        }

        private void AddNewCaller(Type serviceType, HttpApiMethod httpApi)
        {
            //将方法添加到字典
            var callerInfo = new HttpCallerInfo
            {
                CacheTime = httpApi.CacheTime,
                Service = serviceType,
                Method = httpApi.Method,
                TypeString = httpApi.Method.ReturnType == typeof(string),
                Description = httpApi.Description,
                Authorized = httpApi.Authorized,
                AuthParameter = httpApi.AuthParameter,
                HttpMethod = httpApi.HttpMethod
            };

            string fullName = httpApi.Name;
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

            var doc = new HttpDocument(dicCaller, config.HttpPort);
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
                var caller = callers[name];
                var message = new InvokeMessage
                {
                    ServiceName = caller.Service.FullName,
                    MethodName = caller.Method.ToString(),
                    Parameters = parameters
                };

                string thisKey = string.Format("{0}${1}${2}", message.ServiceName, message.MethodName, message.Parameters);
                var cacheKey = string.Format("HttpServiceCaller_{0}", IoCHelper.GetMD5String(thisKey));

                var invokeData = CacheHelper.Get<InvokeData>(cacheKey);
                if (invokeData == null)
                {
                    //获取当前调用者信息
                    var appCaller = new AppCaller
                    {
                        AppPath = AppDomain.CurrentDomain.BaseDirectory,
                        AppName = "HttpServer",
                        HostName = DnsHelper.GetHostName(),
                        IPAddress = DnsHelper.GetIPAddress(),
                        ServiceName = message.ServiceName,
                        MethodName = message.MethodName,
                        Parameters = message.Parameters,
                        CallTime = DateTime.Now
                    };

                    //初始化上下文
                    OperationContext.Current = new OperationContext
                    {
                        Container = container,
                        Caller = appCaller
                    };

                    try
                    {
                        //处理数据返回InvokeData
                        var serviceKey = "Service_" + appCaller.ServiceName;
                        var service = container.Resolve<IService>(serviceKey);

                        //等待超时
                        var timeSpan = TimeSpan.FromSeconds(config.Timeout);
                        if (callTimeouts.ContainsKey(appCaller.ServiceName))
                        {
                            timeSpan = TimeSpan.FromSeconds(callTimeouts[appCaller.ServiceName]);
                        }

                        //启用异步调用服务
                        service = new AsyncService(container, service, timeSpan, config.MaxCalls);

                        //使用Invoke方式调用
                        var invoke = new InvokeCaller(appCaller.AppName, service);
                        invokeData = invoke.CallMethod(message);

                        //插入缓存
                        if (invokeData != null && caller.CacheTime > 0)
                        {
                            CacheHelper.Insert(cacheKey, invokeData, caller.CacheTime);
                        }
                    }
                    finally
                    {
                        //初始化上下文
                        OperationContext.Current = null;
                    }
                }

                //如果缓存不为null，则返回缓存数据
                if (invokeData != null)
                    return invokeData.Value;
            }

            return "null";
        }
    }
}
