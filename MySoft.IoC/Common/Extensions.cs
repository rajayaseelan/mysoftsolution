using System;
using MySoft.IoC.Messages;

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
        internal static void BeginRequest(this IServiceCall call, RequestMessage reqMsg)
        {
            if (call == null) return;

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
                call.BeginCall(callMsg);
            }
            catch (Exception ex)
            {
                //TODO
            }
        }

        /// <summary>
        /// 请求结束
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="resMsg"></param>
        /// <param name="elapsedMilliseconds"></param>
        internal static void EndRequest(this IServiceCall call, RequestMessage reqMsg, ResponseMessage resMsg, long elapsedMilliseconds)
        {
            if (call == null) return;

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
                call.EndCall(callMsg, returnMsg, elapsedMilliseconds);
            }
            catch (Exception ex)
            {
                //TODO
            }
        }
    }
}
