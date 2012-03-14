using System;
using System.Collections;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
using MySoft.IoC.Cache;

namespace MySoft.IoC
{
    /// <summary>
    /// Json处理句柄
    /// </summary>
    public sealed class JsonInvocationHandler : ServiceInvocationHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceInvocationHandler"/> class.
        /// </summary>
        /// <param name="config">config.</param>
        /// <param name="container">The container.</param>
        /// <param name="serviceType">Type of the service interface.</param>
        /// <param name="cache"></param>
        public JsonInvocationHandler(CastleFactoryConfiguration config, IServiceContainer container, IService service, Type serviceType, IServiceCache cache)
            : base(config, container, service, serviceType, cache)
        {
            //构造方法
        }

        /// <summary>
        /// 重载调用服务方法
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        protected override ResponseMessage CallMethod(RequestMessage reqMsg, System.Reflection.MethodInfo method)
        {
            var pis = method.GetParameters();
            reqMsg.InvokeMethod = true;

            //处理参数
            if (pis.Length > 0) HandleInParameter(reqMsg);

            //调用服务
            var resMsg = base.CallMethod(reqMsg, method);

            //处理参数
            if (pis.Length > 0) HandleOutParameter(pis, resMsg);

            //处理返回值
            HandleReturnValue(method, resMsg);

            return resMsg;
        }

        /// <summary>
        /// 处理输入参数
        /// </summary>
        /// <param name="reqMsg"></param>
        private void HandleInParameter(RequestMessage reqMsg)
        {
            if (reqMsg.Parameters.Count > 0)
            {
                string jsonString = reqMsg.Parameters.ToString();
                reqMsg.Parameters.Clear();
                reqMsg.Parameters["InvokeParameter"] = jsonString;
            }
            else
                reqMsg.Parameters["InvokeParameter"] = null;
        }

        /// <summary>
        /// 处理输出参数
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="resMsg"></param>
        private void HandleOutParameter(System.Reflection.ParameterInfo[] parameters, ResponseMessage resMsg)
        {
            if (resMsg.IsError) return;

            var value = resMsg.Value as InvokeData;
            if (!string.IsNullOrEmpty(value.OutParameters))
            {
                var hashtable = SerializationManager.DeserializeJson<Hashtable>(value.OutParameters);
                if (hashtable != null && hashtable.Count > 0)
                {
                    foreach (var parameter in parameters)
                    {
                        if (hashtable.ContainsKey(parameter.Name))
                        {
                            var type = GetElementType(parameter.ParameterType);
                            var obj = SerializationManager.DeserializeJson(type, hashtable[parameter.Name].ToString());
                            resMsg.Parameters[parameter.Name] = obj;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 处理返回值
        /// </summary>
        /// <param name="method"></param>
        /// <param name="resMsg"></param>
        private void HandleReturnValue(System.Reflection.MethodInfo method, ResponseMessage resMsg)
        {
            if (resMsg.IsError) return;

            var value = resMsg.Value as InvokeData;

            //处理返回值
            var returnType = GetElementType(method.ReturnType);
            resMsg.Value = SerializationManager.DeserializeJson(returnType, value.Value);
        }

        /// <summary>
        /// 获取基类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Type GetElementType(Type type)
        {
            if (type.IsByRef) type = type.GetElementType();
            return type;
        }
    }
}
