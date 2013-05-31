using System;
using System.Collections.Generic;
using System.Linq;
using MySoft.RESTful.Utils;
using System.Net;

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
        /// 查找方法
        /// </summary>
        /// <param name="businessKindName"></param>
        /// <param name="businessMethodName"></param>
        /// <returns></returns>
        public BusinessMethodModel FindMethod(string businessKindName, string businessMethodName)
        {
            BusinessKindModel kind = businessPool.Where(e => e.Key.Equals(businessKindName, StringComparison.OrdinalIgnoreCase)).Select(v => v.Value).SingleOrDefault();
            BusinessMethodModel method = null;
            if (kind == null)
            {
                throw new RESTfulException(HttpStatusCode.NotFound, businessKindName + ", did not found!");
            }
            else
            {
                method = kind.MethodModels.Where(e => e.Key.Equals(businessMethodName, StringComparison.OrdinalIgnoreCase)).Select(v => v.Value).SingleOrDefault();
                if (method == null)
                {
                    throw new RESTfulException(HttpStatusCode.NotFound, businessMethodName + ", did not found!");
                }
            }

            return method;
        }
    }
}
