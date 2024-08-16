using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Equity;
using Git_Sandbox.DailyRunJob;
using Git_Sandbox.Model;
using myfinAPI.Model;
using myfinAPI.Model.DTO;
using static myfinAPI.Model.AssetClass;
//using myfinAPI.Model;
using AssetHistory = myfinAPI.Model.AssetHistory;
using AssetType = myfinAPI.Model.AssetClass.AssetType;
using EquityTransaction = myfinAPI.Model.EquityTransaction;

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
		public static bool failure = true;

		static readonly HttpClient s_client = new HttpClient
		{
			MaxResponseContentBufferSize = 1_000_000
		};
		public Eqhelper()
		{
			_htmlHelper = new GenericFunc();
			folioDetail = new List<Portfolio>();
			component.getMySqlObj().GetPortFolio(folioDetail);


			_eqHistory = new List<equityHistory>();
			_previousMonthSnapshot = new Dictionary<string, AssetHistory>();

		}
		public void AddTransactionPbAndMarketCap()
		{
			List<EquityTransaction> eqTran = new List<EquityTransaction>();
			component.getMySqlObj().GetTransactions(eqTran, 0);
			try
			{
				List<EquityTransaction> res = eqTran.FindAll(x => (x.PB_Tran == 0 || x.MarketCap_Tran == 0) && x.equity.assetType == AssetType.Shares
								&& DateTime.Now.Subtract(x.tranDate).TotalDays <= 30);
				foreach (EquityTransaction et in res)
				{

					//dividend d = new dividend();
					//component.getWebScrappertObj().GetDividendAndTotalShare(d, et.equity, "PB");
					et.PB_Tran = (et.equity.PB / et.equity.livePrice) * et.price;
					et.MarketCap_Tran = (et.equity.MarketCap / et.equity.livePrice) * et.price;
					et.verified = true;
					component.getMySqlObj().UpdateTransaction(et);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("AddPbAndMarketCap::" + ex.Message);
			}
		}
		/// <summary>
		/// This function is going to get live NAV for the shares and update in the db table
		/// </summary>
		/// <returns></returns>
		public async Task UpdateEquityLiveData()
		{
			failure = false;
			component.getMySqlObj().GetEquityDetails(equity);
			var stopwatch = Stopwatch.StartNew();
			equity.ToList().ForEach(x => ProcessUrl(x));


			if (DateTime.Now.Day >= 20)
				equity.ToList().ForEach(x => RecordMonthlyAssetPrice(x));

			try
			{

			}
			catch (Exception ex)
			{
				string s = ex.StackTrace;

			}

			stopwatch.Stop();
			Console.WriteLine($"Elapsed time:          {stopwatch.Elapsed}\n");
			Console.WriteLine("Saved all records:" + DateTime.Now.ToString("hh.mm.ss.ffffff"));
			//Console.Read();

		}
		private void RecordMonthlyAssetPrice(EquityBase eq)
		{
			try
			{
				if (component.getMySqlObj().GetHistoricalSharePrice(eq.assetId, DateTime.Now.Month, DateTime.Now.Year) > 0)
				{
					Console.WriteLine("Current Month Price already present for: " + eq.equityName);
				}
				else
				{
					component.getMySqlObj().UpdateEquityMonthlyPrice(eq, DateTime.UtcNow.Month, DateTime.UtcNow.Year);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}
		public void ReadNewExcel()
		{
			//component.getExcelHelperObj().

		}
		public async void ProcessUrl(EquityBase item)
		{
			try
			{
				int retrial = 0;

				if (item.assetType == AssetType.Shares &&
					(item.lastUpdated <= DateTime.UtcNow.AddMinutes(-300) || item.livePrice == 0 || item.MarketCap == 0 || item.PB == 0))
				{
					if (String.IsNullOrEmpty(item.divUrl))
					{
						Console.WriteLine("DivURL empty :: " + item.equityName);
						return;
					}
					//	if (item.assetId == "INE302A01020")
					//	{	
					bool result = await component.getWebScrappertObj().GetEquityDetails(item);
					while (result == false && retrial > 0)
					{
						retrial--;
						result = await component.getWebScrappertObj().GetEquityDetails(item);
					}
					if (result == false)
					{
						failure = true;
						return;
					}

					component.getMySqlObj().UpdateLatesNAV(item);
					Thread.Sleep(200);
					//	}
				}
				else if ((item.assetType == AssetType.Equity_MF || item.assetType == AssetType.Debt_MF) &&
					item.lastUpdated <= DateTime.UtcNow.AddMinutes(-300) || item.livePrice == 0)
				{

					bool result = await component.getWebScrappertObj().GetMFDetails(item);
					while (result == false && retrial > 0)
					{
						retrial--;
						result = await component.getWebScrappertObj().GetEquityDetails(item);
						Console.WriteLine("Retiel :: " + retrial + " for getting MF -" + item.equityName);

					}
					if (result == false)
						return;
					component.getMySqlObj().UpdateLatesNAV(item);
				}



			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("Error in fetching LiveData:" + item.equityName);
				Console.Write(ex.Message);

				//	return item;
			}
			finally
			{
				//Task.Delay(100).Wait();
			}
		}

		

		public void AddBonusTransaction()
		{
			IList<dividend> dividendDetails = new List<dividend>();
			EquityTransaction eqtTran = new EquityTransaction();
			IList<EquityTransaction> eqtTrans = new List<EquityTransaction>();

			//component.getMySqlObj().GetCompaniesID(listCompanies);
			component.getEquityBusinessHelperObj().GetAllTransaction(0, eqtTrans);
			component.getMySqlObj().getDividendDetails(dividendDetails);
			Dictionary<string, int> tempBonus = new Dictionary<string, int>();
			try
			{
				foreach (dividend comp in dividendDetails.Where(x => x.dtUpdated >= DateTime.Now.AddYears(-10) && x.creditType == TranType.Bonus))
				{
					//Get Bonus split
					string[] bonusSplit = comp.value.ToString().Split('.');

					var result = eqtTrans.Where(x => x.equity.assetId == comp.companyid && x.tranDate <= comp.dtUpdated);
					//Check for transaction
					for (int folioId = 1; folioId <= 5; folioId++)
					{
						int totoalEquityCount = Convert.ToInt32(CalculateHolding(result.Where(x => x.portfolioId == folioId),
							folioId, comp.dtUpdated));

						var bonusQuantity = totoalEquityCount * Convert.ToInt32(bonusSplit[0]) / Convert.ToInt32(bonusSplit[1]);

						var match = result.Where(x => x.tranDate == comp.dtUpdated && x.tranType == TranType.Bonus && x.portfolioId == folioId);
						//Check if bonus already added
						if (match.ToArray().Length > 0)
						{
							continue;
						}

						if (totoalEquityCount > 0)
						{
							eqtTran.equity = new EquityBase() { assetId = comp.companyid };
							eqtTran.price = 0;
							eqtTran.tranType = TranType.Bonus;
							eqtTran.portfolioId = folioId;
							eqtTran.qty = bonusQuantity;
							eqtTran.tranDate = comp.dtUpdated;
							eqtTran.verified = false;
							component.getEquityBusinessHelperObj().AddEqtyTransaction(eqtTran);
							eqtTrans.Add(eqtTran);
							Console.WriteLine("BONUS ADDED FOR::" + comp.companyid);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("AddBonusTransaction::" + ex.Message);
			}
			//Add new bonus share
			//component.getEquityBusinessHelperObj().AddEqtyTransaction(eqtTran);
		}
		private decimal CalculateHolding(IEnumerable<EquityTransaction> tran, int folioId, DateTime bonusDate)
		{
			decimal equityCount = 0;
			foreach (EquityTransaction t in tran)
			{
				if (t.tranType == TranType.Buy || (t.tranType == TranType.Bonus && t.tranDate != bonusDate))
					equityCount += t.qty;
				if (t.tranType == TranType.Sell)
					equityCount -= t.qty;
			}
			return equityCount;
		}
		public void AddCorpActionDetails()
		{
			IList<dividend> listCompanies = new List<dividend>();

			component.getMySqlObj().GetEquityDetails(listCompanies);

			IList<EquityBase> Listurl = component.getGenericFunctionObj().GetEquityLinks();

			//Check companies whose dividend details not updated in last 30 days
			foreach (dividend comp in listCompanies)
			{
				try
				{
					//if (comp.companyid == "INE274J01014")
					//{
						component.getMySqlObj().getCorpActionOfCompany(comp);
						if (DateTime.UtcNow.Subtract(comp.dtUpdated).TotalDays >= 0 &&
							DateTime.UtcNow.Subtract(comp.lastCrawledDate).TotalDays > 7)
						{

							Console.WriteLine("Dividend Detail need Fresh from BSE:" + comp.companyid + "::");
							component.getWebScrappertObj().GetEquityDivAndBonusDetail(comp, Listurl.First<EquityBase>(x => x.assetId == comp.companyid), "Dividend");

						}
					}
				//}
				catch (Exception ex)
				{
					string message = ex.StackTrace;
					//component.getWebScrappertObj().Dispose();
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
				IList<BondTransaction> bondTran = new List<BondTransaction>();
				component.getMySqlObj().GetTransactions(transaction, p.folioId);
				component.getBondContextObj().GetBondTransaction(p.folioId, bondTran);

				bool stopY = false;
				for (int y = 2017; y <= DateTime.Now.Year; y++)
				{
					if (DateTime.Now.Year == y)
						stopY = true;

					for (int m = 1; m <= 12; m++)
					{
						if (stopY == false || (stopY == true && DateTime.Now.Month >= m))
						{
							UpdateMonthlyShareSnapshot(m, y, p, transaction.Where(x => x.equity.assetType == AssetType.Shares).ToList());
							UpdateMonthlyMFSnapshot(m, y, p, transaction.Where(x => x.equity.assetType == AssetType.Equity_MF
											&& x.tranDate <= new DateTime(y, m, DateTime.DaysInMonth(y, m))), AssetType.Equity_MF);
							UpdateMonthlyMFSnapshot(m, y, p, transaction.Where(x => x.equity.assetType == AssetType.Debt_MF
										&& x.tranDate <= new DateTime(y, m, DateTime.DaysInMonth(y, m))), AssetType.Debt_MF);
							UpdatePropertySnapshot(m, y, p, myfinAPI.Model.AssetClass.AssetType.Gold);
							UpdatePropertySnapshot(m, y, p, myfinAPI.Model.AssetClass.AssetType.Flat);
							UpdatePropertySnapshot(m, y, p, myfinAPI.Model.AssetClass.AssetType.Plot);
							//UpdatePFSnapshot(m, y, p, AssetType.PPF);
							//UpdatePPFSnapshot(m, y, p, AssetType.PPF);
							UpdateBankSnapshot(m, y, p, myfinAPI.Model.AssetClass.AssetType.Bank);
							UpdateBondSnapshot(m, y, p, myfinAPI.Model.AssetClass.AssetType.Bonds, bondTran.Where(x => x.purchaseDate.Year == y).ToList());
						}
					}
				}
			}
		}
		public void UpdateBondSnapshot(int m, int year, Portfolio p, myfinAPI.Model.AssetClass.AssetType astType,
			IList<BondTransaction> bondTran)
		{
			AssetHistory _pevMonthSnapshot = new AssetHistory();

			_pevMonthSnapshot.portfolioId = p.folioId;
			DateTime dt = new DateTime(year, m, DateTime.DaysInMonth(year, m)).AddMonths(-1);
			_pevMonthSnapshot.month = dt.Month;
			_pevMonthSnapshot.year = dt.Year;
			_pevMonthSnapshot.Assettype = AssetType.Bonds;
			component.getMySqlObj().GetAssetSnapshot(_pevMonthSnapshot);

			_pevMonthSnapshot.Dividend = 0;
			IOrderedEnumerable<BondIntrestYearly> intr = component.getBondBusinessHelperObj().GetMonthlyBondIntrest(year, p.folioId);
			if (intr.Count() > 0)
			{
				if (intr.Where(x => x.month == m).Count() > 0)
				{
					_pevMonthSnapshot.Dividend += intr.First(x => x.month == m).Intrest;
				}
			}
			try
			{
				foreach (BondTransaction tran in bondTran)
				{
					if (tran.purchaseDate.Month == m && tran.purchaseDate.Year == year)
					{
						_pevMonthSnapshot.Investment += tran.InvstPrice * tran.Qty + tran.AccuredIntrest;
						if (tran.BondDetail.LivePrice == 0)
							tran.BondDetail.LivePrice = tran.BondDetail.faceValue;
						_pevMonthSnapshot.AssetValue += tran.BondDetail.LivePrice * tran.Qty;
					}
					if (tran.BondDetail.dateOfMaturity.Month == m && tran.BondDetail.dateOfMaturity.Year == year)
					{
						if (tran.BondDetail.dateOfMaturity.Month == DateTime.UtcNow.Month &&
							tran.BondDetail.dateOfMaturity.Year == DateTime.UtcNow.Year)
						{
							if (tran.BondDetail.dateOfMaturity.Day <= DateTime.UtcNow.Day)
							{
								_pevMonthSnapshot.Investment -= tran.InvstPrice * tran.Qty + tran.AccuredIntrest;
								if (tran.BondDetail.LivePrice == 0)
									throw new Exception();
								_pevMonthSnapshot.AssetValue -= tran.BondDetail.LivePrice * tran.Qty;
							}
						}
						else
						{
							_pevMonthSnapshot.Investment -= tran.InvstPrice * tran.Qty + tran.AccuredIntrest;
							_pevMonthSnapshot.AssetValue -= tran.BondDetail.LivePrice * tran.Qty;
						}
					}
				}
			}
			catch (Exception ex)
			{
				string s = ex.Message;
			}

			_pevMonthSnapshot.month = m;
			_pevMonthSnapshot.year = year;

			//Update Snapshot
			var result = component.getMySqlObj().AddAssetSnapshot(_pevMonthSnapshot);
		}
		public void UpdateBankSnapshot(int m, int y, Portfolio p, myfinAPI.Model.AssetClass.AssetType astType)
		{
			if (DateTime.Now.Month == m && DateTime.Now.Year == y)
				component.getMySqlObj().UpdateBankSnapshot(m, y, p.folioId);
		}
		//public void UpdatePPFSnapshot()
		//{
		//	foreach (Portfolio p in folioDetail)
		//	{
		//		UpdatePPFSnapshot(p, AssetType.PPF);
		//		UpdatePPFSnapshot(p, AssetType.PF);
		//	}
		//}
		private double getBondIntrest(BondTransaction tran, int m, int year)
		{
			try
			{
				double i = 0;
				//foreach (BondIntrest intrest in tran.AccuredIntrest.Where(y => y.intrestPaymentDate.Month == m && y.intrestPaymentDate.Year == year))
				//{
				//	i += intrest.amt;
				//}
				return i;
			}
			catch (Exception ex)
			{
				return 0;
			}

		}
		/// <summary>
		/// This function is stable enough now. Need to modify for only current year PF snapshot update and not from 
		/// First day of investment	
		/// </summary>
		/// <param name="p"></param>
		/// <param name="astType"></param>
		//public void UpdatePPFSnapshot(Portfolio p, AssetType astType)
		//{
		//	AssetHistory _ppfSnapshot = new AssetHistory();
		//	_ppfSnapshot.Assettype = astType;
		//	_ppfSnapshot.portfolioId = p.folioId;

		//	DateTime preMonth = new DateTime();

		//	IList<PFAccount> ppfTransaction = new List<PFAccount>();
		//	component.getMySqlObj().GetPf_PPFTransaction(p.folioId, ppfTransaction, astType);
		//	if (ppfTransaction.Count == 0)
		//		return;

		//	foreach (PFAccount ppf in ppfTransaction)
		//	{
		//		DateTime dtCurr = new DateTime(ppf.DateOfTransaction.Year, ppf.DateOfTransaction.Month, 1);
		//		while (dtCurr > preMonth && preMonth != new DateTime())
		//		{
		//			preMonth = preMonth.AddMonths(1);
		//			_ppfSnapshot.month = preMonth.Month;
		//			_ppfSnapshot.year = preMonth.Year;
		//			Console.WriteLine("Updating "+ astType.ToString()+" ac:" + p.folioId + " for month:" + _ppfSnapshot.month + "-" + _ppfSnapshot.year);
		//			//component.getMySqlObj().UpdatePFSnapshot(_ppfSnapshot);
		//		}
		//		if (ppf.TypeOfTransaction == TranType.Deposit || ppf.TypeOfTransaction == TranType.Carry)
		//		{
		//			_ppfSnapshot.Investment += ppf.InvestmentEmp + ppf.InvestmentEmplr + ppf.Pension;
		//			_ppfSnapshot.AssetValue += ppf.InvestmentEmp + ppf.InvestmentEmplr + ppf.Pension;
		//		}
		//		else
		//		{
		//			_ppfSnapshot.AssetValue += ppf.InvestmentEmp + ppf.InvestmentEmplr;
		//		}
		//		_ppfSnapshot.month = dtCurr.Month;
		//		_ppfSnapshot.year = dtCurr.Year;
		//		//component.getMySqlObj().UpdatePFSnapshot(_ppfSnapshot);
		//		preMonth = dtCurr;
		//	}
		//	preMonth = preMonth.AddMonths(1);
		//	while (DateTime.Now >= preMonth)
		//	{
		//		_ppfSnapshot.month = preMonth.Month;
		//		_ppfSnapshot.year = preMonth.Year;
		//		Console.WriteLine("Updating PF/PPF ac:" + p.folioId + " for month:" + _ppfSnapshot.month + "-" + _ppfSnapshot.year);
		//		//component.getMySqlObj().UpdatePFSnapshot(_ppfSnapshot);
		//		preMonth = preMonth.AddMonths(1);
		//	}

		//	//}
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
					if (history.qty == 0)
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
		private void UpdateMonthlyMFSnapshot(int month, int year, Portfolio p, IEnumerable<EquityTransaction> t,
			myfinAPI.Model.AssetClass.AssetType astType)
		{
			if (t.ToList().Count == 0)
				return;
			//Stop calculating for previous year on Equity and MF for now as we have good data
			if ((astType == AssetType.Shares || astType == AssetType.Debt_MF || astType == AssetType.Equity_MF)
				&& year < DateTime.UtcNow.Year)
			{
				return;
			}
			AssetHistory history = new AssetHistory();
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
			foreach (EquityTransaction tran in t)
			{
				if (tran.tranType == TranType.Buy)
				{
					astHistory.Investment += tran.price * tran.qty;
				}
				else
				{
					astHistory.Investment -= tran.price * tran.qty;
				}
			}
		}
		//Add asset purchased during this particular month in previous month snapshot
		private void UpdateMonthlyMFAssetValue(myfinAPI.Model.AssetHistory astHistory, IList<EquityTransaction> t, int month, int year)
		{
			Dictionary<string, decimal> qty = new Dictionary<string, decimal>();

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
				astHistory.AssetValue += qty[key] * GetMonthPrice(t.First(x => x.equity.assetId == key).equity, month, year);
			}

		}
		private void UpdateMonthlyShareSnapshot(int month, int year, Portfolio p, IList<EquityTransaction> t)
		{

			if (year < DateTime.UtcNow.Year)
				return;

			AssetHistory history = new AssetHistory();
			Dictionary<string, decimal> equities = new Dictionary<string, decimal>();
			IList<Portfolio> folioDetail = new List<Portfolio>();

			history.Assettype = myfinAPI.Model.AssetClass.AssetType.Shares;
			history.month = month;
			history.year = year;
			history.portfolioId = p.folioId;
			history.Investment = 0;

			foreach (EquityTransaction eqt in t.Where(x => (x.tranDate.Year == year && x.tranDate.Month <= month) || x.tranDate.Year < year))
			{
				if (eqt.tranType == TranType.Buy || eqt.tranType == TranType.Bonus)
				{
					history.Investment += eqt.price * eqt.qty;
					history.AssetValue += GetMonthPrice(eqt.equity, month, year) * eqt.qty;
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
					history.AssetValue -= GetMonthPrice(eqt.equity, month, year) * eqt.qty;
				}
			}

			history.Dividend = GetDividendDetails(month, year, p, equities, t);

			if (history.AssetValue != 0)
			{
				Console.WriteLine("Save AssetSnapshot for Portfolio:" + p.folioId + " For Year: " + year + " Month:" + month);
				component.getMySqlObj().AddAssetSnapshot(history);
			}
		}
		private decimal GetDividendDetails(int month, int year, Portfolio p, Dictionary<string, decimal> equities, IList<EquityTransaction> t)
		{
			IList<dividend> dividendDetails = new List<dividend>();
			component.getMySqlObj().GetCompaniesDividendDetails(dividendDetails, p.folioId, month, year);
			decimal dividend = 0;
			foreach (dividend div in dividendDetails)
			{
				IEnumerable<EquityTransaction> selectedTran = t.Where(n => n.equity.assetId == div.companyid);
				decimal qty = 0;
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
		private decimal GetMonthPrice(EquityBase ast, int month, int year, myfinAPI.Model.AssetClass.AssetType typeAsset)
		{
			//EquityBase e = new EquityBase() { assetId = isin };
			component.getMySqlObj().GetCompanyDetails(ast);
			return GetMonthPrice(ast, month, year);
		}
		private decimal GetMonthPrice(EquityBase e, int month, int year)
		{
			decimal itemPrice = 0;
			//e.assetType = typeAsset;
			//Search from nav table
			if (month == DateTime.Now.Month && year == DateTime.Now.Year)
			{
				return component.getMySqlObj().GetLatesNAV(e.assetId);
			}
			else if (year >= 2020 && month >= 1)
			{
				itemPrice = component.getMySqlObj().GetHistoricalSharePrice(e.assetId, month, year);
				if (itemPrice <= 0)
				{
					EquityTransaction t = new EquityTransaction()
					{
						tranDate = new DateTime(year, month, 28),
						equity = e
					};
					itemPrice = getMonthlyPrice(t);
				}
			}
			return itemPrice;
		}
		private decimal getMonthlyPrice(EquityTransaction t)
		{
			decimal itemPrice;
			IDictionary<int, decimal> montlyPrice = new Dictionary<int, decimal>();

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
					t.equity.livePrice = (decimal)eq.price;

					component.getMySqlObj().UpdateEquityMonthlyPrice(t.equity, eq.month, eq.year);
				}

			}
			return itemPrice;
		}
	}
}
