#region Copyright (c) Roni Schuetz - All Rights Reserved
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
// Name:      Program.cs
// 
// Created:   22-01-2007 SharedCache.com, rschuetz
// Modified:  22-01-2007 SharedCache.com, rschuetz : Creation
// Modified:  25-12-2007 SharedCache.com, rschuetz : added additional test options [100 / 101]
// Modified:  02-01-2008 SharedCache.com, rschuetz : re-written test application
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

using SharedCache.WinServiceCommon.Provider.Cache;
using COM = SharedCache.WinServiceCommon;
using SharedCache.WinServiceTestClient.SharedDataObjects;


namespace SharedCache.WinServiceTestClient
{
    /// <summary>
    /// main entry point.
    /// </summary>
    public class Program
    {
        static void Main(string[] args)
        {
            #region Access Log
            COM.Handler.LogHandler.Tracking(
                "Access Method: " + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;"
            );
            #endregion Access Log

            //TestApplication app = new TestApplication();
            //app.Start();

            var cache = COM.Provider.Cache.IndexusDistributionCache.SharedCache;

            for (int i = 0; i < 1; i++)
            {
                Thread thread = new Thread(DoWork);
                thread.Start(cache);
            }

            Console.ReadKey();
        }

        static void DoWork(object s)
        {
            while (true)
            {
                var cache = s as COM.Provider.Cache.IndexusProviderBase;

                var guid = Guid.NewGuid().ToString();
                cache.Add(guid, new Person { FirstName = "maoyong", LastName = guid });
                Person value = cache.Get<Person>(guid);

                Console.WriteLine(value.FirstName + value.LastName);
                Console.Write(cache.GetAllKeys().Count);

                Thread.Sleep(10);
            }
        }
    }

    #region Commented Code
    //static object bulkObject = new object();
    //protected Thread runThread;
    //static DateTime start = DateTime.Now;
    //static List<string> li = new List<string>();
    //static int threadAmount = 5;
    //static int loopAmount = 280;
    //static int threadSleep = 40;
    //static int countCalls = 0;
    //static int successReceived = 0;
    //static int failedReceived = 0;

    // country [1 - 275]
    // region [1 - 5399] 

    //  #region
    //if (args != null && args.Length > 2)
    //{
    //  if (!string.IsNullOrEmpty(args[0]))
    //  {
    //    threadAmount = Convert.ToInt32(args[0]);
    //  }

    //  if (!string.IsNullOrEmpty(args[1]))
    //  {
    //    loopAmount = Convert.ToInt32(args[1]);
    //  }

    //  if (!string.IsNullOrEmpty(args[2]))
    //  {
    //    threadSleep = Convert.ToInt32(args[2]);
    //  }
    //}
    //#endregion
    // BLL.BllRegion region = new BLL.BllRegion();
    // List<Common.Region> regions = region.GetAll();

    //DAL.DalCountry country = new DAL.DalCountry();
    //List<Common.Country> data = country.GetAllCountry();

    //Console.WriteLine(@"Count of data: " + data.Count.ToString());
    //data = country.GetCountryByName(@"Germany");

    //data = country.GetCountryById(91);

    //// DAL.DalRegion region = new DAL.DalRegion();
    // region.GetAllCountry();

    //Console.WriteLine(@"Welcome to the test Application!");
    //Console.WriteLine(COM.Handler.Config.DisplayAppSettings());
    //Console.WriteLine("");
    //bool doBreak = false;
    //do
    //{

    //  switch (Console.ReadLine())
    //  {
    //    case "0":
    //      {
    //        Console.Clear();
    //        break;
    //      }
    //    case "1":
    //      {
    //        OneThread(1, true);
    //        break;
    //      }
    //    case "2":
    //      {
    //        OneThread(0, true);
    //        break;
    //      }
    //    case "100":
    //      {
    //        JustAddObjects();
    //        break;
    //      }
    //    case "101":
    //      {
    //        JustAddStringObjects();
    //        break;
    //      }
    //    case "21":
    //      {
    //        List<string> list = COM.CacheUtil.GetAllKeys();
    //        if (list != null)
    //        {
    //          foreach (string n in list)
    //          {
    //            COM.CacheUtil.Remove(n, string.Empty);
    //            Console.WriteLine("Delete cache object with Key: {0}", n);
    //          }
    //        }
    //        break;
    //      }
    //    case "22":
    //      {
    //        OneThread(2, false);
    //        break;
    //      }
    //    case "23":
    //      {
    //        GetSpecificObject();
    //        break;
    //      }
    //    case "24":
    //      {
    //        RandomizeGetSpecificObject();
    //        break;
    //      }
    //    case "3":
    //      {
    //        MultibleThread();
    //        break;
    //      }
    //    case "4":
    //      {
    //        CleanUp();
    //        break;
    //      }
    //    case "5":
    //      {
    //        Console.Clear();
    //        Console.WriteLine(COM.CacheUtil.Statistic(null));
    //        break;
    //      }
    //    case "50":
    //      {
    //        Console.Clear();
    //        COM.SystemManagement.Memory.LogMemoryData();
    //        do
    //        {
    //          COM.SystemManagement.Cpu.LogCpuData();
    //          Thread.Sleep(150);
    //        } while (true);

    //        break;
    //      }
    //    case "6":
    //      {
    //        Console.Clear();
    //        List<string> list = COM.CacheUtil.GetAllKeys();
    //        if (list.Count == 0)
    //        {
    //          Console.WriteLine("No key's available");
    //          break;
    //        }
    //        foreach (string n in list)
    //          Console.WriteLine(n);
    //        break;
    //      }
    //    case "8":
    //      {
    //        try
    //        {
    //          OneThreadWithProviders(0, false);
    //        }
    //        catch (Exception exc)
    //        {
    //          int i = 0;
    //        }
    //        break;
    //      }
    //      break;
    //    case "9":
    //      {
    //        doBreak = true;
    //        break;
    //      }
    //  }
    //  if (doBreak)
    //    break;
    //} while (true);

    //Console.WriteLine(@"The Application shutdowns");
    //Thread.Sleep(2500);
}


//private static void RandomizeGetSpecificObject()
//{
//  try
//  {
//    List<string> list = COM.CacheUtil.GetAllKeys();
//    if (list != null)
//    {

//      for (int i = 0; i < 100; ++i)
//      {
//        int d = new Random().Next(list.Count);
//        object o = COM.CacheUtil.Get<object>(list[d], null);
//        if(o is UserProfil)
//          Console.WriteLine((o as UserProfil).ToString());
//        else
//        {
//          if (o != null)
//          {
//            Console.Write(@"data received");
//          }
//          else
//          {
//            Console.Write(@"object is null :S");
//          }
//        }

//      }
//    }
//    Console.WriteLine("press enter to go on");
//  }
//  catch (Exception ex)
//  {
//    Console.WriteLine("An exception appears, be careful ;-): " + ex.Message);
//  }
//}

//private static void GetSpecificObject()
//{
//  try
//  {
//    Console.WriteLine("Enter your requestd Cache Key you like to retrieve: ");

//    string key = Console.ReadLine();

//    UserProfil data = COM.CacheUtil.Get<UserProfil>(key, null);
//    if(data == null)
//    {
//      Console.WriteLine(@"Data is null!!!");
//    }
//    else
//    {
//      Console.WriteLine(data.ToString());
//    }
//    Console.WriteLine("press enter to go on");
//    Console.ReadLine();
//  }
//  catch (Exception ex)
//  {
//    Console.WriteLine("An exception appears, be careful ;-): " + ex.Message);
//  }

//}

//private static void JustAddObjects()
//{
//  List<string> li = new List<string>();
//  UserProfil path;
//  for (int i = 1; i <= 50; i++)
//  {
//    path = new UserProfil();
//    string key = Guid.NewGuid().ToString();
//    // string key = i.ToString();
//    COM.CacheUtil.Add(key, path, DateTime.Now.AddMinutes(1));
//    Console.WriteLine("Add item {0} of {1}", i, 50);
//    li.Add(key);
//  }
//}

//private static void JustAddStringObjects()
//{
//  List<string> li = new List<string>();

//  for (int i = 1; i <= 50; i++)
//  {
//    char[] value = new char[new Random().Next(100, 200)];
//    string data = new string(value);
//    string key = Guid.NewGuid().ToString();
//    COM.CacheUtil.Add(key, data, DateTime.Now.AddMinutes(1));
//    Console.WriteLine("Add item {0} of {1}", i, 50);
//    li.Add(key);
//  }
//}

//private static void OneThread(int loopAmountToRun, bool useUserProfilObject)
//{
//  if (loopAmountToRun == 0)
//    loopAmountToRun = 100;

//  Console.WriteLine("To start client press enter. It will loop for: {0} times.", loopAmountToRun);
//  Console.ReadLine();

//  List<string> li = new List<string>();
//  UserProfil path;
//  for (int i = 1; i <= loopAmountToRun; i++)
//  {
//    string data = string.Empty;
//    path = new UserProfil();
//    data = DateTime.Now.Ticks.ToString();

//    string key = Guid.NewGuid().ToString();
//    if (useUserProfilObject)
//      COM.CacheUtil.Add(key, path, DateTime.Now.AddMinutes(10), null, SharedCache.WinServiceCommon.IndexusMessage.CacheItemPriority.High);
//    else
//      COM.CacheUtil.Add(key, data, DateTime.Now.AddMinutes(10));

//    Console.WriteLine("Add item {0} of {1}", i, loopAmountToRun);
//    li.Add(key);
//  }

//  Console.WriteLine("--------------------------End Add------------------------");
//  Console.WriteLine("");
//  Console.WriteLine(COM.CacheUtil.Statistic(null));
//  Console.WriteLine("press enter to get objects");

//  Console.ReadLine();
//  int cntr = 1;
//  foreach (string k in li)
//  {
//    Console.WriteLine(@"Receiving Item {0} of Total {1}", cntr++, li.Count);
//    UserProfil p = null;
//    string d = null;
//    if (useUserProfilObject)
//    {
//      p = COM.CacheUtil.Get<UserProfil>(k.ToString(), string.Empty);
//      if (p != null)
//      {
//        Console.WriteLine(p.ToString());
//      }
//      else
//      {
//        Console.WriteLine("The Key: {0} couldn't be readed from the cache!", k);
//      }
//    }
//    else
//    {
//      d = COM.CacheUtil.Get<string>(k.ToString(), string.Empty);
//      if (d != null)
//      {
//        Console.WriteLine(d);
//      }
//      else
//      {
//        Console.WriteLine("The Key: {0} couldn't be readed from the cache!", k);
//      }
//    }
//  }

//  Console.WriteLine("--------------------------End Get------------------------");
//  Console.WriteLine("");
//  Console.WriteLine(COM.CacheUtil.Statistic(null));
//  Console.WriteLine("press enter to remove key's");
//  Console.ReadLine();

//  cntr = 0;
//  foreach (string k in li)
//  {
//    Console.WriteLine(@"Removeing Item {0} of Total {1}", cntr++, li.Count);
//    COM.CacheUtil.Remove(k, string.Empty);
//  }

//  Console.WriteLine(COM.CacheUtil.Statistic(null));
//}

///// <summary>
///// Called when [thread with providers].
///// </summary>
///// <param name="loopAmountToRun">The loop amount to run.</param>
///// <param name="useUserProfilObject">if set to <c>true</c> [use X path object].</param>
//private static void OneThreadWithProviders(int loopAmountToRun, bool useUserProfilObject)
//{

//  if (loopAmountToRun == 0)
//    loopAmountToRun = 100;

//  Console.WriteLine("To start client press enter. It will loop for: {0} times.", loopAmountToRun);
//  Console.ReadLine();

//  List<string> li = new List<string>();
//  UserProfil path;
//  for (int i = 1; i <= loopAmountToRun; i++)
//  {
//    string data = string.Empty;
//    path = new UserProfil();
//    data = DateTime.Now.Ticks.ToString();

//    string key = Guid.NewGuid().ToString();
//    if (useUserProfilObject)
//      IndexusDistributionCache.CurrentProvider.Add(key, path, DateTime.Now.AddMinutes(10));
//    else
//      IndexusDistributionCache.CurrentProvider.Add(key, data, DateTime.Now.AddMinutes(10));

//    Console.WriteLine("Add item {0} of {1}", i, loopAmountToRun);
//    li.Add(key);
//  }

//  Console.WriteLine("--------------------------End Add------------------------");
//  Console.WriteLine("");
//  Console.WriteLine(COM.CacheUtil.Statistic(null));
//  Console.WriteLine("press enter to get objects");

//  Console.ReadLine();
//  int cntr = 1;
//  foreach (string k in li)
//  {
//    Console.WriteLine(@"Receiving Item {0} of Total {1}", cntr++, li.Count);
//    UserProfil p = null;
//    string d = null;
//    if (useUserProfilObject)
//    {
//      p = IndexusDistributionCache.CurrentProvider.Get<UserProfil>(k.ToString());
//      if (p != null)
//      {
//        Console.WriteLine(p.ToString());
//      }
//      else
//      {
//        Console.WriteLine("The Key: {0} couldn't be readed from the cache!", k);
//      }
//    }
//    else
//    {
//      d = IndexusDistributionCache.CurrentProvider.Get<string>(k.ToString());
//      if (d != null)
//      {
//        Console.WriteLine(d);
//      }
//      else
//      {
//        Console.WriteLine("The Key: {0} couldn't be readed from the cache!", k);
//      }
//    }			
//  }

//  Console.WriteLine("--------------------------End Get------------------------");
//  Console.WriteLine("");
//  Console.WriteLine(COM.CacheUtil.Statistic(null));
//  Console.WriteLine("press enter to remove key's");
//  Console.ReadLine();

//  cntr = 0;
//  foreach (string k in li)
//  {
//    Console.WriteLine(@"Removeing Item {0} of Total {1}", cntr++, li.Count);
//    IndexusDistributionCache.CurrentProvider.Remove(k);
//  }

//  Console.WriteLine(COM.CacheUtil.Statistic(null));
//}

//private static void MultibleThread()
//{
//  #region Access Log
//  COM.Handler.LogHandler.Tracking(
//    "Access Method: " + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;"
//  );
//  #endregion Access Log

//  ThreadStart job = new ThreadStart(ThreadJob);
//  List<Thread> threads = new List<Thread>();
//  for (int i = 0; i < threadAmount; i++)
//  {
//    Thread thread = new Thread(job);
//    thread.Name = (i + 1).ToString();
//    threads.Add(thread);
//    Console.WriteLine("Thread no: " + (i + 1).ToString() + " have been added to list");
//  }
//  // start the threads.
//  foreach (Thread t in threads) { t.Start(); }
//}

//private static void CleanUp()
//{
//  Console.WriteLine("CleanUp Start!");
//  if (li.Count > 0)
//  {
//    foreach (string k in li)
//    {
//      COM.CacheUtil.Remove(k, string.Empty);
//    }
//  }
//  Console.WriteLine("CleanUp End!");
//}

//private static void ThreadJob()
//{
//  #region Access Log
//  COM.Handler.LogHandler.Tracking(
//    "Access Method: " + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;"
//  );
//  #endregion Access Log
//  bool commets = false;
//  if (commets)
//    Console.WriteLine(Thread.CurrentThread.Name + " started to execute!");

//  UserProfil testObject = null;
//  string key = string.Empty;
//  for (int i = 0; i < loopAmount; i++)
//  {
//    Console.WriteLine("Thread No.:{0}; Loop No.: {1} of Total:{2}", Thread.CurrentThread.Name, i, loopAmount);

//    key = Guid.NewGuid().ToString();
//    testObject = new UserProfil();
//    testObject.ID = i;

//    if (commets)
//      Console.WriteLine(Thread.CurrentThread.Name + " is adding data Object: {0}",
//        testObject.ID.ToString());

//    // Add to cache
//    COM.CacheUtil.Add(key, testObject, DateTime.Now.AddMinutes(15));

//    li.Add(key);

//    // Receive data back from cache
//    try
//    {
//      testObject = COM.CacheUtil.Get<UserProfil>(key, string.Empty);

//      if (testObject == null)
//      {
//        Console.WriteLine(@"Key {0} is null", key);
//        Monitor.Enter(bulkObject);
//        li.Add(key);
//        failedReceived++;
//        Monitor.Exit(bulkObject);
//      }
//      else
//      {
//        Console.WriteLine(
//              Thread.CurrentThread.Name +
//              " received the following data: " +
//              Environment.NewLine + "{0}",
//              testObject.ToString()
//            );
//        Monitor.Enter(bulkObject);
//        successReceived++;
//        Monitor.Exit(bulkObject);
//      }
//    }
//    catch (Exception ex)
//    {
//      Console.WriteLine("An Error encounterd: " + ex.Message + Environment.NewLine + ex.StackTrace);
//      Console.ReadLine();
//    }
//    finally
//    {
//      threadSleep = new Random().Next(400, 2500);
//      if (threadSleep > 0)
//        Thread.Sleep(threadSleep);
//    }
//  }
//}
    #endregion Commented Code