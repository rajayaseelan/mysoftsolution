using System;
using System.Collections.Generic;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 状态服务信息
    /// </summary>
    [ServiceContract(CallbackType = typeof(IStatusListener))]
    public interface IStatusService
    {
        /// <summary>
        /// 获取所有应用客户端
        /// </summary>
        /// <returns></returns>
        IList<AppClient> GetAppClients();

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
        /// 退订服务
        /// </summary>
        void Unsubscribe();

        /// <summary>
        /// 获取订阅的类型
        /// </summary>
        /// <returns></returns>
        IList<string> GetSubscribeTypes();

        /// <summary>
        /// 订阅发布类型
        /// </summary>
        /// <param name="subscribeType"></param>
        void SubscribeType(string subscribeType);

        /// <summary>
        /// 退订发布类型
        /// </summary>
        /// <param name="subscribeType"></param>
        void UnsubscribeType(string subscribeType);

        /// <summary>
        /// 获取订阅的应用
        /// </summary>
        /// <returns></returns>
        IList<string> GetSubscribeApps();

        /// <summary>
        /// 订阅发布应用
        /// </summary>
        /// <param name="appName"></param>
        void SubscribeApp(string appName);

        /// <summary>
        /// 退订发布应用
        /// </summary>
        /// <param name="appName"></param>
        void UnsubscribeApp(string appName);

        /// <summary>
        /// 是否存在服务
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
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
