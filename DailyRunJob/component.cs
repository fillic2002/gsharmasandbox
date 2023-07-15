using System;
using System.Collections.Generic;
using System.Text;
using Equity;
using Git_Sandbox.DailyRunJob.Business;
using Git_Sandbox.DailyRunJob.DATA;
using api= myfinAPI.Data;
using business = myfinAPI.Business;
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
		static BondsHelper _bondObject;
		static api.BondsContext _bondContext;

		// Borrowed from Myfin binary
		static business.BondsHelper _bondBusinessHelper;
		static business.Equity _equityBusinessHelper;
		static business.PortfoliMgmt _portfolioHelper;
		
		static Git_Sandbox.DailyRunJob.Business.Expense _expenselHelper;
		

		public static business.PortfoliMgmt getPortfolioHelperObj()
		{
			if (_portfolioHelper == null)
				_portfolioHelper = new business.PortfoliMgmt();

			return _portfolioHelper;
		}
		public static business.Equity getEquityBusinessHelperObj()
		{
			if (_equityBusinessHelper == null)
				_equityBusinessHelper = new business.Equity();

			return _equityBusinessHelper;
		}
		public static business.BondsHelper getBondBusinessHelperObj()
		{
			if (_bondBusinessHelper == null)
				_bondBusinessHelper = new business.BondsHelper();

			return _bondBusinessHelper;
		}
		public static api.BondsContext getBondContextObj()
		{
			if (_bondContext == null)
				_bondContext = new api.BondsContext();

			return _bondContext;
		}
		public static BondsHelper getBondsObj()
		{
			if (_bondObject == null)
				_bondObject = new BondsHelper();

			return _bondObject;
		}
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
		public static Expense getExpenseHelperObj()
		{
			if (_expenselHelper == null)
				_expenselHelper = new Expense();

			return _expenselHelper;
		}

	}
}
