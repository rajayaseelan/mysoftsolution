using System.Collections.Generic;
using System.Configuration;
using System;
using System.Xml.Serialization;

namespace MySoft.Web.Configuration
{
    /// <summary>
    /// 控件缓存配置信息
    /// </summary>
    [Serializable]
    [XmlRoot("cacheControl")]
    public class CacheControlConfiguration
    {
        private bool enabled = true;
        // private member variables
        private CacheControlRuleCollection rules;			// an instance of the StaticPageRuleCollection class...

        /// <summary>
        /// GetConfig() returns an instance of the <b>StaticPageConfiguration</b> class with the values populated from
        /// the Web.config file.  It uses XML deserialization to convert the XML structure in Web.config into
        /// a <b>StaticPageConfiguration</b> instance.
        /// </summary>
        /// <returns>A <see cref="StaticPageConfiguration"/> instance.</returns>
        public static CacheControlConfiguration GetConfig()
        {
            string key = "mysoft.framework/cacheControl";
            CacheControlConfiguration obj = CacheHelper.Get<CacheControlConfiguration>(key);
            if (obj == null)
            {
                var tmp = ConfigurationManager.GetSection(key);
                obj = tmp as CacheControlConfiguration;
                CacheHelper.Permanent(key, obj);
            }

            return obj;
        }

        #region Public Properties

        /// <summary>
        /// 是否启用配置
        /// </summary>
        [XmlAttribute("enabled")]
        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }

        /// <summary>
        /// A <see cref="StaticPageRuleCollection"/> instance that provides access to a set of <see cref="StaticPageRule"/>s.
        /// </summary>
        [XmlArray("rules")]
        [XmlArrayItem("rule", typeof(CacheControlRule))]
        public CacheControlRuleCollection Rules
        {
            get
            {
                return rules;
            }
            set
            {
                rules = value;
            }
        }

        #endregion
    }
}
