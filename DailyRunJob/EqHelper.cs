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
		public void UpdateAssetHistoryPrice()
		{
			IList<Portfolio> folioDetail = new List<Portfolio>();
			component.getMySqlObj().GetPortFolio(folioDetail);

			foreach (Portfolio p in folioDetail)
			{	
				IList<EquityTransaction> transaction = new List<EquityTransaction>();
				component.getMySqlObj().GetTransactions(transaction, p.folioId);
				foreach(EquityTransaction t in transaction)
				{					
					getMonthlyPrice(t);
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
							UpdateMonthlyShareSnapshot(m, y, p,transaction);
							UpdateMonthlyMFSnapshot(m, y, p, transaction, AssetType.EquityMF);
							UpdateMonthlyMFSnapshot(m, y, p, transaction, AssetType.DebtMF);
							UpdatePropertySnapshot(m,y,p,AssetType.Gold);
							UpdatePropertySnapshot(m, y, p, AssetType.Flat);
							UpdatePropertySnapshot(m, y, p, AssetType.Plot);
							UpdatePF_PPFSnapshot(m, y, p, AssetType.PPF);
							UpdateBankSnapshot(m, y, p, AssetType.Bank);
						}
					}
				}				
			}
		}
		public void UpdateBankSnapshot(int m, int y, Portfolio p, AssetType astType)
		{
			if (DateTime.Now.Month==m && DateTime.Now.Year==y)
				component.getMySqlObj().UpdateBankSnapshot(m, y, p.folioId);
		}
		public void UpdatePF_PPFSnapshot(int m,int y,Portfolio p,AssetType astType)
		{
			if((y==2021 & m==DateTime.Now.Month) &&(p.folioId==3 || p.folioId==2 ))
				component.getMySqlObj().UpdatePFSnapshot(m,y,p.folioId);
		}
		public void UpdatePropertySnapshot(int m, int y , Portfolio folio,AssetType typeofAsset)
		{
			AssetHistory history = new AssetHistory();
			IList<propertyTransaction> transaction = new List<propertyTransaction>();
			history.assetType = typeofAsset;
			history.month = m;
			history.year = y;
			history.portfolioId = folio.folioId;
			component.getMySqlObj().GetPropertyTransactions(transaction, folio.folioId);

			foreach (propertyTransaction pt in transaction.Where(x => x.astType == typeofAsset))
			{
				if ((pt.TransactionDate.Year == y && pt.TransactionDate.Month<=m)|| pt.TransactionDate.Year<y)
				{
					if (pt.TypeofTransaction == 'B')
					{
						history.AssetValue += pt.astvalue;
						history.Investment += pt.investment;
					}
					else
					{
						history.AssetValue -= pt.astvalue;
						history.Investment -= pt.investment;
					}
				}
			}
				 
			if (history.Investment > 0)
			{
				component.getMySqlObj().AddAssetSnapshot(history);
			}
		}
		private void UpdateMonthlyMFSnapshot(int month, int year, Portfolio p, IList<EquityTransaction> t, AssetType astType)
		{
			AssetHistory history=new AssetHistory(); ;
			history.portfolioId = p.folioId;
			history.assetType = astType;
			if (month >= 2)
			{
				history.month = month - 1;
				history.year = year;
			}
			else
			{
				history.month = 12;
				history.year = year - 1;
			}
			DateTime dt = new DateTime(year,month,28);
			//Previous month snapshot
			component.getMySqlObj().GetAssetSnapshot(history);
			UpdateMonthlyMFInvestment(history, t.Where(tm => tm.TransactionDate.Month == month && tm.TransactionDate.Year == year && tm.equity.assetType == astType).ToArray());
			UpdateMonthlyMFAssetValue(history, t.Where(tm => tm.TransactionDate.Month == month && tm.TransactionDate.Year == year && tm.equity.assetType == astType).ToArray(), month, year);
			if (history.Investment != 0)
			{
				history.month = month;
				history.year = year;
				component.getMySqlObj().AddAssetSnapshot(history);
			}
		}
		private void UpdateMonthlyMFInvestment(AssetHistory astHistory, IList<EquityTransaction> t)
		{
			foreach (EquityTransaction eqt in t)
			{
				if (eqt.TypeofTransaction == 'B')
				{
					astHistory.Investment += eqt.price * eqt.qty;					 
				}
				else
				{
					astHistory.Investment -= eqt.price * eqt.qty;					 
				}				
			}
		}
		//Get Asset value for a particualar month/year
		private void UpdateMonthlyMFAssetValue(AssetHistory astHistory, IList<EquityTransaction> t,int month, int year)
		{
			Dictionary<string, double> qty = new Dictionary<string, double>();
			AssetType typeofAsset=AssetType.Shares;
			foreach (EquityTransaction eqt in t)
			{
				if (!qty.ContainsKey(eqt.equity.ISIN))
				{
					qty.Add(eqt.equity.ISIN, 0);
				}
				if (eqt.TypeofTransaction == 'B')
				{					
					qty[eqt.equity.ISIN] += eqt.qty;
					typeofAsset = eqt.equity.assetType;
				}
				else
				{
					qty[eqt.equity.ISIN] -= eqt.qty;
					typeofAsset = eqt.equity.assetType;
				}
			}
			foreach(string key in qty.Keys)
			{
				astHistory.AssetValue+=qty[key]* GetMonthPrice(key, month, year, typeofAsset);
			}

		}

		private void UpdateMonthlyShareSnapshot(int month, int year, Portfolio p, IList<EquityTransaction> t)
		{
		 
			AssetHistory history=new AssetHistory();
			Dictionary<string, double> equities = new Dictionary<string, double>();
			IList<dividend> dividendDetails = new List<dividend>();
			IList<Portfolio> folioDetail = new List<Portfolio>();
			history.assetType = AssetType.Shares;
			history.month = month;
			history.year = year;

			component.getMySqlObj().GetAssetSnapshot(history);
			if (history.AssetValue > 0)
				return;

			foreach (EquityTransaction eqt in t)
			{
				if (eqt.equity.assetType == AssetType.Shares && ((eqt.TransactionDate.Year == year && eqt.TransactionDate.Month<=month) 
					|| eqt.TransactionDate.Year<year))
				{
					if (eqt.TypeofTransaction == 'B')
					{
						history.Investment += eqt.price * eqt.qty;
						history.AssetValue += GetMonthPrice(eqt.equity,month, year,AssetType.Shares) * eqt.qty;
						history.portfolioId = eqt.portfolioId;
						history.assetType = AssetType.Shares; 

						if (!equities.ContainsKey(eqt.equity.ISIN))
						{
							equities.Add(eqt.equity.ISIN, 0);
						}
					}
					else
					{
						history.Investment -= eqt.price * eqt.qty;
						history.AssetValue -= GetMonthPrice(eqt.equity, month, year,AssetType.Shares) * eqt.qty;
					}
				}
			}

			component.getMySqlObj().GetCompaniesDividendDetails(dividendDetails, p.folioId);

			double dividend=0;
			foreach (dividend div in dividendDetails)
			{
				IEnumerable<EquityTransaction> selectedTran= t.Where(n=>n.equity.ISIN == div.companyid);
				int qty = 0;
				if ((div.dt.Month <= month && div.dt.Year == year) || div.dt.Year < year)
				{
					foreach (EquityTransaction tran in selectedTran)
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
			 
			if (history.AssetValue != 0)
			{
				Console.WriteLine("Save Record for Portfolio:" + p.folioId + " For Year: " + year + " Month:" + month);
				component.getMySqlObj().AddAssetSnapshot(history);
			}
		}
		private double GetMonthPrice(string isin, int month, int year, AssetType typeAsset)
		{
			equity e = new equity() { ISIN = isin };
			component.getMySqlObj().GetCompanyDetails(e);
			return GetMonthPrice(e, month, year, typeAsset);
		}
		private double GetMonthPrice(equity e,int month, int year,AssetType typeAsset)
		{
			double itemPrice = 0;			 

			//Search from nav table
			if (month == DateTime.Now.Month && year == DateTime.Now.Year)
			{
				return component.getMySqlObj().GetLatesNAV(e.ISIN);
			}
			else if (year >= 2015 && month >= 1)
			{
				itemPrice = component.getMySqlObj().GetHistoricalSharePrice(e.ISIN, month, year);
				if (itemPrice <= 0)
				{					 
					EquityTransaction t = new EquityTransaction() {
						TransactionDate = new DateTime(year, month, 28),
						equity= e
					};
				    itemPrice = getMonthlyPrice(t);			 
				}				 
			}			 
			return itemPrice;
		}

		private double getMonthlyPrice(EquityTransaction t)
		{
			double itemPrice;
			IDictionary<int, double> montlyPrice = new Dictionary<int, double>();

			itemPrice = component.getMySqlObj().GetHistoricalSharePrice(t.equity.ISIN, t.TransactionDate.Month, t.TransactionDate.Year);
			if (itemPrice == 0)
			{
				montlyPrice = component.getWebScrappertObj().GetHistoricalAssetPrice(t.equity.Companyname, t.TransactionDate.Month, t.TransactionDate.Year, t.equity.assetType);

				foreach (int key in montlyPrice.Keys)
				{
					equityHistory eq = new equityHistory()
					{
						month = key,
						year = t.TransactionDate.Year,
						price = montlyPrice[key],
						equityid = t.equity.ISIN,
						assetType = Convert.ToInt32(t.equity.assetType)
					};
					if (key == t.TransactionDate.Month)
					{
						itemPrice = montlyPrice[key];
					}
					_eqHistory.Add(eq);
					component.getMySqlObj().UpdateEquityMonthlyPrice(eq);
				}
				 
			}
			return itemPrice;
		}
	}
}
