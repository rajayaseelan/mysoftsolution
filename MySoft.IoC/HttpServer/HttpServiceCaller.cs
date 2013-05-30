using System;
using System.Collections;
using System.Linq;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
using MySoft.IoC.Services;

namespace MySoft.IoC.HttpServer
{
    /// <summary>
    /// Http服务调用
    /// </summary>
    internal class HttpServiceCaller : IDisposable
    {
        private IServiceContainer container;
        private CastleServiceConfiguration config;
        private HttpCallerInfoCollection callers;
        private AsyncCaller caller;

        /// <summary>
        /// HttpServiceCaller初始化
        /// </summary>
        /// <param name="config"></param>
        /// <param name="container"></param>
        public HttpServiceCaller(CastleServiceConfiguration config, IServiceContainer container)
        {
            this.config = config;
            this.container = container;
            this.caller = new AsyncCaller(true, config.MaxCaller);
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
                    var httpApis = resolver.MethodResolver(type);
                    httpApis = httpApis.OrderBy(p => p.Name).ToList();

                    //添加方法
                    foreach (var httpApi in httpApis)
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
                    ServerUri = string.Format("{0}:{1}", DnsHelper.GetIPAddress(), config.Port),
                    CallerName = caller.CallerName,
                    ServiceName = caller.Service.FullName,
                    MethodName = caller.Method.ToString(),
                    CacheTime = caller.CacheTime,
                    AuthParameter = caller.AuthParameter,
                    Authorized = caller.Authorized,
                    HttpMethod = caller.HttpMethod
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
        public string CallService(string name, string parameters)
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

                var conf = new CastleFactoryConfiguration
                {
                    AppName = "HttpServer",
                    EnableCache = true,
                    MaxCaller = config.MaxCaller
                };

                using (var invoke = new InvokeCaller(conf, this.container, service, this.caller, null, this.container))
                {
                    //定义响应数据体
                    var invokeData = invoke.InvokeResponse(message);

                    //如果缓存不为null，则返回缓存数据
                    if (invokeData != null)
                    {
                        return invokeData.Value;
                    }
                }
            }

            return "null";
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

        #region IDisposable 成员

        public void Dispose()
        {
            caller = null;
            container = null;
            config = null;
            callers.Clear();
            callers = null;
        }

        #endregion
    }
}
