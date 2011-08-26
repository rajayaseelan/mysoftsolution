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

//// *************************************************************************
////
//// Name:      MyLargeTestObject.cs
//// 
//// Created:   22-01-2007 SharedCache.com, rschuetz
//// Modified:  22-01-2007 SharedCache.com, rschuetz : Creation
//// Modified:  31-12-2007 SharedCache.com, rschuetz : changed class name
//// ************************************************************************* 

//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace SharedCache.WinServiceTestClient
//{
//  [Serializable]
//  public class UserProfil
//  {
	
//    // some data to extend the size :-) - use same size like b1!!!
//    public byte[] b1 = new byte[20480];
//    public byte[] b2 = new byte[20480];
//    public byte[] b3 = new byte[20480];
//    public byte[] b4 = new byte[20480];
//    public byte[] b5 = new byte[20480];
	
//    #region Property: Name
//    private string name;
		
//    /// <summary>
//    /// Gets/sets the Name
//    /// </summary>
//    public string Name
//    {
//      [System.Diagnostics.DebuggerStepThrough]
//      get  { return this.name;  }
			
//      [System.Diagnostics.DebuggerStepThrough]
//      set { this.name = value;  }
//    }
//    #endregion
		
//    #region Property: ID
//    private int iD;
		
//    /// <summary>
//    /// Gets/sets the ID
//    /// </summary>
//    public int ID
//    {
//      [System.Diagnostics.DebuggerStepThrough]
//      get  { return this.iD;  }
			
//      [System.Diagnostics.DebuggerStepThrough]
//      set { this.iD = value;  }
//    }
//    #endregion
		
//    #region Property: GuidID
//    private System.Guid guidID;
		
//    /// <summary>
//    /// Gets/sets the GuidID
//    /// </summary>
//    public System.Guid GuidID
//    {
//      [System.Diagnostics.DebuggerStepThrough]
//      get  { return this.guidID;  }
			
//      [System.Diagnostics.DebuggerStepThrough]
//      set { this.guidID = value;  }
//    }
//    #endregion
		
//    #region Property: Init
//    private DateTime init;
		
//    /// <summary>
//    /// Gets/sets the Init
//    /// </summary>
//    public DateTime Init
//    {
//      [System.Diagnostics.DebuggerStepThrough]
//      get  { return this.init;  }
			
//      [System.Diagnostics.DebuggerStepThrough]
//      set { this.init = value;  }
//    }
//    #endregion
		
//    #region Property: Ticks
//    private long ticks;
		
//    /// <summary>
//    /// Gets/sets the Ticks
//    /// </summary>
//    public long Ticks
//    {
//      [System.Diagnostics.DebuggerStepThrough]
//      get  { return this.ticks;  }
			
//      [System.Diagnostics.DebuggerStepThrough]
//      set { this.ticks = value;  }
//    }
//    #endregion

//    public UserProfil()
//    {
//      Random r = new Random();
//      for (int i = 0; i < b1.Length; i++)
//      {
//        int bb = r.Next(65, 97);
//        b1[i] = Convert.ToByte(bb);
//        b2[i] = b1[i];
//        b3[i] = b1[i];
//        b4[i] = b1[i];
//        b5[i] = b1[i];
//      }
		
//      Random random = new Random();
//      this.name = "I'm a MyLargeTestObject Sample Class!";
//      this.iD = random.Next(50, 23233);
//      this.guidID = Guid.NewGuid();
//      this.init = DateTime.Now;
//      this.ticks = this.init.Ticks;
//    }

//    public override string ToString()
//    {
//      string result =
//        @"Name: - {0}" + Environment.NewLine +
//        @"ID: - {1}" + Environment.NewLine +
//        @"GuidId: - {2}" + Environment.NewLine +
//        @"Init: - {3}" + Environment.NewLine +
//        @"Ticks: - {4}" + Environment.NewLine;
				
//      return string.Format(
//                result, 
//                this.Name, 
//                this.ID.ToString(), 
//                this.GuidID.ToString(), 
//                this.Init.ToLocalTime(), 
//                this.Ticks.ToString()
//              );
//    }
//  }
//}
