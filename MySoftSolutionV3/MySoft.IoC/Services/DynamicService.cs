using System;
using System.Collections;
using System.Linq;
using MySoft.IoC.Aspect;
using MySoft.IoC.Messages;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// The dynamic service.
    /// </summary>
    public class DynamicService : BaseService
    {
        private IServiceContainer container;
        private Type serviceType;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicService"/> class.
        /// </summary>
        /// <param name="serviceType">Type of the service interface.</param>
        public DynamicService(IServiceContainer container, Type serviceType)
            : base(container, serviceType)
        {
            this.container = container;
            this.serviceType = serviceType;
        }

        /// <summary>
        /// Runs the specified MSG.
        /// </summary>
        /// <param name="reqMsg">The MSG.</param>
        /// <returns>The msg.</returns>
        protected override ResponseMessage Run(RequestMessage reqMsg)
        {
            ResponseMessage resMsg = new ResponseMessage();
            resMsg.TransactionId = reqMsg.TransactionId;
            resMsg.ServiceName = reqMsg.ServiceName;
            resMsg.MethodName = reqMsg.MethodName;

            #region 获取相应的方法

            string methodKey = string.Format("Method_{0}_{1}", reqMsg.ServiceName, reqMsg.MethodName);
            var method = CacheHelper.Get<System.Reflection.MethodInfo>(methodKey);
            if (method == null)
            {
                method = CoreHelper.GetMethodFromType(serviceType, reqMsg.MethodName);
                if (method == null)
                {
                    string message = string.Format("The server not find called method ({0},{1}).", reqMsg.ServiceName, reqMsg.MethodName);
                    resMsg.Error = new WarningException(message);

                    return resMsg;
                }
                else
                {
                    CacheHelper.Permanent(methodKey, method);
                }
            }

            #endregion

            //获取服务及方法名称
            if (reqMsg.InvokeMethod)
                resMsg.ReturnType = reqMsg.ReturnType;
            else
                resMsg.ReturnType = method.ReturnType;

            //容器实例对象
            object instance = null;

            try
            {
                //解析服务
                instance = container.Resolve(serviceType);

                //返回拦截服务
                var service = AspectFactory.CreateProxyService(serviceType, instance);

                var pis = method.GetParameters();
                if (reqMsg.InvokeMethod)
                {
                    ParseParameter(reqMsg, resMsg, pis);
                }
                else
                {
                    resMsg.Parameters = reqMsg.Parameters;
                }

                //参数赋值
                object[] paramValues = new object[pis.Length];
                for (int i = 0; i < pis.Length; i++)
                {
                    //处理默认值
                    paramValues[i] = resMsg.Parameters[pis[i].Name] ?? CoreHelper.GetTypeDefaultValue(pis[i].ParameterType);
                }

                //调用对应的服务
                object returnValue = DynamicCalls.GetMethodInvoker(method).Invoke(service, paramValues);

                var outValues = new Hashtable();
                for (int i = 0; i < pis.Length; i++)
                {
                    if (pis[i].ParameterType.IsByRef)
                    {
                        resMsg.Parameters[pis[i].Name] = paramValues[i];
                        outValues[pis[i].Name] = paramValues[i];
                    }
                }

                //返回结果数据
                if (reqMsg.InvokeMethod)
                {
                    resMsg.Parameters.Clear();
                    resMsg.Value = returnValue;

                    string json1 = null;
                    string json2 = null;

                    if (method.ReturnType == typeof(string))
                    {
                        json1 = Convert.ToString(returnValue);
                    }
                    else
                    {
                        if (returnValue != null)
                            json1 = SerializationManager.SerializeJson(returnValue);
                        else
                            json1 = "{}";
                    }

                    if (outValues.Count > 0)
                        json2 = SerializationManager.SerializeJson(outValues);

                    returnValue = new InvokeData
                    {
                        Value = json1,
                        Count = resMsg.Count,
                        OutParameters = json2
                    };
                }

                resMsg.Value = returnValue;
            }
            catch (Exception ex)
            {
                //捕获全局错误
                resMsg.Error = ex;
            }
            finally
            {
                //释放资源
                container.Release(instance);
            }

            return resMsg;
        }

        /// <summary>
        /// 解析参数
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="resMsg"></param>
        /// <param name="pis"></param>
        private void ParseParameter(RequestMessage reqMsg, ResponseMessage resMsg, System.Reflection.ParameterInfo[] pis)
        {
            var objValue = reqMsg.Parameters["InvokeParameter"];
            if (!(objValue == null || string.IsNullOrEmpty(objValue.ToString())))
            {
                JObject obj = JObject.Parse(objValue.ToString());
                if (obj.Count > 0)
                {
                    foreach (var info in pis)
                    {
                        var property = obj.Properties().SingleOrDefault(p => string.Compare(p.Name, info.Name, true) == 0);
                        if (property != null)
                        {
                            //获取Json值
                            string value = property.Value.ToString(Newtonsoft.Json.Formatting.None);
                            object jsonValue = CoreHelper.ConvertJsonToObject(info, value);
                            resMsg.Parameters[info.Name] = jsonValue;
                        }
                    }
                }
            }
        }

        private Type GetElementType(Type type)
        {
            if (type.IsByRef) type = type.GetElementType();
            return type;
        }
    }
}
