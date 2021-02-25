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
		 
			equity = component.getMySqlObj().GetPortfolioAssetUrl();
		}
		public async Task fillShareDetailsAsync()
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
						Console.WriteLine("DB Update::" + component.getMySqlObj().UpdateLatesNAV(finishedTask.Result));					 
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
			item.LivePrice = await _htmlHelper.GetMFNAVAsync(item);
			//Console.WriteLine(val);			
			return item;
		}

		public void AddDividendDetails()
		{
			dividend obj = new dividend();
			 
		 
			IList<equity> Listurl = component.getGenericFunctionObj().GetEquityLinks();
			foreach(equity u in Listurl)
			{
				component.getWebScrappertObj().GetDividend(u);
			}			
		}
	}
}
