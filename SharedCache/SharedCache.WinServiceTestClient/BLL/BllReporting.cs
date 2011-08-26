using System;
using System.Collections.Generic;
using System.Text;

namespace SharedCache.WinServiceTestClient.BLL
{
	class BllReporting
	{
		public static void Save(SharedCache.WinServiceTestClient.Common.Reporting report)
		{
			DAL.DalReporting.Save(report);
		}
	}
}
