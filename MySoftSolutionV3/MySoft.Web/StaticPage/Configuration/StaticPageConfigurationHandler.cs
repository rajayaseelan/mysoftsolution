using System;
using System.Configuration;
using System.Xml;
using System.Xml.Serialization;

namespace MySoft.Web.Configuration
{
    /// <summary>
    /// Deserializes the markup in Web.config into an instance of the <see cref="StaticPageConfiguration"/> class.
    /// </summary>
    public class StaticPageConfigurationHandler : IConfigurationSectionHandler
    {
        /// <summary>
        /// Creates an instance of the <see cref="StaticPageConfiguration"/> class.
        /// </summary>
        /// <remarks>Uses XML Serialization to deserialize the XML in the Web.config file into an
        /// <see cref="StaticPageConfiguration"/> instance.</remarks>
        /// <returns>An instance of the <see cref="StaticPageConfiguration"/> class.</returns>
        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            // Create an instance of XmlSerializer based on the StaticPageConfiguration type...
            XmlSerializer ser = new XmlSerializer(typeof(StaticPageConfiguration));

            // Return the Deserialized object from the Web.config XML
            return ser.Deserialize(new XmlNodeReader(section));
        }

    }
}
