using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MySoft.RESTful.Business.Pool;
using MySoft.IoC;
using MySoft.Logger;
using System.Text;

namespace MySoft.RESTful.Business.Register
{
    /// <summary>
    /// 本地业务注册器,读取本地配置文件,加载程序集,反射获取需要绑定的业务接口对象和业务方法
    /// </summary>
    public class NativeBusinessRegister : IBusinessRegister
    {
        private IBusinessPool pool;

        public void Register(IBusinessPool businessPool)
        {
            pool = businessPool;
            //读取配置文件
            try
            {
                BusinessKindModel kindModel = null;
                BusinessMethodModel methodModel = null;
                object instance = null;
                var container = CastleFactory.Create().ServiceContainer;
                foreach (Type serviceType in container.GetInterfaces<PublishKindAttribute>())
                {
                    //获取业务对象
                    try { instance = container[serviceType]; }
                    catch { }

                    if (instance == null)
                    {
                        var iocInstance = CoreHelper.GetTypeAttribute<ServiceContractAttribute>(serviceType);
                        if (iocInstance != null)
                        {
                            var proxy = new ProxyInvocationHandler(serviceType);
                            instance = ProxyFactory.GetInstance().Create(proxy, serviceType, true);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    //获取类特性
                    var kind = CoreHelper.GetTypeAttribute<PublishKindAttribute>(serviceType);
                    if (kind != null)
                    {
                        //如果包含了相同的类别，则继续
                        if (pool.KindMethods.ContainsKey(kind.Name)) continue;

                        kindModel = new BusinessKindModel();
                        kindModel.Name = kind.Name;
                        kindModel.Description = kind.Description;
                        kindModel.State = kind.Enabled ? BusinessState.ACTIVATED : BusinessState.SHUTDOWN;

                        pool.AddKindModel(kind.Name, kindModel);

                        //获取方法特性
                        foreach (MethodInfo info in CoreHelper.GetMethodsFromType(serviceType))
                        {
                            var method = CoreHelper.GetMemberAttribute<PublishMethodAttribute>(info);
                            if (method != null)
                            {
                                //如果包含了相同的方法，则继续
                                if (kindModel.MethodModels.ContainsKey(method.Name)) continue;

                                methodModel = new BusinessMethodModel();
                                methodModel.Name = method.Name;
                                methodModel.Description = method.Description;
                                methodModel.SubmitType = method.Method;
                                methodModel.Authorized = method.Authorized;
                                methodModel.State = method.Enabled ? BusinessState.ACTIVATED : BusinessState.SHUTDOWN;
                                methodModel.Method = info;
                                methodModel.Parameters = info.GetParameters();
                                methodModel.ParametersCount = info.GetParameters().Length;
                                methodModel.Instance = instance;

                                if (method.Method == SubmitType.GET && !CheckGetSubmitType(info.GetParameters()))
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
                if (!(p.ParameterType.IsValueType || p.ParameterType == typeof(string) || p.ParameterType == typeof(AuthenticationUser)))
                {
                    result = false;
                    break;
                }
            }

            return result;
        }
    }
}
