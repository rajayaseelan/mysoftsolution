using System;
using System.Collections.Generic;
using System.Linq;
using MySoft.RESTful.Utils;

namespace MySoft.RESTful.Business.Pool
{
    /// <summary>
    /// 默认业务池
    /// </summary>
    public class DefaultBusinessPool : IBusinessPool
    {
        private IDictionary<string, BusinessKindModel> businessPool;

        /// <summary>
        /// 获取业务池对象
        /// </summary>
        public IDictionary<string, BusinessKindModel> KindMethods
        {
            get { return businessPool; }
        }

        /// <summary>
        /// 实例化DefaultBusinessPool
        /// </summary>
        public DefaultBusinessPool()
        {
            businessPool = new Dictionary<string, BusinessKindModel>();
        }

        /// <summary>
        /// 获取业务模型
        /// </summary>
        /// <param name="businessKindName"></param>
        /// <returns></returns>
        public BusinessKindModel GetKindModel(string businessKindName)
        {
            BusinessKindModel model = businessPool.Where(e => e.Key.Equals(businessKindName, StringComparison.OrdinalIgnoreCase)).Select(v => v.Value).SingleOrDefault();
            return model;
        }

        /// <summary>
        /// 添加业务模型
        /// </summary>
        /// <param name="businessKindName"></param>
        /// <param name="businessKindModel"></param>
        /// <returns></returns>
        public void AddKindModel(string businessKindName, BusinessKindModel businessKindModel)
        {
            BusinessKindModel model = businessPool.Where(e => e.Key.Equals(businessKindName, StringComparison.OrdinalIgnoreCase)).Select(v => v.Value).SingleOrDefault();
            if (model == null)
            {
                businessPool.Add(businessKindName, businessKindModel);
            }
        }

        /// <summary>
        /// 移除业务模型
        /// </summary>
        /// <param name="businessKindName"></param>
        /// <returns></returns>
        public BusinessKindModel RemoveKindModel(string businessKindName)
        {
            BusinessKindModel model = businessPool.Where(e => e.Key.Equals(businessKindName, StringComparison.OrdinalIgnoreCase)).Select(v => v.Value).SingleOrDefault();
            if (model != null)
            {
                businessPool.Remove(businessKindName);
            }
            return model;
        }

        /// <summary>
        /// 移除业务模型
        /// </summary>
        /// <param name="businessKindName"></param>
        /// <param name="businessMethodName"></param>
        /// <returns></returns>
        public void RemoveMethodModel(string businessKindName, string businessMethodName)
        {
            BusinessKindModel model = businessPool.Where(e => e.Key.Equals(businessKindName, StringComparison.OrdinalIgnoreCase)).Select(v => v.Value).SingleOrDefault();
            if (model != null)
            {
                model.MethodModels.Remove(businessMethodName);
            }
        }

        /// <summary>
        /// 检查方法
        /// </summary>
        /// <param name="parameterFormat"></param>
        /// <param name="businessKindName"></param>
        /// <param name="businessMethodName"></param>
        /// <returns></returns>
        public bool CheckAuthorized(ParameterFormat parameterFormat, string businessKindName, string businessMethodName)
        {
            BusinessKindModel kind = businessPool.Where(e => e.Key.Equals(businessKindName, StringComparison.OrdinalIgnoreCase)).Select(v => v.Value).SingleOrDefault();
            if (kind != null)
            {
                BusinessMethodModel method = kind.MethodModels.Where(e => e.Key.Equals(businessMethodName, StringComparison.OrdinalIgnoreCase)).Select(v => v.Value).SingleOrDefault();
                if (method != null)
                {
                    if (method.HttpMethod == HttpMethod.GET)
                        return method.Authorized;
                    else
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 查找方法
        /// </summary>
        /// <param name="businessKindName"></param>
        /// <param name="businessMethodName"></param>
        /// <returns></returns>
        public BusinessMethodModel FindMethod(string businessKindName, string businessMethodName)
        {
            bool hasException = false;
            string msg = string.Empty;
            RESTfulCode code = RESTfulCode.OK;
            BusinessKindModel kind = businessPool.Where(e => e.Key.Equals(businessKindName, StringComparison.OrdinalIgnoreCase)).Select(v => v.Value).SingleOrDefault();
            BusinessMethodModel method = null;
            if (kind == null)
            {
                hasException = true;
                msg = businessKindName + ", did not found!";
                code = RESTfulCode.BUSINESS_KIND_NOT_FOUND;
            }
            else
            {
                if (kind.State == BusinessState.ACTIVATED)
                {
                    method = kind.MethodModels.Where(e => e.Key.Equals(businessMethodName, StringComparison.OrdinalIgnoreCase)).Select(v => v.Value).SingleOrDefault();
                    if (method == null)
                    {
                        hasException = true;
                        msg = businessMethodName + ", did not found!";
                        code = RESTfulCode.BUSINESS_METHOD_NOT_FOUND;
                    }
                    else
                    {
                        if (method.State != BusinessState.ACTIVATED)
                        {
                            hasException = true;
                            msg = businessMethodName + ", did not Activeted!";
                            code = RESTfulCode.BUSINESS_KIND_NO_ACTIVATED;
                        }
                    }
                }
                else
                {
                    hasException = true;
                    msg = businessKindName + ", did not Activeted!";
                    code = RESTfulCode.BUSINESS_KIND_NO_ACTIVATED;
                }
            }
            if (hasException)
            {
                throw new RESTfulException(msg) { Code = code };
            }

            return method;
        }
    }
}
