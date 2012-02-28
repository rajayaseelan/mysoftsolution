using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MySoft20.DynamicProxy;
using System.Collections;
using MySoft20;

namespace MySoft.IoC
{
    /// <summary>
    /// Castle访问类
    /// </summary>
    public class CastleFactory
    {
        private Assembly assembly;
        private Type factoryType, invokeType, valueType, managerType;
        private object instance;
        private static CastleFactory singleton;

        private CastleFactory()
        {
            var assembly2 = Assembly.LoadFrom("MySoft.IoC.dll");
            this.factoryType = assembly2.GetType("MySoft.IoC.CastleFactory");
            this.invokeType = assembly2.GetType("MySoft.IoC.Messages.InvokeMessage");
            this.valueType = assembly2.GetType("MySoft.IoC.Messages.InvokeData");

            this.assembly = Assembly.LoadFrom("MySoft.Core.dll");
            this.managerType = assembly.GetType("MySoft.SerializationManager");

            //创建castle对象
            var method = factoryType.GetMethod("Create");
            this.instance = DynamicCalls.GetMethodInvoker(method).Invoke(null, null);
        }

        /// <summary>
        /// 创建CastleFactory对象
        /// </summary>
        /// <returns></returns>
        public static CastleFactory Create()
        {
            if (singleton == null)
            {
                singleton = new CastleFactory();
            }

            return singleton;
        }

        /// <summary>
        /// 获取服务对象
        /// </summary>
        /// <typeparam name="IServiceInterfaceType"></typeparam>
        /// <returns></returns>
        public IServiceInterfaceType GetChannel<IServiceInterfaceType>()
        {
            var serviceType = typeof(IServiceInterfaceType);
            var serviceKey = string.Format("CastleFactory_2.0_{0}", serviceType.FullName);
            var service = CacheHelper.Get(serviceKey);
            if (service == null)
            {
                var handler = new ServiceProxyInvocationHandler(assembly, serviceType, factoryType, invokeType, valueType, managerType, instance);
                service = ProxyFactory.GetInstance().Create(handler, serviceType, true);
            }

            return (IServiceInterfaceType)service;
        }
    }

    /// <summary>
    /// 服务代理
    /// </summary>
    public class ServiceProxyInvocationHandler : IProxyInvocationHandler
    {
        private Assembly assembly;
        private Type serviceType;
        private Type factoryType, invokeType, valueType, managerType;
        private PropertyInfo serviceProperty, methodProperty, parametersProperty, valueProperty, outProperty;
        private object instance;
        private MethodInfo serializeMethod, deserializeMethod, invokeMethod;

        /// <summary>
        /// 实例化ServiceProxyInvocationHandler
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="serviceType"></param>
        /// <param name="factoryType"></param>
        /// <param name="invokeType"></param>
        /// <param name="valueType"></param>
        /// <param name="managerType"></param>
        public ServiceProxyInvocationHandler(Assembly assembly, Type serviceType, Type factoryType, Type invokeType, Type valueType, Type managerType, object instance)
        {
            this.assembly = assembly;
            this.serviceType = serviceType;
            this.factoryType = factoryType;
            this.invokeType = invokeType;
            this.valueType = valueType;
            this.managerType = managerType;
            this.instance = instance;

            this.serviceProperty = invokeType.GetProperty("ServiceName");
            this.methodProperty = invokeType.GetProperty("MethodName");
            this.parametersProperty = invokeType.GetProperty("Parameters");

            this.valueProperty = valueType.GetProperty("Value");
            this.outProperty = valueType.GetProperty("OutParameters");

            this.deserializeMethod = managerType.GetMethod("DeserializeJson", new Type[]{
                    typeof(Type),
                    typeof(string),
                    assembly.GetType("Newtonsoft.Json.JsonConverter[]")
            });

            this.serializeMethod = managerType.GetMethod("SerializeJson", new Type[]{
                    typeof(object),
                    assembly.GetType("Newtonsoft.Json.JsonConverter[]")
            });

            this.invokeMethod = factoryType.GetMethod("Invoke", new Type[] { invokeType });
        }

        #region IProxyInvocationHandler 成员

        /// <summary>
        /// 服务调用
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object Invoke(object proxy, MethodInfo method, object[] parameters)
        {
            try
            {
                var invoke = DynamicCalls.GetInstanceCreator(invokeType)();
                DynamicCalls.GetPropertySetter(serviceProperty).Invoke(invoke, serviceType.FullName);
                DynamicCalls.GetPropertySetter(methodProperty).Invoke(invoke, method.ToString());

                var dictIndex = new Dictionary<string, InvokeResult>();
                var dictParameter = new Hashtable();
                var index = 0;
                foreach (var p in method.GetParameters())
                {
                    if (p.IsOut || p.ParameterType.IsByRef)
                    {
                        dictIndex[p.Name] = new InvokeResult { Index = index, ResultType = GetElementType(p.ParameterType) };
                        if (p.IsOut)
                        {
                            index++;
                            continue;
                        }
                    }

                    dictParameter[p.Name] = parameters[index];
                    index++;
                }
                string paramString = null;
                if (dictParameter.Count > 0)
                    paramString = (string)DynamicCalls.GetMethodInvoker(serializeMethod).Invoke(null, new object[] { dictParameter, null });

                DynamicCalls.GetPropertySetter(parametersProperty).Invoke(invoke, paramString);

                var value = DynamicCalls.GetMethodInvoker(invokeMethod).Invoke(instance, new object[] { invoke });
                var json = DynamicCalls.GetPropertyGetter(valueProperty).Invoke(value);
                var retValue = DynamicCalls.GetMethodInvoker(deserializeMethod).Invoke(null, new object[] { GetElementType(method.ReturnType), json, null });

                if (dictIndex.Count > 0)
                {
                    var outJson = DynamicCalls.GetPropertyGetter(outProperty).Invoke(value);
                    var dictOut = DynamicCalls.GetMethodInvoker(deserializeMethod).Invoke(null, new object[] { typeof(Hashtable), outJson, null });

                    //返回参数赋值
                    dictParameter = dictOut as Hashtable;
                    foreach (var kv in dictIndex)
                    {
                        var result = kv.Value;
                        object outValue = DynamicCalls.GetMethodInvoker(deserializeMethod)
                                .Invoke(null, new object[] { result.ResultType, dictParameter[kv.Key].ToString(), null });

                        parameters[result.Index] = outValue;
                    }
                }

                return retValue;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;
                else
                    throw ex;
            }
        }

        /// <summary>
        /// 获取基类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Type GetElementType(Type type)
        {
            if (type.IsByRef) type = type.GetElementType();
            return type;
        }

        #endregion
    }

    /// <summary>
    /// Invoke返回值
    /// </summary>
    internal class InvokeResult
    {
        /// <summary>
        /// 索引值
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 返回类型
        /// </summary>
        public Type ResultType { get; set; }
    }
}
