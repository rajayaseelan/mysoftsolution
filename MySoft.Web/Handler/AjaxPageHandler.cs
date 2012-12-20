using System.Text;
using System.Web;
using System.Web.SessionState;

namespace MySoft.Web
{
    /// <summary>
    /// 异步处理Handler
    /// </summary>
    public class AjaxPageHandler : IHttpHandler, IRequiresSessionState
    {
        // 摘要:
        //     获取一个值，该值指示其他请求是否可以使用 System.Web.IHttpHandler 实例。
        //
        // 返回结果:
        //     如果 System.Web.IHttpHandler 实例可再次使用，则为 true；否则为 false。
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        // 摘要:
        //     通过实现 System.Web.IHttpHandler 接口的自定义 HttpHandler 启用 HTTP Web 请求的处理。
        //
        // 参数:
        //   context:
        //     System.Web.HttpContext 对象，它提供对用于为 HTTP 请求提供服务的内部服务器对象（如 Request、Response、Session
        //     和 Server）的引用。
        public void ProcessRequest(HttpContext context)
        {
            string ajaxKey = "AjaxProcess", url = string.Empty, space = string.Empty;
            string[] split = HttpUtility.UrlDecode(context.Request.Url.Query.Remove(0, 1)).Split(';');

            url = CoreHelper.Decrypt(split[0], ajaxKey);
            url = (url.IndexOf('/') >= 0 ? url : "/" + url);
            if (split.Length > 1) space = CoreHelper.Decrypt(split[1], ajaxKey);
            if (string.IsNullOrEmpty(space)) space = "AjaxMethods";

            StringBuilder sb = new StringBuilder();
            sb.Append("var ajaxPage = { \r\n");
            sb.Append("\t\t\"url\" : \"" + url + "\",\r\n");
            sb.Append("\t\t\"key\" : \"" + WebHelper.MD5Encrypt(ajaxKey) + "\"\r\n");
            sb.Append("\t};\r\n\r\n");
            sb.Append(string.Format("var {0} = Ajax.registerPage(this);", space));

            //写入javascript代码
            context.Response.ContentType = "text/javascript;charset=utf-8";

            //将javascript代码输出到文件
            context.Response.Clear();
            context.Response.Write(sb.ToString());
            context.Response.Flush();
            //context.Response.End();

            //结束请求
            context.Response.Close();
            context.ApplicationInstance.CompleteRequest();
        }
    }
}
