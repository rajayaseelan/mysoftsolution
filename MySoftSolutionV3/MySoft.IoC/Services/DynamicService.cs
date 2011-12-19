using System;
using System.Reflection;
using MySoft.IoC.Aspect;
using MySoft.IoC.Messages;
using MySoft.Logger;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// The dynamic service.
    /// </summary>
    public class DynamicService : BaseService
    {
        private ILog logger;
        private Type classType;
        private object instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicService"/> class.
        /// </summary>
        /// <param name="classType">Type of the service interface.</param>
        public DynamicService(ILog logger, Type classType, object instance)
            : base(logger, classType.FullName)
        {
            this.logger = logger;
            this.instance = instance;
            this.classType = classType;
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
            resMsg.Expiration = reqMsg.Expiration;

            #region 获取相应的方法

            string methodKey = string.Format("Method_{0}_{1}", reqMsg.ServiceName, reqMsg.MethodName);
            MethodInfo method = CacheHelper.Get<MethodInfo>(methodKey);
            if (method == null)
            {
                method = CoreHelper.GetMethodFromType(classType, reqMsg.MethodName);
                if (method == null)
                {
                    string title = string.Format("The server not find called method ({0},{1}).", reqMsg.ServiceName, reqMsg.MethodName);
                    var exception = new WarningException(title)
                    {
                        ApplicationName = reqMsg.AppName,
                        ExceptionHeader = string.Format("Application【{0}】occurs error. ==> Comes from {1}({2}).", reqMsg.AppName, reqMsg.HostName, reqMsg.IPAddress)
                    };

                    resMsg.Error = exception;
                    return resMsg;
                }
                else
                {
                    CacheHelper.Insert(methodKey, method, 60);
                }
            }

            #endregion

            //获取服务及方法名称
            if (reqMsg.InvokeMethod)
                resMsg.ReturnType = reqMsg.ReturnType;
            else
                resMsg.ReturnType = method.ReturnType;

            //返回拦截服务
            var service = AspectManager.GetService(instance);

            try
            {
                var pis = method.GetParameters();
                if (reqMsg.InvokeMethod)
                {
                    //解析参数
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
                                    string value = property.Value.ToString(Newtonsoft.Json.Formatting.None);
                                    object jsonValue = null;

                                    //处理反系列化数据
                                    if (!(string.IsNullOrEmpty(value) || value == "{}"))
                                    {
                                        if (value.Contains("new Date"))
                                            jsonValue = SerializationManager.DeserializeJson(GetPrimitiveType(info.ParameterType), value, new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter());
                                        else
                                            jsonValue = SerializationManager.DeserializeJson(GetPrimitiveType(info.ParameterType), value);
                                    }

                                    //处理参数
                                    if (jsonValue == null)
                                        resMsg.Parameters[info.Name] = CoreHelper.GetTypeDefaultValue(GetPrimitiveType(info.ParameterType));
                                    else
                                        resMsg.Parameters[info.Name] = jsonValue;
                                }
                            }
                        }
                    }
                }
                else
                {
                    resMsg.Parameters = reqMsg.Parameters;
                }

                //参数赋值
                object[] paramValues = new object[pis.Length];
                for (int i = 0; i < pis.Length; i++)
                {
                    if (!pis[i].ParameterType.IsByRef)
                    {
                        paramValues[i] = resMsg.Parameters[pis[i].Name];
                    }
                    else if (!pis[i].IsOut)
                    {
                        paramValues[i] = resMsg.Parameters[pis[i].Name];
                    }
                    else
                    {
                        paramValues[i] = CoreHelper.GetTypeDefaultValue(pis[i].ParameterType);
                    }
                }

                //调用对应的服务
                object returnValue = DynamicCalls.GetMethodInvoker(method).Invoke(service, paramValues);

                //把返回值传递回去
                if (reqMsg.InvokeMethod) resMsg.Parameters.Clear();

                for (int i = 0; i < pis.Length; i++)
                {
                    if (pis[i].ParameterType.IsByRef)
                    {
                        resMsg.Parameters[pis[i].Name] = paramValues[i];
                    }
                }

                //返回结果数据
                if (reqMsg.InvokeMethod)
                {
                    string json1 = null;
                    string json2 = null;

                    if (returnValue != null)
                    {
                        if (returnValue is string)
                            json1 = returnValue.ToString();
                        else
                            json1 = SerializationManager.SerializeJson(returnValue);
                    }

                    if (resMsg.Parameters.Count > 0)
                        json2 = resMsg.Parameters.ToString();

                    returnValue = new InvokeData { Value = json1, Parameter = json2 };
                }

                resMsg.Value = returnValue;
            }
            catch (Exception ex)
            {
                //捕获全局错误
                if (reqMsg.InvokeMethod)
                {
                    var e = ErrorHelper.GetInnerException(ex);
                    resMsg.Error = new Exception(e.Message);
                }
                else
                    resMsg.Error = ex;
            }

            return resMsg;
        }

        private Type GetPrimitiveType(Type type)
        {
            if (type.IsByRef) type = type.GetElementType();
            return type;
        }
    }
}
