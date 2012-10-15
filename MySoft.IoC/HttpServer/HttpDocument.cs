using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Net;

namespace MySoft.IoC.HttpServer
{
    /// <summary>
    /// API文档类
    /// </summary>
    internal class HttpDocument
    {
        private HttpCallerInfoCollection callers;
        private int port;
        private string htmlTemplate, itemTemplate;

        public HttpDocument(HttpCallerInfoCollection callers, int port)
        {
            this.callers = callers;
            this.port = port;

            #region 读取资源

            try
            {
                var assm = this.GetType().Assembly;
                var helpStream = assm.GetManifestResourceStream("MySoft.IoC.HttpServer.Template.help.htm");
                var helpitemStream = assm.GetManifestResourceStream("MySoft.IoC.HttpServer.Template.helpitem.htm");

                //读取主模板
                using (var helpReader = new StreamReader(helpStream))
                {
                    htmlTemplate = helpReader.ReadToEnd();
                }

                //读取子项模板
                using (var helpitemReader = new StreamReader(helpitemStream))
                {
                    itemTemplate = helpitemReader.ReadToEnd();
                }
            }
            catch
            {
                htmlTemplate = string.Empty;
                itemTemplate = string.Empty;
            }

            #endregion
        }

        /// <summary>
        /// 生成文档
        /// </summary>
        /// <returns></returns>
        public string MakeDocument(string name)
        {
            string uri = string.Format("http://{0}:{1}/", IPAddress.Loopback, port);
            var html = htmlTemplate.Replace("${uri}", uri);
            html = html.Replace("${count}", callers.Count.ToString());

            StringBuilder sbUrl = new StringBuilder();
            foreach (var caller in callers.ToValueList())
            {
                sbUrl.Append(GetItemDocument(caller, name));
            }

            html = html.Replace("${body}", sbUrl.ToString());
            return html;
        }

        /// <summary>
        /// 获取Item文档
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetItemDocument(HttpCallerInfo caller, string name)
        {
            var template = itemTemplate;
            var plist = new List<string>();
            foreach (var p in caller.Method.GetParameters())
            {
                if (caller.Authorized && string.Compare(p.Name, caller.AuthParameter, true) == 0)
                    continue;

                plist.Add(string.Format("{0}=[{0}]", p.Name).Replace('[', '{').Replace(']', '}'));
            }

            string uri = string.Empty;
            if (caller.HttpMethod == HttpMethod.GET && plist.Count > 0)
                uri = string.Format("/{0}?{1}", caller.CallerName, string.Join("&", plist.ToArray()));
            else
                uri = string.Format("/{0}", caller.CallerName);

            var url = string.Format("<a rel=\"operation\" target=\"_blank\" title=\"{0}\" href=\"{0}\">{0}</a> 处的服务", uri.ToLower());

            var description = caller.Description == null ? null : caller.Description.Replace("\r\n", "<br/>");
            var serviceInfo = string.Format("【{0}】\r\n【{1}】", caller.Service.FullName, caller.Method.ToString());
            template = template.Replace("${method}", string.Format("<p title=\"分布式服务接口:\r\n{2}\"><b><a href='/help/{0}'>{0}</a></b><br/>{1}</p>",
                caller.CallerName, description, serviceInfo));

            var strParameter = GetMethodParameter(caller.Method, caller.Authorized, caller.AuthParameter, name);
            if (string.IsNullOrEmpty(strParameter))
                template = template.Replace("${parameter}", "&nbsp;");
            else
                template = template.Replace("${parameter}", strParameter);

            template = template.Replace("${type}", caller.HttpMethod == HttpMethod.GET ? "GET<br/>POST" : "<font color='red'>POST</font>");
            template = template.Replace("${auth}", caller.Authorized ? "<font color='red'>是</font>" : "&nbsp;");
            if (caller.Authorized)
                template = template.Replace("${authparameter}", caller.AuthParameter);
            else
                template = template.Replace("${authparameter}", "&nbsp;");
            template = template.Replace("${uri}", url.ToString());

            return template;
        }

        private string GetMethodParameter(MethodInfo method, bool authorized, string authParameter, string name)
        {
            StringBuilder buider = new StringBuilder();

            var pis = method.GetParameters();
            var parametersCount = pis.Count();

            if (authorized) parametersCount--;
            if (parametersCount > 0) buider.Append("<b>INPUT</b> -><br/>");

            foreach (var p in pis)
            {
                if (authorized && string.Compare(p.Name, authParameter, true) == 0)
                    continue;

                if (!string.IsNullOrEmpty(name))
                    buider.Append(GetTypeDetail(p.Name, p.ParameterType, 1));
                else
                    buider.AppendFormat(string.Format("&nbsp;&nbsp;&nbsp;&nbsp;&lt;{0} : {1}&gt;", p.Name, GetTypeName(p.ParameterType)) + "<br/>");
            }

            if (parametersCount > 0) buider.Append("<hr/>");
            var value = string.Format("<b>OUTPUT</b> -> {0}<br/>", GetTypeName(method.ReturnType));
            buider.Append("<font color=\"#336699\">").Append(value);
            if (!string.IsNullOrEmpty(name)) buider.Append(GetTypeDetail(null, method.ReturnType, 1));
            buider.Append("</font>");

            return buider.ToString();
        }

        #region 处理参数

        private bool GetTypeClass(Type type)
        {
            if (type.IsGenericType)
            {
                foreach (var t in type.GetGenericArguments())
                {
                    var isClass = GetTypeClass(t);
                    if (isClass) return isClass;
                }

                return false;
            }
            else
                return (type.IsClass && type != typeof(string)) || type.IsEnum;
        }

        private string GetTypeName(Type type)
        {
            return CoreHelper.GetTypeName(type).Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private string GetTypeDetail(string name, Type type, int index)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(name))
            {
                for (int i = 0; i < index; i++) sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;");
                sb.AppendFormat(string.Format("&lt;{0} : {1}&gt;", name, GetTypeName(type)) + "<br/>");
            }

            type = CoreHelper.GetPrimitiveType(type);
            if (GetTypeClass(type))
            {
                if (type.IsEnum)
                {
                    var names = Enum.GetNames(type);
                    var values = Enum.GetValues(type);

                    for (int i = 0; i < index; i++) sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;");
                    sb.Append("<b style='color:#999;'>[" + GetTypeName(type) + "]</b><br/>");
                    for (int n = 0; n < names.Length; n++)
                    {
                        for (int i = 0; i <= index; i++) sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;");
                        sb.AppendFormat(string.Format("&lt;{0} : {1}&gt;", names[n], Convert.ToInt32(values.GetValue(n))) + "<br/>");
                    }
                }
                else
                {
                    for (int i = 0; i < index; i++) sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;");
                    sb.Append("<b style='color:#999;'>[" + GetTypeName(type) + "]</b><br/>");

                    foreach (var p in CoreHelper.GetPropertiesFromType(type))
                    {
                        if (GetTypeClass(p.PropertyType) && type != p.PropertyType)
                        {
                            sb.Append(GetTypeDetail(p.Name, p.PropertyType, index + 1));
                        }
                        else
                        {
                            for (int i = 0; i <= index; i++) sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;");
                            sb.AppendFormat(string.Format("&lt;{0} : {1}&gt;", p.Name, GetTypeName(p.PropertyType)) + "<br/>");
                        }
                    }

                    foreach (var p in type.GetFields())
                    {
                        if (GetTypeClass(p.FieldType))
                        {
                            sb.Append(GetTypeDetail(p.Name, p.FieldType, index + 1));
                        }
                        else
                        {
                            for (int i = 0; i <= index; i++) sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;");
                            sb.AppendFormat(string.Format("&lt;{0} : {1}&gt;", p.Name, GetTypeName(p.FieldType)) + "<br/>");
                        }
                    }
                }
            }

            return sb.ToString();
        }

        #endregion
    }
}