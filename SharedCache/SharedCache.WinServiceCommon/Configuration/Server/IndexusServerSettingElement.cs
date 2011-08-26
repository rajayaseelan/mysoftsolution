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
// Name:      IndexusServerSettingElement.cs
// 
// Created:   24-02-2008 SharedCache.com, rschuetz
// Modified:  24-02-2008 SharedCache.com, rschuetz : Creation
// ************************************************************************* 

using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.ComponentModel;

namespace SharedCache.WinServiceCommon.Configuration.Server
{
	/// <summary>
	/// Define server settings instead of using AppSection
	/// </summary>
	public class IndexusServerSettingElement : ConfigurationElement
	{		
		//SharedCacheVersionNumber=""
		/// <summary>
		/// Gets or sets the shared cache version number.
		/// </summary>
		/// <value>The shared cache version number.</value>
		[ConfigurationProperty("SharedCacheVersionNumber", IsRequired = true)]
		public string SharedCacheVersionNumber
		{
			get { return (string)base["SharedCacheVersionNumber"]; }
			set { base["SharedCacheVersionNumber"] = (string)value; }
		}
		//LoggingEnable=""
		/// <summary>
		/// Gets or sets if logging is enabled.
		/// </summary>
		/// <value>The logging enable.</value>
		[ConfigurationProperty("LoggingEnable", IsRequired = true, DefaultValue = 0), IntegerValidator(MinValue = 0, MaxValue = 1)]
		public int LoggingEnable
		{
			get { return (int)base["LoggingEnable"]; }
			set { base["LoggingEnable"] = (int)value; }
		}

		//ServiceCacheIpPort=""
		/// <summary>
		/// Gets or sets the service cache ip port.
		/// </summary>
		/// <value>The service cache ip port.</value>
		[ConfigurationProperty("ServiceCacheIpPort", IsRequired = true, DefaultValue = 48888), IntegerValidator(MinValue = 1065, MaxValue = 65000)]
		public int ServiceCacheIpPort
		{
			get { return (int)base["ServiceCacheIpPort"]; }
			set { base["ServiceCacheIpPort"] = (int)value; }
		}
		
		//ServiceCacheIpAddress=""
		/// <summary>
		/// Gets or sets the service cache ip address.
		/// </summary>
		/// <value>The service cache ip address.</value>
		[ConfigurationProperty("ServiceCacheIpAddress", IsRequired = true, DefaultValue = "127.0.0.1")]
		public string ServiceCacheIpAddress
		{
			get { return (string)base["ServiceCacheIpAddress"]; }
			set { base["ServiceCacheIpAddress"] = (string)value; }
		}
		
		//ServiceFamilyMode=""
		/// <summary>
		/// Gets or sets the service family mode.
		/// </summary>
		/// <value>The service family mode.</value>
		[ConfigurationProperty("ServiceFamilyMode", IsRequired = true, DefaultValue = 0), IntegerValidator(MinValue = 0, MaxValue = 1)]
		public int ServiceFamilyMode
		{
			get { return (int)base["ServiceFamilyMode"]; }
			set { base["ServiceFamilyMode"] = (int)value; }
		}
		
		//CacheAmountOfObjects=""
		/// <summary>
		/// Gets or sets the cache amount of objects.
		/// </summary>
		/// <value>The cache amount of objects.</value>
		[ConfigurationProperty("CacheAmountOfObjects", IsRequired = true, DefaultValue = -1), IntegerValidator(MinValue = -1)]
		public int CacheAmountOfObjects
		{
			get { return (int)base["CacheAmountOfObjects"]; }
			set { base["CacheAmountOfObjects"] = (int)value; }
		}

		//CacheAmountFillFactorInPercentage=""
		/// <summary>
		/// Gets or sets the cache amount fill factor in percentage.
		/// </summary>
		/// <value>The cache amount fill factor in percentage.</value>
		[ConfigurationProperty("CacheAmountFillFactorInPercentage", IsRequired = true, DefaultValue = 90), IntegerValidator(MinValue = 10, MaxValue=100)]
		public int CacheAmountFillFactorInPercentage
		{
			get { return (int)base["CacheAmountFillFactorInPercentage"]; }
			set { base["CacheAmountFillFactorInPercentage"] = (int)value; }
		}
		
		//ServiceCacheCleanupThreadJob=""
		/// <summary>
		/// Gets or sets the service cache cleanup thread job.
		/// </summary>
		/// <value>The service cache cleanup thread job.</value>
		[ConfigurationProperty("ServiceCacheCleanupThreadJob", IsRequired = true, DefaultValue = 60000), IntegerValidator(MinValue = -1, MaxValue = int.MaxValue)]
		public int ServiceCacheCleanupThreadJob
		{
			get { return (int)base["ServiceCacheCleanupThreadJob"]; }
			set { base["ServiceCacheCleanupThreadJob"] = (int)value; }
		}

		//ServiceCacheCleanup=""
		/// <summary>
		/// Gets or sets the service cache cleanup.
		/// </summary>
		/// <value>The service cache cleanup.</value>
		[ConfigurationProperty("ServiceCacheCleanup", IsRequired = true, DefaultValue = "LRU"), ConfigurationValidator(typeof(IndexusServerSettingElement.CleanupValidator))]
		public Enums.ServiceCacheCleanUp ServiceCacheCleanup
		{
			get { return (Enums.ServiceCacheCleanUp)base["ServiceCacheCleanup"]; }
			set { base["ServiceCacheCleanup"] = EnumUtil<Enums.ServiceCacheCleanUp>.Parse((string)value.ToString()); }
		}

		/// <summary>
		/// Validate configured cleanup
		/// </summary>
		class CleanupValidator : ConfigurationValidatorBase
		{
			/// <summary>
			/// Determines whether an object can be validated based on type.
			/// </summary>
			/// <param name="type">The object type.</param>
			/// <returns>
			/// true if the type parameter value matches the expected type; otherwise, false.
			/// </returns>
			public override bool CanValidate(Type type)
			{
				if (type == typeof(Enums.ServiceCacheCleanUp))
					return true;

				return base.CanValidate(type);
			}
			/// <summary>
			/// Determines whether the value of an object is valid.
			/// </summary>
			/// <param name="value">The object value.</param>
			public override void Validate(object value)
			{
				Enums.ServiceCacheCleanUp t = EnumUtil<Enums.ServiceCacheCleanUp>.Parse(value.ToString());
			}
		}

		//TcpServerMaxThreadToSet=""
		/// <summary>
		/// Gets or sets the TCP server max thread to set.
		/// </summary>
		/// <value>The TCP server max thread to set.</value>
		[ConfigurationProperty("TcpServerMaxThreadToSet", IsRequired = true, DefaultValue = -1), IntegerValidator(MinValue = -1)]
		public int TcpServerMaxThreadToSet
		{
			get { return (int)base["TcpServerMaxThreadToSet"]; }
			set { base["TcpServerMaxThreadToSet"] = (int)value; }
		}

		//TcpServerMinThreadToSet=""
		/// <summary>
		/// Gets or sets the TCP server min thread to set.
		/// </summary>
		/// <value>The TCP server min thread to set.</value>
		[ConfigurationProperty("TcpServerMinThreadToSet", IsRequired = true, DefaultValue = -1), IntegerValidator(MinValue = -1)]
		public int TcpServerMinThreadToSet
		{
			get { return (int)base["TcpServerMinThreadToSet"]; }
			set { base["TcpServerMinThreadToSet"] = (int)value; }
		}

		/// <summary>
		/// Gets or Sets a value that specifies the amount of time a client contains a open connection to the server before it get dropped.
		/// </summary>
		/// <value></value>
		[ConfigurationProperty("SocketPoolTimeout", IsRequired = false, DefaultValue = "00:02:00"), PositiveTimeSpanValidator, TypeConverter(typeof(InfiniteTimeSpanConverter))]
		public TimeSpan SocketPoolTimeout
		{
			get { return (TimeSpan)base["SocketPoolTimeout"]; }
			set { base["SocketPoolTimeout"] = value; }
		}

		/// <summary>
		/// Gets or Sets a value that specifies the amount of time a client contains a open connection to the server before it get dropped.
		/// </summary>
		/// <value></value>
		[ConfigurationProperty("SocketPoolValidationInterval", IsRequired = false, DefaultValue = "00:02:00"), PositiveTimeSpanValidator, TypeConverter(typeof(InfiniteTimeSpanConverter))]
		public TimeSpan SocketPoolValidationInterval
		{
			get { return (TimeSpan)base["SocketPoolValidationInterval"]; }
			set { base["SocketPoolValidationInterval"] = value; }
		}

		/// <summary>
		/// Gets or sets the size of the socket pool min available.
		/// </summary>
		/// <value>The size of the socket pool min available.</value>
		[ConfigurationProperty("SocketPoolMinAvailableSize", IsRequired = false, DefaultValue = 5), IntegerValidator(MinValue = 1, MaxValue = 25)]
		public int SocketPoolMinAvailableSize
		{
			get { return (int)base["SocketPoolMinAvailableSize"]; }
			set { base["SocketPoolMinAvailableSize"] = (int)value; }
		}
	} 
}
