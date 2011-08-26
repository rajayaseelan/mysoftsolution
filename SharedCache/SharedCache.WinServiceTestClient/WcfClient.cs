using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace SharedCache.WinServiceTestClient
{
	[DataContract]
	public class WcfClient
	{
		[DataMember] 
		public string ClientName { get; set; }
		[DataMember]
		public string ClientIp { get; set; }

		public WcfClient()
		{
			this.ClientIp = string.Empty;
			this.ClientName = string.Empty;
		}

		public WcfClient(string clientIp, string clientName)
		{
			this.ClientIp = clientIp;
			this.ClientName = ClientName;
		}
	}
}
