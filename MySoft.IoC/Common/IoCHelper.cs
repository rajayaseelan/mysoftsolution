using System;
using System.Linq;
using System.Net.Sockets;
using MySoft.IoC.Messages;
using Newtonsoft.Json.Linq;

namespace MySoft.IoC
{
    /// <summary>
    /// IoC帮助类
    /// </summary>
    public static class IoCHelper
    {
        /// <summary>
        /// 控制台输出
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void WriteLine(string format, params object[] args)
        {
            WriteLine(string.Format(format, args));
        }

        /// <summary>
        /// 控制台输出
        /// </summary>
        /// <param name="message"></param>
        public static void WriteLine(string message)
        {
            if (Console.Out != null)
            {
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// 控制台输出
        /// </summary>
        /// <param name="color"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void WriteLine(ConsoleColor color, string format, params object[] args)
        {
            WriteLine(color, string.Format(format, args));
        }

        /// <summary>
        /// 控制台输出
        /// </summary>
        /// <param name="color"></param>
        /// <param name="message"></param>
        public static void WriteLine(ConsoleColor color, string message)
        {
            if (Console.Out != null)
            {
                lock (Console.Out)
                {
                    var oldColor = Console.ForegroundColor;
                    Console.ForegroundColor = color;
                    Console.WriteLine(message);
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
            var pis = method.GetParameters();
            object[] parameters = new object[pis.Length];

            if (!string.IsNullOrEmpty(jsonString))
            {
                JObject obj = JObject.Parse(jsonString);
                if (obj.Count > 0)
                {
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
                }
            }

            //创建参数集合
            return CreateParameters(method, parameters);
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
                ServiceName = reqMsg.ServiceName,
                MethodName = reqMsg.MethodName,
                Error = ex
            };

            return resMsg;
        }

        #region 获取异常

        /// <summary>
        /// 获取IoCException
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static IoCException GetException(AppCaller caller, string message)
        {
            //创建IoC异常
            var exception = new MySoft.IoC.WarningException(message);

            //设置调用信息
            SetAppCaller(caller, exception);

            return exception;
        }

        /// <summary>
        /// 获取异常
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static IoCException GetException(AppCaller caller, System.TimeoutException error)
        {
            //创建IoC异常
            var exception = new MySoft.IoC.TimeoutException(error.Message);

            //设置调用信息
            SetAppCaller(caller, exception);

            return exception;
        }

        /// <summary>
        /// 获取IoCException
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        /// <returns></returns>
        public static IoCException GetException(AppCaller caller, string message, Exception inner)
        {
            //创建IoC异常
            var exception = new MySoft.IoC.IoCException(message, inner);

            //设置调用信息
            SetAppCaller(caller, exception);

            return exception;
        }

        /// <summary>
        /// 设置调用信息
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="error"></param>
        private static void SetAppCaller(AppCaller caller, IoCException error)
        {
            error.ApplicationName = caller.AppName;
            error.ServiceName = caller.ServiceName;
            error.ErrorHeader = string.Format("App【{0}】occurs error, comes from {1}({2}).", caller.AppName, caller.HostName, caller.IPAddress);

            if (!string.IsNullOrEmpty(caller.AppPath))
            {
                error.ErrorHeader = string.Format("{0}\r\nApplication Path: {1}", error.ErrorHeader, caller.AppPath);
            }
        }

        #endregion
    }
}
