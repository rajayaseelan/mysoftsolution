using System;
using System.Linq;
using System.Net.Sockets;
using MySoft.IoC.Messages;
using Newtonsoft.Json.Linq;

namespace MySoft.IoC
{
    internal static class IoCHelper
    {
        /// <summary>
        /// 控制台输出
        /// </summary>
        /// <param name="color"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void WriteLine(ConsoleColor color, string format, params object[] args)
        {
            if (Console.Out != null)
            {
                lock (Console.Out)
                {
                    var oldColor = Console.ForegroundColor;
                    Console.ForegroundColor = color;
                    Console.WriteLine(format, args);
                    Console.ForegroundColor = oldColor;
                }
            }
        }

        /// <summary>
        /// 设置参数值
        /// </summary>
        /// <param name="method"></param>
        /// <param name="collection"></param>
        /// <param name="parameters"></param>
        public static void SetRefParameters(System.Reflection.MethodInfo method, ParameterCollection collection, object[] parameters)
        {
            if (collection.Count == 0) return;

            var index = 0;
            foreach (var p in method.GetParameters())
            {
                //给参数赋值
                if (p.ParameterType.IsByRef)
                    parameters[index] = collection[p.Name];

                index++;
            }
        }

        /// <summary>
        /// 设置参数值
        /// </summary>
        /// <param name="method"></param>
        /// <param name="collection"></param>
        public static object[] CreateParameters(System.Reflection.MethodInfo method, ParameterCollection collection)
        {
            if (collection.Count == 0) return new object[0];

            var index = 0;
            var pis = method.GetParameters();
            var parameters = new object[pis.Length];
            foreach (var p in pis)
            {
                //给参数赋值
                if (collection[p.Name] == null)
                    parameters[index] = CoreHelper.GetTypeDefaultValue(p.ParameterType);
                else
                    parameters[index] = collection[p.Name];

                index++;
            }

            return parameters;
        }

        /// <summary>
        /// 设置ParameterCollection值
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <param name="collection"></param>
        public static void SetRefParameters(System.Reflection.MethodInfo method, object[] parameters, ParameterCollection collection)
        {
            if (parameters == null || parameters.Length == 0) return;

            int index = 0;
            foreach (var p in method.GetParameters())
            {
                //给参数赋值
                if (p.ParameterType.IsByRef)
                    collection[p.Name] = parameters[index];

                index++;
            }
        }

        /// <summary>
        /// 创建一个ParameterCollection
        /// </summary>
        /// <param name="method"></param>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static ParameterCollection CreateParameters(System.Reflection.MethodInfo method, string jsonString)
        {
            var collection = new ParameterCollection();

            if (!string.IsNullOrEmpty(jsonString))
            {
                JObject obj = JObject.Parse(jsonString);
                if (obj.Count > 0)
                {
                    var pis = method.GetParameters();
                    object[] parameters = new object[pis.Length];
                    var index = 0;
                    foreach (var p in pis)
                    {
                        var property = obj.Properties().SingleOrDefault(o => string.Compare(o.Name, p.Name, true) == 0);
                        if (property != null)
                        {
                            //获取Json值
                            string value = property.Value.ToString(Newtonsoft.Json.Formatting.None);
                            object jsonValue = CoreHelper.ConvertJsonValue(p.ParameterType, value);
                            parameters[index] = jsonValue;
                        }

                        index++;
                    }

                    //创建参数集合
                    collection = CreateParameters(method, parameters);
                }
            }

            //如果json检测不通过，返回空的参数集合
            return collection;
        }

        /// <summary>
        /// 创建一个ParameterCollection
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static ParameterCollection CreateParameters(System.Reflection.MethodInfo method, object[] parameters)
        {
            var collection = new ParameterCollection();

            int index = 0;
            foreach (var p in method.GetParameters())
            {
                if (parameters[index] == null)
                    collection[p.Name] = CoreHelper.GetTypeDefaultValue(p.ParameterType);
                else
                    collection[p.Name] = parameters[index];

                index++;
            }

            return collection;
        }

        /// <summary>
        /// 获取请求消息
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static ResponseMessage GetResponse(RequestMessage reqMsg, Exception ex)
        {
            var resMsg = new ResponseMessage
            {
                TransactionId = reqMsg.TransactionId,
                ReturnType = reqMsg.ReturnType,
                ServiceName = reqMsg.ServiceName,
                MethodName = reqMsg.MethodName,
                Parameters = reqMsg.Parameters,
                Error = ex
            };

            return resMsg;
        }

        #region 获取异常

        /// <summary>
        /// 获取IoCException
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        /// <returns></returns>
        public static IoCException GetException(OperationContext context, RequestMessage reqMsg, string message, Exception inner)
        {
            var exception = new IoCException(message, inner);

            //获取IoC异常
            return GetException(context, reqMsg, exception);
        }

        /// <summary>
        /// 获取异常
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static IoCException GetException(OperationContext context, RequestMessage reqMsg, IoCException exception)
        {
            exception.ApplicationName = reqMsg.AppName;
            exception.ServiceName = reqMsg.ServiceName;
            exception.ErrorHeader = string.Format("App【{0}】occurs error, comes from {1}({2}).", reqMsg.AppName, reqMsg.HostName, reqMsg.IPAddress);

            //上下文不为null
            if (context != null && context.Caller != null)
            {
                var caller = context.Caller;
                if (!string.IsNullOrEmpty(caller.AppPath))
                {
                    exception.ErrorHeader = string.Format("{0}\r\nApplication Path: {1}", exception.ErrorHeader, caller.AppPath);
                }
            }

            return exception;
        }

        #endregion
    }
}
