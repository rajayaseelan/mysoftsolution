using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SharedCache.WinServiceTestClient.Common
{
	[Serializable]
	public class Report
	{
		public List<Reporting> List = new List<Reporting>();
	}

	[Serializable]
	public class Reporting
	{
		#region Property: runDateTime
		private DateTime runDateTime;
		
		/// <summary>
		/// Gets/sets the RunId
		/// </summary>
		[XmlAttribute("RunDateTime")]
		public DateTime RunDateTime
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.runDateTime; }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.runDateTime = value; }
		}
		#endregion
		#region Property: ReportingOption
		private List<ReportingOption> reportingOption;
		
		/// <summary>
		/// Gets/sets the ReportingOption
		/// </summary>
		public List<ReportingOption> ReportingOption
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.reportingOption;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.reportingOption = value;  }
		}
		#endregion
		#region Property: VersionNumber
		private string versionNumber;
		
		/// <summary>
		/// Gets/sets the VersionNumber
		/// </summary>
		public string VersionNumber
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.versionNumber;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.versionNumber = value;  }
		}
		#endregion
	}

	[Serializable]
	public class ReportingOption
	{
		#region Property: runDateTime
		private DateTime runDateTime;

		/// <summary>
		/// Gets/sets the RunId
		/// </summary>
		[XmlAttribute("RunDateTime")]
		public DateTime RunDateTime
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.runDateTime; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.runDateTime = value; }
		}
		#endregion
		#region Property: LoggingEnabled 
		private bool loggingEnabled ;
		
		/// <summary>
		/// Gets/sets the LoggingEnabled 
		/// </summary>
		public bool LoggingEnabled 
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.loggingEnabled ;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.loggingEnabled  = value;  }
		}
		#endregion
		#region Property: OneThread
		private bool oneThread;
		
		/// <summary>
		/// Gets/sets the OneThread
		/// </summary>
		public bool OneThread
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.oneThread;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.oneThread = value;  }
		}
		#endregion
		#region Property: ZipEnabled
		private bool zipEnabled;
		
		/// <summary>
		/// Gets/sets the ZipEnabled
		/// </summary>
		public bool ZipEnabled
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.zipEnabled;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.zipEnabled = value;  }
		}
		#endregion
		#region Property: Option
		private string option;
		
		/// <summary>
		/// Gets/sets the Option
		/// </summary>
		public string Option
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.option;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.option = value;  }
		}
		#endregion
		#region Property: HashingAlgorithm
		private string hashingAlgorithm;
		
		/// <summary>
		/// Gets/sets the HashingAlgorithm
		/// </summary>
		public string HashingAlgorithm
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.hashingAlgorithm;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.hashingAlgorithm = value;  }
		}
		#endregion
		#region Property: NeededAddTime
		private long neededAddTime;
		
		/// <summary>
		/// Gets/sets the NeededAddTime
		/// </summary>
		public long NeededAddTime
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.neededAddTime;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.neededAddTime = value;  }
		}
		#endregion
		#region Property: NeededGetTime
		private long neededGetTime;
		
		/// <summary>
		/// Gets/sets the NeededGetTime
		/// </summary>
		public long NeededGetTime
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.neededGetTime;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.neededGetTime = value;  }
		}
		#endregion
		#region Property: NeededDelTime		
		private long neededDelTime;
		
		/// <summary>
		/// Gets/sets the NeededDelTime		
		/// </summary>
		public long NeededDelTime		
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.neededDelTime		;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.neededDelTime		 = value;  }
		}
		#endregion
		#region Property: CompressionMinSize
		private int compressionMinSize;
		
		/// <summary>
		/// Gets/sets the CompressionMinSize
		/// </summary>
		public int CompressionMinSize
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.compressionMinSize;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.compressionMinSize = value;  }
		}
		#endregion
	}
}
