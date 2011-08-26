#region Copyright (c) 2005 - 2008 MergeSystem GmbH, Switzerland. All Rights Reserved
// * --------------------------------------------------------------------- *
// *                            Merge System GmbH                          *
// *              Copyright (c) 2008 All Rights reserved                   *
// *                                                                       *
// *                                                                       *
// * This file and its contents are protected by Swiss and International   *
// * copyright laws. Unauthorized reproduction and/or distribution of all  *
// * or any portion of the code contained herein is strictly prohibited    *
// * and will result in severe civil and criminal penalties. Any           *
// * violations of this copyright will be prosecuted to the fullest        *
// * extent possible under law.                                            *
// *                                                                       *
// * THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
// * TO AUTHORIZED CUSTOMERS FOR THE PURPOSES OF EDUCATION AND             *
// * TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
// * CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
// * THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF Merge System GMBH. *
// *                                                                       *
// * UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
// * PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
// * SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY Merge System GMBH        *
// * PRODUCT.                                                              *
// *                                                                       *
// * THE AUTHORIZED CUSTOMER ACKNOWLEDGES THAT THIS SOURCE CODE            *
// * CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF Merge System GMBH, *
// * THE AUTHORIZED CUSTOMER AGREES TO EXPEND EVERY EFFORT TO INSURE       *
// * ITS CONFIDENTIALITY.                                                  *
// *                                                                       *
// * THE LICENSE AGREEMENT ACCOMPANYING THE PRODUCT DOES NOT PROVIDE ANY   *
// * RIGHTS REGARDING THE SOURCE CODE CONTAINED HEREIN.                    *
// *                                                                       *
// * THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
// * --------------------------------------------------------------------- *
#endregion 

// *************************************************************************
//
// Name:      ConCurrentClient.cs
// 
// Created:   12-02-2008 SharedCache.com, rschuetz
// Modified:  12-02-2008 SharedCache.com, rschuetz : Creation
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using SharedCache.WinServiceCommon.Provider.Cache;
using SharedCache.WinServiceTestClient.SharedDataObjects;

namespace SharedCache.WinServiceTestClient.SharedDataObjects
{
	[Serializable]
	public class Person
	{
		#region Property: FirstName
		private string firstName;
		
		/// <summary>
		/// Gets/sets the FirstName
		/// </summary>
		public string FirstName
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.firstName;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.firstName = value;  }
		}
		#endregion
		#region Property: LastName
		private string lastName;
		
		/// <summary>
		/// Gets/sets the LastName
		/// </summary>
		public string LastName
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.lastName;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.lastName = value;  }
		}
		#endregion
		#region Property: DateOfBirth
		private DateTime dateOfBirth;
		
		/// <summary>
		/// Gets/sets the DateOfBirth
		/// </summary>
		public DateTime DateOfBirth
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.dateOfBirth;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.dateOfBirth = value;  }
		}
		#endregion
	}
}


namespace SharedCache.WinServiceTestClient
{
	public class ConCurrentClient
	{
		public static int NotReceived = 0;

		public void DoIt()
		{
			// Initialise some threads
			int THREAD_COUNT = 25; // How many threads to start

			Thread[] putThreads = new Thread[THREAD_COUNT];
			for (int i = 0; i < THREAD_COUNT; i++)
			{
				// Instantiate and start up the "put" threads:
				PutThread putter = new PutThread();
				putter.ThreadNum = i;

				ThreadStart ts = new ThreadStart(putter.Put);
				Thread thread = new Thread(ts);
				thread.Name = i.ToString();
				putThreads[i] = thread;

				putThreads[i].Start();
			}

			// Wait for all the threads to finish, so we can display a "completed" message
			for (int i = 0; i < THREAD_COUNT; i++)
			{
				putThreads[i].Join();
			}
			Console.WriteLine("Not Received Person Objects: " + NotReceived);
			Console.Out.WriteLine("DoIt: PUT threads completed");
		}

	}

	/// <summary>
	/// This class is intended to be used as a "thread". It puts an object into the cache, and obtains
	/// the object from the cache again, many times in a loop.
	/// </summary>
	public class PutThread
	{
		private Random r = new Random((int)DateTime.Now.Ticks);

		/// <summary>
		/// Gets and sets a simple identifier for the thread.
		/// </summary>
		#region Property: ThreadNum
		private int threadNum;
		
		/// <summary>
		/// Gets/sets the ThreadNum
		/// </summary>
		public int ThreadNum
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.threadNum;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.threadNum = value;  }
		}
		#endregion
		

		/// <summary>
		/// The "thread method". This method contains a loop which uses the cache for storing and
		/// fetching an object.
		/// </summary>
		public void Put()
		{
			// Random start up delay
			int mss = r.Next(1000, 10000);
			Console.Out.WriteLine("PutThread: start " + ThreadNum + " (delay=" + mss + ")");
			Thread.Sleep(mss);

			int NUM_CACHE_REQUESTS = 100;
			
			for (int i = 0; i < NUM_CACHE_REQUESTS; i++)
			{
				try
				{
					Console.Out.WriteLine("PutThread: Put Person " + i + " (" + ThreadNum + ")");

					Person person = new Person();
					person.FirstName = "Peter Alan";
					person.LastName = "Kirk";
					person.DateOfBirth = new DateTime(2000, 12, 31);

					string key = "Indexus Person Cache";
					key += i.ToString() + "_" + Thread.CurrentThread.Name;
					
					IndexusDistributionCache.SharedCache.Add(key, person);
					
					int ms = 10;
					Thread.Sleep(ms);

					// Get the object again:
					Person gotten = IndexusDistributionCache.SharedCache.Get<Person>(key);
					if (gotten == null)
					{
						Interlocked.Increment(ref ConCurrentClient.NotReceived);
					}
					int ms1 = 100;
					//Thread.Sleep(ms1);

					// Get the object again:
					IndexusDistributionCache.SharedCache.Remove(key);

					Thread.Sleep(ms1);
				}
				catch (Exception ex)
				{
					Console.Out.WriteLine("Error in thread " + ThreadNum + ":" + ex.Message);
				}
			}
			Console.Out.WriteLine("PutThread: end " + ThreadNum);
		}
	}



	///// <summary>
	///// Summary description for Concurrent Test
	///// </summary>
	//public class ConCurrent
	//{
	//  DateTime start = DateTime.MinValue;
	//  DateTime end = DateTime.MinValue;

	//  public static object bulkObject = new object();
	//  public static long counter = 0;
	//  public static int amount = 25;

	//  public void Add(object key)
	//  {
	//    IndexusDistributionCache.SharedCache.Add((string)key, new TestSizeObjectCon(ObjectSize.Hundert));

	//    lock (bulkObject)
	//    {
	//      if (++counter == amount)
	//      {
	//        end = DateTime.Now;
	//        Done("add");
	//      }
	//    }
	//  }

	//  public void Remove(object key)
	//  {
	//    IndexusDistributionCache.SharedCache.Remove((string)key);

	//    lock (bulkObject)
	//    {
	//      if (++counter == amount)
	//      {
	//        end = DateTime.Now;
	//        Done("remove");
	//      }
	//    }
	//  }

	//  public void Get(object key)
	//  {
	//    TestSizeObjectCon a = IndexusDistributionCache.SharedCache.Get<TestSizeObjectCon>((string)key);

	//    lock (bulkObject)
	//    {
	//      if (++counter == amount)
	//      {
	//        end = DateTime.Now;
	//        Done("get");
	//      }
	//    }
	//  }

	//  public void Done(string doing)
	//  { 
	//    TimeSpan span = end - start;
	//    Console.WriteLine();
	//    Console.WriteLine(doing + ": " + span.Seconds + "s " + span.Milliseconds + "ms");
	//    System.Diagnostics.Debug.WriteLine(doing + ": " +span.Seconds + "s " + span.Milliseconds + "ms");
	//  }

	//  public void RunPerfTest()
	//  {
	//    Thread.Sleep(130);
	//    Console.WriteLine();
	//    Console.WriteLine("You have configured the following nodes in your config file:");

	//    foreach (string n in IndexusDistributionCache.SharedCache.Servers)
	//    {
	//      Console.WriteLine(n);
	//    }

	//    Console.WriteLine("The test will create {0} Threads to [Add / Get / Remove] and work with ~100k objects ", amount);

	//    List<Thread> th = new List<Thread>();
	//    start = DateTime.Now;
	//    for (int i = 0; i < amount; ++i)
	//    {
	//      Thread t = new Thread(this.Add);
	//      th.Add(t);
	//    }
	//    int cntr = 0;
	//    foreach (Thread tt in th)
	//    {
	//      cntr++;
	//      tt.Start(cntr.ToString());
	//    }

	//    Thread.Sleep(8000);
	//    counter = 0;

	//    start = DateTime.Now;
	//    for (int i = 0; i < amount; ++i)
	//    {
	//      Thread t = new Thread(this.Add);
	//      t.Start(i.ToString());
	//    }
	//    Thread.Sleep(8000);
	//    counter = 0;

	//    start = DateTime.Now;
	//    for (int i = 0; i < amount; ++i)
	//    {
	//      Thread t = new Thread(this.Add);
	//      t.Start(i.ToString());
	//    }
	//    Thread.Sleep(8000);
	//    counter = 0;
	//  }
	//}
	///// <summary>
	///// defines object size
	///// </summary>
	//public enum ObjectSize
	//{
	//  /// <summary>
	//  /// 1 kb
	//  /// </summary>
	//  One,
	//  /// <summary>
	//  /// 100 kb
	//  /// </summary>
	//  Hundert,
	//  /// <summary>
	//  /// 1 mb
	//  /// </summary>
	//  Thousend
	//}

	///// <summary>
	///// Test Size Object
	///// </summary>
	//[Serializable]
	//public class TestSizeObjectCon
	//{
	//  /// <summary>
	//  /// an byte array which contains object payload.
	//  /// </summary>
	//  public byte[] byteArray;
	//  /// <summary>
	//  /// object id
	//  /// </summary>
	//  public string Id = Guid.NewGuid().ToString();

	//  /// <summary>
	//  /// Initializes a new instance of the <see cref="TestSizeObject"/> class.
	//  /// </summary>
	//  public TestSizeObjectCon()
	//  { }

	//  /// <summary>
	//  /// Initializes a new instance of the <see cref="TestSizeObject"/> class.
	//  /// </summary>
	//  /// <param name="size">The size.</param>
	//  public TestSizeObjectCon(ObjectSize size)
	//  {
	//    switch (size)
	//    {
	//      case ObjectSize.One:
	//        byteArray = new byte[1024];
	//        break;
	//      case ObjectSize.Hundert:
	//        byteArray = new byte[1024 * 128];
	//        break;
	//      case ObjectSize.Thousend:
	//        byteArray = new byte[1024 * 1024];
	//        break;
	//    }

	//    Random r = new Random();
	//    for (int i = 0; i < byteArray.Length; i++)
	//    {
	//      int bb = r.Next(65, 97);
	//      byteArray[i] = Convert.ToByte(bb);
	//    }
	//  }
	//}
}
