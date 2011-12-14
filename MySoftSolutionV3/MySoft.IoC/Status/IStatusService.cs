using System;
using System.Collections.Generic;

namespace MySoft.IoC.Status
{
    /// <summary>
    /// 订阅选项（默认全部启用）
    /// </summary>
    [Serializable]
    public class SubscribeOptions
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
        /// 定时推送状态定时：单位（秒）
        /// </summary>
        public int StatusTimer { get; set; }

        /// <summary>
        /// 推送客户端连接信息
        /// </summary>
        public bool PushClientConnect { get; set; }

        /// <summary>
        /// 实例化SubscribeOptions
        /// </summary>
        public SubscribeOptions()
        {
            this.CallTimeout = 5; //调用超时为5秒
            this.PushCallTimeout = true;
            this.PushCallError = true;
            this.StatusTimer = 5; //默认间隔为5秒
            this.PushServerStatus = true;
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
        void Subscribe(params string[] subscribeTypes);

        /// <summary>
        /// 订阅服务
        /// </summary>
        /// <param name="callTimeout">调用超时时间</param>
        void Subscribe(double callTimeout, params string[] subscribeTypes);

        /// <summary>
        /// 订阅服务
        /// </summary>
        /// <param name="statusTimer">定时推送时间</param>
        void Subscribe(int statusTimer, params string[] subscribeTypes);

        /// <summary>
        /// 订阅服务
        /// </summary>
        /// <param name="callTimeout">调用超时时间</param>
        /// <param name="statusTimer">定时推送时间</param>
        void Subscribe(double callTimeout, int statusTimer, params string[] subscribeTypes);

        /// <summary>
        /// 订阅服务
        /// </summary>
        /// <param name="options">订阅选项</param>
        void Subscribe(SubscribeOptions options, params string[] subscribeTypes);

        /// <summary>
        /// 获取订阅的类型
        /// </summary>
        /// <returns></returns>
        IList<string> GetSubscribeTypes();

        /// <summary>
        /// 添加发布类型
        /// </summary>
        /// <param name="subscribeType"></param>
        void AddSubscribeType(string subscribeType);

        /// <summary>
        /// 添加发布类型
        /// </summary>
        /// <param name="subscribeType"></param>
        void RemoveSubscribeType(string subscribeType);

        /// <summary>
        /// 退订服务
        /// </summary>
        void Unsubscribe();

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
        IList<ServiceInfo> GetServiceList();

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
        IList<ClientInfo> GetClientList();
    }
}
