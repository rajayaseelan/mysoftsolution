using System;
using System.Configuration;
using System.Web;
using System.Xml.Serialization;

namespace MySoft.RESTful.Configuration
{
    /// <summary>
    /// Specifies the configuration settings in the Web.config for the Auth.
    /// </summary>
    [Serializable]
    [XmlRoot("restful")]
    public class RESTfulConfiguration
    {
        private AuthenticationCollection auths;

        /// <summary>
        /// GetConfig() returns an instance of the <b>StaticPageConfiguration</b> class with the values populated from
        /// the Web.config file.  It uses XML deserialization to convert the XML structure in Web.config into
        /// a <b>StaticPageConfiguration</b> instance.
        /// </summary>
        /// <returns>A <see cref="StaticPageConfiguration"/> instance.</returns>
        public static RESTfulConfiguration GetConfig()
        {
            string key = "mysoft.framework/restful";
            RESTfulConfiguration obj = CacheHelper.Get<RESTfulConfiguration>(key);
            if (obj == null)
            {
                var tmp = ConfigurationManager.GetSection(key);
                obj = tmp as RESTfulConfiguration;
                CacheHelper.Insert(key, obj, 60);
            }

            return obj;
        }

        #region Public Properties

        /// <summary>
        /// A <see cref="AuthenticationCollection"/> instance that provides access to a set of <see cref="StaticPageRule"/>s.
        /// </summary>
        [XmlArray("auths")]
        [XmlArrayItem("auth", typeof(Authentication))]
        public AuthenticationCollection Auths
        {
            get
            {
                return auths;
            }
            set
            {
                auths = value;
            }
        }

        #endregion
    }
}
