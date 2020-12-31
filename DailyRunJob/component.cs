using System;
using System.Collections.Generic;
using System.Text;

namespace Git_Sandbox.DailyRunJob
{
	public static class component
	{
		public static MysqlHelper mysqlObj;

		public static MysqlHelper getMySqlObj()
		{
			if (mysqlObj == null)
				mysqlObj = new MysqlHelper();

			return mysqlObj;
		}
	}
}
