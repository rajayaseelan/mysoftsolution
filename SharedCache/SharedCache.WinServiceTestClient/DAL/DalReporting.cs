using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Xml.Serialization;
using System.Xml.Serialization.Advanced;
using System.Xml.Serialization.Configuration;
using System.IO;

namespace SharedCache.WinServiceTestClient.DAL
{
	public class DalReporting
	{

		/// <summary>
		/// Deserializes an xml document back into an object
		/// </summary>
		/// <param name="xml">The xml data to deserialize</param>
		/// <param name="type">The type of the object being deserialized</param>
		/// <returns>A deserialized object</returns>
		public static object Deserialize(XmlDocument xml, Type type)
		{
			XmlSerializer s = new XmlSerializer(type);
			string xmlString = xml.OuterXml.ToString();
			byte[] buffer = ASCIIEncoding.UTF8.GetBytes(xmlString);
			MemoryStream ms = new MemoryStream(buffer);
			XmlReader reader = new XmlTextReader(ms);
			Exception caught = null;

			try
			{
				object o = s.Deserialize(reader);
				return o;
			}

			catch (Exception e)
			{
				caught = e;
			}
			finally
			{
				reader.Close();

				if (caught != null)
					throw caught;
			}
			return null;
		}

		/// <summary>
		/// Serializes an object into an Xml Document
		/// </summary>
		/// <param name="o">The object to serialize</param>
		/// <returns>An Xml Document consisting of said object's data</returns>
		public static XmlDocument Serialize(object o)
		{
			XmlSerializer s = new XmlSerializer(o.GetType());

			MemoryStream ms = new MemoryStream();
			XmlTextWriter writer = new XmlTextWriter(ms, new UTF8Encoding());
			writer.Formatting = Formatting.Indented;
			writer.IndentChar = ' ';
			writer.Indentation = 5;
			Exception caught = null;

			try
			{
				s.Serialize(writer, o);
				XmlDocument xml = new XmlDocument();
				string xmlString = ASCIIEncoding.UTF8.GetString(ms.ToArray());
				xml.LoadXml(xmlString);
				return xml;
			}
			catch (Exception e)
			{
				caught = e;
			}
			finally
			{
				writer.Close();
				ms.Close();

				if (caught != null)
					throw caught;
			}
			return null;
		}

		private static XmlDocument SaveDocument(XmlDocument doc)
		{
			try
			{
				doc.Save(Common.Util.GetReportingPath());				
			}
			catch (Exception ex)
			{
				Console.WriteLine(@"A problem appears to save the reporting file from following location: " + Environment.NewLine + " -> " + Common.Util.GetRegionPath());
			}
			return null;
		}

		private static XmlDocument LoadDocument()
		{
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(Common.Util.GetReportingPath());
				return doc;
			}
			catch (Exception ex)
			{
				Console.WriteLine(@"A problem appears to load the reporting file from following location: " + Environment.NewLine + " -> " + Common.Util.GetRegionPath());
			}
			return null;
		}

		internal static void Save(SharedCache.WinServiceTestClient.Common.Reporting report)
		{
			XmlDocument doc = LoadDocument();
			Common.Report re = (Common.Report)Deserialize(doc, typeof(Common.Report));
			re.List.Add(report);

			SaveDocument(Serialize(re));			
		}
	}
}
