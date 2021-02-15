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
		//static  IEnumerable<string> s_urlList;
		static IList<equity> equity=new List<equity>();
		//= new string[]
		//{
		//	"https://www.moneycontrol.com/india/stockpricequote/oil-drilling-and-exploration/gailindia/GAI",
		//	"https://www.moneycontrol.com/india/stockpricequote/electricals/bharatelectronics/BE03",
		//	"https://www.moneycontrol.com/india/stockpricequote/diversified/nesco/NES",
		//	"https://www.moneycontrol.com/india/stockpricequote/refineries/mahanagargas/MG02",
		//	"https://www.moneycontrol.com/india/stockpricequote/hospitals-medical-services/kovaimedicalcenterhospital/KMC02",
		//	"https://www.moneycontrol.com/india/stockpricequote/chemicals/tatachemicals/TC",
		//	"https://www.moneycontrol.com/india/stockpricequote/diversified/generalinsurancecorporationindia/GIC12"

		//};
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
					 
						Console.WriteLine("DB Update::" + component.getMySqlObj().UpdateLatesNAV(finishedTask.Result.ISIN, finishedTask.Result.LivePrice));
					 
				}
				
				//total += await finishedTask;
			}
			stopwatch.Stop();

			Console.WriteLine($"\nTotal bytes returned:  {total:#,#}");
			Console.WriteLine($"Elapsed time:          {stopwatch.Elapsed}\n");

		//	Task task1 = Task.Factory.StartNew(()=> 
		//		{
		//			int id = (int)CompanyName.GAIL;
		//		//	_currentNav.Add(id, Convert.ToDouble(_htmlHelper.GetMFNAV(id)));
		//			Console.WriteLine("Fetching from "+  _currentNav.Keys);
		//		}
		//	);
			
		////	string s2=string.Empty;
		//	Task task2 = Task.Factory.StartNew(() =>
		//		{
		//			int id = (int)CompanyName.HAL;
		//		//	_currentNav.Add(id, Convert.ToDouble(_htmlHelper.GetMFNAV(id)));
		//	//		Console.WriteLine("Fetching from Task2");
		//		}
		//	);
			
		////	string s3= string.Empty; 
		//	Task task3 = Task.Factory.StartNew(() =>
		//		{
		//			//s3 = _htmlHelper.GetMFNAV(3);
		//			int id = (int)CompanyName.SRIKALAHASTI;
		////			_currentNav.Add(id, Convert.ToDouble(_htmlHelper.GetMFNAV(id)));
		//	//		Console.WriteLine("Fetching from Task3");
		//		}
		//	);
			 
		////	string s4= string.Empty; 
		//	Task task4 = Task.Factory.StartNew(()=>
		//		{
		//			int id = (int)CompanyName.PETRONETLNG;
		//	//		_currentNav.Add(id, Convert.ToDouble(_htmlHelper.GetMFNAV(id)));
		//	//		Console.WriteLine("Fetching from Task4");
		//		}
		//	);

		////	string s5 = string.Empty;
		//	Task task5 = Task.Factory.StartNew(() =>
		//	{
		//		int id = (int)CompanyName.PETRONETLNG;
		////		_currentNav.Add(id, Convert.ToDouble(_htmlHelper.GetMFNAV(id)));
		//	//	Console.WriteLine("Fetching from Task5");
		//	}
		//	);
		////	string s6 = string.Empty;
		//	Task task6 = Task.Factory.StartNew(() =>
		//	{
		//		int id = (int)CompanyName.PETRONETLNG;
		//	//	_currentNav.Add(id, Convert.ToDouble(_htmlHelper.GetMFNAV(id)));
		//	//	Console.WriteLine("Fetching For GAIL:"+ s6.ToString());
		//	}
		//	);
		////	string s7 = string.Empty;
		//	Task task7 = Task.Factory.StartNew(() =>
		//	{
		//		int id = (int)CompanyName.PETRONETLNG;
		//	//	_currentNav.Add(id, Convert.ToDouble(_htmlHelper.GetMFNAV(id)));
		//		//Console.WriteLine("Fetching for MAHANAGAE GAS:"+s7.ToString());
		//	}
		//	);
		////	string s8 = string.Empty;
		//	Task task8 = Task.Factory.StartNew(() =>
		//	{
		//		int id = (int)CompanyName.PETRONETLNG;
		//	//	_currentNav.Add(id, Convert.ToDouble(_htmlHelper.GetMFNAV(id)));
		//		//Console.WriteLine("Fetching For NESCO:"+ s8.ToString());
		//	}
		//	);

		////	string s9 = string.Empty;
		//	Task task9 = Task.Factory.StartNew(() =>
		//	{
		//		int id = (int)CompanyName.PETRONETLNG;
		//	//	_currentNav.Add(id, Convert.ToDouble(_htmlHelper.GetMFNAV(id)));
		//		//Console.WriteLine("Fetching For KOVAI:" + s9.ToString());
		//	}
		//	);
		//	string s10 = string.Empty;
		//	Task task10 = Task.Factory.StartNew(() =>
		//	{
		//		int id = (int)CompanyName.PETRONETLNG;
		//	//	_currentNav.Add(id, Convert.ToDouble(_htmlHelper.GetMFNAV(id)));
		//		//Console.WriteLine("Fetching For TataChemical:" + s10.ToString());
		//	}
		//	);
		//	string s11 = string.Empty;
		//	Task task11 = Task.Factory.StartNew(() =>
		//	{
		//		int id = (int)CompanyName.BEL;
		//	//	_currentNav.Add(id, Convert.ToDouble(_htmlHelper.GetMFNAV(id)));
		//		//Console.WriteLine("Fetching For BEL:" + s11.ToString());
		//	}
		//	);
		//	string s12 = string.Empty;
		//	Task task12 = Task.Factory.StartNew(() =>
		//	{
		//	int id = (int)CompanyName.GIC;
		//	//	_currentNav.Add(id, Convert.ToDouble(_htmlHelper.GetMFNAV(id)));
		//	//	Console.WriteLine("Fetching For GIC:" + s12.ToString());
		//	}
		//	);
		//	string s13 = string.Empty;
		//	Task task13 = Task.Factory.StartNew(() =>
		//	{
		//	//	s13 = _htmlHelper.GetMFNAV(6);
		//		//_currentNav.Add(7, Convert.ToDouble(_htmlHelper.GetMFNAV(7)));
		//		Console.WriteLine("Fetching from Task13");
		//	}
		//	);


		//	Console.WriteLine("Async Call Finished:" + DateTime.Now);
			//Task.WaitAll(task1, task2, task3, task4, task5,task6,task7,task8,task9,task10);
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
			//byte[] content = await _.GetByteArrayAsync(url);
			//	Console.WriteLine($"{url,-60} {content.Length,10:#,#}");
			
			item.LivePrice =	await _htmlHelper.GetMFNAVAsync(item.sourceurl);
			//Console.WriteLine(val);			
			return item;
		}
	}
}
