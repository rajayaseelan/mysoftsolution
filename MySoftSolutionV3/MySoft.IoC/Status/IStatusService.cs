using System;
using System.Collections.Generic;

namespace MySoft.IoC.Status
{
    /// <summary>
    /// 订阅选项（默认全部启用）
    /// </summary>
    [Serializable]
    public class SubscibeOptions
    {
        /// <summary>
        /// 超时时间，用于监控超时服务调用
        /// </summary>
        public double CallTimeout { get; set; }

        /// <summary>
        /// 推送调用超时
        /// </summary>
        public bool PushCallTimeout { get; set; }

        /// <summary>
        /// 推送调用错误
        /// </summary>
        public bool PushCallError { get; set; }

        /// <summary>
        /// 推送服务状态信息
        /// </summary>
        public bool PushServerStatus { get; set; }

        /// <summary>
        /// 推送客户端连接信息
        /// </summary>
        public bool PushClientConnect { get; set; }

        /// <summary>
        /// 实例化SubscibeOptions
        /// </summary>
        public SubscibeOptions()
        {
            this.CallTimeout = 5; //调用超时为5秒
            this.PushCallTimeout = true;
            this.PushCallError = true;
            this.PushServerStatus = false;
            this.PushClientConnect = true;
        }
    }

    /// <summary>
    /// 状态服务信息
    /// </summary>
    [ServiceContract(CallbackType = typeof(IStatusListener))]
    public interface IStatusService
    {
        /// <summary>
        /// 订阅服务
        /// </summary>
        void Subscibe(params Type[] subscibeTypes);

        /// <summary>
        /// 订阅服务
        /// </summary>
        /// <param name="callTimeout">调用超时时间</param>
        void Subscibe(double callTimeout, params Type[] subscibeTypes);

        /// <summary>
        /// 订阅服务
        /// </summary>
        /// <param name="options">订阅选项</param>
        void Subscibe(SubscibeOptions options, params Type[] subscibeTypes);

        /// <summary>
        /// 退订服务
        /// </summary>
        void Unsubscibe();

        /// <summary>
        /// 是否存在服务
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        [OperationContract(CacheTime = 30)]
        bool ContainsService(string serviceName);

        /// <summary>
        /// 获取服务信息列表
        /// </summary>
        /// <returns></returns>
        IList<Type> GetServiceList();

        /// <summary>
        /// 获取服务状态信息（包括SummaryStatus，HighestStatus，TimeStatus）
        /// </summary>
        /// <returns></returns>
        ServerStatus GetServerStatus();

        /// <summary>
        /// 清除服务器状态
        /// </summary>
        void ClearServerStatus();

        /// <summary>
        /// 获取时段的服务状态信息
        /// </summary>
        /// <returns></returns>
        IList<TimeStatus> GetTimeStatusList();

        /// <summary>
        /// 获取所有的客户端信息
        /// </summary>
        /// <returns></returns>
        IList<ClientInfo> GetClientInfoList();
    }
}
