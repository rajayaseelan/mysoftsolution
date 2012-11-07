using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.IoC.Messages;
using MySoft.IoC.Logger;
using MySoft.Cache;

namespace MySoft.IoC
{
    /// <summary>
    /// 扩展类
    /// </summary>
    public static class LogExtensions
    {
        /// <summary>
        /// 请求开始
        /// </summary>
        /// <param name="reqMsg"></param>
        internal static void BeginRequest(this IServiceLog logger, RequestMessage reqMsg)
        {
            if (logger == null) return;

            try
            {
                var callMsg = new CallMessage
                {
                    AppName = reqMsg.AppName,
                    IPAddress = reqMsg.IPAddress,
                    HostName = reqMsg.HostName,
                    ServiceName = reqMsg.ServiceName,
                    MethodName = reqMsg.MethodName,
                    Parameters = reqMsg.Parameters
                };

                //开始调用
                logger.Begin(callMsg);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 请求结束
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="resMsg"></param>
        /// <param name="elapsedMilliseconds"></param>
        internal static void EndRequest(this IServiceLog logger, RequestMessage reqMsg, ResponseMessage resMsg, long elapsedMilliseconds)
        {
            if (logger == null) return;

            try
            {
                var callMsg = new CallMessage
                {
                    AppName = reqMsg.AppName,
                    IPAddress = reqMsg.IPAddress,
                    HostName = reqMsg.HostName,
                    ServiceName = reqMsg.ServiceName,
                    MethodName = reqMsg.MethodName,
                    Parameters = reqMsg.Parameters
                };

                var returnMsg = new ReturnMessage
                {
                    ServiceName = resMsg.ServiceName,
                    MethodName = resMsg.MethodName,
                    Parameters = resMsg.Parameters,
                    Count = resMsg.Count,
                    Error = resMsg.Error,
                    Value = resMsg.Value
                };

                //结束调用
                logger.End(callMsg, returnMsg, elapsedMilliseconds);
            }
            catch
            {
            }
        }
    }
}
