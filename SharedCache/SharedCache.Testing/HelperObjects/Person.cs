using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedCache.Testing.HelperObjects
{
	[Serializable]
	public class Person
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public int Age { get; set; }
		public string Salutation { get; set; }
		public List<Address> Address { get; set; }

		public override string ToString()
		{
			string n = string.Empty;
			
			if (this.Address != null)
			{
				foreach (var item in this.Address)
				{
					n += item.ToString();
				}
			}

			return string.Format("{0} {1} {2} is {3} years old. " + Environment.NewLine + " {4}",
				this.Salutation,
				this.FirstName,
				this.LastName,
				this.Age,
				n);
		}
	}
}
