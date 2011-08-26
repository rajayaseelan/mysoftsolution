using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedCache.Testing.HelperObjects
{
	[Serializable]
	public class Address
	{
		public Address()
		{ }
		
		public string Street { get; set; }
		public string StreetNo { get; set; }
		public string ZipCode { get; set; }
		public string Country { get; set; }
		public string CountryCode { get; set; }

		public override string ToString()
		{
			return string.Format("{0} {0}" + Environment.NewLine + "{0} - {0} {0}",
					this.Street,
					this.StreetNo,
					this.CountryCode,
					this.ZipCode,
					this.Country
				);
		}
	}
}
