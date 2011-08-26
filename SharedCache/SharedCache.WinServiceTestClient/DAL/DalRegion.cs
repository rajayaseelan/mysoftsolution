#region Copyright (c) Roni Schuetz - All Rights Reserved
// * --------------------------------------------------------------------- *
// *                              Roni Schuetz                             *
// *              Copyright (c) 2008 All Rights reserved                   *
// *                                                                       *
// * Shared Cache high-performance, distributed caching and    *
// * replicated caching system, generic in nature, but intended to         *
// * speeding up dynamic web and / or win applications by alleviating      *
// * database load.                                                        *
// *                                                                       *
// * This Software is written by Roni Schuetz (schuetz AT gmail DOT com)   *
// *                                                                       *
// * This library is free software; you can redistribute it and/or         *
// * modify it under the terms of the GNU Lesser General Public License    *
// * as published by the Free Software Foundation; either version 2.1      *
// * of the License, or (at your option) any later version.                *
// *                                                                       *
// * This library is distributed in the hope that it will be useful,       *
// * but WITHOUT ANY WARRANTY; without even the implied warranty of        *
// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU      *
// * Lesser General Public License for more details.                       *
// *                                                                       *
// * You should have received a copy of the GNU Lesser General Public      *
// * License along with this library; if not, write to the Free            *
// * Software Foundation, Inc., 59 Temple Place, Suite 330,                *
// * Boston, MA 02111-1307 USA                                             *
// *                                                                       *
// *       THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.        *
// * --------------------------------------------------------------------- *
#endregion 

// *************************************************************************
//
// Name:      DalRegion.cs
// 
// Created:   01-01-2008 SharedCache.com, rschuetz
// Modified:  01-01-2008 SharedCache.com, rschuetz : Creation
// ************************************************************************* 

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace SharedCache.WinServiceTestClient.DAL
{
	/// <summary>
	/// Data Access Layer for Region Data
	/// </summary>
	public class DalRegion
	{
		#region xpath queries
		/// <summary>
		/// XPath query to retrieve all region elments
		/// </summary>
		private static string getAllRegion = @"/Region/ConRegion";
		/// <summary>
		/// XPath query to retrieve a specfice element based on name
		/// </summary>
		private static string getRegionByName = @"/Region/ConRegion[cName = '{0}']";
		/// <summary>
		/// XPath query to retrieve a specfice element based on region id
		/// </summary>
		private static string getRegionById = @"/Region/ConRegion[nRegionId = '{0}']";
		/// <summary>
		/// XPath query to retrieve a specfice element based on related country id
		/// </summary>
		private static string getRegionByCountryId = @"/Region/ConRegion[nCountryId = '{0}']";
		/// <summary>
		/// XPath query to retrieve a specfice element based on region Iso 2 code
		/// </summary>
		private static string getRegionByIso2 = @"/Region/ConRegion[cRegionIso2 = '{0}']";
		#endregion xpath queries

		#region Public Methods
		/// <summary>
		/// Gets all regions.
		/// </summary>
		/// <returns>A list of <see cref="Common.Region"/> objects.</returns>
		public List<Common.Region> GetAllRegion()
		{
			List<Common.Region> result = new List<Common.Region>();
			XmlDocument doc = LoadDocument();
			if (doc != null)
			{
				foreach (XmlNode node in this.GetNodeList(doc, getAllRegion))
				{
					XmlNodeList list = node.ChildNodes;
					result.Add(MapData(list));
				}
			}
			return result;
		}

		/// <summary>
		/// Gets the node by the region name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>A list of <see cref="Common.Region"/> objects.</returns>
		public List<Common.Region> GetRegionByName(string name)
		{
			List<Common.Region> result = new List<Common.Region>();
			XmlDocument doc = LoadDocument();
			if (doc != null)
			{
				foreach (XmlNode node in this.GetNodeList(doc, string.Format(getRegionByName, name)))
				{
					XmlNodeList list = node.ChildNodes;
					result.Add(MapData(list));
				}
			}
			return result;
		}

		/// <summary>
		/// Gets the node by the region id.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns>A list of <see cref="Common.Region"/> objects.</returns>
		public List<Common.Region> GetRegionById(int id)
		{
			List<Common.Region> result = new List<Common.Region>();
			XmlDocument doc = LoadDocument();
			if (doc != null)
			{
				foreach (XmlNode node in this.GetNodeList(doc, string.Format(getRegionById, id)))
				{
					XmlNodeList list = node.ChildNodes;
					result.Add(MapData(list));
				}
			}
			return result;
		}

		/// <summary>
		/// Gets the node by the region country reference id.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns>A list of <see cref="Common.Region"/> objects.</returns>
		public List<Common.Region> GetRegionByCountryId(int id)
		{
			List<Common.Region> result = new List<Common.Region>();
			XmlDocument doc = LoadDocument();
			if (doc != null)
			{
				foreach (XmlNode node in this.GetNodeList(doc, string.Format(getRegionByCountryId, id)))
				{
					XmlNodeList list = node.ChildNodes;
					result.Add(MapData(list));
				}
			}
			return result;
		}

		/// <summary>
		/// Gets the node by the region Iso 2 code.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>A list of <see cref="Common.Region"/> objects.</returns>
		public List<Common.Region> GetRegionByIso2(string name)
		{
			List<Common.Region> result = new List<Common.Region>();
			XmlDocument doc = LoadDocument();
			if (doc != null)
			{
				foreach (XmlNode node in this.GetNodeList(doc, string.Format(getRegionByIso2, name)))
				{
					XmlNodeList list = node.ChildNodes;
					result.Add(MapData(list));
				}
			}
			return result;
		}
		#endregion Public Methods

		#region Private Methods

		/// <summary>
		/// Mapping Xml Data which has been retrieved by an xpath expression.
		/// </summary>
		/// <param name="list">The list.</param>
		/// <returns>a mapped item of type: <see cref="Common.Region"/></returns>
		private Common.Region MapData(XmlNodeList list)
		{
			Common.Region item = new Common.Region();
			foreach (XmlNode n in list)
			{
				#region Map Data
				switch (n.Name)
				{
					case "nRegionId":
						item.RegionId = int.Parse(n.InnerText);
						break;
					case "nCountryId":
						item.CountryId = int.Parse(n.InnerText);
						break;
					case "cRegionIso2":
						item.RegionIso2Code = n.InnerText;
						break;
					case "cName":
						item.Name = n.InnerText;
						break;
				}
				#endregion Map Data
			}
			return item;
		}

		/// <summary>
		/// Gets the node list.
		/// </summary>
		/// <param name="doc">The doc.</param>
		/// <param name="query">The query.</param>
		/// <returns>a <see cref="XmlNodeList"/> list</returns>
		private XmlNodeList GetNodeList(XmlDocument doc, string query)
		{
			return doc.SelectNodes(query);
		}

		/// <summary>
		/// Loading specific document.
		/// </summary>
		/// <returns>An document of type <see cref="XmlDocument"/></returns>
		private XmlDocument LoadDocument()
		{
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(Common.Util.GetRegionPath());
				return doc;
			}
			catch (Exception ex)
			{
				Console.WriteLine(@"A problem appears to load the region file from following location: " + Environment.NewLine + " -> " + Common.Util.GetRegionPath());
			}
			return null;
		}
		#endregion Private Methods
	}
}
