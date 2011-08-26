using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SCCACHE = SharedCache.WinServiceCommon.Provider.Cache.IndexusDistributionCache;

namespace SharedCache.Testing
{
	/// <summary>
	/// Summary description for SharedCacheProvider
	/// </summary>
	[TestClass]
	public class SharedCacheProvider
	{
		public SharedCacheProvider()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void AddDataToCache()
		{
			List<HelperObjects.Person> data = new List<SharedCache.Testing.HelperObjects.Person>()
			{
				new HelperObjects.Person(){Salutation = "MR", FirstName = "Abcd", LastName = "Lmno", Age = 20},
				new HelperObjects.Person(){Salutation = "MR", FirstName = "Efgh", LastName = "Pqrs", Age = 21},
				new HelperObjects.Person(){Salutation = "MR", FirstName = "Ijkl", LastName = "tuvw", Age = 22}
			};
			SCCACHE.SharedCache.Clear();
			foreach (var item in data)
			{
				item.Address = new List<SharedCache.Testing.HelperObjects.Address>()
				{
					new HelperObjects.Address(){ Country = "Switzerland", CountryCode = "CH", ZipCode = "8000", StreetNo = "223", Street = "Bahnhofstrasse" },
					new HelperObjects.Address(){ Country = "United States of America", CountryCode = "US", ZipCode = "917", StreetNo = "10025", Street = "947 Amsterdam Ave" },
					new HelperObjects.Address(){ Country = "Germany", CountryCode = "DE", ZipCode = "50000", StreetNo = "223", Street = "Gartenstrasse" }
				};
				
				SCCACHE.SharedCache.Add("test_" + item.GetHashCode().ToString(), item);
			}

			List<string> keys = SCCACHE.SharedCache.GetAllKeys();
			
			Assert.IsNotNull(keys);

			Assert.IsTrue(keys.Count > 0);



			

			//  
		}
	}
}
