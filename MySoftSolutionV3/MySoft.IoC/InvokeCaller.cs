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
        private AppClient client;

        /// <summary>
        /// 实例化InvokeCaller
        /// </summary>
        /// <param name="client"></param>
        /// <param name="service"></param>
        public InvokeCaller(AppClient client, IService service)
        {
            this.service = service;
            this.client = client;
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public InvokeData CallMethod(InvokeMessage message)
        {
            #region 设置请求信息

            RequestMessage reqMsg = new RequestMessage();
            reqMsg.Invoked = true;
            reqMsg.AppName = client.AppName;                                //应用名称
            reqMsg.HostName = client.HostName;                              //客户端名称
            reqMsg.IPAddress = client.IPAddress;                            //客户端IP地址
            reqMsg.ServiceName = message.ServiceName;                       //服务名称
            reqMsg.MethodName = message.MethodName;                         //方法名称
            reqMsg.ReturnType = typeof(string);                             //返回类型
            reqMsg.TransactionId = Guid.NewGuid();                          //传输ID号

            #endregion

            //给参数赋值
            reqMsg.Parameters["InvokeParameter"] = message.Parameters;

            //调用服务
            var resMsg = service.CallService(reqMsg);

            //如果有异常，向外抛出
            if (resMsg.IsError) throw resMsg.Error;

            //返回数据
            return resMsg.Value as InvokeData;
        }
    }
}
