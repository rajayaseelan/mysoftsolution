using System;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 调用者
    /// </summary>
    public class InvokeCaller
    {
        private IService service;
        private string appName;
        private string hostName;
        private string ipAddress;

        /// <summary>
        /// 实例化InvokeCaller
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="service"></param>
        public InvokeCaller(string appName, IService service)
        {
            this.service = service;
            this.appName = appName;
            this.hostName = DnsHelper.GetHostName();
            this.ipAddress = DnsHelper.GetIPAddress();
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public object CallMethod(InvokeMessage message)
        {
            #region 设置请求信息

            RequestMessage reqMsg = new RequestMessage();
            reqMsg.InvokeMethod = true;
            reqMsg.AppName = appName;                                       //应用名称
            reqMsg.HostName = hostName;                                     //客户端名称
            reqMsg.IPAddress = ipAddress;                                   //客户端IP地址
            reqMsg.ServiceName = message.ServiceName;                       //服务名称
            reqMsg.MethodName = message.MethodName;                         //方法名称
            reqMsg.ReturnType = typeof(string);                             //返回类型
            reqMsg.TransactionId = Guid.NewGuid();                          //传输ID号

            #endregion

            //给参数赋值
            reqMsg.Parameters["InvokeParameter"] = message.Parameters;

            //调用服务
            var resMsg = service.CallService(reqMsg);

            //如果数据为null,则返回null
            if (resMsg == null) return null;

            //如果有异常，向外抛出
            if (resMsg.IsError) throw resMsg.Error;

            //返回数据
            return resMsg.Value;
        }
    }
}
