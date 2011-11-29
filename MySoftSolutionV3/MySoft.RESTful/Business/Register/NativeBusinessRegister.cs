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

namespace MySoft.RESTful.Business.Register
{
    /// <summary>
    /// 本地业务注册器,读取本地配置文件,加载程序集,反射获取需要绑定的业务接口对象和业务方法
    /// </summary>
    public class NativeBusinessRegister : IBusinessRegister
    {
        private IBusinessPool pool;

        /// <summary>
        /// 获取约束的接口
        /// </summary>
        /// <returns></returns>
        private Type[] GetInterfaces<ContractType>(IWindsorContainer container)
        {
            List<Type> typelist = new List<Type>();
            GraphNode[] nodes = container.Kernel.GraphNodes;
            nodes.Cast<ComponentModel>().ForEach(model =>
            {
                bool markedWithServiceContract = false;
                var attr = CoreHelper.GetTypeAttribute<ContractType>(model.Service);
                if (attr != null)
                {
                    markedWithServiceContract = true;
                }

                if (markedWithServiceContract)
                {
                    typelist.Add(model.Service);
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
                    container = new WindsorContainer(new ServiceInterpreter(new ConfigResource("mysoft.framework/restful")));

                foreach (Type serviceType in GetInterfaces<PublishKindAttribute>(container))
                {
                    //获取业务对象
                    object instance = null;
                    try { instance = container.Resolve(serviceType); }
                    catch { }

                    if (instance == null)
                    {
                        var contract = CoreHelper.GetTypeAttribute<ServiceContractAttribute>(serviceType);
                        if (contract != null) //表明是Castle服务
                        {
                            //使用代理实现IoC服务的调用
                            var proxy = new ProxyInvocationHandler(serviceType);
                            instance = ProxyFactory.GetInstance().Create(proxy, serviceType, true);
                        }
                    }

                    //获取类特性
                    var kind = CoreHelper.GetTypeAttribute<PublishKindAttribute>(serviceType);
                    if (kind != null)
                    {
                        kind.Name = kind.Name.ToLower();

                        //如果包含了相同的类别，则继续
                        if (pool.KindMethods.ContainsKey(kind.Name))
                        {
                            kindModel = pool.KindMethods[kind.Name];
                        }
                        else
                        {
                            kindModel = new BusinessKindModel();
                            kindModel.Name = kind.Name;
                            kindModel.Description = kind.Description;
                            kindModel.State = kind.Enabled ? BusinessState.ACTIVATED : BusinessState.SHUTDOWN;

                            pool.AddKindModel(kind.Name, kindModel);
                        }

                        //获取方法特性
                        foreach (MethodInfo info in CoreHelper.GetMethodsFromType(serviceType))
                        {
                            var method = CoreHelper.GetMemberAttribute<PublishMethodAttribute>(info);
                            if (method != null)
                            {
                                method.Name = method.Name.ToLower();

                                //如果包含了相同的方法，则继续
                                if (kindModel.MethodModels.ContainsKey(method.Name)) continue;

                                methodModel = new BusinessMethodModel();
                                methodModel.Name = method.Name;
                                methodModel.Description = method.Description;
                                methodModel.HttpMethod = method.Method;
                                methodModel.Authorized = !string.IsNullOrEmpty(method.UserParameter);
                                methodModel.State = method.Enabled ? BusinessState.ACTIVATED : BusinessState.SHUTDOWN;
                                methodModel.Method = info;
                                methodModel.Parameters = info.GetParameters();
                                methodModel.UserParameter = method.UserParameter;
                                methodModel.ParametersCount = methodModel.Parameters.Count();
                                methodModel.Instance = instance;

                                if (method.Method != HttpMethod.POST && !CheckGetSubmitType(info.GetParameters()))
                                {
                                    methodModel.IsPassCheck = false;
                                    methodModel.CheckMessage = string.Format("{0} business is not pass check, because the SubmitType of 'GET' parameters only suport primitive type.", kindModel.Name + "." + methodModel.Name);
                                }
                                else
                                {
                                    methodModel.IsPassCheck = true;
                                }

                                kindModel.MethodModels.Add(method.Name, methodModel);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLog(ex);
            }
        }

        /// <summary>
        /// 检查Get类型的参数
        /// </summary>
        /// <param name="paramsInfo"></param>
        /// <returns></returns>
        private bool CheckGetSubmitType(ParameterInfo[] paramsInfo)
        {
            //如果参数为0
            if (paramsInfo.Length == 0) return true;

            bool result = true;
            StringBuilder sb = new StringBuilder();
            foreach (ParameterInfo p in paramsInfo)
            {
                if (!(p.ParameterType.IsValueType || p.ParameterType == typeof(string)))
                {
                    result = false;
                    break;
                }
            }

            return result;
        }
    }
}
