using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC
{
    [Serializable]
    public class ServiceCache
    {
        private object cacheObject;
        /// <summary>
        /// 缓存对象
        /// </summary>
        public object CacheObject
        {
            get { return cacheObject; }
            set { cacheObject = value; }
        }

        private ParameterCollection parameters;
        /// <summary>
        /// 参数信息
        /// </summary>
        public ParameterCollection Parameters
        {
            get
            {
                return parameters;
            }
            set
            {
                parameters = value;
            }
        }
    }
}
