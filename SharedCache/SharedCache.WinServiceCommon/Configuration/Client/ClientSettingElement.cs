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
// Name:      ClientSettingElement.cs
// 
// Created:   24-02-2008 SharedCache.com, rschuetz
// Modified:  24-02-2008 SharedCache.com, rschuetz : Creation
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.ComponentModel;

namespace SharedCache.WinServiceCommon.Configuration.Client
{
	/// <summary>
	/// Instead of using appSettings all settings are within 
	/// configuration section.
	/// </summary>
	public class ClientSettingElement : ConfigurationElement
	{
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

		/// <summary>
		/// Gets or sets if logging is enabled.
		/// </summary>
		/// <value>The logging enable.</value>
		[ConfigurationProperty("LoggingEnable", IsRequired = true, DefaultValue=0), IntegerValidator(MinValue = 0, MaxValue = 1)]
		public int LoggingEnable
		{
			get { return (int)base["LoggingEnable"]; }
			set { base["LoggingEnable"] = (int)value; }
		}

		/// <summary>
		/// Gets or sets the if compression is enabled.
		/// </summary>
		/// <value>The compression enabled.</value>
		[ConfigurationProperty("CompressionEnabled", IsRequired = true, DefaultValue=0), IntegerValidator(MinValue = 0, MaxValue = 1)]
		public int CompressionEnabled
		{
			get { return (int)base["CompressionEnabled"]; }
			set { base["CompressionEnabled"] = (int)value; }
		}

		/// <summary>
		/// Gets or sets the size of the min compression.
		/// </summary>
		/// <value>The size of the compression min.</value>
		[ConfigurationProperty("CompressionMinSize", IsRequired = true, DefaultValue=153600), IntegerValidator(MinValue = 153600, MaxValue = int.MaxValue)]
		public int CompressionMinSize
		{
			get { return (int)base["CompressionMinSize"]; }
			set { base["CompressionMinSize"] = (int)value; }
		}

		/// <summary>
		/// Gets or sets the size of the socket pool min available.
		/// </summary>
		/// <value>The size of the socket pool min available.</value>
		[ConfigurationProperty("SocketPoolMinAvailableSize", IsRequired = false, DefaultValue=5), IntegerValidator(MinValue = 1, MaxValue = 25)]
		public int SocketPoolMinAvailableSize
		{
			get { return (int)base["SocketPoolMinAvailableSize"]; }
			set { base["SocketPoolMinAvailableSize"] = (int)value; }
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
		/// Gets or sets the hashing algorithm.
		/// </summary>
		/// <value>The hashing algorithm.</value>
		[ConfigurationProperty("HashingAlgorithm", IsRequired = true, DefaultValue = "Hashing"), ConfigurationValidator(typeof(ClientSettingElement.HashingValidator))]
		public Enums.HashingAlgorithm HashingAlgorithm
		{
			get { return (Enums.HashingAlgorithm)base["HashingAlgorithm"]; }
			set { base["HashingAlgorithm"] = EnumUtil<Enums.HashingAlgorithm>.Parse((string)value.ToString()); }
		}

		/// <summary>
		/// Validate configured hashing
		/// </summary>
		class HashingValidator : ConfigurationValidatorBase
		{
			public override bool CanValidate(Type type)
			{
				if (type == typeof(Enums.HashingAlgorithm))
					return true;

				return base.CanValidate(type);
			}
			public override void Validate(object value)
			{
				Enums.HashingAlgorithm t = EnumUtil<Enums.HashingAlgorithm>.Parse(value.ToString());
			}
		}
	}
}
