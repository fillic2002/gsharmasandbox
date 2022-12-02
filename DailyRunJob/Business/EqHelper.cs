using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Equity;
using Git_Sandbox.DailyRunJob;
using Git_Sandbox.DailyRunJob.Common;
using Git_Sandbox.Model;
//using myfinAPI.Model;
using AssetHistory =  myfinAPI.Model.AssetHistory;
using EquityTransaction = myfinAPI.Model.EquityTransaction;
using AssetType = myfinAPI.Model.AssetClass.AssetType;
using static myfinAPI.Model.AssetClass;
using myfinAPI.Model;
using myfinAPI.Model.DTO;

namespace DailyRunEquity
{
	public class Eqhelper
	{
		private GenericFunc _htmlHelper;
		private bool _updatedPPF = false;
		AssetHistory _previoudMthSnapshot;
		IDictionary<string, AssetHistory> _previousMonthSnapshot;
		 
		pf _previousMonthCont;
		double _previousMonthInvst;
		private Dictionary<int, double> _currentNav;
		private string _mfHistoricalNav = "https://www.amfiindia.com/net-asset-value/nav-history";
		IList<equityHistory> _eqHistory;
		
		IList<Portfolio> folioDetail;

		static IList<EquityBase> equity = new List<EquityBase>();

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
			_previousMonthSnapshot = new Dictionary<string, AssetHistory>();
			
		}
		public void AddPbAndMarketCap()
		{
			List<EquityTransaction> eqTran = new List<EquityTransaction>();
			component.getMySqlObj().GetTransactions(eqTran, 0);
			List<EquityTransaction> res = eqTran.FindAll(x => (x.equity.PB ==0 || x.equity.MarketCap==0) && x.equity.assetType==AssetType.Shares 
							&& DateTime.Now.Subtract(x.tranDate).TotalDays <= 30);
			foreach(EquityTransaction et in res)
			{
				dividend d = new dividend();
				component.getWebScrappertObj().GetDividendAndTotalShare(d,et.equity,"PB");
				et.equity.PB = (et.equity.PB / et.equity.livePrice) * et.price;
				et.equity.MarketCap = (et.equity.MarketCap / et.equity.livePrice) * et.price;
				component.getMySqlObj().UpdateTransaction(et);				
			}			
		}
		/// <summary>
		/// This function is going to get live NAV for the shares and update in the db table
		/// </summary>
		/// <returns></returns>
		public async Task UpdateEquityLiveData()
		{
			var stopwatch = Stopwatch.StartNew();
			IEnumerable<Task<EquityBase>> downloadTasksQuery =
				   from item in equity
				   select ProcessUrlAsync(item);

			List<Task<EquityBase>> downloadTasks = downloadTasksQuery.ToList();

			while (downloadTasks.Any())
			{
				try
				{
					Task<EquityBase> finishedTask = await Task.WhenAny(downloadTasks);
					downloadTasks.Remove(finishedTask);
					if (finishedTask.Status != TaskStatus.Faulted && finishedTask.Result!=null)
					{
						//Console.WriteLine("DB Update::" + finishedTask.Result.equityName+ " IS ::"+component.getMySqlObj().UpdateLatesNAV(finishedTask.Result));
						//Thread.Sleep(100);
						RecordMonthlyAssetPrice(finishedTask.Result);
					}
				}
				catch(Exception ex)
				{
					string s = ex.StackTrace;
					continue;
				}
			}
			stopwatch.Stop();
			Console.WriteLine($"Elapsed time:          {stopwatch.Elapsed}\n");
			Console.WriteLine("Saved all records:" + DateTime.Now.ToString("hh.mm.ss.ffffff"));

		}

		private void RecordMonthlyAssetPrice(EquityBase eq)
		{
			try
			{
				if (DateTime.Now.Day >= 26)
				{
					if (component.getMySqlObj().GetHistoricalSharePrice(eq.assetId, DateTime.Now.Month, DateTime.Now.Year) > 0)
					{
						Console.WriteLine("Current Month Price already present for: " + eq.equityName);
					}
					else
					{
						component.getMySqlObj().UpdateEquityMonthlyPrice(new equityHistory()
						{
							equityid = eq.assetId,
							month = DateTime.Now.Month,
							price = eq.livePrice,
							year = DateTime.Now.Year,
							assetType = Convert.ToInt32(eq.assetType)
						});
					}
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}
		public void ReadNewExcel()
		{
			excelhelpernew.ReadExcelFile();
		}
		async Task<EquityBase> ProcessUrlAsync(EquityBase item)
		{
			try
			{			
				component.getMySqlObj().GetCompanyDetails(item);
				if (item.lastUpdated <= DateTime.UtcNow.AddMinutes(-200) || item.livePrice==0)
				{
					if (item.assetType == AssetType.Shares)
					{
						await component.getWebScrappertObj().GetEquityDetails(item);
						component.getMySqlObj().UpdateLatesNAV(item);
					}
					else if(item.lastUpdated < DateTime.UtcNow.AddDays(-1) )
					{
						await component.getWebScrappertObj().GetMFDetails(item);
						component.getMySqlObj().UpdateLatesNAV(item);
					}
					else
					{
						return null;
					}
					//component.getMySqlObj().UpdateLatesNAV(item);
					Thread.Sleep(100);
					return item;
				}
				return null;
			}
			catch(Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("Error in fetching LiveData:" + item.equityName);
				Console.Write(ex.Message);

				return item;
			}
		}

		//public void UpdateCompanyDetails()
		//{
		//	IList<equity> listOfCompanies=new List<equity>();

		//	component.getMySqlObj().GetCompaniesMissingInformation(listOfCompanies);

		//	//for (char c = 'A'; c < 'Z'; c++)
		//	//{
		//		//Console.Write(c );
		//		//This function not in use 
		//		//component.getWebScrappertObj().UpdateCompanyDetails(listOfCompanies.Where(x => x.Companyname.StartsWith(c)).ToList());
				
		//	//}			 
 	//	}

		public void AddBonusTransaction() 
		{
			IList<dividend> dividendDetails = new List<dividend>();
			EquityTransaction eqtTran = new EquityTransaction();
			IList<EquityTransaction> eqtTrans = new List<EquityTransaction>();

			//component.getMySqlObj().GetCompaniesID(listCompanies);
			component.getEquityBusinessHelperObj().GetAllTransaction(0, eqtTrans);
			component.getMySqlObj().getDividendDetails(dividendDetails);
			Dictionary<string, int> tempBonus = new Dictionary<string, int>();
			foreach (dividend comp in dividendDetails.Where(x=>x.dtUpdated >= DateTime.Now.AddYears(-10) && x.creditType==TypeOfCredit.Bonus))
			{
				//Get Bonus split
				string[] bonusSplit = comp.value.ToString().Split('.');
				
				var result= eqtTrans.Where(x => x.equity.assetId == comp.companyid && x.tranDate <= comp.dtUpdated);
				//Check for transaction
				for(int folioId=1; folioId <= 5; folioId++ )
				{
					int totoalEquityCount=Convert.ToInt32(CalculateHolding(result.Where(x => x.portfolioId == folioId), 
						folioId,comp.dtUpdated));
					
					var bonusQuantity = totoalEquityCount * Convert.ToInt32(bonusSplit[0]) / Convert.ToInt32(bonusSplit[1]);

					var match = result.Where(x => x.tranDate == comp.dtUpdated && x.tranType == TranType.Bonus && x.portfolioId==folioId);
					//Check if bonus already added
					if (match.ToArray().Length > 0)
					{
						continue;
					}

					if (totoalEquityCount > 0)
					{	
						eqtTran.equity= new EquityBase() { assetId = comp.companyid };
						eqtTran.price = 0;
						eqtTran.tranType = TranType.Bonus;
						eqtTran.portfolioId = folioId;
						eqtTran.qty = bonusQuantity;
						eqtTran.tranDate = comp.dtUpdated;
						component.getEquityBusinessHelperObj().AddEqtyTransaction(eqtTran);
						eqtTrans.Add(eqtTran);
						Console.WriteLine("BONUS ADDED FOR::"+ comp.companyid);
					}
				}			

				//Add new bonus share
				//component.getEquityBusinessHelperObj().AddEqtyTransaction(eqtTran);
				
			}
	
		}
		private double CalculateHolding(IEnumerable<EquityTransaction> tran,int folioId,DateTime bonusDate)
		{
			double equityCount = 0;
			foreach(EquityTransaction t in tran)
			{
				if (t.tranType == TranType.Buy|| (t.tranType==TranType.Bonus && t.tranDate != bonusDate))
					equityCount += t.qty;
				if (t.tranType == TranType.Sell)
					equityCount -= t.qty;
			}
			return equityCount;
		}

		public void AddDividendDetails()
		{
			IList<dividend> listCompanies = new List<dividend>();

			component.getMySqlObj().GetEquityDetails(listCompanies);

			IList<EquityBase> Listurl = component.getGenericFunctionObj().GetEquityLinks();

			//Check companies whose dividend details not updated in last 30 days
			foreach (dividend comp in listCompanies)
			{
				try
				{
					//Console.WriteLine("Getting Dividend detail from DB for Company ID:" + comp.companyid);
					component.getMySqlObj().getLastDividendOfCompany(comp);
					if (DateTime.UtcNow.Subtract(comp.lastCrawledDate).TotalDays >= 7)
					{
						Console.WriteLine("Dividend Detail need Fresh from BSE:" + comp.companyid);
						component.getWebScrappertObj().GetEquityDivAndBonusDetail(comp, Listurl.First<EquityBase>(x => x.assetId == comp.companyid), "Dividend");
					}
				}
				catch(Exception ex)
				{
					string message = ex.StackTrace;
					component.getWebScrappertObj().Dispose();
					continue;
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

				bool stopY = false;
				for (int y = 2017; y <= DateTime.Now.Year; y++)
				{
					if (DateTime.Now.Year == y)
						stopY = true;

					for (int m = 1; m <= 12; m++)
					{
						if (stopY == false || (stopY == true && DateTime.Now.Month >= m))
						{
							UpdateMonthlyShareSnapshot(m, y, p, transaction);
							UpdateMonthlyMFSnapshot(m, y, p, transaction.Where(x => x.equity.assetType== AssetType.Equity_MF && x.tranDate <= new DateTime(y, m, DateTime.DaysInMonth(y, m))), AssetType.Equity_MF);
							UpdateMonthlyMFSnapshot(m, y, p, transaction.Where(x => x.equity.assetType == AssetType.Debt_MF && x.tranDate <= new DateTime(y, m, DateTime.DaysInMonth(y, m))), AssetType.Debt_MF);
							UpdatePropertySnapshot(m, y, p, myfinAPI.Model.AssetClass.AssetType.Gold);
							UpdatePropertySnapshot(m, y, p, myfinAPI.Model.AssetClass.AssetType.Flat);
							UpdatePropertySnapshot(m, y, p, myfinAPI.Model.AssetClass.AssetType.Plot);
							//UpdatePFSnapshot(m, y, p, AssetType.PPF);
							//UpdatePPFSnapshot(m, y, p, AssetType.PPF);
							UpdateBankSnapshot(m, y, p, myfinAPI.Model.AssetClass.AssetType.Bank);
							UpdateBondSnapshot(m, y, p, myfinAPI.Model.AssetClass.AssetType.Bonds);
						}
					}
				}
			}
		}
		public void UpdateBondSnapshot(int m, int y, Portfolio p, myfinAPI.Model.AssetClass.AssetType astType)
		{
		 
			AssetHistory _pevMonthSnapshot=new AssetHistory();
			 
			_pevMonthSnapshot.portfolioId = p.folioId;
			DateTime dt = new DateTime(y, m, DateTime.DaysInMonth(y, m)).AddMonths(-1);
			_pevMonthSnapshot.month = dt.Month;
			_pevMonthSnapshot.year = dt.Year;
			_pevMonthSnapshot.Assettype = AssetType.Bonds;

			component.getMySqlObj().GetAssetSnapshot(_pevMonthSnapshot);			 

			IList<myfinAPI.Model.DTO.BondTransaction> bondTran = new List<myfinAPI.Model.DTO.BondTransaction>();
			 
			component.getBondContextObj().GetBondTransaction(p.folioId, bondTran);
			 
			//Get Bond Details & tran details
			foreach (BondTransaction tran in 
				bondTran.ToList().Where(x=>x.purchaseDate.Month == m && x.purchaseDate.Year==y))
			{				 
				if(tran.BondDetail.dateOfMaturity> new DateTime(y, m, DateTime.DaysInMonth(y, m)) && tran.folioId==p.folioId)
				{
					//bondSnapshot.Investment += tran.InvstPrice * tran.Qty;
					_pevMonthSnapshot.Investment += tran.InvstPrice * tran.Qty;
					_pevMonthSnapshot.AssetValue += tran.LivePrice * tran.Qty;
				}				 
			}
			_pevMonthSnapshot.month = m;
			_pevMonthSnapshot.year = y;
			 
			//Update Snapshot
			var result=component.getMySqlObj().AddAssetSnapshot(_pevMonthSnapshot);
		}
		public void UpdateBankSnapshot(int m, int y, Portfolio p, myfinAPI.Model.AssetClass.AssetType astType)
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
			_ppfSnapshot.Assettype = astType;
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
					Console.WriteLine("Updating PF/PPF ac:" + p.folioId + " for month:" + _ppfSnapshot.month + "-" + _ppfSnapshot.year);
					component.getMySqlObj().UpdatePFSnapshot(_ppfSnapshot);
				}
				if (ppf.type == "deposit"|| ppf.type == "carry")
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
				Console.WriteLine("Updating PF/PPF ac:" + p.folioId + " for month:" + _ppfSnapshot.month + "-" + _ppfSnapshot.year);
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
		public void UpdatePropertySnapshot(int m, int y, Portfolio folio, myfinAPI.Model.AssetClass.AssetType typeofAsset)
		{
			myfinAPI.Model.AssetHistory history = new myfinAPI.Model.AssetHistory();
			IList<propertyTransaction> transaction = new List<propertyTransaction>();
			history.Assettype = typeofAsset;
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
						history.qty += pt.qty;
					}
					else
					{
						history.qty -= pt.qty;			
					}
					if(history.qty==0)
					{
						history.AssetValue = 0;
					}
				}
			}

			if (history.Investment > 0)
			{
				component.getMySqlObj().AddAssetSnapshot(history);
			}
		}
		private void UpdateMonthlyMFSnapshot(int month, int year, Portfolio p, IEnumerable<EquityTransaction> t, myfinAPI.Model.AssetClass.AssetType astType)
		{
			if (t.ToList().Count == 0)
				return;
			AssetHistory history = new  AssetHistory();
			history.portfolioId = p.folioId;
			history.Investment = 0;
			history.Assettype = astType;
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
			UpdateMonthlyMFInvestment(history, t.Where(x => x.tranDate.Month == month && x.tranDate.Year == year).ToArray());
			//In case any purchase made during this month, or asset price changed
			UpdateMonthlyMFAssetValue(history, t.ToArray(), month, year);
			if (history.Investment != 0)
			{
				history.month = month;
				history.year = year;
				component.getMySqlObj().AddAssetSnapshot(history);
			}
		}
		private void UpdateMonthlyMFInvestment(myfinAPI.Model.AssetHistory astHistory, IList<EquityTransaction> t)
		{
			if (t.Count == 0)
				return;
			foreach (EquityTransaction eqt in t)
			{
				if (eqt.tranType == TranType.Buy)
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
		private void UpdateMonthlyMFAssetValue(myfinAPI.Model.AssetHistory astHistory, IList<EquityTransaction> t, int month, int year)
		{
			Dictionary<string, double> qty = new Dictionary<string, double>();

			myfinAPI.Model.AssetClass.AssetType typeofAsset = astHistory.Assettype;
			astHistory.AssetValue = 0;

			foreach (EquityTransaction eqt in t)
			{
				if (!qty.ContainsKey(eqt.equity.assetId))
				{
					qty.Add(eqt.equity.assetId, 0);
				}
				if (eqt.tranType == TranType.Buy)
				{
					qty[eqt.equity.assetId] += eqt.qty;
					typeofAsset = eqt.equity.assetType;
				}
				else
				{
					qty[eqt.equity.assetId] -= eqt.qty;
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

			myfinAPI.Model.AssetHistory history = new myfinAPI.Model.AssetHistory();
			Dictionary<string, double> equities = new Dictionary<string, double>();
			
			IList<Portfolio> folioDetail = new List<Portfolio>();
			history.Assettype = myfinAPI.Model.AssetClass.AssetType.Shares;
			history.month = month;
			history.year = year;
			history.portfolioId = p.folioId;
			history.Investment = 0;		 

			foreach (EquityTransaction eqt in t.Where(x=>x.equity.assetType==AssetType.Shares && 
			((x.tranDate.Year==year && x.tranDate.Month<=month)|| x.tranDate.Year<year)))
			{
				 
					if (eqt.tranType == TranType.Buy|| eqt.tranType ==TranType.Bonus)
					{
						history.Investment += eqt.price * eqt.qty;
						history.AssetValue += GetMonthPrice(eqt.equity.assetId, month, year,AssetType.Shares) * eqt.qty;
						history.portfolioId = eqt.portfolioId;
						history.Assettype = myfinAPI.Model.AssetClass.AssetType.Shares; 

						if (!equities.ContainsKey(eqt.equity.assetId))
						{
							equities.Add(eqt.equity.assetId, 0);
						}
					}
					else
					{
						history.Investment -= eqt.price * eqt.qty;
						history.AssetValue -= GetMonthPrice(eqt.equity.assetId, month, year,AssetType.Shares) * eqt.qty;
					}				 
			}			 		

			history.Dividend = GetDividendDetails(month, year, p, equities,t);
			 
			if (history.AssetValue != 0)
			{
				Console.WriteLine("Save AssetSnapshot for Portfolio:" + p.folioId + " For Year: " + year + " Month:" + month);
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
				IEnumerable<EquityTransaction> selectedTran = t.Where(n => n.equity.assetId == div.companyid);
				double qty = 0;
				if ((div.dtUpdated.Month <= month && div.dtUpdated.Year == year) || div.dtUpdated.Year < year)
				{
					foreach (EquityTransaction tran in selectedTran)
					{
						if (tran.equity.assetId == div.companyid && tran.tranDate < div.dtUpdated)
						{
							if (tran.tranType == TranType.Buy)
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
		private double GetMonthPrice(string isin, int month, int year, myfinAPI.Model.AssetClass.AssetType typeAsset)
		{
			EquityBase e = new EquityBase() { assetId = isin };
			component.getMySqlObj().GetCompanyDetails(e);
			return GetMonthPrice(e, month, year, typeAsset);
		}
		private double GetMonthPrice(EquityBase e,int month, int year,AssetType typeAsset)
		{
			double itemPrice = 0;
			e.assetType = typeAsset;
			//Search from nav table
			if (month == DateTime.Now.Month && year == DateTime.Now.Year)
			{
				return component.getMySqlObj().GetLatesNAV(e.assetId);
			}
			else if (year >= 2015 && month >= 1)
			{
				itemPrice = component.getMySqlObj().GetHistoricalSharePrice(e.assetId, month, year);
				if (itemPrice <= 0)
				{					 
					EquityTransaction t = new EquityTransaction() {
						tranDate = new DateTime(year, month, 28),
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

			itemPrice = component.getMySqlObj().GetHistoricalSharePrice(t.equity.assetId, t.tranDate.Month, t.tranDate.Year);
			if (itemPrice == 0)
			{
				montlyPrice = component.getWebScrappertObj().GetHistoricalAssetPrice(t.equity.equityName, t.tranDate.Month, t.tranDate.Year, t.equity.assetType);

				foreach (int key in montlyPrice.Keys)
				{
					equityHistory eq = new equityHistory()
					{
						month = key,
						year = t.tranDate.Year,
						price = montlyPrice[key],
						equityid = t.equity.assetId,
						assetType = Convert.ToInt32(t.equity.assetType)
					};
					if (key == t.tranDate.Month)
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
