
namespace MySoft.RESTful.Business
{
    /// <summary>
    /// 服务控制器
    /// </summary>
    public interface IServiceController
    {
        /// <summary>
        /// 开始调用
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        void BeginCall(AppCaller caller);

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        object CallService(AppCaller caller, object instance);

        /// <summary>
        /// 结束调用
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="value"></param>
        /// <param name="elapsedTime"></param>
        void EndCall(AppCaller caller, object value, long elapsedTime);
    }
}
