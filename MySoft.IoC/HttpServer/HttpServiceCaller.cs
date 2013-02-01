using System;
using System.Collections;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
using MySoft.IoC.Services;

namespace MySoft.IoC.HttpServer
{
    /// <summary>
    /// Http服务调用
    /// </summary>
    internal class HttpServiceCaller
    {
        private IServiceContainer container;
        private CastleServiceConfiguration config;
        private HttpCallerInfoCollection callers;
        private SyncCaller caller;

        /// <summary>
        /// HttpServiceCaller初始化
        /// </summary>
        /// <param name="config"></param>
        /// <param name="container"></param>
        /// <param name="caller"></param>
        public HttpServiceCaller(CastleServiceConfiguration config, IServiceContainer container, SyncCaller caller)
        {
            this.config = config;
            this.container = container;
            this.caller = caller;
            this.callers = new HttpCallerInfoCollection();
        }

        /// <summary>
        /// 初始化Caller
        /// </summary>
        /// <param name="resolver"></param>
        public void InitCaller(IHttpApiResolver resolver)
        {
            //清理资源
            callers.Clear();

            //获取拥有ServiceContract约束的服务
            var types = container.GetServiceTypes<ServiceContractAttribute>();

            //初始化字典
            foreach (var type in types)
            {
                //状态服务跳过
                if (type == typeof(IStatusService)) continue;

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
        public string GetHttpDocument(string name)
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
        /// 获取服务文档
        /// </summary>
        /// <returns></returns>
        public string GetTcpDocument()
        {
            var doc = new TcpDocument(container, config.Port);
            return doc.MakeDocument();
        }

        /// <summary>
        /// 获取API文档
        /// </summary>
        /// <returns></returns>
        public string GetAPIText()
        {
            var array = new ArrayList();
            foreach (var caller in callers.ToValueList())
            {
                array.Add(new
                {
                    Name = caller.CallerName,
                    Authorized = caller.Authorized,
                    TypeString = caller.TypeString
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
                    Parameters = parameters,
                    CacheTime = caller.CacheTime
                };

                //创建服务
                var service = ParseService(message.ServiceName);

                //定义响应数据体
                var invokeData = GetInvokeData(service, message);

                //如果缓存不为null，则返回缓存数据
                if (invokeData != null)
                {
                    return invokeData.Value;
                }
            }

            return "null";
        }

        /// <summary>
        /// 返回响应数据
        /// </summary>
        /// <param name="service"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private InvokeData GetInvokeData(IService service, InvokeMessage message)
        {
            var conf = new CastleFactoryConfiguration
            {
                AppName = "HttpServer",
                EnableCache = config.EnableCache
            };

            //使用Invoke方式调用
            using (var invoke = new InvokeCaller(conf, container, service, caller))
            {
                return invoke.InvokeResponse(message);
            }
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private IService ParseService(string serviceName)
        {
            //处理数据返回InvokeData
            string serviceKey = "Service_" + serviceName;

            if (container.Kernel.HasComponent(serviceKey))
            {
                return container.Resolve<IService>(serviceKey);
            }
            else
            {
                string body = string.Format("The server not find matching service ({0}).", serviceName);

                //返回异常信息
                throw new WarningException(body);
            }
        }
    }
}
