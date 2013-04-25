using MySoft.Web.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
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

            //写入javascript代码
            var ajaxType = Type.GetType(System.IO.Path.GetFileNameWithoutExtension(context.Request.Url.AbsolutePath));

            if (ajaxType != null)
            {
                var ajaxMethods = AjaxMethodHelper.GetAjaxMethods(ajaxType);
                if (ajaxMethods.Count > 0)
                {
                    sb.Append(GetAjaxMethods(url, ajaxMethods).Replace("{space}", space));
                }
            }

            context.Response.Buffer = true;
            context.Response.Clear();
            context.Response.ClearHeaders();

            context.Response.ContentType = "text/javascript;charset=utf-8";

            //将javascript代码输出到文件
            context.Response.Write(sb.ToString());
            context.Response.End();
        }

        private string GetAjaxMethods(string url, IDictionary<string, AsyncMethodInfo> ajaxMethods)
        {
            List<AjaxMethodInfo> methodInfoList = new List<AjaxMethodInfo>();
            List<string> paramList = new List<string>();
            foreach (string key in ajaxMethods.Keys)
            {
                paramList.Clear();
                AjaxMethodInfo methodInfo = new AjaxMethodInfo();
                methodInfo.Name = key;
                foreach (ParameterInfo pi in ajaxMethods[key].Method.GetParameters())
                {
                    paramList.Add(pi.Name);
                }

                methodInfo.Async = ajaxMethods[key].Async;
                methodInfo.Paramters = paramList.ToArray();
                methodInfoList.Add(methodInfo);
            }

            return GetAjaxString(url, methodInfoList);
        }

        private string GetAjaxString(string url, IList<AjaxMethodInfo> methods)
        {
            var sb = new StringBuilder("var Ajax_class = __Class.create();\r\n");
            sb.Append("Object.extend(Ajax_class.prototype, ");
            sb.Append("Object.extend(new AjaxClass(), {\r\n");
            sb.Append("\turl : '" + url + "',\r\n");
            for (int i = 0; i < methods.Count; i++)
            {
                var method = methods[i];
                sb.Append("\t" + method.Name + " : function(");
                var sp = new StringBuilder("{\r\n");
                var isContent = false;
                if (method.Paramters.Length > 0)
                {
                    isContent = true;
                    for (var p = 0; p < method.Paramters.Length; p++)
                    {
                        var paramter = method.Paramters[p];
                        if (p == method.Paramters.Length - 1)
                        {
                            sp.Append("\t\t\t\t" + paramter + " : Ajax.toJSON(param)");
                        }
                        else
                        {
                            sp.Append("\t\t\t\t" + paramter + " : Ajax.toJSON(" + paramter + ")");
                        }
                        sb.Append(paramter);
                        if (p < method.Paramters.Length - 1)
                        {
                            sp.Append(",\r\n");
                            sb.Append(",");
                        }
                    }
                }
                sp.Append("\r\n\t\t\t}");
                if (method.Async)
                {
                    if (isContent) sb.Append(",callback){\r\n");
                    else sb.Append("callback)\r\n\t{\r\n");
                }
                else
                {
                    sb.Append(")\r\n\t{\r\n");
                }
                if (isContent)
                {
                    sb.Append("\t\tvar param=[],pm=[];\r\n");
                    sb.Append("\t\tpm.addRange(arguments);\r\n");
                    sb.Append("\t\tif(pm.length>" + method.Paramters.Length + "){\r\n");
                    sb.Append("\t\t\tparam.addRange(pm);\r\n");
                    if (method.Async)
                    {
                        sb.Append("\t\t\tif(typeof(arguments[arguments.length-1])=='function')\r\n");
                        sb.Append("\t\t\t\tcallback=arguments[arguments.length-1];\r\n");
                    }
                    if (method.Paramters.Length > 1)
                    {
                        sb.Append("\t\t\tparam.splice(0," + (method.Paramters.Length - 1) + ");\r\n");
                    }
                    sb.Append("\t\t} else {\r\n");
                    sb.Append("\t\t\tparam=" + method.Paramters[method.Paramters.Length - 1] + ";\r\n");
                    sb.Append("\t\t}\r\n\r\n");
                    sb.Append("\t\tvar args = " + sp.ToString() + ";\r\n");
                }
                else sb.Append("\t\tvar args = null;\r\n");
                sb.Append("\r\n\t\treturn this.invoke('" + method.Name + "'");
                sb.Append(",args");
                if (isContent) sb.Append(",'POST'");
                else sb.Append(",'GET'");
                if (method.Async) sb.Append(",true");
                else sb.Append(",false");
                if (method.Async) sb.Append(",callback");
                sb.Append(");\r\n\t}");
                if (i < methods.Count - 1) sb.Append(",\r\n");
            }

            sb.Append("\r\n}));\r\n\r\n");
            sb.Append("var {space} = new Ajax_class();");

            return sb.ToString();
        }
    }
}
