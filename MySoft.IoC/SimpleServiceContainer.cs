using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Castle.Core;
using Castle.Core.Internal;
using Castle.Core.Resource;
using Castle.DynamicProxy;
using Castle.Facilities.Startable;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using MySoft.IoC.Aspect;
using MySoft.IoC.Services;
using MySoft.Logger;

namespace MySoft.IoC
{
    /// <summary>
    /// The simple service container.
    /// </summary>
    public sealed class SimpleServiceContainer : IServiceContainer
    {
        #region Private Members

        private IWindsorContainer container;
        private void Init(CastleFactoryType type)
        {
            this.container = new WindsorContainer();

            lock (container)
            {
                //加载自启动注入
                this.container.AddFacility(new StartableFacility());

                //加载服务解析
                this.container.AddFacility(new ServiceDiscoverFacility(this));

                //如果不是远程模式，则加载配置节
                var sectionKey = "mysoft.framework/castle";
                var castle = ConfigurationManager.GetSection(sectionKey);

                if (castle != null)
                {
                    //只解析本地服务
                    if (type != CastleFactoryType.Remote)
                    {
                        //解析服务
                        this.DiscoverServices(sectionKey);
                    }
                }
            }
        }

        /// <summary>
        /// 处理服务
        /// </summary>
        /// <param name="sectionKey"></param>
        private void DiscoverServices(string sectionKey)
        {
            //加载服务
            using (var windsorContainer = new WindsorContainer(new XmlInterpreter(new ConfigResource(sectionKey))))
            {
                //如果容易为空，则不加载
                if (windsorContainer.Kernel.GraphNodes.Length > 0)
                {
                    //获取组件
                    var models = GetComponentModels<ServiceContractAttribute>(windsorContainer);
                    var components = CreateComponents(models);

                    //注册组件
                    container.Register(components.ToArray());
                }
            }
        }

        /// <summary>
        /// 创建组件
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        private IList<IRegistration> CreateComponents(ComponentModel[] models)
        {
            var components = new List<IRegistration>();

            foreach (var model in models)
            {
                //注册服务
                var component = Component.For(model.Services).Named(model.Name).ImplementedBy(model.Implementation);

                if (model.LifestyleType == LifestyleType.Undefined)
                    component = component.LifeStyle.Pooled.LifestylePooled(10, 100);
                else
                    component = component.LifeStyle.Is(model.LifestyleType);

                #region 处理拦截器

                var interceptors = AspectFactory.GetInterceptors(model.Implementation);
                if (interceptors.Count > 0)
                {
                    //注册拦截器
                    foreach (var type in interceptors)
                    {
                        components.Add(Component.For<IInterceptor>().ImplementedBy(type).LifeStyle.Singleton);
                    }

                    var references = interceptors.Select(type => InterceptorReference.ForType(type)).ToArray();
                    component = component.Interceptors(references).SelectedWith(new InterceptorSelector()).Anywhere;
                }

                #endregion

                components.Add(component);
            }

            return components;
        }

        /// <summary>
        /// 获取约束的实现
        /// </summary>
        /// <typeparam name="ContractType"></typeparam>
        /// <param name="container"></param>
        /// <returns></returns>
        private ComponentModel[] GetComponentModels<ContractType>(WindsorContainer container)
        {
            var typelist = new List<ComponentModel>();
            var nodes = container.Kernel.GraphNodes;
            nodes.Cast<ComponentModel>().ForEach(model =>
            {
                bool markedWithServiceContract = false;
                var attr = CoreHelper.GetMemberAttribute<ContractType>(model.Services.First());
                if (attr != null)
                {
                    markedWithServiceContract = true;
                }

                if (markedWithServiceContract)
                {
                    typelist.Add(model);
                }
            });

            return typelist.ToArray();
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleServiceContainer"/> class.
        /// </summary>
        /// <param name="config"></param>
        public SimpleServiceContainer(CastleFactoryType type)
        {
            Init(type);
        }

        #endregion

        #region IServiceContainer Members

        /// <summary>
        /// Gets the kernel.
        /// </summary>
        /// <value>The kernel.</value>
        public IKernel Kernel
        {
            get { return container.Kernel; }
        }

        /// <summary>
        /// Registers the component.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="serviceType"></param>
        /// <param name="classType"></param>
        public void RegisterComponent(string key, Type serviceType, Type classType)
        {
            if (!container.Kernel.HasComponent(key))
                container.Register(Component.For(serviceType).Named(key).ImplementedBy(classType).LifeStyle.Singleton);
        }

        /// <summary>
        /// Registers the component.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="serviceType"></param>
        /// <param name="instance"></param>
        public void RegisterComponent(string key, Type serviceType, object instance)
        {
            if (!container.Kernel.HasComponent(key))
                container.Register(Component.For(serviceType).Named(key).Instance(instance).LifeStyle.Singleton);
        }

        /// <summary>
        /// Registers the components.
        /// </summary>
        /// <param name="serviceKeyTypes">The service key types.</param>
        public void RegisterComponents(IDictionary<Type, object> serviceKeyTypes)
        {
            foreach (var kvp in serviceKeyTypes)
            {
                //注册服务
                RegisterComponent(kvp.Key.FullName, kvp.Key, kvp.Value);
            }
        }

        #region Register component

        /// <summary>
        /// Register ocal service
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="classType"></param>
        public void Register(Type serviceType, Type classType)
        {
            this.Register(serviceType.FullName, serviceType, classType);
        }

        /// <summary>
        /// Register ocal service
        /// </summary>
        /// <param name="key"></param>
        /// <param name="serviceType"></param>
        /// <param name="instance"></param>
        public void Register(string key, Type serviceType, Type classType)
        {
            this.RegisterComponent(key, serviceType, classType);
        }

        /// <summary>
        /// Register ocal service
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="classType"></param>
        public void Register(Type serviceType, object instance)
        {
            this.Register(serviceType.FullName, serviceType, instance);
        }

        /// <summary>
        /// Register ocal service
        /// </summary>
        /// <param name="key"></param>
        /// <param name="serviceType"></param>
        /// <param name="instance"></param>
        public void Register(string key, Type serviceType, object instance)
        {
            this.RegisterComponent(key, serviceType, instance);
        }

        #endregion

        #region Resolve component

        /// <summary>
        /// Releases the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        public void Release(object obj)
        {
            if (obj != null)
            {
                container.Release(obj);
            }
        }

        /// <summary>
        /// Resolve local service
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Resolve(Type type)
        {
            return container.Resolve(type);
        }

        /// <summary>
        /// Resolve local service
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Resolve(string key)
        {
            return container.Resolve<object>(key);
        }

        /// <summary>
        /// Resolve local service
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public TService Resolve<TService>()
        {
            return container.Resolve<TService>();
        }

        /// <summary>
        /// Resolve local service
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public TService Resolve<TService>(string key)
        {
            return container.Resolve<TService>(key);
        }

        #endregion

        /// <summary>
        /// 是否包含服务
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public bool Contains<ContractType>(string serviceName)
        {
            GraphNode[] nodes = this.Kernel.GraphNodes;
            return nodes.Cast<ComponentModel>().Any(model =>
              {
                  bool markedWithServiceContract = false;
                  var attr = CoreHelper.GetMemberAttribute<ContractType>(model.Services.First());
                  if (attr != null)
                  {
                      markedWithServiceContract = true;
                  }

                  return markedWithServiceContract
                      && model.Services.First().FullName == serviceName;
              });
        }

        /// <summary>
        /// 获取约束的接口
        /// </summary>
        /// <returns></returns>
        public Type[] GetServiceTypes<ContractType>()
        {
            var typelist = new List<Type>();
            var nodes = this.Kernel.GraphNodes;
            nodes.Cast<ComponentModel>().ForEach(model =>
            {
                bool markedWithServiceContract = false;
                var attr = CoreHelper.GetMemberAttribute<ContractType>(model.Services.First());
                if (attr != null)
                {
                    markedWithServiceContract = true;
                }

                if (markedWithServiceContract)
                {
                    typelist.Add(model.Services.First());
                }
            });

            return typelist.ToArray();
        }

        #endregion

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName
        {
            get { return typeof(SimpleServiceContainer).FullName; }
        }

        #region ILogable Members

        /// <summary>
        /// OnLog event.
        /// </summary>
        public event LogEventHandler OnLog;

        #endregion

        #region IErrorLogable Members

        /// <summary>
        /// OnError event.
        /// </summary>
        public event ErrorLogEventHandler OnError;

        #endregion

        #region IServiceContainer 成员

        /// <summary>
        /// 输出日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="type"></param>
        public void WriteLog(string log, LogType type)
        {
            try
            {
                if (OnLog != null) OnLog(log, type);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 输出错误
        /// </summary>
        /// <param name="error"></param>
        public void WriteError(Exception error)
        {
            try
            {
                if (OnError != null) OnError(error);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Disposes this object and closes underlying connection.
        /// </summary>
        public void Dispose()
        {
            container.Dispose();
        }

        #endregion
    }
}
