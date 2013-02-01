using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using IOC = MySoft.IoC.Messages;

namespace MySoft.IoC.HttpServer
{
    /// <summary>
    /// API文档类
    /// </summary>
    internal class TcpDocument
    {
        private int port;
        private IContainer container;
        private string htmlTemplate, itemTemplate;

        public TcpDocument(IContainer container, int port)
        {
            this.container = container;
            this.port = port;

            #region 读取资源

            try
            {
                var assm = this.GetType().Assembly;
                var helpStream = assm.GetManifestResourceStream("MySoft.IoC.HttpServer.Template.Service.help.htm");
                var helpitemStream = assm.GetManifestResourceStream("MySoft.IoC.HttpServer.Template.Service.helpitem.htm");

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
        public string MakeDocument()
        {
            string uri = string.Format("tcp://{0}:{1}/", IPAddress.Loopback, port);
            var html = htmlTemplate.Replace("${uri}", uri);

            StringBuilder sbUrl = new StringBuilder();
            var service = container.Resolve<IStatusService>();
            var list = service.GetServiceList();
            html = html.Replace("${count}", string.Format("{0}/{1}", list.Count, list.Sum(p => p.Methods.Count)));

            foreach (var s in list)
            {
                sbUrl.Append(GetItemDocument(s));
            }

            html = html.Replace("${body}", sbUrl.ToString());
            return html;
        }

        /// <summary>
        /// 获取Item文档
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        private string GetItemDocument(IOC.ServiceInfo service)
        {
            var list = new List<string>();
            var index = 0;
            foreach (var p in service.Methods)
            {
                var template = itemTemplate;
                if (index == 0)
                {
                    var serviceName = string.Format("<b style=\"color:#336699;\">{0}</b> [<font color='red'>{1}</font>]", service.FullName, service.Methods.Count);
                    if (!string.IsNullOrEmpty(service.ServiceDescription))
                    {
                        serviceName += "<br/>【" + service.ServiceDescription + "】";
                    }
                    template = template.Replace("${service}", serviceName);
                    template = template.Replace("${rowspan}", service.Methods.Count.ToString());
                    index++;
                }
                else
                {
                    int start = template.IndexOf("<td");
                    int end = template.IndexOf("</td>") + 5;
                    template = template.Substring(0, start) + template.Substring(end);
                }

                var methodName = string.Format("{0} [<font color='red'>{1}</font>]", p.FullName, p.Parameters.Count);
                if (!string.IsNullOrEmpty(p.MethodDescription))
                {
                    methodName += "<br/>【" + p.MethodDescription + "】";
                }
                template = template.Replace("${method}", methodName);
                if (p.Parameters.Count > 0)
                {
                    template = template.Replace("${parameter}", GetMethodParameter(p.Parameters));
                }
                else
                {
                    template = template.Replace("${parameter}", string.Empty);
                }

                list.Add(template);
            }
            return string.Join("", list.ToArray());
        }

        private string GetMethodParameter(IList<IOC.ParameterInfo> parameters)
        {
            StringBuilder buider = new StringBuilder();

            foreach (var p in parameters)
            {
                buider.AppendFormat("&lt;{0} : {1}&gt;", p.Name, p.TypeFullName);
                buider.Append("<br/>");
            }

            return buider.ToString();
        }
    }
}