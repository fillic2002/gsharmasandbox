using System;
using System.Collections.Generic;
using System.Text;
using Equity;
using Git_Sandbox.DailyRunJob.DATA;
using api= myfinAPI.Data;
using WebScrapper = Git_Sandbox.DailyRunJob.DATA.WebScrapper;

namespace Git_Sandbox.DailyRunJob
{
	public static class component
	{
		static MysqlHelper _mysqlObj;
		static GenericFunc _gnrObj;
		static WebScrapper _webObj;
		static ExcelHelper _excelHelper;
		static Procurement _eprocObj;
		static api.mysqlContext _sqlContext;

		public static api.mysqlContext getDBContextObj()
		{
			if (_sqlContext == null)
				_sqlContext = new api.mysqlContext();

			return _sqlContext;
		}
		public static Procurement getEprocObj()
		{
			if (_eprocObj == null)
				_eprocObj = new Procurement();

			return _eprocObj;
		}
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
		public static ExcelHelper getExcelHelperObj()
		{
			if (_excelHelper == null)
				_excelHelper = new ExcelHelper();

			return _excelHelper;
		}

	}
}
