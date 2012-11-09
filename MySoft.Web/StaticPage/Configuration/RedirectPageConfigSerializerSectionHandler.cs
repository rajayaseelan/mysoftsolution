using System;
using System.Configuration;
using System.Xml;
using System.Xml.Serialization;

namespace MySoft.Web.Configuration
{
    /// <summary>
    /// Deserializes the markup in Web.config into an instance of the <see cref="RedirectPageConfiguration"/> class.
    /// </summary>
    [Serializable]
    public class RedirectPageConfigSerializerSectionHandler : IConfigurationSectionHandler
    {
        /// <summary>
        /// Creates an instance of the <see cref="RedirectPageConfiguration"/> class.
        /// </summary>
        /// <remarks>Uses XML Serialization to deserialize the XML in the Web.config file into an
        /// <see cref="RedirectPageConfiguration"/> instance.</remarks>
        /// <returns>An instance of the <see cref="RedirectPageConfiguration"/> class.</returns>
        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            // Create an instance of XmlSerializer based on the RedirectPageConfiguration type...
            XmlSerializer ser = new XmlSerializer(typeof(RedirectPageConfiguration));

            // Return the Deserialized object from the Web.config XML
            return ser.Deserialize(new XmlNodeReader(section));
        }

    }
}
