using System;
using System.Configuration;
using System.Web;
using System.Xml.Serialization;

namespace MySoft.Installer.Configuration
{
    /// <summary>
    /// Specifies the configuration settings in the Web.config for the Installer.
    /// </summary>
    [Serializable]
    [XmlRoot("installer")]
    public class InstallerConfiguration
    {
        /// <summary>
        /// GetConfig() returns an instance of the <b>StaticPageConfiguration</b> class with the values populated from
        /// the Web.config file.  It uses XML deserialization to convert the XML structure in Web.config into
        /// a <b>StaticPageConfiguration</b> instance.
        /// </summary>
        /// <returns>A <see cref="StaticPageConfiguration"/> instance.</returns>
        public static InstallerConfiguration GetConfig()
        {
            string key = "mysoft.framework/installer";
            InstallerConfiguration obj = CacheHelper.Get<InstallerConfiguration>(key);
            if (obj == null)
            {
                var tmp = ConfigurationManager.GetSection(key);
                obj = tmp as InstallerConfiguration;
                CacheHelper.Permanent(key, obj);
            }

            return obj;
        }

        public InstallerConfiguration()
        {
            this.AutoRun = true;
            this.Type = "network";
        }

        #region Public Properties

        /// <summary>
        /// 服务路径
        /// </summary>
        [XmlElement("servicePath")]
        public string ServicePath { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        [XmlElement("serviceName")]
        public string ServiceName { get; set; }

        /// <summary>
        /// 服务显示名称
        /// </summary>
        [XmlElement("displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// 服务描述
        /// </summary>
        [XmlElement("description")]
        public string Description { get; set; }

        /// <summary>
        /// 帐户类型
        /// </summary>
        [XmlAttribute("type")]
        public string Type { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [XmlAttribute("userName")]
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [XmlAttribute("password")]
        public string Password { get; set; }

        /// <summary>
        /// 自动启动
        /// </summary>
        [XmlAttribute("autoRun")]
        public bool AutoRun { get; set; }

        #endregion
    }
}
