using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.IoC.Messages;
using MySoft.IoC.Configuration;
using System.Collections;

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
        /// <param name="container">config.</param>
        /// <param name="container">The container.</param>
        /// <param name="serviceInterfaceType">Type of the service interface.</param>
        public JsonInvocationHandler(CastleFactoryConfiguration config, IServiceContainer container, IService service, Type serviceType)
            : base(config, container, service, serviceType)
        {
            //构造方法
        }

        /// <summary>
        /// 处理输入参数
        /// </summary>
        /// <param name="reqMsg"></param>
        protected override void JsonInParameter(RequestMessage reqMsg)
        {
            reqMsg.InvokeMethod = true;

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
        /// <param name="resMsg"></param>
        protected override void JsonOutParameter(System.Reflection.ParameterInfo[] parameters, ResponseMessage resMsg)
        {
            var value = resMsg.Value as InvokeData;
            var hashtable = SerializationManager.DeserializeJson<Hashtable>(value.OutParameters);
            foreach (var parameter in parameters)
            {
                if (hashtable.ContainsKey(parameter.Name))
                {
                    var type = GetPrimitiveType(parameter.ParameterType);
                    var obj = SerializationManager.DeserializeJson(type, hashtable[parameter.Name].ToString());
                    resMsg.Parameters[parameter.Name] = obj;
                }
            }

            //处理返回值
            resMsg.Value = SerializationManager.DeserializeJson(GetPrimitiveType(resMsg.ReturnType), value.Value);
        }

        /// <summary>
        /// 获取基类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Type GetPrimitiveType(Type type)
        {
            if (type.IsByRef) type = type.GetElementType();
            return type;
        }
    }
}
