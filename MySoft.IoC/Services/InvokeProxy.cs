using MySoft.IoC.Messages;
using MySoft.Logger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// json方式 invoke远程代理服务
    /// </summary>
    public class InvokeProxy : RemoteProxy
    {
        /// <summary>
        /// Invoke 代理
        /// </summary>
        /// <param name="node"></param>
        /// <param name="logger"></param>
        public InvokeProxy(ServerNode node, ILog logger)
            : base(node, logger, false) { }

        /// <summary>
        /// Calls the service.
        /// </summary>
        /// <param name="reqMsg">The MSG.</param>
        /// <returns>The result.</returns>
        public override ResponseMessage CallService(RequestMessage reqMsg)
        {
            if (reqMsg.RespType == ResponseType.Json)
            {
                return base.CallService(reqMsg);
            }
            else
            {
                var method = reqMsg.MethodInfo;

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
        /// 处理输入参数
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="method"></param>
        private void HandleBegin(RequestMessage reqMsg, System.Reflection.MethodInfo method)
        {
            //设置Invoke方式
            reqMsg.InvokeMethod = true;

            var pis = method.GetParameters();
            if (pis.Length > 0)
            {
                if (reqMsg.Parameters.Count > 0)
                {
                    string jsonString = reqMsg.Parameters.ToString();
                    reqMsg.Parameters.Clear();
                    reqMsg.Parameters["InvokeParameter"] = jsonString;
                }
                else
                {
                    reqMsg.Parameters["InvokeParameter"] = null;
                }
            }
        }

        /// <summary>
        /// 处理输出参数
        /// </summary>
        /// <param name="resMsg"></param>
        /// <param name="method"></param>
        private void HandleEnd(ResponseMessage resMsg, System.Reflection.MethodInfo method)
        {
            if (resMsg == null) return;
            if (resMsg.IsError) return;

            var invokeData = resMsg.Value as InvokeData;

            if (invokeData == null && resMsg is ResponseBuffer)
            {
                //反序列化对象
                var buffer = (resMsg as ResponseBuffer).Buffer;

                invokeData = IoCHelper.DeserializeObject(buffer) as InvokeData;
            }

            //处理参数
            HandleParameters(resMsg, method, invokeData);

            //处理值
            HandleValue(resMsg, method, invokeData);
        }

        /// <summary>
        /// 处理参数
        /// </summary>
        /// <param name="resMsg"></param>
        /// <param name="method"></param>
        /// <param name="invokeData"></param>
        private void HandleParameters(ResponseMessage resMsg, System.Reflection.MethodInfo method, InvokeData invokeData)
        {
            var pis = method.GetParameters().Where(p => p.ParameterType.IsByRef);
            if (pis.Count() > 0)
            {
                if (!string.IsNullOrEmpty(invokeData.OutParameters))
                {
                    var jobject = JObject.Parse(invokeData.OutParameters);
                    if (jobject != null && jobject.Count > 0)
                    {
                        foreach (var p in pis)
                        {
                            var type = GetElementType(p.ParameterType);
                            var jsonString = jobject[p.Name].ToString(Formatting.Indented);
                            var obj = SerializationManager.DeserializeJson(type, jsonString);

                            resMsg.Parameters[p.Name] = obj;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 处理值
        /// </summary>
        /// <param name="resMsg"></param>
        /// <param name="method"></param>
        /// <param name="invokeData"></param>
        private void HandleValue(ResponseMessage resMsg, System.Reflection.MethodInfo method, InvokeData invokeData)
        {
            //处理返回值
            var returnType = GetElementType(method.ReturnType);

            if (returnType == typeof(void))
            {
                resMsg.Value = null;
            }
            else
            {
                if (resMsg is ResponseBuffer)
                {
                    var value = SerializationManager.DeserializeJson(returnType, invokeData.Value);
                    (resMsg as ResponseBuffer).Buffer = IoCHelper.SerializeObject(value);
                }
                else
                {
                    resMsg.Value = SerializationManager.DeserializeJson(returnType, invokeData.Value);
                }
            }
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
