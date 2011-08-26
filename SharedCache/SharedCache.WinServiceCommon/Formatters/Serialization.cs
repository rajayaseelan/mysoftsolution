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
// Name:      Serialization.cs
// 
// Created:   25-01-2007 SharedCache.com, rschuetz
// Modified:  25-01-2007 SharedCache.com, rschuetz : Creation
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace SharedCache.WinServiceCommon.Formatters
{
	/// <summary>
	/// <b>Serialization contains a set of Utils for various serilization by usage of Generics</b>
	/// more information available at: <![CDATA[http://www.codeproject.com/soap/Coreweb03.asp]]>
	/// <remarks>
	/// with soap usage you need to add a reference to 
	/// System.Runtime.Serialization.Formatter.Soap assembly
	/// </remarks>
	/// </summary>
	public static class Serialization
	{
		#region binary serialization

		/// <summary>
		/// Binaries the serialize.
		/// </summary>
		/// <param name="obj">The obj. A <see cref="T:System.Object"/> Object.</param>
		/// <returns>A <see cref="T:System.Byte[]"/> Object.</returns>
		[System.Diagnostics.DebuggerStepThrough]
		public static byte[] BinarySerialize(Object obj)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(Serialization).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			byte[] serializedObject;
			MemoryStream ms = new MemoryStream();
			BinaryFormatter b = new BinaryFormatter();
			b.Serialize(ms, obj);
			ms.Seek(0, 0);
			serializedObject = ms.ToArray();
			ms.Close();
			return serializedObject;
		}

		/// <summary>
		/// Desirializes the given type.
		/// </summary>
		/// <param name="serializedObject">The serialized object. A <see cref="T:System.Byte[]"/> Object.</param>
		/// <returns>A <see cref="T:T"/> Object.</returns>
		//[System.Diagnostics.DebuggerStepThrough]
		public static T BinaryDeSerialize<T>(byte[] serializedObject)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(Serialization).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			try
			{
				MemoryStream ms = new MemoryStream();
				ms.Write(serializedObject, 0, serializedObject.Length);
				ms.Seek(0, 0);
				BinaryFormatter b = new BinaryFormatter();
				Object obj = b.Deserialize(ms);
				ms.Close();
				return (T)obj;
			}
			catch (Exception ex)
			{
				MemoryStream ms = new MemoryStream();
				ms.Write(serializedObject, 0, serializedObject.Length);
				ms.Seek(0, 0);
				BinaryFormatter b = new BinaryFormatter();
				Object obj = b.Deserialize(ms);
				ms.Close();
				CacheException cex = (CacheException)obj;
				if (cex != null)
				{ 
					// TODO: Shared Cache Exception
					Handler.LogHandler.Error(cex.StackTrace, ex);
				}
				return (T)obj;
			}

			
		}

		#endregion binary serialization

		#region file binary serialization

		/// <summary>
		/// Serialize Files
		/// </summary>
		/// <param name="obj">The obj. A <see cref="T:System.Object"/> Object.</param>
		/// <param name="filePath">The file path. A <see cref="T:System.String"/> Object.</param>
		[System.Diagnostics.DebuggerStepThrough]
		public static void FileSerialize(Object obj, string filePath)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(Serialization).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			FileStream fileStream = null;
			try
			{
				fileStream = new FileStream(filePath, FileMode.Create);
				BinaryFormatter b = new BinaryFormatter();
				b.Serialize(fileStream, obj);
			}
			catch
			{
				throw;
			}
			finally
			{
				if (fileStream != null)
					fileStream.Close();
			}
		}

		/// <summary>
		/// deserialize Files
		/// </summary>
		/// <param name="filePath">The file path. A <see cref="T:System.String"/> Object.</param>
		/// <returns>A <see cref="T:T"/> Object.</returns>
		[System.Diagnostics.DebuggerStepThrough]
		public static T FileDeSerialize<T>(string filePath)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(Serialization).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			FileStream fileStream = null;
			Object obj;
			try
			{
				if (File.Exists(filePath) == false)
					throw new FileNotFoundException("The file was not found.", filePath);
				fileStream = new FileStream(filePath, FileMode.Open);
				BinaryFormatter b = new BinaryFormatter();
				obj = b.Deserialize(fileStream);
			}
			catch
			{
				throw;
			}
			finally
			{
				if (fileStream != null)
					fileStream.Close();
			}
			return (T)obj;
		}
		#endregion file binary serialization

		#region soap binary serialization

		/// <summary>
		/// SOAPs the memory stream serialization.
		/// </summary>
		/// <param name="obj">The obj. A <see cref="T:System.Object"/> Object.</param>
		/// <param name="encodingType">Type of the encoding.</param>
		/// <returns>A <see cref="T:System.String"/> Object.</returns>
		[System.Diagnostics.DebuggerStepThrough]
		public static string SoapMemoryStreamSerialization(object obj, Encoding encodingType)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(Serialization).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			string xmlResult;
			using (Stream stream = new MemoryStream())
			{
				try
				{
					SoapFormatter sf = new SoapFormatter();
					sf.Serialize(stream, obj);
				}
				catch
				{
					throw;
				}
				stream.Position = 0;
				byte[] b = new byte[stream.Length];
				stream.Read(b, 0, (int)stream.Length);
				xmlResult = encodingType.GetString(b, 0, b.Length);
			}
			return xmlResult;
		}

		/// <summary>
		/// SOAP deserailization.
		/// </summary>
		/// <param name="input">The input. A <see cref="T:System.String"/> Object.</param>
		/// <param name="encodingType">Type of the encoding.</param>
		/// <returns>A <see cref="T:T"/> Object.</returns>
		[System.Diagnostics.DebuggerStepThrough]
		public static T SoapDeserailization<T>(string input, System.Text.Encoding encodingType)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(Serialization).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			Object obj = null;
			using (StringReader sr = new StringReader(input))
			{
				byte[] b;
				b = encodingType.GetBytes(input);
				Stream stream = new MemoryStream(b);
				try
				{
					SoapFormatter sf = new SoapFormatter();
					obj = (object)sf.Deserialize(stream);
				}
				catch
				{
					throw;
				}
			}
			return (T)obj;
		}

		#endregion soap binary serialization

		#region DataContract binary serialization

		/// <summary>
		/// A helper method to identify if the attribute of an object is 
		/// serializable
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		internal static bool IsSerializable(Type type)
		{
			if (type.IsSerializable)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Binaries the serialize.
		/// </summary>
		/// <param name="obj">The obj. A <see cref="T:System.Object"/> Object.</param>
		/// <returns>A <see cref="T:System.Byte[]"/> Object.</returns>
		// [System.Diagnostics.DebuggerStepThrough]
		public static byte[] DataContractBinarySerialize(Object obj)
		{
			#region Access Log
#if TRACE			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(Serialization).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			MemoryStream ms = new MemoryStream();
			DataContractSerializer ser = new DataContractSerializer(obj.GetType());
			ser.WriteObject(ms, obj);
			byte[] data = ms.ToArray();
			return data;
		}

		/// <summary>
		/// Desirializes the given type.
		/// </summary>
		/// <param name="serializedObject">The serialized object. A <see cref="T:System.Byte[]"/> Object.</param>
		/// <returns>A <see cref="T:T"/> Object.</returns>
		//[System.Diagnostics.DebuggerStepThrough]
		public static T DataContractBinaryDeSerialize<T>(byte[] serializedObject)
		{
			#region Access Log
#if TRACE			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(Serialization).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log
			try
			{
				MemoryStream ms = new MemoryStream(serializedObject);
				ms.Write(serializedObject, 0, serializedObject.Length);
				ms.Seek(0, 0);
				DataContractSerializer ser = new DataContractSerializer(typeof(T));
				Object obj = ser.ReadObject(ms);
				ms.Close();
				return (T)obj;
			}
			catch (Exception ex)
			{
				MemoryStream ms = new MemoryStream();
				ms.Write(serializedObject, 0, serializedObject.Length);
				ms.Seek(0, 0);
				BinaryFormatter b = new BinaryFormatter();
				Object obj = b.Deserialize(ms);
				ms.Close();
				CacheException cex = (CacheException)obj;
				if (cex != null)
				{
					// TODO: Shared Cache Exception
					Handler.LogHandler.Error(cex.StackTrace, ex);
				}
				return (T)obj;
			}
		}
		#endregion DataContract binary serialization
	}

}
