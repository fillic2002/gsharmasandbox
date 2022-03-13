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
		private bool _updatedPPF = false;
		AssetHistory _previoudMthSnapshot;
		pf _previousMonthCont;
		double _previousMonthInvst;
		private Dictionary<int, double> _currentNav;
		private string _mfHistoricalNav = "https://www.amfiindia.com/net-asset-value/nav-history";
		IList<equityHistory> _eqHistory;
		//ExcelHelper _excelHelper;
		//excelhelpernew _excelHelper;
		IList<Portfolio> folioDetail;

		static IList<equity> equity = new List<equity>();

		static readonly HttpClient s_client = new HttpClient
		{
			MaxResponseContentBufferSize = 1_000_000
		};
		public Eqhelper()
		{
			_htmlHelper = new GenericFunc();
			folioDetail = new List<Portfolio>();
			component.getMySqlObj().GetPortFolio(folioDetail);

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
					Console.WriteLine("DB Update::" + component.getMySqlObj().UpdateLatesNAV(finishedTask.Result));
					RecordMonthlyAssetPrice(finishedTask.Result);
				}

				//total += await finishedTask;
			}
			stopwatch.Stop();

			Console.WriteLine($"Elapsed time:          {stopwatch.Elapsed}\n");

			Console.WriteLine("Saved all records:" + DateTime.Now.ToString("hh.mm.ss.ffffff"));

		}

		private void RecordMonthlyAssetPrice(equity eq)
		{
			if (DateTime.Now.Day >= 26)
			{
				if (component.getMySqlObj().GetHistoricalSharePrice(eq.ISIN, DateTime.Now.Month, DateTime.Now.Year) > 0)
				{
					Console.WriteLine("Current Month Price already present for: " + eq.Companyname);
				}
				else
				{
					component.getMySqlObj().UpdateEquityMonthlyPrice(new equityHistory()
					{
						equityid = eq.ISIN,
						month = DateTime.Now.Month,
						price = eq.LivePrice,
						year = DateTime.Now.Year,
						assetType = Convert.ToInt32(eq.assetType)
					});
				}
			}
		}
		public void ReadNewExcel()
		{
			excelhelpernew.ReadExcelFile();
		}
		async Task<equity> ProcessUrlAsync(equity item)
		{
			
			item.LivePrice = await _htmlHelper.GetAssetNAVAsync(item);
				
			return item;
		}

		public void UpdateCompanyDetails()
		{
			IList<equity> listOfCompanies=new List<equity>();

			component.getMySqlObj().GetCompaniesMissingInformation(listOfCompanies);

			for (char c = 'A'; c < 'Z'; c++)
			{
				Console.Write(c );
				
				component.getWebScrappertObj().UpdateCompanyDetails(listOfCompanies.Where(x => x.Companyname.StartsWith(c)).ToList());
				
			}			 
 		}

		public void AddDividendDetails()
		{
			IList<dividend> listCompanies = new List<dividend>();

			component.getMySqlObj().GetStaleDividendCompanies(listCompanies);

			IList<equity> Listurl = component.getGenericFunctionObj().GetEquityLinks();

			//Check companies whose dividend details not updated in last 30 days
			foreach (dividend u in listCompanies)
			{
				Console.WriteLine("Stale Company:" + u.companyid);
				component.getMySqlObj().getLastDividendOfCompany(u);
				if (DateTime.Now.Subtract(u.dt).TotalDays >= 90 && DateTime.Now.Subtract(u.lastCrawledDate).TotalDays >= 30)
				{
					component.getWebScrappertObj().GetDividend(u, Listurl.First<equity>(x => x.ISIN == u.companyid));
				}
			}
		}
		public void UpdateAssetHistoryPrice(Portfolio p)
		{

			IList<EquityTransaction> transaction = new List<EquityTransaction>();
			component.getMySqlObj().GetTransactions(transaction, p.folioId);
			foreach (EquityTransaction t in transaction)
			{
				getMonthlyPrice(t);
			}

		}
		public void UpdateAssetHistory()
		{
			foreach (Portfolio p in folioDetail)
			{
				_previoudMthSnapshot = new AssetHistory();
				_previousMonthCont = new pf();
				IList<dividend> dividendDetails = new List<dividend>();
				IList<EquityTransaction> transaction = new List<EquityTransaction>();
				component.getMySqlObj().GetTransactions(transaction, p.folioId);
				//component.getMySqlObj().GetCompaniesDividendDetails(dividendDetails, p.folioId);

				bool stopY = false;
				for (int y = 2012; y <= DateTime.Now.Year; y++)
				{
					if (DateTime.Now.Year == y)
						stopY = true;

					for (int m = 1; m <= 12; m++)
					{
						if (stopY == false || (stopY == true && DateTime.Now.Month >= m))
						{
							UpdateMonthlyShareSnapshot(m, y, p, transaction);
							UpdateMonthlyMFSnapshot(m, y, p, transaction.Where(x => x.equity.assetType == AssetType.EquityMF && x.TransactionDate <= new DateTime(y, m, DateTime.DaysInMonth(y, m))), AssetType.EquityMF);
							UpdateMonthlyMFSnapshot(m, y, p, transaction.Where(x => x.equity.assetType == AssetType.DebtMF && x.TransactionDate <= new DateTime(y, m, DateTime.DaysInMonth(y, m))), AssetType.DebtMF);
							UpdatePropertySnapshot(m, y, p, AssetType.Gold);
							UpdatePropertySnapshot(m, y, p, AssetType.Flat);
							UpdatePropertySnapshot(m, y, p, AssetType.Plot);
							//UpdatePFSnapshot(m, y, p, AssetType.PPF);
							//UpdatePPFSnapshot(m, y, p, AssetType.PPF);
							UpdateBankSnapshot(m, y, p, AssetType.Bank);
						}
					}
				}
			}
		}
		public void UpdateBankSnapshot(int m, int y, Portfolio p, AssetType astType)
		{
			if (DateTime.Now.Month == m && DateTime.Now.Year == y)
				component.getMySqlObj().UpdateBankSnapshot(m, y, p.folioId);
		}
		public void UpdatePPFSnapshot()
		{
			foreach (Portfolio p in folioDetail)
			{
				UpdatePPFSnapshot(p, AssetType.PPF);
				UpdatePPFSnapshot(p, AssetType.PF);
			}
		}
		public void UpdatePPFSnapshot(Portfolio p, AssetType astType)
		{
			AssetHistory _ppfSnapshot = new AssetHistory();
			_ppfSnapshot.assetType = astType;
			_ppfSnapshot.portfolioId = p.folioId;
			 
			DateTime preMonth = new DateTime();

			IList<pf> ppfChangeDetail = new List<pf>();
			component.getMySqlObj().GetPf_PPFTransaction(p.folioId, ppfChangeDetail, astType);
			if (ppfChangeDetail.Count == 0)
				return;
			 
			foreach (pf ppf in ppfChangeDetail)
			{
				DateTime dtCurr = new DateTime(ppf.dtOfChange.Year, ppf.dtOfChange.Month, 1);
				while (dtCurr > preMonth && preMonth != new DateTime())
				{
					preMonth = preMonth.AddMonths(1);
					_ppfSnapshot.month = preMonth.Month;
					_ppfSnapshot.year = preMonth.Year;
					Console.WriteLine("Updating ac:" + p.folioId + " for month:" + _ppfSnapshot.month + "-" + _ppfSnapshot.year);
					component.getMySqlObj().UpdatePFSnapshot(_ppfSnapshot);
				}
				if (ppf.type == "Deposit"|| ppf.type == "carry")
				{
					_ppfSnapshot.Investment += ppf.empCont+ppf.emplyrCont+ppf.pension;
					_ppfSnapshot.AssetValue += ppf.empCont + ppf.emplyrCont + ppf.pension;
				}
				else
				{
					_ppfSnapshot.AssetValue += ppf.empCont+ppf.emplyrCont;
				}
				_ppfSnapshot.month = dtCurr.Month;
				_ppfSnapshot.year = dtCurr.Year;
				component.getMySqlObj().UpdatePFSnapshot(_ppfSnapshot);
				preMonth = dtCurr;
			}
			preMonth = preMonth.AddMonths(1);
			while (DateTime.Now >=preMonth )
			{				 
				_ppfSnapshot.month = preMonth.Month;
				_ppfSnapshot.year = preMonth.Year;
				Console.WriteLine("Updating ac:" + p.folioId + " for month:" + _ppfSnapshot.month + "-" + _ppfSnapshot.year);
				component.getMySqlObj().UpdatePFSnapshot(_ppfSnapshot);
				preMonth = preMonth.AddMonths(1);
			}
			 
			//}
		}
		//public void UpdatePFSnapshot(int m, int y, Portfolio p, AssetType astType)
		//{
		//	_previoudMthSnapshot.assetType = AssetType.PF;
		//	//Update for current month
		//	if (y == DateTime.Now.Year & m == DateTime.Now.Month)
		//	{
		//		component.getMySqlObj().UpdatePFSnapshot(m, y, p.folioId, _previoudMthSnapshot.Investment + _previousMonthInvst);
		//	}
		//	//Update historically
		//	else
		//	{
		//		DateTime dtCal = new DateTime(y, m, 1);
		//		double curMonthIntrest = 0;
		//		IList<pf> pfChangeDetail = new List<pf>();
		//		component.getMySqlObj().GetPf_PPFTransaction(p.folioId, pfChangeDetail, AssetType.PF);
		//		if (pfChangeDetail.Count == 0)
		//			return;

		//		if (_previoudMthSnapshot.AssetValue > 0)
		//		{
		//			foreach (pf pfchangeTran in pfChangeDetail)
		//			{
		//				_previoudMthSnapshot.month = m;
		//				_previoudMthSnapshot.year = y;
		//				if (pfchangeTran.dtOfChange < dtCal)
		//				{
		//					continue;
		//				}
		//				if (pfchangeTran.type == "Deposit")
		//				{
		//					if (pfchangeTran.dtOfChange == dtCal)
		//					{	
		//						_previousMonthInvst = pfchangeTran.empCont + pfchangeTran.emplyrCont + pfchangeTran.pension;
		//					}
		//				}
		//				if ((pfchangeTran.type == "int" || pfchangeTran.type == "Adj" || pfchangeTran.type == "carry") && pfchangeTran.dtOfChange == dtCal)
		//				{
		//					curMonthIntrest += pfchangeTran.empCont + pfchangeTran.emplyrCont;
		//				}
		//			}
		//			_previoudMthSnapshot.AssetValue += _previousMonthInvst + curMonthIntrest;
		//			_previoudMthSnapshot.Investment += _previousMonthInvst;
		//		}
		//		else if (_previoudMthSnapshot.AssetValue == 0 && pfChangeDetail[0].dtOfChange.Month == m && pfChangeDetail[0].dtOfChange.Year == y)
		//		{
		//			_previoudMthSnapshot.Investment = pfChangeDetail[0].empCont + pfChangeDetail[0].emplyrCont + pfChangeDetail[0].pension;
		//			_previoudMthSnapshot.AssetValue = pfChangeDetail[0].empCont + pfChangeDetail[0].emplyrCont + pfChangeDetail[0].pension;
		//			_previoudMthSnapshot.assetType = AssetType.PF;
		//			_previoudMthSnapshot.Dividend = 0;
		//			_previoudMthSnapshot.month = m;
		//			_previoudMthSnapshot.year = y;
		//			_previoudMthSnapshot.portfolioId = p.folioId;
		//		}
		//		if (_previoudMthSnapshot.AssetValue > 0)
		//			component.getMySqlObj().UpdatePFSnapshot(_previoudMthSnapshot);

		//	}


		//}
		public void UpdatePropertySnapshot(int m, int y, Portfolio folio, AssetType typeofAsset)
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
				if ((pt.TransactionDate.Year == y && pt.TransactionDate.Month <= m) || pt.TransactionDate.Year < y)
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
		private void UpdateMonthlyMFSnapshot(int month, int year, Portfolio p, IEnumerable<EquityTransaction> t, AssetType astType)
		{
			if (t.ToList().Count == 0)
				return;
			AssetHistory history = new AssetHistory();
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
			DateTime dt = new DateTime(year, month, 28);
			//Previous month snapshot
			component.getMySqlObj().GetAssetSnapshot(history);
			//In case any purchase made during month in question, then add that as part of invstm
			UpdateMonthlyMFInvestment(history, t.Where(x => x.TransactionDate.Month == month && x.TransactionDate.Year == year).ToArray());
			//In case any purchase made during this month, or asset price changed
			UpdateMonthlyMFAssetValue(history, t.ToArray(), month, year);
			if (history.Investment != 0)
			{
				history.month = month;
				history.year = year;
				component.getMySqlObj().AddAssetSnapshot(history);
			}
		}
		private void UpdateMonthlyMFInvestment(AssetHistory astHistory, IList<EquityTransaction> t)
		{
			if (t.Count == 0)
				return;
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
		//Add asset purchased during this particular month in previous month snapshot
		private void UpdateMonthlyMFAssetValue(AssetHistory astHistory, IList<EquityTransaction> t, int month, int year)
		{
			Dictionary<string, double> qty = new Dictionary<string, double>();

			AssetType typeofAsset = astHistory.assetType;
			astHistory.AssetValue = 0;

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
			foreach (string key in qty.Keys)
			{
				astHistory.AssetValue += qty[key] * GetMonthPrice(key, month, year, typeofAsset);
			}

		}

		private void UpdateMonthlyShareSnapshot(int month, int year, Portfolio p, IList<EquityTransaction> t)
		{

			AssetHistory history = new AssetHistory();
			Dictionary<string, double> equities = new Dictionary<string, double>();
			//IList<dividend> dividendDetails = new List<dividend>();
			IList<Portfolio> folioDetail = new List<Portfolio>();
			history.assetType = AssetType.Shares;
			history.month = month;
			history.year = year;
			history.portfolioId = p.folioId;

			//component.getMySqlObj().GetAssetSnapshot(history);
			//if (month == DateTime.Now.Month && year == DateTime.Now.Year)
			//{
			//	history.Investment = 0;history.AssetValue = 0;history.Dividend = 0;
			//}
				
			//else if (history.AssetValue > 0)
			//	return;

			foreach (EquityTransaction eqt in t.Where(x=>x.equity.assetType==AssetType.Shares && 
			((x.TransactionDate.Year==year && x.TransactionDate.Month<=month)|| x.TransactionDate.Year<year)))
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

			history.Dividend = GetDividendDetails(month, year, p, equities,t);
			 
			if (history.AssetValue != 0)
			{
				Console.WriteLine("Save Record for Portfolio:" + p.folioId + " For Year: " + year + " Month:" + month);
				component.getMySqlObj().AddAssetSnapshot(history);
			}
		}

		private double GetDividendDetails(int month, int year, Portfolio p, Dictionary<string, double> equities, IList<EquityTransaction> t)
		{
			IList<dividend> dividendDetails = new List<dividend>();
			component.getMySqlObj().GetCompaniesDividendDetails(dividendDetails, p.folioId,month,year);
			double dividend = 0;
			foreach (dividend div in dividendDetails)
			{
				IEnumerable<EquityTransaction> selectedTran = t.Where(n => n.equity.ISIN == div.companyid);
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
			return dividend;
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
