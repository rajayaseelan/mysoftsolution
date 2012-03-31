using System;
using System.Collections;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
using MySoft.IoC.Cache;
using MySoft.Logger;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// json方式 invoke远程代理服务
    /// </summary>
    public class InvokeProxy : RemoteProxy
    {
        public InvokeProxy(ServerNode node, ILog logger)
            : base(node, logger)
        {
            //TO DO
        }

        /// <summary>
        /// Calls the service.
        /// </summary>
        /// <param name="reqMsg">The MSG.</param>
        /// <returns>The result.</returns>
        public override ResponseMessage CallService(RequestMessage reqMsg)
        {
            //如果已经是Invoke调用，则直接返回
            if (reqMsg.InvokeMethod)
            {
                return base.CallService(reqMsg);
            }
            else
            {
                var method = GetInvokeMethod(reqMsg);

                //处理开始
                HandleBegin(reqMsg, method);

                //调用服务
                var resMsg = base.CallService(reqMsg);

                //处理结束
                HandleEnd(resMsg, method);

                return resMsg;
            }
        }

        /// <summary>
        /// 获取Invoke方法
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private System.Reflection.MethodInfo GetInvokeMethod(RequestMessage reqMsg)
        {
            var invoke = reqMsg as IInvoking;
            var method = invoke.MethodInfo;
            invoke.MethodInfo = null;

            return method;
        }

        /// <summary>
        /// 处理输入参数
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="method"></param>
        private void HandleBegin(RequestMessage reqMsg, System.Reflection.MethodInfo method)
        {
            //设置Invoke方式
            reqMsg.InvokeMethod = true;

            var parameters = method.GetParameters();
            if (parameters.Length > 0)
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
        }

        /// <summary>
        /// 处理输出参数
        /// </summary>
        /// <param name="resMsg"></param>
        /// <param name="method"></param>
        private void HandleEnd(ResponseMessage resMsg, System.Reflection.MethodInfo method)
        {
            if (resMsg.IsError) return;

            var parameters = method.GetParameters();
            if (parameters.Length > 0)
            {
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

            var invokeData = resMsg.Value as InvokeData;

            //处理返回值
            var returnType = GetElementType(method.ReturnType);
            resMsg.Value = SerializationManager.DeserializeJson(returnType, invokeData.Value);
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
