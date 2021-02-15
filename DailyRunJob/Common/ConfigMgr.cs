using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Git_Sandbox.DailyRunJob.Common
{
	public static class ConfigMgr
	{
		public static string SAVINGMODE = ConfigurationManager.AppSettings["Destination"];
	}
}
