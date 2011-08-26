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
// Name:      Menu.cs
// 
// Created:   01-01-2008 SharedCache.com, rschuetz
// Modified:  01-01-2008 SharedCache.com, rschuetz : Creation
// Modified:  04-01-2008 SharedCache.com, rschuetz : added 530 - clear cache option
// Modified:  12-02-2008 SharedCache.com, rschuetz : added test no 410
// Modified:  12-02-2008 SharedCache.com, rschuetz : added test option 800
// Modified:  12-02-2008 SharedCache.com, rschuetz : added test option no 540
// ************************************************************************* 

using System;
using System.Collections.Generic;
using System.Text;

namespace SharedCache.WinServiceTestClient
{
	/// <summary>
	/// Console menu
	/// </summary>
	public class Menu
	{
		/// <summary>
		/// Prints the menu.
		/// </summary>
		public static void PrintMenu()
		{
			Console.WriteLine("Please enter one of the following options and press enter:");
			Console.WriteLine(@"- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - ");
			Console.WriteLine();
			Console.WriteLine(@"Country Options [100 range]");
			Console.WriteLine(@"- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - ");
			Console.WriteLine("100 - Load all Countries with dependend Regions - with printout");
			Console.WriteLine("110 - Load all Countries with dependend Regions - without printout");
			Console.WriteLine("120 - Load all Countries without dependend Regions - withuot printout");
			Console.WriteLine("130 - Load 100 Countries without dependend Regions - randomize access to 100 countries");
			Console.WriteLine();
			
			Console.WriteLine(@"Speed Tests [200 / 300 range]");
			Console.WriteLine(@"- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - ");
			Console.WriteLine("200 - Adding objects based on user input");
			Console.WriteLine();

			Console.WriteLine("Enhancements");
			Console.WriteLine(@"- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - ");
			Console.WriteLine("300 - Multi Operations - Adding 100 Objects and receive all at once.");
			Console.WriteLine("310 - RegEx Remove");
			Console.WriteLine();

			Console.WriteLine(@"Speed Tests [400 range]");
			Console.WriteLine(@"- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - ");
			Console.WriteLine("400 - Compare Compression Usage");
			Console.WriteLine("410 - Concurrent Usage Test");
			Console.WriteLine("420 - Add and Get Key's which are not available");
			Console.WriteLine();
			
			Console.WriteLine(@"Cache Options [500]");
			Console.WriteLine(@"- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - ");
			Console.WriteLine("500 - Statistics");
			Console.WriteLine("510 - Retrieve randomized 10 object's from the cache.");
			Console.WriteLine("520 - Get All Keys in Cache");
			Console.WriteLine("530 - Clear Cache");
			Console.WriteLine("540 - Check Hit Ratio");
			Console.WriteLine("550 - Check Special Key's for CJK and Heb");
			Console.WriteLine("560 - Add simple types to cache like byte / byte[] / int / bool / etc.");
			Console.WriteLine("570 - Test Provider Cache Key's and Stats");
			Console.WriteLine("580 (NEW)- Test Check Absolut Expiraiton Time");
			Console.WriteLine();

			Console.WriteLine(@"Compare between Objects with different members [600]");
			Console.WriteLine(@"- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - ");
			Console.WriteLine("600 - Add  100 objects with the size approx. of 100kb.");
			Console.WriteLine("610 - Add 1000 objects with the size approx. of 100kb.");
			Console.WriteLine();

			Console.WriteLine(@"Differnet Cache Test [700]");
			Console.WriteLine(@"- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - ");
			Console.WriteLine(@"700 - Add 10 objects with TimeSpan of 1 min.");
			Console.WriteLine(@"710 (NEW) - Extend TTL");
			Console.WriteLine();

			Console.WriteLine(@"Long Term Tests Cache Test [800]");
			Console.WriteLine(@"- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - ");
			Console.WriteLine(@"800 - This test is running several combinations of above metion tests");
			Console.WriteLine(@"      This test takes several minutes.");
			Console.WriteLine();

			Console.WriteLine(@"Continous Cache Test - Time measuring [900]");
			Console.WriteLine(@"- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - ");
			Console.WriteLine(@"900 - This test is running 25 times and adding each time 250 objects");
			Console.WriteLine(@"      of 1kb / 100 kb / 1000 kb. Data can be used to see how changes can");
			Console.WriteLine(@"      have impact. this test takes several minutes.");
			Console.WriteLine();

			Console.WriteLine(@"DataContract Attribute [1000]");
			Console.WriteLine(@"- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - ");
			Console.WriteLine(@"1000 - Usage of DataContract for WCF Services without the need to add ");
			Console.WriteLine(@"       additionally Serializable Attribute.");
			Console.WriteLine();

			Console.WriteLine(@"Console Options");
			Console.WriteLine(@"- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - ");
			Console.WriteLine("0  - Clean Screen");
			Console.WriteLine("9  - Exit");
			Console.WriteLine();
		}
	}
}
