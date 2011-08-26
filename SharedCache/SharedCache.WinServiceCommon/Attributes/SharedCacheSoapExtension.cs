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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web.Services.Protocols;

namespace SharedCache.WinServiceCommon.Attributes
{
	/// <summary>
	/// Define a SOAP Extension that traces the SOAP request and SOAP response for 
	/// the XML Web service method the SOAP extension is applied to.
	/// </summary>
	public class SharedCacheSoapExtension : SoapExtension
	{
		// Fields
		string filename = "C:\\ronitest.txt";
		private Stream newStream;
		private Stream oldStream;
		private int keep = 59999;
		private string cacheKey = string.Empty;

		/// <summary>
		/// The SOAP extension was configured to run using a configuration file instead 
		/// of an attribute applied to a specific XML Web service method.
		/// </summary>
		/// <param name="serviceType">A <see cref="Type"/> obejct.</param>
		/// <returns>An <see cref="object"/> obejct.</returns>
		public override object GetInitializer(Type serviceType)
		{
			return typeof(SharedCacheSoapExtension);
		}

		/// <summary>
		/// When the SOAP extension is accessed for the first time, the XML Web service method it is applied to 
		/// is accessed to store the file name passed in, using the corresponding SoapExtensionAttribute.
		/// </summary>
		/// <param name="serviceType">A <see cref="Type"/> obejct.</param>
		/// <param name="attribute">A <see cref="SoapExtensionAttribute"/> object.</param>
		/// <returns>An <see cref="object"/> obejct.</returns>
		public override object GetInitializer(LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute)
		{
			// keep = ((SharedCacheSoapExtensionAttribute)attribute).CacheInSecond;
			return attribute;
		}
		
		/// <summary>
		/// Receive the file name stored by GetInitializer and store it in 
		/// a member variable for this specific instance.
		/// </summary>
		/// <param name="initializer">An <see cref="object"/> obejct.</param>
		public override void Initialize(object initializer)
		{
			//You'd usually get the attribute here and pull whatever you need off it.
			SharedCacheSoapExtensionAttribute attr = initializer as SharedCacheSoapExtensionAttribute;
			if (attr != null)
			{
				keep = attr.CacheInSecond;
			}
		}

		public override Stream ChainStream(Stream stream)
		{
			this.oldStream = stream;
			this.newStream = new MemoryStream();
			return this.newStream;
			// return base.ChainStream(stream);
		}

		public MemoryStream YankIt(Stream streamToPrefix)
		{
			MemoryStream outStream = new MemoryStream();
			//debug
			outStream.Seek(0, SeekOrigin.Begin);
			//outStream.Position = 0L;
			StreamReader reader2 = new StreamReader(outStream);
			string s = reader2.ReadToEnd();
			System.Diagnostics.Debug.WriteLine(s);

			outStream.Position = 0L;
			outStream.Seek(0, SeekOrigin.Begin);
			return outStream;
		}

		private void GetReady()
		{
			this.Copy(this.oldStream, this.newStream);
			this.newStream.Position = 0L;
		}

		// Methods
		private void StripWhitespace()
		{
			this.newStream.Position = 0L;
			this.newStream = this.YankIt(this.newStream);
			this.Copy(this.newStream, this.oldStream);
		}

		private void Copy(Stream from, Stream to)
		{
			TextReader reader = new StreamReader(from);
			TextWriter writer = new StreamWriter(to);
			writer.WriteLine(reader.ReadToEnd());
			writer.Flush();
		}

		public override void ProcessMessage(SoapMessage message)
		{
			switch (message.Stage)
			{
				case SoapMessageStage.BeforeSerialize:
					break;
				case SoapMessageStage.AfterSerialize:
					WriteOutput((SoapServerMessage)message);
					break;
				case SoapMessageStage.BeforeDeserialize:
					WriteInput((SoapServerMessage)message);
					break;
				case SoapMessageStage.AfterDeserialize:
					break;
				default:
					throw new Exception("invalid stage");
			}
		}

		// Write the contents of the incoming SOAP message to the log file.
		public void WriteInput(SoapServerMessage message)
		{
			// Utility method to copy the contents of one stream to another. 
			Copy(oldStream, newStream);
			FileStream myFileStream = new FileStream(filename, FileMode.Append, FileAccess.Write);
			StreamWriter myStreamWriter = new StreamWriter(myFileStream);
			myStreamWriter.WriteLine("================================== Request at "
				 + DateTime.Now);
			myStreamWriter.WriteLine("The method that has been invoked is : ");
			myStreamWriter.WriteLine("\t" + message.MethodInfo.Name);
			myStreamWriter.WriteLine("The contents of the SOAP envelope are : ");
			myStreamWriter.Flush();
			newStream.Position = 0;
			Copy(newStream, myFileStream);
			myFileStream.Close();
			newStream.Position = 0;
		}

		// Write the contents of the outgoing SOAP message to the log file.
		public void WriteOutput(SoapServerMessage message)
		{
			newStream.Position = 0;
			FileStream myFileStream = new FileStream(filename, FileMode.Append, FileAccess.Write);
			StreamWriter myStreamWriter = new StreamWriter(myFileStream);
			myStreamWriter.WriteLine("---------------------------------- Response at "
																					+ DateTime.Now);
			myStreamWriter.Flush();
			// Utility method to copy the contents of one stream to another. 
			Copy(newStream, myFileStream);
			myFileStream.Close();
			newStream.Position = 0;
			Copy(newStream, oldStream);
		}



		/// <summary>
		/// If the SoapMessageStage is such that the SoapRequest or SoapResponse 
		/// is still in the SOAP format to be sent or received, save it out to a file.
		/// </summary>
		/// <param name="message">A <see cref="SoapMessage"/> object.</param>
		//public override void ProcessMessage(SoapMessage message)
		//{			
		//  //switch (message.Stage)
		//  //{
		//  //  case SoapMessageStage.BeforeDeserialize:
		//  //    {
		//  //      this.cacheKey = message.Action.ToString();
		//  //      Stream m = Provider.Cache.IndexusDistributionCache.SharedCache.Get<Stream>(this.cacheKey);
		//  //      if (m == null)
		//  //      {
		//  //        Provider.Cache.IndexusDistributionCache.SharedCache.Add(this.cacheKey,
		//  //          Formatters.Serialization.BinarySerialize(message.Stream)
		//  //          );
		//  //      }
		//  //    }
		//  //    // this.GetReady();
		//  //    return;
		//  //  case SoapMessageStage.AfterDeserialize:
		//  //    return;
		//  //  case SoapMessageStage.BeforeSerialize:
		//  //    return;
		//  //  case SoapMessageStage.AfterSerialize:
		//  //    // this.StripWhitespace();
		//  //    return;
		//  //}
		//  //throw new Exception("invalid stage");

		//  //switch (message.Stage)
		//  //{
		//  //  case SoapMessageStage.AfterDeserialize:
		//  //    System.Diagnostics.Debug.WriteLine(message.Stage.ToString());
		//  //    break;
		//  //  case SoapMessageStage.AfterSerialize:
		//  //    System.Diagnostics.Debug.WriteLine(message.Stage.ToString());
		//  //    break;
		//  //  case SoapMessageStage.BeforeDeserialize:
		//  //    System.Diagnostics.Debug.WriteLine(message.Stage.ToString());					
		//  //    this.cacheKey = message.Action.ToString();
					
		//  //    break;
		//  //  case SoapMessageStage.BeforeSerialize:
		//  //    System.Diagnostics.Debug.WriteLine(message.Stage.ToString());
		//  //    break;
		//  //  default:
		//  //    break;
		//  //}
		//}
	}
}
