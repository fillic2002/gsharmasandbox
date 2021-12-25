using System;

using System.Collections.Generic;

using System.Reflection;
using Git_Sandbox.DailyRunJob;
using Git_Sandbox.Model;
using HtmlAgilityPack;
using Microsoft.Office.Interop.Excel;
using Range = Microsoft.Office.Interop.Excel.Range;

namespace DailyRunEquity
{
	public class WebHelper
	{
		private static string _firstYear;
		private static string _secondYear;
		private static string _thirdYear;
		private static string _lastQ;
		private static string _secondLastQ;
		private static string _thirdLastQ;
		private static string _y2016;
		private static string _y2015;
		private static string _y2014;
		private static string _y2013;
		private static string _y2012;
		private static string _eps16;
		private static string _eps15;
		private static string _eps14;

		private static string _Dec16;
		private static string _Sep16;
		private static string _Jun16;

		private static double _closingPriceAvgY2015;
		private static double _closingPriceAvgY2014;
		private static double _closingPriceAvgY2013;
		private static double _closingPriceAvgY2016;
		public List<double> _scriptList = new List<double>() { };
		public List<double> _currentValues = new List<double>();
		//Master list will contain key as companycode and dictionary as individual cell values
		public Dictionary<double, Dictionary<int, string>> _masterScriptList = new Dictionary<double, Dictionary<int, string>>();

		public string Script;
		private SortedDictionary<double, string> _companyName = new SortedDictionary<double, string>();
		int startPoint = 1500;
		int fetchTill = 2500;

		//Excel related var
		//Microsoft.Office.Interop.Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application();
		//ExcelApp.Visible = true;
		//Microsoft.Office.Interop.Excel.Workbook wb;
		//Microsoft.Office.Interop.Excel.Worksheet sh;
		//Microsoft.Office.Interop.Excel.Worksheet _boughtByBulls;

		public SortedDictionary<double,string> CompanyName
		{
			get{ return _companyName; }
			set{ _companyName = value; }
		}

		public WebHelper()
		{
			//string filePath = @"C:\d\Personal\Doc\Equity.xlsx";
			//Microsoft.Office.Interop.Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application();
			
			//wb = ExcelApp.Workbooks.Open(filePath, Missing.Value, false, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
			//sh = (Microsoft.Office.Interop.Excel.Worksheet)wb.Sheets["Equity"];
			//_boughtByBulls = (Microsoft.Office.Interop.Excel.Worksheet)wb.Sheets["BoughtByBulls"];
		}
		public static void GetLastThreeYearNPM(double script)
		{
			try
			{
				
				string url = "http://www.bseindia.com/stock-share-price/stockreach_financials.aspx?scripcode=" + script.ToString() + "&expandable=0";

				_firstYear = String.Empty;
				_secondYear = String.Empty;
				_thirdYear = String.Empty;

				HtmlWeb web = new HtmlWeb();
				HtmlAgilityPack.HtmlDocument doc = web.Load(url);

				string NPM = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_quatre')]//table//tr[17]//td[1]")[0].InnerText;
				
				if (NPM == "NPM %")
				{
					var _firstYear = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_quatre')]//table//tr[17]//td[2]")[0].InnerText;
					if (_firstYear != "--")
					{
						if (Convert.ToDouble(_firstYear) > 10)
						{
							_secondYear = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_quatre')]//table//tr[18]//td[3]")[0].InnerText;
							_thirdYear = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_quatre')]//table//tr[19]//td[4]")[0].InnerText;
						}
					}
				}			
			}
			catch(Exception ex)
			{
				Console.WriteLine("Exception for item:" + script);
			}
		}

		public void GetGivenQuartersResultCumulative(List<string> quarter)
		{
			GenerateScriptList();
			int i = 0;
			foreach (double scriptid in _scriptList)
			{
				try
				{
					GetBalanceSheetDetails(scriptid);
					double revTillDate = Convert.ToDouble(_Dec16) + Convert.ToDouble(_Sep16) + Convert.ToDouble(_Jun16);

					Dictionary<int, string> values = new Dictionary<int, string>();
					values.Add(15, revTillDate.ToString());
					_masterScriptList[scriptid] = values;

				Console.Write(i++);
				}
				catch (Exception ex)
				{
					if(ex.Message.Contains("Input string was not in a correct format."))
					{
					Console.WriteLine("Known Error");
					}

				}
			}
			
			SaveToEquityFile();
			//ExcelApp.Quit();
		}
	
		public void GetBalanceSheetDetails(double script)
		{
			string url = "http://www.bseindia.com/stock-share-price/stockreach_financials.aspx?scripcode=" + script.ToString() + "&expandable=0";

			_y2012= String.Empty;
			_y2013= String.Empty;
			_y2014= String.Empty;
			_y2015 = String.Empty;
			_y2016 = String.Empty;

			_Dec16 = String.Empty;
			_Sep16 = String.Empty;
			_Jun16 = String.Empty;



			try
			{
				HtmlWeb web = new HtmlWeb();
				HtmlAgilityPack.HtmlDocument doc = web.Load(url);

				//Uncomment if u need yearly details
				//string Y1 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr[1]/td[2]")[0].InnerText;
				//SetYear(Y1, doc, "2");
				//string Y2 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr[1]/td[3]")[0].InnerText;
				//SetYear(Y2, doc, "3");
				//string Y3 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr[1]/td[4]")[0].InnerText;
				//SetYear(Y3, doc, "4");
				//string Y4 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr[1]/td[5]")[0].InnerText;
				//SetYear(Y4, doc, "5");
				//string Y5 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr[1]/td[6]")[0].InnerText;
				//SetYear(Y5, doc, "6");

				string Q317 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_quatre')]//table//tr[1]//td[1]//table//tr[1]/td[2]")[0].InnerText;
				SetYear(Q317, doc, "2");
				string Q217 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_quatre')]//table//tr[1]//td[1]//table//tr[1]/td[3]")[0].InnerText;
				SetYear(Q217, doc, "3");
				string Q117 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_quatre')]//table//tr[1]//td[1]//table//tr[1]/td[4]")[0].InnerText;
				SetYear(Q117, doc, "4");

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.StackTrace);
				
			}

		}

		private void SetYear(string ColumnName, HtmlAgilityPack.HtmlDocument doc, string loc)
		{
			if (ColumnName == "2016")
			{
				_y2016 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr[3]/td[" + loc + "]")[0].InnerText;
				return;
			}
			if (ColumnName == "2015")
			{
				_y2015 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr[3]/td[" + loc + "]")[0].InnerText;
				return;
			}
			if (ColumnName == "2014")
			{
				_y2014 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr[3]/td[" + loc + "]")[0].InnerText;
				return;
			}
			if (ColumnName == "2013")
			{
				_y2013 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr[3]/td[" + loc + "]")[0].InnerText;
				return;
			}
			if (ColumnName == "2012")
			{
				_y2012 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr[3]/td[" + loc + "]")[0].InnerText;
				return;
			}
			if (ColumnName == "Dec-16")
			{
				_Dec16 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_quatre')]//table//tr[1]//td[1]//table//tr[3]/td[" + loc + "]")[0].InnerText;
				return;
			}
			if (ColumnName == "Sep-16")
			{
				_Sep16 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_quatre')]//table//tr[1]//td[1]//table//tr[3]/td[" + loc + "]")[0].InnerText;
				return;
			}
			if (ColumnName == "Jun-16")
			{
				_Jun16 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_quatre')]//table//tr[1]//td[1]//table//tr[3]/td[" + loc + "]")[0].InnerText;
				return;
			}
		}

		private string GetShareHoldingPattern(double script, string q)
		{
			try
			{
				string url = "http://www.bseindia.com/corporates/shpSecurities.aspx?scripcd="+ script.ToString() +"&qtrid="+ q +"&Flag=New";

				_lastQ = String.Empty;
				_secondLastQ = String.Empty;
				_thirdLastQ = String.Empty;

				HtmlWeb web = new HtmlWeb();
				HtmlAgilityPack.HtmlDocument doc = web.Load(url);

				 
				string Promotor = doc.DocumentNode.SelectNodes("//div[contains(@id, 'tdData')]//table//tr[3]//table//tr[4]//td[1]")[0].InnerText;
				if (Promotor == "(A) Promoter & Promoter Group")
				{
					return doc.DocumentNode.SelectNodes("//div[contains(@id, 'tdData')]//table//tr[3]//table/tr[4]//td[6]")[0].InnerText;

				}
				return string.Empty;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception for item:" + ex);
				return string.Empty;
			}
		}

		public void GenerateScriptList()
		{			
			//for (int i = startPoint; i < fetchTill; i++)
			//{
			//	Range script = (Range)sh.Cells[i, 1];
			//	Range companyName = (Range)sh.Cells[i, 2];
			//	_companyName.Add((double)script.Value2, companyName.Value2.ToString());
			//	if ((int)script.Value2 > 0 )
			//	{
			//		_masterScriptList.Add((double)script.Value2, new Dictionary<int, string>());
			//		_scriptList.Add((double)script.Value2);
					 
			//	}
			//}
			//ExcelApp.Quit();
		}
		 
		public void ReadAllScript()
		{
			string filePath = @"C:\d\Personal\Doc\Equity.xlsx";
			Microsoft.Office.Interop.Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application();
			//ExcelApp.Visible = true;
			Microsoft.Office.Interop.Excel.Workbook wb = ExcelApp.Workbooks.Open(filePath, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);

			Microsoft.Office.Interop.Excel.Worksheet sh = (Microsoft.Office.Interop.Excel.Worksheet)wb.Sheets["Equity"];
			//Microsoft.Office.Interop.Excel.Range xlRng = sh.get_Range("A2", "A30");


			for (int i = 2; i < 2916; i++)
			{
				Microsoft.Office.Interop.Excel.Range range = (Microsoft.Office.Interop.Excel.Range)sh.Cells[i, 1];
				Microsoft.Office.Interop.Excel.Range rangeComapny = (Microsoft.Office.Interop.Excel.Range)sh.Cells[i, 2];
				Microsoft.Office.Interop.Excel.Range rangeLastYearPrice = (Microsoft.Office.Interop.Excel.Range)sh.Cells[i, 15];
				Microsoft.Office.Interop.Excel.Range secondYearRange = (Microsoft.Office.Interop.Excel.Range)sh.Cells[i, 16];
				Microsoft.Office.Interop.Excel.Range thirdYearRange = (Microsoft.Office.Interop.Excel.Range)sh.Cells[i, 17];
				//Revenue details
				Microsoft.Office.Interop.Excel.Range y2016 = (Microsoft.Office.Interop.Excel.Range)sh.Cells[i, 21];
				Microsoft.Office.Interop.Excel.Range y2015 = (Microsoft.Office.Interop.Excel.Range)sh.Cells[i, 22];
				Microsoft.Office.Interop.Excel.Range y2014= (Microsoft.Office.Interop.Excel.Range)sh.Cells[i, 23];
				Microsoft.Office.Interop.Excel.Range y2013 = (Microsoft.Office.Interop.Excel.Range)sh.Cells[i, 24];
				Microsoft.Office.Interop.Excel.Range y2012 = (Microsoft.Office.Interop.Excel.Range)sh.Cells[i, 25];

				if (!String.IsNullOrEmpty((string)rangeComapny.Value2))
				{
					if (y2016.Value2 == null)
					{
						#region GetLastThreeYearNPM
						//GetLastThreeYearNPM((double)range.Value2);
						//rangeLastYearPrice.Value2 = _firstYear;  //GetShareHoldingPattern((double)range.Value2, "91.00");
						//secondYearRange.Value2 = _secondYear; //GetShareHoldingPattern((double)range.Value2, "90.00");
						//thirdYearRange.Value2 = _thirdYear; //GetShareHoldingPattern((double)range.Value2, "89.00");
						#endregion
						#region Get last five year revenue
							GetBalanceSheetDetails((double)range.Value2);
							y2016.Value2 = _y2016;
							y2015.Value2 = _y2015;
							y2014.Value2 = _y2014;
							y2013.Value2 = _y2013;
							y2012.Value2 = _y2012;
						#endregion
						//catch(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
						//{
						//	rangeLastYearPrice.Value2 = GetShareHoldingPattern((double)range.Value2, "91.00");
						//	secondYearRange.Value2 = GetShareHoldingPattern((double)range.Value2, "90.00");
						//	thirdYearRange.Value2 = GetShareHoldingPattern((double)range.Value2, "89.00");
						//}

					}
				}
			 
				Console.Write(i + "-");
			}

			ExcelApp.Quit();
		}

		public void CalculateHistoricalPE(string script)
		{
			string filePath = @"C:\Users\u0156319\Downloads\" + script + ".csv";
			Microsoft.Office.Interop.Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application();
			//ExcelApp.Visible = true;
			Microsoft.Office.Interop.Excel.Workbook wb = ExcelApp.Workbooks.Open(filePath, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
			Microsoft.Office.Interop.Excel.Worksheet sh = (Microsoft.Office.Interop.Excel.Worksheet)wb.Sheets[script];
			Double y2012Closing = 0;
			Double y2015Closing = 0;
			Double y2014Closing = 0;
			Double y2013Closing = 0;
			int Days2015 = 0;
			int Days2014 = 0;
			int Days2013 = 0;
			int Days2012 = 0;

			DateTime fy2015 = new DateTime(2015, 4, 1);
			DateTime fy2014 = new DateTime(2014, 4, 1);
			DateTime fy2013 = new DateTime(2013, 4, 1);
			DateTime fy2012 = new DateTime(2012, 4, 1);
			for (int i = 2; i < 1250; i++)
			{
				Microsoft.Office.Interop.Excel.Range y = (Microsoft.Office.Interop.Excel.Range)sh.Cells[i, 1];
				Microsoft.Office.Interop.Excel.Range c = (Microsoft.Office.Interop.Excel.Range)sh.Cells[i, 5];
				if (y.Value2 != null)
				{
					if (DateTime.FromOADate((double)y.Value2) >= fy2015)
					{
						y2015Closing += (double)c.Value2;
						Days2015++;
						continue;
					}
					else if (DateTime.FromOADate((double)y.Value2) >= fy2014)
					{
						y2014Closing += (double)c.Value2;
						Days2014++;
						continue;
					}
					else if (DateTime.FromOADate((double)y.Value2) >= fy2013)
					{
						y2013Closing += (double)c.Value2;
						Days2013++;
						continue;
					}
					else if (DateTime.FromOADate((double)y.Value2) >= fy2012)
					{
						y2012Closing +=(double)c.Value2;
						Days2012++;
						continue;
					}
				}
			}
			ExcelApp.Quit();
			_closingPriceAvgY2016 = y2015Closing / Days2015;
			_closingPriceAvgY2015 = y2014Closing / Days2014;
			_closingPriceAvgY2014 = y2013Closing / Days2014;
			//SaveToEquityFile(27, _closingPriceAvgY2015.ToString());
			//SaveToEquityFile(28, _closingPriceAvgY2014.ToString());
			//SaveToEquityFile(29, _closingPriceAvgY2013.ToString());
			Dictionary<int, string> values = new Dictionary<int, string>();
			values.Add(26, _closingPriceAvgY2016.ToString());
			values.Add(27, _closingPriceAvgY2015.ToString());
			values.Add(28, _closingPriceAvgY2014.ToString());

			_masterScriptList.Add(Convert.ToDouble(script), values);
			SaveToEquityFile();
		}

		public void GetEPSForYear()
		{
			int i = 0;

			foreach (double scripid in _scriptList)
			{
				string EPS;
				string url = "http://www.bseindia.com/stock-share-price/stockreach_financials.aspx?scripcode=" + scripid + "&expandable=0";
				HtmlWeb web = new HtmlWeb();
				HtmlAgilityPack.HtmlDocument doc = web.Load(url);
				try
				{
					
					string y1 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr//td//table//tr//td[2]")[0].InnerText;
					if (y1 == "2016")
					{
						Dictionary<int, string> CellToModify = new Dictionary<int, string>();
						_y2016 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr//td//table//tr//td[2]")[0].InnerText;
						_eps16 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr[14]//td[2]")[0].InnerText;
						string interest16  = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr[7]//td[2]")[0].InnerText;
						CellToModify.Add(10,interest16);
						CellToModify.Add(30, _eps16);

						_masterScriptList[scripid] = CellToModify;

						_y2015 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr//td[3]")[0].InnerText;
						_eps15 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr[14]//td[3]")[0].InnerText;
						string interest15 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr[7]//td[3]")[0].InnerText;
						CellToModify.Add(31, _eps15);
						CellToModify.Add(11, interest15);

						_masterScriptList[scripid] = CellToModify;

						_y2014 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr//td[4]")[0].InnerText;
						_eps14 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr[14]//td[4]")[0].InnerText;
						string interest14 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr[7]//td[4]")[0].InnerText;


						CellToModify.Add(32, _eps14);
						CellToModify.Add(12, interest14);

						string interest13 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr[7]//td[5]")[0].InnerText;
						CellToModify.Add(13, interest13);

						_masterScriptList[scripid] = CellToModify;
					}
					if (y1 == "2015")
					{
						_y2015 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr//td[2]")[0].InnerText;
						_eps15 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr[14]//td[2]")[0].InnerText;
						string interest15 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr[7]//td[2]")[0].InnerText;

						_y2014 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr//td[3]")[0].InnerText;
						_eps14 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr[14]//td[3]")[0].InnerText;
						string interest14 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr[7]//td[3]")[0].InnerText;

						string interest13 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_anntre')]//table//tr[1]//td[1]//table//tr[7]//td[4]")[0].InnerText;

						Dictionary<int, string> CellToModify = new Dictionary<int, string>();

						CellToModify.Add(31, _eps15);
						CellToModify.Add(32, _eps14);
						CellToModify.Add(11, interest15);
						CellToModify.Add(12, interest14);
						CellToModify.Add(13, interest13);

						_masterScriptList[scripid] = CellToModify;
					}
					Console.Write(i++);
				}

				catch (Exception ex)
				{
					Console.WriteLine(i++);
					Console.WriteLine(ex.StackTrace);
					continue;
				}
			}
		}
		

		public void SaveToEquityFile()
		{
			//int cell = startPoint;
			//foreach(double companyid in _scriptList)
			//{
			//	Dictionary<int, string> cellsValue = _masterScriptList[companyid];
			//	foreach (var item in cellsValue)
			//	{
			//		Microsoft.Office.Interop.Excel.Range ColumnTobeUpdated = (Microsoft.Office.Interop.Excel.Range)sh.Cells[cell, item.Key];
			//		ColumnTobeUpdated.Value2 = item.Value;
			//	}
			//	cell++;
			//}

			
			//wb.Save();
			//ExcelApp.Quit();
		}

		public void ValuePresent(int ColumnId)
		{
			//for (int i = 2500; i < 2916; i++)
			//{
			//	Microsoft.Office.Interop.Excel.Range CompanyCode = (Microsoft.Office.Interop.Excel.Range)sh.Cells[i, 1];
			//	Microsoft.Office.Interop.Excel.Range ColumnTobeUpdated = (Microsoft.Office.Interop.Excel.Range)sh.Cells[i, ColumnId];
			//	if (ColumnTobeUpdated.Value2 == null)
			//	{
			//		_currentValues.Add((double)CompanyCode.Value2);
			//		Console.WriteLine("Script Added" + i);
			//	}			
			//}
			 
			//ExcelApp.Quit();
			//Console.WriteLine("Total Script Added" + _currentValues.Count);
		}
		public void GetPreviousInterestPaidCapital()
		{
			//GenerateScriptList();

			//GetEPSForYear();

			//SaveToEquityFile();
			//ExcelApp.Quit();
		}

		public void getbigbullpurchase()
		{
			GenerateScriptList();
			int rowid = 3;
			int dataid = 11;
			HtmlWeb web = new HtmlWeb();
			foreach (double script in _scriptList)
			{
				rowid++;
				if (script <= 532416)
					continue;

				string url = "http://www.bseindia.com/stock-share-price/stockreach_bulkblock.aspx?scripcode="+ script + "&expandable=9";				
				HtmlAgilityPack.HtmlDocument doc = web.Load(url);
				try
				{
					int i = 2;
					
					 
					while (true)
					{
						string buyer = doc.DocumentNode.SelectNodes("//table[contains(@id, 'ctl00_ContentPlaceHolder1_gvData')]//tr["+ i +"]//td[2]")[0].InnerText;
						if (buyer.Trim() == "PORINJUV VELIYATH" || buyer == "RAKESH RADHEYSHYAM JHUNJHUNWALA")
						{
							Console.Write(rowid);

							//Microsoft.Office.Interop.Excel.Range scriptid = (Microsoft.Office.Interop.Excel.Range)_boughtByBulls.Cells[dataid, 1];
							//Microsoft.Office.Interop.Excel.Range bullname = (Microsoft.Office.Interop.Excel.Range)_boughtByBulls.Cells[dataid, 2];
							//scriptid.Value2 = script;
							//bullname.Value2 = buyer;
							//dataid++;
							//wb.Save();
							 
							
						}
						i++;
					}
					
				}
				catch(Exception ex)
				{					
					
					continue;
				}
			}
			//ExcelApp.Quit();
		}

		public void GetProcurementDetails()
		{
			component.getEprocObj().ShowProcurementInfo();
			 
		}
		

	}
}
