using System;
using System.Collections.Generic;
using System.Text;
using Equity;
using Git_Sandbox.DailyRunJob.DATA;

namespace Git_Sandbox.DailyRunJob
{
	public static class component
	{
		static MysqlHelper _mysqlObj;
		static GenericFunc _gnrObj;
		static WebScrapper _webObj;

		public static MysqlHelper getMySqlObj()
		{
			if (_mysqlObj == null)
				_mysqlObj = new MysqlHelper();

			return _mysqlObj;
		}
		public static GenericFunc getGenericFunctionObj()
		{
			if (_gnrObj == null)
				_gnrObj = new GenericFunc();

			return _gnrObj;
		}
		public static WebScrapper getWebScrappertObj()
		{
			if (_webObj== null)
				_webObj = new WebScrapper();

			return _webObj;
		}
		 
	}
}
