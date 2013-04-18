using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using Castle.Core;
using Castle.Core.Internal;
using Castle.Core.Resource;
using Castle.Windsor;
using MySoft.IoC;
using MySoft.Logger;
using MySoft.RESTful.Business.Pool;
using Castle.Windsor.Configuration.Interpreters;

namespace MySoft.RESTful.Business.Register
{
    /// <summary>
    /// 本地业务注册器,读取本地配置文件,加载程序集,反射获取需要绑定的业务接口对象和业务方法
    /// </summary>
    public class NativeBusinessRegister : IBusinessRegister
    {
        /// <summary>
        /// 容器对象
        /// </summary>
        private IContainer container;
        private IBusinessPool pool;

        /// <summary>
        /// 获取约束的接口
        /// </summary>
        /// <returns></returns>
        private Type[] GetInterfaceServices<ContractType>(IWindsorContainer container)
        {
            List<Type> typelist = new List<Type>();
            GraphNode[] nodes = container.Kernel.GraphNodes;
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

        public void Register(IBusinessPool businessPool)
        {
            pool = businessPool;
            //读取配置文件
            try
            {
                BusinessKindModel kindModel = null;
                BusinessMethodModel methodModel = null;

                var container = new WindsorContainer();
                if (ConfigurationManager.GetSection("mysoft.framework/restful") != null)
                    container = new WindsorContainer(new XmlInterpreter(new ConfigResource("mysoft.framework/restful")));

                //给当前容器赋值
                this.container = new ServiceContainer(container);

                foreach (var type in GetInterfaceServices<PublishKindAttribute>(container))
                {
                    //获取类特性
                    var kind = CoreHelper.GetMemberAttribute<PublishKindAttribute>(type);
                    if (kind == null) continue;

                    if (string.IsNullOrEmpty(kind.Name)) kind.Name = type.Name;
                    kind.Name = kind.Name.ToLower();

                    //如果包含了相同的类别，则继续
                    if (pool.KindMethods.ContainsKey(kind.Name))
                    {
                        kindModel = pool.KindMethods[kind.Name];
                    }
                    else
                    {
                        kindModel = new BusinessKindModel
                        {
                            Name = kind.Name,
                            Description = string.IsNullOrEmpty(kind.Description) ? "暂无描述信息" : kind.Description,
                        };

                        pool.AddKindModel(kind.Name, kindModel);
                    }

                    //获取方法特性
                    foreach (MethodInfo info in CoreHelper.GetMethodsFromType(type))
                    {
                        var method = CoreHelper.GetMemberAttribute<PublishMethodAttribute>(info);
                        if (method != null)
                        {
                            if (string.IsNullOrEmpty(method.Name)) method.Name = info.Name;
                            method.Name = method.Name.ToLower();

                            //如果包含了相同的方法，则继续
                            if (kindModel.MethodModels.ContainsKey(method.Name))
                            {
                                //处理重复的方法
                                for (int i = 0; i < 10000; i++)
                                {
                                    var name = method.Name + (i + 1);
                                    if (!kindModel.MethodModels.ContainsKey(name))
                                    {
                                        method.Name = name;
                                        break;
                                    }
                                }
                            }

                            methodModel = new BusinessMethodModel
                            {
                                Name = method.Name,
                                Description = string.IsNullOrEmpty(method.Description) ? "暂无描述信息" : method.Description,
                                HttpMethod = HttpMethod.GET,
                                AuthorizeType = method.AuthorizeType,
                                Method = info,
                                Parameters = info.GetParameters(),
                                ParametersCount = info.GetParameters().Count(),
                                Service = type
                            };

                            var types = info.GetParameters().Select(p => p.ParameterType).ToArray();
                            if (!CoreHelper.CheckPrimitiveType(types))
                            {
                                methodModel.HttpMethod = HttpMethod.POST;
                            }

                            kindModel.MethodModels.Add(method.Name, methodModel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("RESTful", ex);
            }
        }

        #region IContainer 成员

        /// <summary>
        /// 解析服务
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public object Resolve(Type service)
        {
            return container.Resolve(service);
        }

        /// <summary>
        /// 释放对象
        /// </summary>
        /// <param name="instance"></param>
        public void Release(object instance)
        {
            container.Release(instance);
        }

        #endregion
    }
}
