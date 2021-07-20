using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Equity;
using Git_Sandbox.DailyRunJob;
using Git_Sandbox.DailyRunJob.Common;
using Git_Sandbox.Model;

namespace DailyRunEquity
{
	public class Eqhelper
	{
		private GenericFunc _htmlHelper;
		private Dictionary<int, double> _currentNav;
		private string _mfHistoricalNav ="https://www.amfiindia.com/net-asset-value/nav-history";
		IList<equityHistory> _eqHistory;
		//ExcelHelper _excelHelper;
		//excelhelpernew _excelHelper;

		public enum CompanyName
		{
			BEL=0,
			GAIL=1,
			HAL=2,
			SRIKALAHASTI=3,
			PETRONETLNG=4,
			BALMERLAWRIE=5,
			KOVAIMEDICAL=6,
			TATACHEMICAL=7,
			MAHANAGAR=8,
			NESCO=9,
			POWERGRID=10,
			ONGC=11,
			GIC=12,
			NIACL=13,
			BPCL=14
		}
		static IList<equity> equity=new List<equity>();
	
		static readonly HttpClient s_client = new HttpClient
		{
			MaxResponseContentBufferSize = 1_000_000
		};
		public Eqhelper()
		{
			_htmlHelper = new GenericFunc();
		 
			equity = component.getMySqlObj().GetEquityNavUrl();
			_eqHistory = new List<equityHistory>();
		}
		/// <summary>
		/// This function is going to get live NAV for the shares and update in the db table
		/// </summary>
		/// <returns></returns>
		public async Task UpdateShareCurrentPrice()
		{
			var stopwatch = Stopwatch.StartNew();
			IEnumerable<Task<equity>> downloadTasksQuery =
				   from item in equity
				   select ProcessUrlAsync(item);

			List<Task<equity>> downloadTasks = downloadTasksQuery.ToList();

			//int total = 0;
			while (downloadTasks.Any())
			{
				Task<equity> finishedTask = await Task.WhenAny(downloadTasks);
				downloadTasks.Remove(finishedTask);
				if (finishedTask.Status != TaskStatus.Faulted)
				{				 
					Console.WriteLine("DB Update::" +  component.getMySqlObj().UpdateLatesNAV(finishedTask.Result));
				}
				
				//total += await finishedTask;
			}
			stopwatch.Stop();

			
			Console.WriteLine($"Elapsed time:          {stopwatch.Elapsed}\n");
			 
			Console.WriteLine("Saved all records:" + DateTime.Now.ToString("hh.mm.ss.ffffff"));
			 
		}
		public void ReadNewExcel()
		{
			excelhelpernew.ReadExcelFile();
		}
		async Task<equity> ProcessUrlAsync(equity item)
		{
			item.LivePrice = await _htmlHelper.GetAssetNAVAsync(item);
			//Console.WriteLine(val);			
			return item;
		}

		public void AddDividendDetails()
		{
			IList<dividend> listCompanies = new List<dividend>();

			component.getMySqlObj().GetStaleDividendCompanies(listCompanies);

			IList<equity> Listurl = component.getGenericFunctionObj().GetEquityLinks();

			//Check companies whose dividend details not updated in last 30 days
			foreach(dividend u in listCompanies)
			{
			
				component.getMySqlObj().getLastDividendOfCompany(u);
				if (DateTime.Now.Subtract(u.dt).TotalDays >= 90 && DateTime.Now.Subtract(u.lastCrawledDate).TotalDays >= 30)
				{
					component.getWebScrappertObj().GetDividend(u, Listurl.First<equity>(x => x.ISIN == u.companyid));
				}
			}			
		}

		public void UpdateAssetHistory()
		{
			IList<Portfolio> folioDetail = new List<Portfolio>();
			component.getMySqlObj().GetPortFolio(folioDetail);
			

			foreach (Portfolio p in folioDetail)
			{
			IList<dividend> dividendDetails = new List<dividend>();

				IList<EquityTransaction> transaction = new List<EquityTransaction>();
				component.getMySqlObj().GetTransactions(transaction, p.folioId);
				component.getMySqlObj().GetCompaniesDividendDetails(dividendDetails, p.folioId);

				bool stopY = false;		
				for (int y = 2021; y <= 2021; y++)
				{
					if (DateTime.Now.Year == y)
						stopY = true;

					for (int m = 1; m <= 12; m++)
					{
						if (stopY == false || (stopY == true && DateTime.Now.Month >= m))
						{							 
							UpdateMonthlyShareCashFlow(m, y, p, transaction,dividendDetails);
							//UpdateMonthlyMFCashFlow(m, y, p, transaction, AssetType.EquityMF);
							//UpdateMonthlyMFCashFlow(m, y, p, transaction, AssetType.DebtMF);
						}
					}
				}				
			}
		}
		public void UpdateMonthlyMFCashFlow(int month, int year, Portfolio p, IList<EquityTransaction> t, AssetType astType)
		{

			AssetHistory history;
			Dictionary<string, double> equities = new Dictionary<string, double>();
			IList<dividend> dividendDetails = new List<dividend>();
			IList<Portfolio> folioDetail = new List<Portfolio>(); history = new AssetHistory();

			foreach (EquityTransaction eqt in t)
			{
				if (eqt.equity.assetType == astType && ((eqt.TransactionDate.Year == year && eqt.TransactionDate.Month <= month) ||
						eqt.TransactionDate.Year < year))
				{
					if (eqt.TypeofTransaction == 'B')
					{
						history.Investment += eqt.price * eqt.qty;
						history.AssetValue += GetMonthlyPrice(eqt.equity.ISIN, month, year,eqt.equity.Companyname,astType) * eqt.qty;
				
						if (!equities.ContainsKey(eqt.equity.ISIN))
						{
							equities.Add(eqt.equity.ISIN, 0);
						}
					}
					else
					{
						history.Investment -= eqt.price * eqt.qty;
						history.AssetValue -= GetMonthlyPrice(eqt.equity.ISIN, month, year, eqt.equity.Companyname,astType) * eqt.qty;
					}
				}
			}

			history.portfolioId = p.folioId;
			history.assetType = (int)astType;
			history.Dividend = 0;			
			history.qurarter = month;
			history.year = year;
			if (history.Investment != 0)
			{
				Console.WriteLine("Save Record for Portfolio:" + p.folioId + " For Year: " + year + " Month:" + month);
				component.getMySqlObj().AddAssetSnapshot(history);
			}

		}

		public void UpdateMonthlyShareCashFlow(int month, int year, Portfolio p, IList<EquityTransaction> t, IList<dividend> d)
		{
		 
			AssetHistory history;
			Dictionary<string, double> equities = new Dictionary<string, double>();
			IList<dividend> dividendDetails = new List<dividend>();
			IList<Portfolio> folioDetail = new List<Portfolio>();history = new AssetHistory();
			 
			foreach (EquityTransaction eqt in t)
			{
				if (eqt.equity.assetType == AssetType.Shares && ((eqt.TransactionDate.Year == year && eqt.TransactionDate.Month<=month) || eqt.TransactionDate.Year<year))
				{
					if (eqt.TypeofTransaction == 'B')
					{
						history.Investment += eqt.price * eqt.qty;
						history.AssetValue += GetMonthlyPrice(eqt.equity.ISIN,month, year,eqt.equity.Companyname, eqt.equity.assetType) * eqt.qty;
						history.portfolioId = eqt.portfolioId;
						history.assetType = (int)AssetType.Shares; 

						if (!equities.ContainsKey(eqt.equity.ISIN))
						{
							equities.Add(eqt.equity.ISIN, 0);
						}
					}
					else
					{
						history.Investment -= eqt.price * eqt.qty;
						history.AssetValue -= GetMonthlyPrice(eqt.equity.ISIN, month, year,eqt.equity.Companyname, eqt.equity.assetType) * eqt.qty;
					}
				}
			}
				 

			double dividend=0;
			foreach (dividend div in d)
			{
				int qty = 0;
				if ((div.dt.Month <= month && div.dt.Year == year) || div.dt.Year < year)
				{
					foreach (EquityTransaction tran in t)
					{
						if (tran.equity.ISIN == div.companyid && tran.TransactionDate < div.dt)
						{
							if (tran.TypeofTransaction == 'B')
								qty += tran.qty;
							else
								qty -= tran.qty;
						}
					}						 
				}
				if (qty > 0)
				{
					equities[div.companyid] += qty * div.value;
					dividend += qty * div.value;					
				}
			}
			

			history.Dividend = dividend;
			history.assetType = (int)AssetType.Shares;
			history.qurarter = month;
			history.year = year;
			if (history.AssetValue != 0)
			{
				Console.WriteLine("Save Record for Portfolio:" + p.folioId + " For Year: " + year + " Month:" + month);
				component.getMySqlObj().AddAssetSnapshot(history);
			}

		}

		public double GetMonthlyPrice(string ISIN,int month, int year,string companyname, AssetType assetType)
		{
			double itemPrice = 0;
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Getting Monthly price for:" + companyname + " for the month of :" + month + "-" + year);
			Console.ForegroundColor = ConsoleColor.White;

			//Search from nav table
			if (month == DateTime.Now.Month && year == DateTime.Now.Year)
			{
				return component.getMySqlObj().GetLatesNAV(ISIN);
			}
			else if(year >=2021 && month>=1)
			{
				if(_eqHistory.Count>0)
				{					 
					var item=_eqHistory.FirstOrDefault(y => y.equityid == ISIN && y.month==month && y.year==year);
					if(item!=null && item.price >0)
					{
						itemPrice= item.price;
					}
					else
					{
						itemPrice=updateequityprice(  ISIN,   month,   year,   assetType,   companyname);
					}
				}
				else
				{
					itemPrice=updateequityprice(ISIN, month, year, assetType, companyname);					 
				}
				 
				return itemPrice;
			}
			else
			{
				return component.getExcelHelperObj().GetMonthlySharePrice(ISIN, month, year);
			}
		}

		private double updateequityprice(string ISIN,int month,int year,AssetType assetType,string companyname)
		{
			double itemPrice;
			IDictionary<int, double> montlyPrice = new Dictionary<int, double>();

			itemPrice = component.getMySqlObj().GetHistoricalSharePrice(ISIN,month,year);
			if (itemPrice == 0)
			{				 
				montlyPrice = component.getWebScrappertObj().GetHistoricalAssetPrice(companyname, month, year, assetType);
			}
			foreach(int key in montlyPrice.Keys)
			{
				equityHistory eq = new equityHistory()
				{
					month = key,
					year = year,
					price = montlyPrice[key],
					equityid = ISIN,
					assetType = Convert.ToInt32(assetType)
				};
				if(key==month)
				{
					itemPrice= montlyPrice[key];
				}
				_eqHistory.Add(eq);
				component.getMySqlObj().UpdateEquityMonthlyPrice(eq);
			}
			return itemPrice;
		}
	}
}
