using System.Configuration;

namespace MySoft.IoC.Configuration
{
    /// <summary>
    /// 服务配置类
    /// </summary>
    public class CastleFactoryConfigurationHandler : IConfigurationSectionHandler
    {
        #region IConfigurationSectionHandler 成员

        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            CastleFactoryConfiguration config = new CastleFactoryConfiguration();
            config.LoadValuesFromConfigurationXml(section);
            return config;
        }

        #endregion
    }
}
