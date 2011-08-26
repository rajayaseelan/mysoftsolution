
namespace MySoft.Web.UI
{
    /// <summary>
    /// 提供异步回调处理的接口
    /// </summary>
    public interface IAjaxProcessHandler
    {
        void OnAjaxProcess(CallbackParams callbackParams);
    }
}