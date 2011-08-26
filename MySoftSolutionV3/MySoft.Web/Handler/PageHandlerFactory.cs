using System.Web;
using System.Web.SessionState;
using System.Web.UI;

namespace MySoft.Web
{
    public class PageHandlerFactory : IHttpHandlerFactory, IRequiresSessionState
    {
        // 摘要:
        //     返回实现 System.Web.IHttpHandler 接口的类的实例。
        //
        // 参数:
        //   pathTranslated:
        //     所请求资源的 System.Web.HttpRequest.PhysicalApplicationPath。
        //
        //   url:
        //     所请求资源的 System.Web.HttpRequest.RawUrl。
        //
        //   context:
        //     System.Web.HttpContext 类的实例，它提供对用于为 HTTP 请求提供服务的内部服务器对象（如 Request、Response、Session
        //     和 Server）的引用。
        //
        //   requestType:
        //     客户端使用的 HTTP 数据传输方法（GET 或 POST）。
        //
        // 返回结果:
        //     处理请求的新的 System.Web.IHttpHandler 对象。
        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            string sendToUrl = context.Request.Url.PathAndQuery;
            string filePath = pathTranslated;
            string sendToUrlLessQString;
            RewriterUtils.RewriteUrl(context, sendToUrl, out sendToUrlLessQString, out filePath);
            return PageParser.GetCompiledPageInstance(sendToUrlLessQString, filePath, context);
        }

        //
        // 摘要:
        //     使工厂可以重用现有的处理程序实例。
        //
        // 参数:
        //   handler:
        //     要重用的 System.Web.IHttpHandler 对象。
        public void ReleaseHandler(IHttpHandler handler)
        { }
    }
}
