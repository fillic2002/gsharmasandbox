using System.Configuration;

namespace Git_Sandbox.DailyRunJob.Common
{
	public static class ConfigMgr
	{
		public static string SAVINGMODE = ConfigurationManager.AppSettings["Destination"];
	}
}
