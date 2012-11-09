using System;
using System.Configuration;
using System.Web;
using System.Xml.Serialization;

namespace MySoft.Web.Configuration
{
    /// <summary>
    /// Specifies the configuration settings in the Web.config for the RedirectPageRule.
    /// </summary>
    [Serializable]
    [XmlRoot("RedirectPageConfig")]
    public class RedirectPageConfiguration
    {
        // private member variables
        private RedirectPageRuleCollection rules;			// an instance of the RedirectPageRuleCollection class...

        /// <summary>
        /// GetConfig() returns an instance of the <b>RedirectPageConfiguration</b> class with the values populated from
        /// the Web.config file.  It uses XML deserialization to convert the XML structure in Web.config into
        /// a <b>RedirectPageConfiguration</b> instance.
        /// </summary>
        /// <returns>A <see cref="RedirectPageConfiguration"/> instance.</returns>
        public static RedirectPageConfiguration GetConfig()
        {
            if (HttpContext.Current.Cache["RedirectPageConfig"] == null)
                HttpContext.Current.Cache.Insert("RedirectPageConfig", ConfigurationManager.GetSection("RedirectPageConfig"));

            return (RedirectPageConfiguration)HttpContext.Current.Cache["RedirectPageConfig"];
        }

        #region Public Properties
        /// <summary>
        /// A <see cref="RedirectPageRuleCollection"/> instance that provides access to a set of <see cref="RedirectPageRule"/>s.
        /// </summary>
        public RedirectPageRuleCollection Rules
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
