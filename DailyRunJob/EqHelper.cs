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

			int total = 0;
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

			Console.WriteLine($"\nTotal bytes returned:  {total:#,#}");
			Console.WriteLine($"Elapsed time:          {stopwatch.Elapsed}\n");

			//Console.WriteLine("Async Call Response Recieved:" + DateTime.Now);


			//_excelHelper.SaveToSharesWorksheet("Shares", s1, 3);
			//Console.WriteLine("Saved for s1:" + DateTime.Now);
			//_excelHelper.SaveToSharesWorksheet("Shares", s2, 10);
			//Console.WriteLine("Saved for s2:" + DateTime.Now);

			//_excelHelper.SaveToSharesWorksheet("Shares", s3, 12);
			//Console.WriteLine("Saved for s3:" + DateTime.Now);

			//_excelHelper.SaveToSharesWorksheet("Shares", s4, 17);
			//Console.WriteLine("Saved for s4:" + DateTime.Now);
			//_excelHelper.SaveToSharesWorksheet("Shares", s5, 21);
		//	Console.WriteLine("Saved for s5:" + DateTime.Now);

			//_excelHelper.SaveToSharesWorksheet("Shares", s13, 27);
			//Console.WriteLine("Saved for s13:" + DateTime.Now);

			//_excelHelper.SaveToFinancialWorksheet("FinancialAnalysis", s6, 3,((int)CompanyName.GAIL)+60);
			//Console.WriteLine("Saved for s6:" + DateTime.Now);

			if (ConfigMgr.SAVINGMODE == "DB")
			{
				//Console.WriteLine("DB Update::" + component.getMySqlObj().UpdateLatesNAV());
			}
			else
			{
				//_excelHelper.SaveToFinancialWorksheet("FinancialAnalysis", s7, 3, ((int)CompanyName.MAHANAGAR) + 60);
				//_excelHelper.SaveToFinancialWorksheet("FinancialAnalysis", s8, 3, ((int)CompanyName.NESCO) + 60);
				//_excelHelper.SaveToFinancialWorksheet("FinancialAnalysis", s9, 3, ((int)CompanyName.KOVAIMEDICAL) + 60);
				//_excelHelper.SaveToFinancialWorksheet("FinancialAnalysis", s10, 3, ((int)CompanyName.TATACHEMICAL) + 60);
				//_excelHelper.SaveToFinancialWorksheet("FinancialAnalysis", s11, 3, ((int)CompanyName.BEL) + 60);
				//_excelHelper.SaveToFinancialWorksheet("FinancialAnalysis", s12, 3, ((int)CompanyName.GIC) + 60);
			}
			Console.WriteLine("Saved all records:" + DateTime.Now.ToString("hh.mm.ss.ffffff"));
			//_excelHelper.CloseExcel();

			//Console.ReadKey();
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
			foreach(dividend u in listCompanies)
			{
				component.getWebScrappertObj().GetDividend(u, Listurl.First<equity>(x => x.ISIN == u.companyid));
			}			
		}

		public void UpdateMonthlyAsset(int month, int year)
		{
			IList<EquityTransaction> transaction = new List<EquityTransaction>();
			AssetHistory history;
			Dictionary<string, double> equities = new Dictionary<string, double>();
			IList<dividend> dividendDetails = new List<dividend>();
			IList<Portfolio> folioDetail = new List<Portfolio>();

			component.getMySqlObj().GetPortFolio(folioDetail);

			foreach (Portfolio p in folioDetail)
			{
				transaction.Clear();
				dividendDetails.Clear();
				history = new AssetHistory();
				equities.Clear();

				component.getMySqlObj().GetTransactions(transaction, p.folioId);

				//Find equity and their invst till today
				foreach (EquityTransaction eqt in transaction)
				{
					if (eqt.equity.assetType == AssetType.Shares && eqt.portfolioId==p.folioId && ((eqt.TransactionDate.Year == year && eqt.TransactionDate.Month<=month) || eqt.TransactionDate.Year<year))
					{
						if (eqt.TypeofTransaction == 'B')
						{
							history.Investment += eqt.price * eqt.qty;
							history.AssetValue += GetLivePrice(eqt.equity.ISIN,month, year) * eqt.qty;
							history.portfolioId = eqt.portfolioId;


							if (!equities.ContainsKey(eqt.equity.ISIN))
							{
								equities.Add(eqt.equity.ISIN, 0);
							}
						}
						else
						{
							history.Investment -= eqt.price * eqt.qty;
							history.AssetValue -= GetLivePrice(eqt.equity.ISIN, month, year) * eqt.qty;
						}
					}
				}
				component.getMySqlObj().GetDividend(dividendDetails,p.folioId);

				double dividend=0;
				foreach (dividend d in dividendDetails)
				{
					int qty = 0;
					foreach (EquityTransaction t in transaction)
					{
						if (t.portfolioId == p.folioId)
						{
							if (t.equity.ISIN == d.companyid && t.TransactionDate < d.dt && ((t.TransactionDate.Year==year && t.TransactionDate.Month<=month)|| t.TransactionDate.Year<year))
							{
								if(t.TypeofTransaction=='B')
									qty += t.qty;
								else
									qty -= t.qty;
							}
						}
					}
					if (qty > 0)
					{
						equities[d.companyid] += qty * d.value;
						dividend += qty * d.value;
					}

				}
				history.Dividend = dividend;
				//history.qurarter = (DateTime.Now.Month - 1) / 3 + 1;
				history.qurarter = month;
				history.year = year;
				Console.WriteLine("Save Record for Portfolio:"+ p.folioId + " For Year: "+ year + " Month:" + month);
				component.getMySqlObj().AddAssetSnapshot(history);
			}

		}

		public double GetLivePrice(string ISIN,int month, int year)
		{
			if (month == DateTime.Now.Month && year == DateTime.Now.Year)
			{
				return component.getMySqlObj().GetLatesNAV(ISIN);
			}
			else
			{
				return component.getExcelHelperObj().GetMonthlySharePrice(ISIN, month, year);
			}
		}
	}
}
