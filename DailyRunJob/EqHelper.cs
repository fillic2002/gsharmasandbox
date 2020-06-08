using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equity;

namespace DailyRunEquity
{
	public class Eqhelper
	{
		private GenericFunc _htmlHelper;
		ExcelHelper _excelHelper;

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

		public Eqhelper()
		{
			_htmlHelper = new GenericFunc();
			_excelHelper = new ExcelHelper();
		}
		public void fillShareDetails()
		{
			Console.WriteLine("Start:" + DateTime.Now.ToString("hh.mm.ss.ffffff"));
			string s1= string.Empty;
			Task task1 = Task.Factory.StartNew(()=> 
				{
					s1=_htmlHelper.GetMFNAV(1);
					Console.WriteLine("Fetching from Task1");
				}
			);
			
			string s2=string.Empty;
			Task task2 = Task.Factory.StartNew(() =>
				{
					s2=_htmlHelper.GetMFNAV(2);
					Console.WriteLine("Fetching from Task2");
				}
			);
			
			string s3= string.Empty; 
			Task task3 = Task.Factory.StartNew(() =>
				{
					s3 = _htmlHelper.GetMFNAV(3);
					Console.WriteLine("Fetching from Task3");
				}
			);
			 
			string s4= string.Empty; 
			Task task4 = Task.Factory.StartNew(()=>
				{
					s4 = _htmlHelper.GetMFNAV(4);
					Console.WriteLine("Fetching from Task4");
				}
			);

			string s5 = string.Empty;
			Task task5 = Task.Factory.StartNew(() =>
			{
				s5 = _htmlHelper.GetMFNAV(5);
				Console.WriteLine("Fetching from Task5");
			}
			);
			string s6 = string.Empty;
			Task task6 = Task.Factory.StartNew(() =>
			{
				s6 = _htmlHelper.GetCompanyMarketValue((int)CompanyName.GAIL);
				Console.WriteLine("Fetching For GAIL:"+ s6.ToString());
			}
			);
			string s7 = string.Empty;
			Task task7 = Task.Factory.StartNew(() =>
			{
				s7 = _htmlHelper.GetCompanyMarketValue((int)CompanyName.MAHANAGAR);
				Console.WriteLine("Fetching for MAHANAGAE GAS:"+s7.ToString());
			}
			);
			string s8 = string.Empty;
			Task task8 = Task.Factory.StartNew(() =>
			{
				s8 = _htmlHelper.GetCompanyMarketValue((int)CompanyName.NESCO);
				Console.WriteLine("Fetching For NESCO:"+ s8.ToString());
			}
			);

			string s9 = string.Empty;
			Task task9 = Task.Factory.StartNew(() =>
			{
				s9 = _htmlHelper.GetCompanyMarketValue((int)CompanyName.KOVAIMEDICAL);
				Console.WriteLine("Fetching For KOVAI:" + s9.ToString());
			}
			);
			string s10 = string.Empty;
			Task task10 = Task.Factory.StartNew(() =>
			{
				s10 = _htmlHelper.GetCompanyMarketValue((int)CompanyName.TATACHEMICAL);
				Console.WriteLine("Fetching For TataChemical:" + s10.ToString());
			}
			);
			string s11 = string.Empty;
			Task task11 = Task.Factory.StartNew(() =>
			{
				s11 = _htmlHelper.GetCompanyMarketValue((int)CompanyName.BEL);
				Console.WriteLine("Fetching For BEL:" + s11.ToString());
			}
			);
			string s12 = string.Empty;
			Task task12 = Task.Factory.StartNew(() =>
			{
				s12 = _htmlHelper.GetCompanyMarketValue((int)CompanyName.GIC);
				Console.WriteLine("Fetching For GIC:" + s12.ToString());
			}
			);
			string s13 = string.Empty;
			Task task13 = Task.Factory.StartNew(() =>
			{
				s13 = _htmlHelper.GetMFNAV(6);
				Console.WriteLine("Fetching from Task13");
			}
			);


			Console.WriteLine("Async Call Finished:" + DateTime.Now);
			Task.WaitAll(task1, task2, task3, task4, task5,task6,task7,task8,task9,task10);
			Console.WriteLine("Async Call Response Recieved:" + DateTime.Now);


			_excelHelper.SaveToSharesWorksheet("Shares", s1, 3);
			Console.WriteLine("Saved for s1:" + DateTime.Now);
			_excelHelper.SaveToSharesWorksheet("Shares", s2, 10);
			Console.WriteLine("Saved for s2:" + DateTime.Now);

			_excelHelper.SaveToSharesWorksheet("Shares", s3, 12);
			Console.WriteLine("Saved for s3:" + DateTime.Now);

			_excelHelper.SaveToSharesWorksheet("Shares", s4, 17);
			Console.WriteLine("Saved for s4:" + DateTime.Now);
			_excelHelper.SaveToSharesWorksheet("Shares", s5, 21);
			Console.WriteLine("Saved for s5:" + DateTime.Now);

			_excelHelper.SaveToSharesWorksheet("Shares", s13, 27);
			Console.WriteLine("Saved for s13:" + DateTime.Now);

			_excelHelper.SaveToFinancialWorksheet("FinancialAnalysis", s6, 3,((int)CompanyName.GAIL)+60);
			Console.WriteLine("Saved for s6:" + DateTime.Now);
			
			_excelHelper.SaveToFinancialWorksheet("FinancialAnalysis", s7, 3, ((int)CompanyName.MAHANAGAR) + 60);
			_excelHelper.SaveToFinancialWorksheet("FinancialAnalysis", s8, 3, ((int)CompanyName.NESCO) + 60);
			_excelHelper.SaveToFinancialWorksheet("FinancialAnalysis", s9, 3, ((int)CompanyName.KOVAIMEDICAL) + 60);
			_excelHelper.SaveToFinancialWorksheet("FinancialAnalysis", s10, 3, ((int)CompanyName.TATACHEMICAL) + 60);
			_excelHelper.SaveToFinancialWorksheet("FinancialAnalysis", s11, 3, ((int)CompanyName.BEL) + 60);
			_excelHelper.SaveToFinancialWorksheet("FinancialAnalysis", s12, 3, ((int)CompanyName.GIC) + 60);

			Console.WriteLine("Saved all records:" + DateTime.Now.ToString("hh.mm.ss.ffffff"));
			_excelHelper.CloseExcel();

			//Console.ReadKey();
		}
	}
}
