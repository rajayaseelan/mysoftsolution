using MySoft.IoC.HttpServer.Config;
using MySoft.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MySoft.IoC.HttpServer
{
    /// <summary>
    /// 默认的Api解析器
    /// </summary>
    public class DefaultApiResolver : IHttpApiResolver
    {
        #region IHttpApiResolver 成员

        /// <summary>
        /// 将服务解析成Http接口方法
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public virtual IList<HttpApiMethod> MethodResolver(Type interfaceType)
        {
            IList<HttpApiMethod> list = new List<HttpApiMethod>();
            var fileName = CoreHelper.GetFullPath("/config/httpapi.config");

            if (File.Exists(fileName))
            {
                try
                {
                    var xml = File.ReadAllText(fileName, Encoding.UTF8);
                    if (!string.IsNullOrEmpty(xml))
                    {
                        var config = SerializationManager.DeserializeXml<HttpApiConfig>(xml);
                        if (config != null && config.ApiServices != null)
                        {
                            var items = config.ApiServices.Where(p => !string.IsNullOrEmpty(p.FullName)).ToList();
                            var item = items.FirstOrDefault(p => string.Compare(p.FullName, interfaceType.FullName, true) == 0);

                            //判断指定的服务是否在配置文件列表中
                            if (item != null && item.ApiItems != null)
                            {
                                list = ReadFromType(interfaceType, item);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //写错误日志
                    SimpleLog.Instance.WriteLogForDir("httpapi", ex);
                }
            }

            return list;
        }

        #endregion

        /// <summary>
        /// 从类型中读取
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="apiService"></param>
        /// <returns></returns>
        private IList<HttpApiMethod> ReadFromType(Type interfaceType, HttpApiService apiService)
        {
            var list = new List<HttpApiMethod>();

            var serviceName = apiService.Name;
            var description = apiService.Description;
            if (string.IsNullOrEmpty(serviceName))
            {
                serviceName = interfaceType.Name.ToLower();
                if (serviceName.EndsWith("service"))
                    serviceName = serviceName.Substring(1, serviceName.LastIndexOf("service") - 1);
                else
                    serviceName = serviceName.Substring(1);
            }

            //获取类型中的方法信息
            foreach (var method in CoreHelper.GetMethodsFromType(interfaceType))
            {
                var items = apiService.ApiItems.Where(p => !string.IsNullOrEmpty(p.FullName)).ToList();
                var item = items.FirstOrDefault(p => string.Compare(p.FullName, method.ToString(), true) == 0);
                if (item == null)
                {
                    //跟方法名称比较
                    item = apiService.ApiItems.FirstOrDefault(p => string.Compare(p.FullName, method.Name, true) == 0);
                }

                //判断指定的服务是否在配置文件列表中
                if (item != null)
                {
                    var methodName = item.Name;
                    if (string.IsNullOrEmpty(methodName))
                    {
                        methodName = method.Name.ToLower();
                    }

                    //判断认证参数是否存在
                    var exists = method.GetParameters().Any(p => string.Compare(p.Name, item.AuthParameter, true) == 0);

                    //实例化HttpApi方法
                    var httpApi = new HttpApiMethod(method)
                    {
                        Name = string.Format("{0}.{1}", serviceName, methodName),
                        Description = description == null ? item.Description : string.Format("【{0}】{1}", description, item.Description),
                        Authorized = exists ? item.Authorized : false,
                        AuthParameter = exists ? item.AuthParameter : null,
                        CacheTime = item.CacheTime,
                        HttpMethod = item.HttpMethod,
                    };

                    list.Add(httpApi);
                }
            }

            return list;
        }
    }
}