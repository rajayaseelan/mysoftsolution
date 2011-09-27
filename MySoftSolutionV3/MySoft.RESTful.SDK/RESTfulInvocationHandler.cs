using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace MySoft.RESTful.SDK
{
    /// <summary>
    /// RESTful代理服务
    /// </summary>
    public class RESTfulInvocationHandler : IProxyInvocationHandler
    {
        private DataFormat format;
        private Token token;
        private string url;
        private int timeout;
        private PublishKindAttribute attribute;

        /// <summary>
        /// 实例化RESTfulInvocationHandler
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="token"></param>
        /// <param name="foramt"></param>
        public RESTfulInvocationHandler(string url, PublishKindAttribute attribute, Token token, DataFormat foramt, int timeout)
        {
            this.format = foramt;
            this.attribute = attribute;
            this.token = token;
            this.url = url;
            this.timeout = timeout;
        }

        #region IProxyInvocationHandler 成员

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object Invoke(object proxy, MethodInfo method, object[] parameters)
        {
            var attr = CoreHelper.GetMemberAttribute<PublishMethodAttribute>(method);
            if (attr == null) return null;

            string name = attribute.Name + "." + attr.Name;
            RESTfulParameter parameter = new RESTfulParameter(name, attr.Method, format);
            parameter.Token = token;

            if (attr.Method == HttpMethod.GET)
            {
                //添加参数
                var plist = method.GetParameters();
                for (int index = 0; index < parameters.Length; index++)
                {
                    parameter.AddParameter(plist[index].Name, parameters[index]);
                }
            }
            else
            {
                var collection = new Dictionary<string, object>();

                //添加参数
                var plist = method.GetParameters();
                for (int index = 0; index < parameters.Length; index++)
                {
                    collection[plist[index].Name] = parameters[index];
                }

                parameter.DataObject = collection;
            }

            RESTfulRequest request = new RESTfulRequest(parameter);
            if (!string.IsNullOrEmpty(url)) request.Url = url;
            request.Timeout = timeout;

            return request.GetResponse(method.ReturnType);
        }

        #endregion
    }
}
