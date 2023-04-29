using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
//using Microsoft.Office.Interop.Excel;
//using OfficeOpenXml;
//using Range = Microsoft.Office.Interop.Excel.Range;

namespace Equity
{
	public class ExcelHelper
	{
		//Application ExcelApp;
		//private Dictionary<double, string> _companyName = new Dictionary<double, string>();
		//int startPoint = 3;
		//int fetchTill = 2900;
		
		//Worksheet EQUITY_SHEET, ASSET_SHEET;
		//Workbook wb;
		//Workbook _assetWorkBook;

		//public Dictionary<double, Dictionary<int, string>> _masterScriptList = new Dictionary<double, Dictionary<int, string>>();
		//public Dictionary<double, Dictionary<int, string>> _roceList = new Dictionary<double, Dictionary<int, string>>();
		//public List<double> _scriptList = new List<double>() { };

	    string EQUITY_FILE_PATH = ConfigurationManager.AppSettings["EQUITY_FILE_PATH"];

		public string BOND_FILE_PATH = @"C:\Users\fillic\Downloads\SECURITY_LIST_DETAILS.csv";//ConfigurationManager.AppSettings["BOND_FILE_PATH"];	
		public static string BOND_LIVE_PRICE = @"C:\Users\fillic\Downloads\MW-Bonds-on-CM-"+ DateTime.Now.ToString("dd-MMM-yyyy")+".csv";//ConfigurationManager.AppSettings["BOND_FILE_PATH"];

		public static Dictionary<string,int> bondColumnName;
		public static Dictionary<string, int> bondLivePriceMapping;


		public ExcelHelper()
		{
			bondColumnName = new Dictionary<string, int>();
			bondColumnName.TryAdd("Name of Issuer", 2);
			bondColumnName.TryAdd("ISIN", 1);
			bondColumnName.TryAdd("Date of Allotment", 10);
			bondColumnName.TryAdd("Date of Redemption/Conversion", 11);
			bondColumnName.TryAdd("Face Value (In Rs.)", 8);
			bondColumnName.TryAdd("Frequency of Interest Payment", 14);
			bondColumnName.TryAdd("Coupon Rate (%)", 12);
			bondColumnName.TryAdd("Credit Ratings Agency (Multiple)", 0);
			bondColumnName.TryAdd("Credit Rating", 15);
			bondColumnName.Add("Instrument Status", 0);
			bondColumnName.Add("First IP date", 0);
			bondColumnName.Add("Security Description", 4);
			 
			bondLivePriceMapping = new Dictionary<string, int>();
			bondLivePriceMapping.TryAdd("SYMBOL", 0);
			bondLivePriceMapping.TryAdd("SERIES", 1);
			bondLivePriceMapping.TryAdd("LTP", 5);

		}



		///// <summary>
		///// Get list of equity in _scriptlist
		///// 
		///// </summary>
		//public void GenerateRoceList()
		//{
		//	wb = ExcelApp.Workbooks.Open(EQUITY_FILE_PATH, Missing.Value, false, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
		//	EQUITY_SHEET = (Worksheet)wb.Sheets["ROCE"];

		//	for (int i = startPoint; i < fetchTill; i++)
		//	{
		//		Range comapnyid = (Range)EQUITY_SHEET.Cells[i, 1];
		//		Range roce_16 = (Range)EQUITY_SHEET.Cells[i, 3];
		//		if (roce_16.Value2 == null)
		//		{
		//			if ((int)comapnyid.Value2 > 0)
		//			{
		//				_roceList.Add((int)comapnyid.Value2, new Dictionary<int, string>());
		//			}
		//		}
		//	}
		//	//wb.Close();
		//	//ExcelApp.Quit();
		//}

		//public void GenerateScriptList()
		//{
		//	wb = ExcelApp.Workbooks.Open(EQUITY_FILE_PATH, Missing.Value, false, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
		//	EQUITY_SHEET = (Worksheet)wb.Sheets["Equity"];

		//	for (int i = startPoint; i < fetchTill; i++)
		//	{
		//		Range script = (Range)EQUITY_SHEET.Cells[i, 1];
		//		Range companyName = (Range)EQUITY_SHEET.Cells[i, 2];
		//		_companyName.Add((int)script.Value2, companyName.Value2.ToString());
		//		if ((int)script.Value2 > 0)
		//		{
		//			_masterScriptList.Add((int)script.Value2, new Dictionary<int, string>());
		//			_scriptList.Add((int)script.Value2);

		//		}
		//	}
		//	//wb.Close();
		//	//ExcelApp.Quit();
		//}
		//public void SaveToEquityFile(string sheet)
		//{

		//	EQUITY_SHEET = (Worksheet)wb.Sheets[sheet];			
		//	int cell = startPoint;
		//	foreach (double companyid in _scriptList)
		//	{
		//		Dictionary<int, string> cellsValue = _masterScriptList[companyid];
		//		foreach (var item in cellsValue)
		//		{
		//			Range ColumnTobeUpdated = (Range)EQUITY_SHEET.Cells[cell, item.Key];
		//			ColumnTobeUpdated.Value2 = item.Value;
		//		}
		//		cell++;
		//	}
		//	wb.Save();		 
		//}

		//public void SaveToFinancialWorksheet(string sheet, string value, int mfColumnID, int cellId)
		//{
		//	//int startpoint = 1;
		//	//int cell = 3;
		//	try
		//	{
		//		ASSET_SHEET = (Worksheet)_assetWorkBook.Sheets[sheet];


		//		Range ColumnTobeUpdated = (Range)ASSET_SHEET.Cells[cellId, mfColumnID];
		//		ColumnTobeUpdated.Value2 = value;
		//		_assetWorkBook.Save();
		//	}
		//	catch (Exception ex)
		//	{
		//		string error = ex.StackTrace;
		//	}
		//}

		//public void	SaveToSharesWorksheet(string sheet, string value, int MFColumnID)
		//{
		//	int startpoint = 1;
		//	int cell = 3;
		//	try
		//	{
		//		ASSET_SHEET = (Worksheet)_assetWorkBook.Sheets[sheet];


		//		while (true)
		//		{
		//			var date = (Range)ASSET_SHEET.Cells[cell, startpoint];
		//			if ((double)date.Value2 == DateTime.Today.ToOADate())
		//			{
		//				Range ColumnTobeUpdated = (Range)ASSET_SHEET.Cells[cell, MFColumnID];
		//				ColumnTobeUpdated.Value2 = value;
		//				break;
		//			}
		//			else
		//			{
		//				cell++;
		//			}
		//		}
		//		_assetWorkBook.Save();
		//		//_assetWorkBook.Close();
		//	}
		//	catch(Exception ex)
		//	{
		//		string error = ex.StackTrace;
		//	}
		//}
		//public void KillSpecificExcelFileProcess()
		//{
		//	foreach (Process clsProcess in Process.GetProcesses())
		//	{
		//		if (clsProcess.ProcessName.Equals("EXCEL.EXE"))
		//		{
		//			clsProcess.Kill();
		//			break;
		//		}
		//	}
		//}

		//public Dictionary<string,string> FindValuationBelow(double percentage)
		//{
		//	int cell = startPoint;
		//	Dictionary<string, string> Companys = new Dictionary<string, string>();
		//	EQUITY_SHEET = (Worksheet)wb.Sheets["Revenue"];
		//	foreach (double item in _scriptList)
		//	{
		//		try
		//		{
		//			Range valuation = (Range)EQUITY_SHEET.Cells[cell, 7];
		//			if ((double)valuation.Value2 < percentage)
		//			{
		//				string name = ((Range)EQUITY_SHEET.Cells[cell, 2]).Value2.ToString();
		//				Companys.Add(name,valuation.Value2.ToString());
		//			}
		//			cell++;
		//		}
		//		catch(Exception ex)
		//		{
		//			continue;
		//		}
		//	}

		//	return Companys;

		//}

		public string getMonth(int mnth)
		{
			string month= string.Empty;
			switch(mnth)
			{
				case 1:
					month= "Jan";
				break;

				case 2:
					month= "Feb";
					break;

				case 3:
					month= "Mar";
					break;

				case 4:
					month= "Apr";
					break;

				case 5:
					month= "May";
					break;

				case 6:
					month= "Jun";
					break;
				case 7:
					month= "Jul";
					break;

				case 8:
					month= "Aug";
					break;

				case 9:
					month= "Sep";
					break;

				case 10:
					month= "Oct";
					break;

				case 11:
					month= "Nov";
					break;

				case 12:
					month= "Dec";
					break;
			}
			return month;
		}
		

		public void ReadBondData(IList<string> data,string filePath, Dictionary<string, int> mapping)
		{
			string[] lines = File.ReadAllLines(filePath);	 
			int lineNumber = 0;
			//IList<string> data = new List<string>();
			Dictionary<string, int> collection2 = new Dictionary<string, int>(mapping);
			var csvSplit = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.Compiled);
			string[] csvlines = File.ReadAllLines(filePath.ToString());
			foreach (string line in csvlines)
			{
				string[] item = line.Split(',');
				
				if(lineNumber==0)
				{
					foreach (var key in collection2.Keys)
					{
						int place = item.findIndex(key);
						if (place >= 0)
						{
							mapping[key] = place;
						}
					}
					lineNumber++;
				}
				else
				{
					try
					{
						data.Add(line);
					}
					catch(Exception ex)
					{
						string s = ex.Message;
					}
				}
			}	 

		}
		public double GetMonthlySharePrice(string companyid, int mnth, int year)
		{
			string[] lines = File.ReadAllLines(EQUITY_FILE_PATH);
			double sharePrice=0;
			bool companyMatched = false;

			foreach (string line in lines)
			{
				string[] item= line.Split(',');
			
				if(item[1] == "")
				{
					
					if (item[0] == companyid)
						companyMatched = true;
				}
				else
				{
					if (companyMatched)
					{
						if (item[0] != "Date")
						{
							if (item[0].Split('-')[1] == year.ToString().Substring(2) && item[0].Split('-')[0] == getMonth(mnth))
							{
								sharePrice = Convert.ToDouble(item[4]);
								break;
							}
						}
					}
				}
			}
			return sharePrice;
		}

			//	return CompanysValuation;

			//}
			//public void CloseExcel()
			//{
			//	ExcelApp.Quit();
			//}


		}

	public static class Extensions
	{
		public static int findIndex<T>(this T[] array, T item)
		{
			return Array.FindIndex(array, val => val.Equals(item));
		}
	}
}
