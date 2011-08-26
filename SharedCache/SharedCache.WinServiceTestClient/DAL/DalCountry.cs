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
// Name:      DalCountry.cs
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
	/// Data Access Layer for Country Data
	/// </summary>
	public class DalCountry
	{
		#region xpath queries
		/// <summary>
		/// XPath query to retrieve all country elments
		/// </summary>
		private static string getAllCountries = @"/Country/ConCountry";
		/// <summary>
		/// XPath query to retrieve an country node based on the name
		/// </summary>
		private static string getCountryByName = @"/Country/ConCountry[cName = '{0}']";
		/// <summary>
		/// XPath query to retrieve an country node based on country id
		/// </summary>
		private static string getCountryById = @"/Country/ConCountry[nCountryId = '{0}']";

		#endregion xpath queries

		#region Public Methods
		/// <summary>
		/// Gets all countries
		/// </summary>
		/// <returns></returns>
		public List<Common.Country> GetAllCountry()
		{
			List<Common.Country> result = new List<Common.Country>();
			XmlDocument doc = LoadDocument();
			if (doc != null)
			{
				foreach (XmlNode node in this.GetNodeList(doc, getAllCountries))
				{
					XmlNodeList list = node.ChildNodes;
					result.Add(MapData(list));
				}
			}			
			return result;
		}

		/// <summary>
		/// Gets country by.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public List<Common.Country> GetCountryByName(string name)
		{
			List<Common.Country> result = new List<Common.Country>();
			XmlDocument doc = LoadDocument();
			if (doc != null)
			{
				foreach (XmlNode node in this.GetNodeList(doc, string.Format(getCountryByName, name)))
				{
					XmlNodeList list = node.ChildNodes;
					result.Add(MapData(list));
				}
			}
			return result;
		}

		/// <summary>
		/// Gets the country by id.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns></returns>
		public List<Common.Country> GetCountryById(int id)
		{
			List<Common.Country> result = new List<Common.Country>();
			XmlDocument doc = LoadDocument();
			if (doc != null)
			{
				foreach (XmlNode node in this.GetNodeList(doc, string.Format(getCountryById, id)))
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
		/// <returns>a mapped item of type: <see cref="Common.Country"/></returns>
		private Common.Country MapData(XmlNodeList list)
		{
			Common.Country item = new Common.Country();
			foreach (XmlNode n in list)
			{
				#region Map Data
				switch (n.Name)
				{
					case "nCountryId":
						item.CountryId = int.Parse(n.InnerText);
						break;
					case "cName":
						item.Name = n.InnerText;
						break;
					case "cIso2":
						item.Iso2 = n.InnerText;
						break;
					case "cIso3":
						item.Iso3 = n.InnerText;
						break;
					case "cCapitalCity":
						item.CapitalCityName = n.InnerText;
						break;
					case "cMapReference":
						item.MapReference = n.InnerText;
						break;
					case "cCurrency":
						item.CurrencyName = n.InnerText;
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
				doc.Load(Common.Util.GetCountryPath());
				return doc;
			}
			catch (Exception ex)
			{
				Console.WriteLine(@"A problem appears to load the country file from following location: " + Environment.NewLine + " -> " + Common.Util.GetCountryPath());
			}
			return null;
		}
		#endregion Private Methods
	}
}
