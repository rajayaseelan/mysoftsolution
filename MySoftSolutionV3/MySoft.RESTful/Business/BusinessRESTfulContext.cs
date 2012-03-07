using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel.Web;
using System.Text;
using MySoft.RESTful.Business.Pool;
using MySoft.RESTful.Business.Register;
using MySoft.RESTful.Utils;
using Newtonsoft.Json.Linq;

namespace MySoft.RESTful.Business
{
    /// <summary>
    /// 默认服务上下文
    /// </summary>
    public class BusinessRESTfulContext : IRESTfulContext
    {
        #region IRESTfulContext 成员

        /// <summary>
        /// 业务池
        /// </summary>
        private IBusinessPool pool;

        /// <summary>
        /// 业务注册
        /// </summary>
        private IBusinessRegister register;

        /// <summary>
        /// 实例化BusinessRESTfulContext
        /// </summary>
        public BusinessRESTfulContext()
            : this(new DefaultBusinessPool(), new NativeBusinessRegister()) { }

        /// <summary>
        /// 实例化BusinessRESTfulContext
        /// </summary>
        /// <param name="pool"></param>
        public BusinessRESTfulContext(IBusinessPool pool)
            : this(pool, new NativeBusinessRegister()) { }

        /// <summary>
        /// 实例化BusinessRESTfulContext
        /// </summary>
        /// <param name="register"></param>
        public BusinessRESTfulContext(IBusinessRegister register)
            : this(new DefaultBusinessPool(), register) { }

        /// <summary>
        /// 实例化BusinessRESTfulContext
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="register"></param>
        public BusinessRESTfulContext(IBusinessPool pool, IBusinessRegister register)
        {
            this.pool = pool;
            this.register = register;
            Init();
        }

        private void Init()
        {
            register.Register(pool);
        }

        /// <summary>
        /// 是否需要认证
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public bool IsAuthorized(string kind, string method)
        {
            return pool.CheckAuthorized(kind, method);
        }

        /// <summary>
        /// 方法调用
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object Invoke(string kind, string method, string parameters, out Type retType)
        {
            WebOperationContext context = WebOperationContext.Current;
            JObject obj = new JObject();
            BusinessMethodModel metadata = pool.FindMethod(kind, method);

            //返回类型
            retType = metadata.Method.ReturnType;

            try
            {
                if (metadata.HttpMethod == HttpMethod.POST && context.IncomingRequest.Method.ToUpper() == "GET")
                {
                    throw new RESTfulException((int)HttpStatusCode.MethodNotAllowed, "Resources can only by the [" + metadata.HttpMethod + "] way to acquire!");
                }

                if (!string.IsNullOrEmpty(parameters))
                {
                    obj = JObject.Parse(parameters);
                }

                //解析QueryString
                var nvs = context.IncomingRequest.UriTemplateMatch.QueryParameters;
                if (nvs.Count > 0)
                {
                    var jo = ParameterHelper.Resolve(nvs);
                    foreach (var o in jo) obj[o.Key] = o.Value;
                }
            }
            catch (RESTfulException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new RESTfulException((int)HttpStatusCode.BadRequest, string.Format("Fault parameters: {0}!", parameters));
            }

            //实例对象
            object instance = null;

            try
            {
                object[] arguments = ParameterHelper.Convert(metadata.Parameters, obj);
                instance = register.Resolve(metadata.Service);
                return DynamicCalls.GetMethodInvoker(metadata.Method)(instance, arguments);
            }
            finally
            {
                register.Release(instance);
            }
        }

        #endregion

        /// <summary>
        /// 生成API文档
        /// </summary>
        /// <returns></returns>
        public string MakeDocument(Uri requestUri, string kind, string method)
        {
            #region 读取资源

            Assembly assm = this.GetType().Assembly;
            Stream helpStream = assm.GetManifestResourceStream("MySoft.RESTful.Template.help.htm");
            Stream helpitemStream = assm.GetManifestResourceStream("MySoft.RESTful.Template.helpitem.htm");

            StreamReader helpReader = new StreamReader(helpStream);
            StreamReader helpitemReader = new StreamReader(helpitemStream);

            string html = helpReader.ReadToEnd(); helpReader.Close();
            string item = helpitemReader.ReadToEnd(); helpitemReader.Close();

            #endregion

            string uri = string.Format("http://{0}/", requestUri.Authority.Replace("127.0.0.1", DnsHelper.GetIPAddress()));
            html = html.Replace("${uri}", uri);

            var sb = new StringBuilder();
            foreach (BusinessKindModel e in pool.KindMethods.Values.OrderBy(p => p.Name).ToList())
            {
                sb.AppendFormat("<a href='/help/{0}' title='{1}'>{0}</a>", e.Name, e.Description);
                sb.AppendFormat("&nbsp;&nbsp;");
            }

            html = html.Replace("${kind}", sb.ToString());

            StringBuilder table = new StringBuilder();
            List<BusinessKindModel> list = new List<BusinessKindModel>();
            if (string.IsNullOrEmpty(kind))
            {
                list.AddRange(pool.KindMethods.Values);
            }
            else
            {
                var model = pool.GetKindModel(kind);
                if (model != null)
                {
                    if (string.IsNullOrEmpty(method))
                    {
                        list.Add(model);
                    }
                    else
                    {
                        var m = model.MethodModels.Values.Where(p => string.Compare(p.Name, method, true) == 0).FirstOrDefault();
                        if (m != null)
                        {
                            var mod = new BusinessKindModel
                            {
                                Name = model.Name,
                                Description = model.Description
                            };
                            mod.MethodModels.Add(m.Name, m);
                            list.Add(mod);
                        }
                        else
                        {
                            table.Append("<tr><td colspan=\"5\" style=\"padding: 30px 300px 30px 300px;\">没有匹配到指定方法的服务！</td></tr>");
                        }
                    }
                }
                else
                {
                    table.Append("<tr><td colspan=\"5\" style=\"padding: 30px 300px 30px 300px;\">没有匹配到指定类型的服务！</td></tr>");
                }
            }

            var kinds = list.OrderBy(p => p.Name).ToList();
            foreach (BusinessKindModel e in kinds)
            {
                StringBuilder items = new StringBuilder();
                var methods = e.MethodModels.Values.OrderBy(p => p.Name).ToList();
                foreach (BusinessMethodModel model in methods)
                {
                    string template = item;
                    var tempStr = string.Format("<a href='/help/{0}/{1}'>{0}.{2}</a><br/>{3}", e.Name, model.Name, model.Name, model.Description);
                    template = template.Replace("${method}", tempStr);

                    var plist = new List<string>();
                    foreach (var p in model.Parameters)
                    {
                        if (CoreHelper.CheckPrimitiveType(p.ParameterType))
                        {
                            plist.Add(string.Format("{0}=[{0}]", p.Name.ToLower()).Replace('[', '{').Replace(']', '}'));
                        }
                    }

                    string strParameter = GetMethodParameter(model, kind, method);
                    if (string.IsNullOrEmpty(strParameter))
                        template = template.Replace("${parameter}", "&nbsp;");
                    else
                        template = template.Replace("${parameter}", strParameter);

                    template = template.Replace("${type}", model.HttpMethod == HttpMethod.GET ? "GET<br/>POST" : "<font color='red'>POST</font>");
                    template = template.Replace("${auth}", model.Authorized ? "<font color='red'>是</font>" : "&nbsp;");

                    StringBuilder anchor = new StringBuilder();
                    anchor.Append(CreateAnchorHtml(requestUri, e, model, plist, model.HttpMethod, "xml"));
                    anchor.Append("<br/>");
                    anchor.Append(CreateAnchorHtml(requestUri, e, model, plist, model.HttpMethod, "json"));
                    if (model.HttpMethod == HttpMethod.GET)
                    {
                        anchor.Append("<br/>");
                        anchor.Append(CreateAnchorHtml(requestUri, e, model, plist, model.HttpMethod, "jsonp"));

                        if (model.Method.ReturnType == typeof(string))
                        {
                            anchor.Append("<br/>");
                            anchor.Append(CreateAnchorHtml(requestUri, e, model, plist, model.HttpMethod, "text"));
                            anchor.Append("<br/>");
                            anchor.Append(CreateAnchorHtml(requestUri, e, model, plist, model.HttpMethod, "html"));
                        }
                    }

                    template = template.Replace("${uri}", anchor.ToString());
                    items.Append(template);
                }

                table.Append(items.ToString());
            }

            return html.Replace("${body}", table.ToString());
        }

        private string GetMethodParameter(BusinessMethodModel model, string kind, string method)
        {
            StringBuilder buider = new StringBuilder();
            var parametersCount = model.ParametersCount;
            if (parametersCount > 0) buider.Append("<b>INPUT</b> -><br/>");
            foreach (var p in model.Parameters)
            {
                if (!string.IsNullOrEmpty(kind) && !string.IsNullOrEmpty(method))
                    buider.Append(GetTypeDetail(p.Name, p.ParameterType, 1));
                else
                    buider.AppendFormat(string.Format("&nbsp;&nbsp;&nbsp;&nbsp;&lt;{0} : {1}&gt;", p.Name, GetTypeName(p.ParameterType)) + "<br/>");
            }
            if (parametersCount > 0) buider.Append("<hr/>");
            var value = string.Format("<b>OUTPUT</b> -> {0}<br/>", GetTypeName(model.Method.ReturnType));
            buider.Append("<font color=\"#336699\">").Append(value);
            if (!string.IsNullOrEmpty(kind) && !string.IsNullOrEmpty(method))
                buider.Append(GetTypeDetail(null, model.Method.ReturnType, 1));
            buider.Append("</font>");

            return buider.ToString();
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
            if (!CoreHelper.CheckPrimitiveType(type) || type.IsEnum)
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
                        if (!CoreHelper.CheckPrimitiveType(p.PropertyType) && type != p.PropertyType)
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
                        if (!CoreHelper.CheckPrimitiveType(p.FieldType))
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

        private string CreateAnchorHtml(Uri requestUri, BusinessKindModel e, BusinessMethodModel model, List<string> plist, HttpMethod mode, string format)
        {
            string url = string.Empty;
            string method = mode.ToString().ToLower();
            if (mode == HttpMethod.GET && plist.Count > 0)
                url = string.Format("/{0}.{1}/{2}.{3}?{4}", method, format, e.Name, model.Name, string.Join("&", plist.ToArray()));
            else
                url = string.Format("/{0}.{1}/{2}.{3}", method, format, e.Name, model.Name);

            if (!string.IsNullOrEmpty(requestUri.Query))
            {
                if (url.IndexOf('?') >= 0)
                    url += "&" + requestUri.Query.Substring(1);
                else
                    url += requestUri.Query;
            }

            if (format == "jsonp")
            {
                if (url.IndexOf('?') >= 0)
                    url += "&callback={callback}";
                else
                    url += "?callback={callback}";
            }

            return string.Format("<a rel=\"operation\" target=\"_blank\" title=\"{0}\" href=\"{0}\">{0}</a> 处的服务", url);
        }
    }
}