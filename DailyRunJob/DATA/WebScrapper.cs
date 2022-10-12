﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Git_Sandbox.Model;
using OpenQA.Selenium.Support.UI;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Threading.Tasks;
using OpenQA.Selenium.Interactions;

namespace Git_Sandbox.DailyRunJob.DATA
{
	public class WebScrapper : IDisposable
	{
		IWebDriver _driver;
		ChromeOptions chromeOptions;
		string _webScrapperUrl= string.Empty;
		IDictionary<int, double> yearlyPrice;
		const string _mc = "https://www.moneycontrol.com/india/stockpricequote/";
		//Specific to MF INDIA NAMING CONVENTION
		const string _idfcMfCBF = "IDFC Corporate Bond Fund - Direct Growth";
		const string _idfcMfMRF = "IDFC Bond Fund - Medium Term Plan-Direct Plan-Growth";
		const string _idfcMfCRF = "IDFC Credit Risk Fund-Direct Plan-Growth";


		public WebScrapper()
		{
			chromeOptions = new ChromeOptions();
			//chromeOptions.AddArguments("headless");			 
			_driver = new ChromeDriver(chromeOptions);
			 
		}
		private void GetChromeINstance()
		{
			Dispose();
			_driver = new ChromeDriver(chromeOptions);
		}
		public void Dispose()
		{
			if(_driver !=null)
				_driver.Quit();
		}
		public async Task<bool> GetMFDetails(equity eq)
		{
			if (string.IsNullOrEmpty(eq.sourceurl))
			{
				return false;
			}
			
			try
			{
				Console.WriteLine("Access URL");
				_driver.Navigate().GoToUrl(eq.sourceurl);
				Thread.Sleep(5000);

				Console.WriteLine("URL Opened");
				IList<IWebElement> pb = _driver.FindElements(By.XPath("//div/span[@class='amt']"));

				Console.WriteLine("Access AMT detail");
				Thread.Sleep(1000);
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Price for::"+ eq.Companyname  +"::"+pb[0].Text);
				Console.ResetColor();
				Thread.Sleep(1000);
				var price = pb[0].Text.Replace(" ", "").Replace("?", "");
				Thread.Sleep(2000);
				eq.LivePrice = Convert.ToDouble(price.Substring(1, price.Length - 1));
				Thread.Sleep(1500);
				IList<IWebElement> fundSize = _driver.FindElements(By.XPath("//span[@class='amt']"));
				Thread.Sleep(1000);
				var fundS = fundSize[1].Text;
				eq.PB= Convert.ToDouble(fundS.Substring(1, fundS.Length-3));
				
				Thread.Sleep(1500);
				return true;
			}
			catch(Exception ex)
			{
				return false;
			}

		}
		public async Task<bool> GetEquityDetails(equity eq)
		{
			
			if (string.IsNullOrEmpty(eq.divUrl))
			{
				Console.WriteLine("DivURL is empty::"+eq.Companyname);
				return false;
			}
			_driver.Navigate().GoToUrl(eq.divUrl);
			Thread.Sleep(150);
			try
			{
				IList<IWebElement> title = new List<IWebElement>();

				IList<IWebElement> pb = _driver.FindElements(By.XPath("//div[@class='whitebox']"));
				var pricetobook = pb[2].FindElements(By.XPath("//td[@class='textvalue ng-binding']"))[18].Text;
				var mc = pb[2].FindElements(By.XPath("//td[@class='textvalue ng-binding']"))[12].Text;
				IList<IWebElement> prc = _driver.FindElements(By.XPath("//strong[@id='idcrval']"));
				var pr = prc[0].Text;
				eq.LivePrice = Convert.ToDouble(pr);
				title = _driver.FindElements(By.XPath("//h1[@class='panel-title']"));
				if (pricetobook != "-")
				{
					eq.PB = Convert.ToDouble(pricetobook);
					eq.MC = Convert.ToDouble(mc);					 
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine("Some problem fetching company details:"+eq.ISIN);
				Console.WriteLine(ex.StackTrace);
			}
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Successfully got company details:" + eq.ISIN+":: Price::"+eq.LivePrice);
			Console.ResetColor();
			return true;
		}
		public void GetDividendAndTotalShare(dividend d,equity e, string flag)
		{
			if (e.divUrl.Contains("mutual-funds"))
			{
				return;
			}
			if(string.IsNullOrEmpty(e.divUrl))
			{
				Console.WriteLine("DivURL missing for company:"+e.ISIN);
				return;
			}
			dividend div = new dividend();
			div.companyid = e.ISIN;
			
			GetChromeINstance();
			_driver.Navigate().GoToUrl(e.divUrl);
			Thread.Sleep(150);
			
			bool updated = false;
		
			try
			{
				IList<IWebElement> title= new List<IWebElement>();
				IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
				if (!updated)
				{
					IList<IWebElement> pb = _driver.FindElements(By.XPath("//div[@class='whitebox']"));
					var pricetobook = pb[2].FindElements(By.XPath("//td[@class='textvalue ng-binding']"))[18].Text;
					var mc = pb[2].FindElements(By.XPath("//td[@class='textvalue ng-binding']"))[12].Text;
					IList<IWebElement> prc = _driver.FindElements(By.XPath("//strong[@id='idcrval']"));
					var pr = prc[0].Text;
					e.LivePrice = Convert.ToDouble(pr);
					title = _driver.FindElements(By.XPath("//h1[@class='panel-title']"));
					if (pricetobook != "-")
						{
						e.PB = Convert.ToDouble(pricetobook);
						e.MC = Convert.ToDouble(mc);
						if (flag == "PB")
							return;
						title = _driver.FindElements(By.XPath("//h1[@class='panel-title']/a"));
						Thread.Sleep(2500);
						//IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
						js.ExecuteScript("arguments[0].scrollIntoView();", title[4]);
						Thread.Sleep(2500);
						title[4].Click();
						Thread.Sleep(1500);
						int counter = 0;
						Int64 noOfShare=0;
						IList<IWebElement> shrHld = _driver.FindElements(By.XPath("//div[@class='largetable']//td"));
						foreach(IWebElement el in shrHld)
						{
							if(el.Text=="Grand Total")
							{	
								counter = 4;							 
							}
							if (counter > 0)
							{
								counter--;
							}
							if (counter == 1)
							{
								noOfShare= long.Parse(el.Text.Replace(",", ""));
								e.noOfShare = noOfShare;
								break;
							}
						}
					
					}
					updated = true;
					}
				js.ExecuteScript("arguments[0].scrollIntoView();", title[3]);
				Thread.Sleep(2500);
				title[3].Click();
			}
			catch (Exception ex)
			{
				string msg = ex.StackTrace;		
			}
		
			Thread.Sleep(3000);
			int i= 1;
			//Past 2 dividend details
			DateTime previoudDivDate=DateTime.Now;
			double previousDivValue=0;
			while (i <= 5)
			{
				try
				{
					if (e.divUrl.Contains("moneycontrol"))
					{
						GetDividendFromMoneyConterol(div, i);
						i++;
					}
					else
					{
						GetDividendFromBse(div, i);
						i++;						
					}
					Console.WriteLine("Dividend Added:: Companyid:"+ div.companyid +" Date::"+ div.dtUpdated +" Value::"+div.value);
 					if(previoudDivDate == div.dtUpdated && previousDivValue != div.value)
					{
						div.value += previousDivValue;
					}
					div.lastCrawledDate = DateTime.Now;
					component.getMySqlObj().ReplaceDividendDetails(div);
					previoudDivDate = div.dtUpdated;
					previousDivValue = div.value;
				}
				catch (Exception ex)
				{
					i++;
					string msg = ex.StackTrace;
					continue;
				}
			}
			Dispose();
		}

		public IDictionary<int,double> GetHistoricalAssetPrice(string name, int month, int year, AssetType assetType)
		{
			
			if (assetType== AssetType.Shares)
			{
				_webScrapperUrl= "https://www.moneycontrol.com/stocks/histstock.php?classic=true";
				return GetHistoricalSharePrice(name, month, year);
			}
			else
			{
				_webScrapperUrl = "https://www.amfiindia.com/net-asset-value/nav-history";
				//GetHistoricalMFPrice(name, month, year);
				Dictionary<int,double> price= new Dictionary<int, double>();
				// This is a webscrapper
				price.Add(month, GetHistoricalMFPrice(name, month, year));
				//This is to read from a file
				//GetHistoricalMFPrice(assetType);
				return price;
			}
			return GetHistoricalSharePrice(name, month, year);
		}
		CsvConfiguration csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
		{
			HasHeaderRecord = false
		};

		//private void GetHistoricalMFPrice(AssetType assetType)
		//{
		//	try
		//	{
		//		using var streamReader = File.OpenText("..\\..\\..\\AxisLongTerm.csv");
		//		using var csvReader = new CsvReader(streamReader, csvConfig);

		//		string value;

		//		while (csvReader.Read())
		//		{
		//			int month= 0;
		//			int year = 0;
		//			for (int i = 0; csvReader.TryGetField<string>(i, out value); i++)
		//			{
						
		//				switch(i)
		//				{
		//					case 0:
		//						if (value.Contains("Date"))
		//							break;
		//						if (value.Contains("January"))
		//							month = 1;
		//						if (value.Contains("February"))
		//							month = 2;
		//						if (value.Contains("March"))
		//							month = 3;
		//						if (value.Contains("April"))
		//							month = 4;
		//						if (value.Contains("May"))
		//							month = 5;
		//						if (value.Contains("June"))
		//							month = 6;
		//						if (value.Contains("July"))
		//							month = 7;
		//						if (value.Contains("August"))
		//							month = 8;
		//						if (value.Contains("September"))
		//							month = 9;
		//						if (value.Contains("October"))
		//							month = 10;
		//						if (value.Contains("November"))
		//							month = 11;
		//						if (value.Contains("December"))
		//							month = 12;
		//						if (value.Contains("2018"))
		//							year = 2018;
		//						if (value.Contains("2017"))
		//							year = 2017;
		//						if (value.Contains("2019"))
		//							year = 2019;
		//						if (value.Contains("2020"))
		//							year = 2020;
		//						if (value.Contains("2021"))
		//							year = 2021;
		//						if (value.Contains("2016"))
		//							year = 2016;
		//						if (value.Contains("2015"))
		//							year = 2015;
		//						break;
		//					case 1:
		//						break;
		//					case 2:
		//						component.getMySqlObj().UpdateEquityMonthlyPrice(new equityHistory()
		//						{
		//							equityid = "4",
		//							month = month,
		//							price = Convert.ToDouble(value),
		//							year = year,
		//							assetType = Convert.ToInt32(AssetType.EquityMF)
		//						});
		//						break;
		//				}						
		//			}

		//			Console.WriteLine();
		//		}
		//	}
		//	catch(Exception ex)
		//	{
		//		string s = ex.Message;
		//	}
		//}
		private IDictionary<int,double> GetHistoricalSharePrice(string name, int month, int year)
		{			
			try
			{
				int length = 3;
				yearlyPrice = new Dictionary<int, double>();

				Thread.Sleep(2500);				 
				GetChromeINstance();
				_driver.Navigate().GoToUrl(_webScrapperUrl);
				Thread.Sleep(2500);
				IList<IWebElement> input = _driver.FindElements(By.ClassName("inptSrch"));
				string sub = name.Substring(0, length);
				input[0].SendKeys(sub);
				Thread.Sleep(2500);

				IList<IWebElement> suggest = _driver.FindElements(By.XPath("//*[@id='suggest']/ul/li/a"));
				
				while(suggest.Count > 1 || suggest.Count==0 )
				{
					if (name.Length != length)
					{
						length++;
						sub = name.Substring(0, length);
						input[0].Clear();
						input[0].SendKeys(sub);
						Thread.Sleep(2500);
						suggest = _driver.FindElements(By.XPath("//*[@id='suggest']/ul/li/a"));
					}
					else
						break;
					
				}
				suggest[0].Click();
				Thread.Sleep(2500);
				_driver.FindElement(By.XPath("//*[@id='mc_mainWrapper']/div[2]/div[1]/div[5]/div[2]/div[6]/table/tbody/tr/td[3]/form/div[4]/select[1]/option[12]")).Click();
				Thread.Sleep(2500);
				IWebElement selYear= _driver.FindElement(By.XPath("//select[@name='mth_to_yr']"));
				SelectElement el = new SelectElement(selYear);
				el.SelectByValue(year.ToString());
				Thread.Sleep(3000);
				IWebElement selFrmYear = _driver.FindElement(By.XPath("//select[@name='mth_frm_yr']"));
				SelectElement elFrm = new SelectElement(selFrmYear);
				elFrm.SelectByValue(year.ToString());
				Thread.Sleep(3000);

				IList <IWebElement> button = _driver.FindElements(By.XPath("//*[@id='mc_mainWrapper']/div[2]/div[1]/div[5]/div[2]/div[6]/table/tbody/tr/td[3]/form/div[4]/input[1]"));
				// Need to add month and year here
				button[0].Click();
				//TODO- Need to change the logic to check month name here
				int row;
				if (year < DateTime.Now.Year)
				{
					row = month;
				}
				else
				{
					row = DateTime.Now.Month - month;
				}
				 
				int items = _driver.FindElements(By.XPath("//*[@id='mc_mainWrapper']/div[2]/div[1]/div[4]/div[4]/table/tbody/tr")).Count;
				for(int i=2; i<= items;i++)
				{
					string m = _driver.FindElement(By.XPath("//*[@id='mc_mainWrapper']/div[2]/div[1]/div[4]/div[4]/table/tbody/tr[" + i.ToString() + "]/td[1]")).Text;
					string price = _driver.FindElement(By.XPath("//*[@id='mc_mainWrapper']/div[2]/div[1]/div[4]/div[4]/table/tbody/tr[" + i.ToString() + "]/td[5]")).Text;
					GetYearlyPrice(m, price);
				} 
				//var result = _driver.FindElement(By.XPath("//*[@id='mc_mainWrapper']/div[2]/div[1]/div[4]/div[4]/table/tbody/tr["+ row.ToString() +"]/td[5]")).Text;
							
				return yearlyPrice;
			}
			catch(Exception e)
			{
				return yearlyPrice;
			}
			Dispose();
		}
		public string GetActualFundName(string DBSavedMFName)
		{
			if (DBSavedMFName.Contains("IDFC Corporate"))
				return _idfcMfCBF;
			if (DBSavedMFName.Contains("IDFC Credit"))
				return _idfcMfCRF;
			if (DBSavedMFName.Contains("IDFC Bond"))
				return _idfcMfCRF;
			if (DBSavedMFName.Contains("Axis Long"))
				return "Axis Long Term Equity Fund - Direct Plan - Growth Option";
			if (DBSavedMFName.Contains("Kotak Corporate"))
				return "Kotak Corporate Bond Fund- Direct Plan- Growth Option";
			if (DBSavedMFName.Contains("SBI Long"))
				return "SBI LONG TERM ADVANTAGE FUND - SERIES I - DIRECT PLAN - GROWTH";
			return "";
		}
		private double GetHistoricalMFPrice(string name, int month, int year)
		{
			try
			{
				int length = 10;
				if (name == null)
					return 0;
				Thread.Sleep(1500);
				string mfiFundName=GetActualFundName(name);	
				_driver.Navigate().GoToUrl(_webScrapperUrl);

				Thread.Sleep(1500);
				IList<IWebElement> option = _driver.FindElements(By.XPath("//form[@id='navhistory']"));
				IList<IWebElement> tableRow = option[0].FindElements(By.TagName("input"));
				tableRow[2].Click();

				
				//option[2].Click();
				string sub = name.Substring(0, length);
				tableRow[5].SendKeys(name.Substring(0,4));
				//IWebElement input = _driver.FindElement(By.XPath("//*[@id='mfSearchInput']"));
				IList<IWebElement> suggest = _driver.FindElements(By.XPath("//ul//li[@role='presentation']"));
				//input.SendKeys(sub);
				IWebElement mfNames=option[0].FindElement(By.XPath("//ul//li[@role='presentation']//a"));
				//SelectElement mfName = new SelectElement(mfNames);
				mfNames.Click();
				Thread.Sleep(3000);
				//IList <IWebElement> suggest = _driver.FindElements(By.XPath("//ul//li[@role='presentation']"));
				tableRow[7].Click();
				Thread.Sleep(1000);
				tableRow[7].SendKeys(mfiFundName.Substring(0,mfiFundName.Length-1));
				Thread.Sleep(2000);
				suggest = _driver.FindElements(By.XPath("//ul//li[@role='presentation']"));
				Thread.Sleep(2000);
				while (suggest.Count > 2 || suggest.Count == 0)
				{
					if (name.Length != length)
					{

						string c = name.Substring(length, 1);
						length++;
						//sub += c;
						//Thread.Sleep(1000);
						//input.Clear();
						//tableRow[7].Click();
						Thread.Sleep(400);
						//input.SendKeys(c);
						tableRow[7].SendKeys(c);
						//Thread.Sleep(2500);
						suggest = _driver.FindElements(By.XPath("//ul//li[@role='presentation']"));
					}
					else
						break;

				}
				suggest[1].Click();
				Thread.Sleep(3500);
				WebDriverWait wait = new WebDriverWait(_driver,new TimeSpan(1500));

				tableRow[9].Click();
				 
				//IWebElement dayStart = _driver.FindElement(By.XPath("//tr[@id='trToDate']/div/input[1]"));
				tableRow[9].SendKeys("22-Jun-2022");
				//dayStart.SendKeys("25");
				tableRow[10].Click();
				tableRow[10].SendKeys("25-Jun-2022");
				//IWebElement monthStart = _driver.FindElement(By.XPath("//*[@id='fromDate']/div/input[2]"));
				//monthStart.SendKeys(month.ToString());
				//IWebElement yrStart = _driver.FindElement(By.XPath("//*[@id='fromDate']/div/input[3]"));
				//yrStart.SendKeys(year.ToString());

				//wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("//*[@id='toDate']/div/input[1]")));
				//wait.Until(x => x.FindElement(By.XPath("//*[@id='toDate']/div/input[1]")));
				//IWebElement today = _driver.FindElement(By.XPath("//a[@id='hrfGo']"));
				//today.SendKeys("30");
				//IWebElement toMonth = _driver.FindElement(By.XPath("//*[@id='toDate']/div/input[2]"));
				//toMonth.SendKeys(month.ToString());
				//IWebElement toYear = _driver.FindElement(By.XPath("//*[@id='toDate']/div/input[3]"));
				//toYear.SendKeys(year.ToString());

				//todate.SendKeys(Keys.Tab);
				 
				Thread.Sleep(1500);
				_driver.FindElement(By.XPath("//tr[@id='trToDate']")).Click();
				Thread.Sleep(500);
				_driver.FindElement(By.XPath("//*[@id='hrfGo']")).Click();

				Thread.Sleep(1500);
				IWebElement result = _driver.FindElement(By.XPath("//*[@id='divExcelPeriod']/table/tbody/tr[6]/td[1]"));
				var re = result.Text;
				return Convert.ToDouble(result.Text);
				//var result = _driver.FindElement(By.XPath("//*[@id='mc_mainWrapper']/div[2]/div[1]/div[4]/div[4]/table/tbody/tr[3]/td[5]")).Text;

				//return Convert.ToDouble(result);
			}
			catch (Exception e)
			{
				return 0;
			}
			Dispose();
		}

		private void GetDividendFromBse(dividend item, int yr)
		{ 
			var data = _driver.FindElements(By.XPath("//*[@id='tblinsidertrd']/tbody/tr[" + yr + "]/td[2]"))[0].Text;
			Thread.Sleep(2000);
			var dtt = _driver.FindElements(By.XPath("//*[@id='tblinsidertrd']/tbody/tr[" + yr + "]/td[3]"))[0].Text;
			item.dtUpdated = Convert.ToDateTime(dtt);
			item.value = Convert.ToDouble(data);
		}
		private void GetDividendFromMoneyConterol(dividend item, int yr)
		{   //From MoneyControl
			var data = _driver.FindElements(By.XPath("//*[@id='mc_content']/div[2]/section[2]/div/div[2]/table/tbody/tr["+yr+"]/td[5]"))[0].Text;			
			var dtt = _driver.FindElements(By.XPath("//*[@id='mc_content']/div[2]/section[2]/div/div[2]/table/tbody/tr["+yr+"]/td[1]"))[0].Text;			
			item.dtUpdated = Convert.ToDateTime(dtt);
			item.value = Convert.ToDouble(data.Substring(data.IndexOf("Rs.") + 3, 6));
		}
		private void GetYearlyPrice(string month, string price)
		{
			if (month.Contains("Jan"))
				yearlyPrice.Add(1, Convert.ToDouble(price));
			else if (month.Contains("Feb"))
				yearlyPrice.Add(2, Convert.ToDouble(price));
			else if (month.Contains("Mar"))
				yearlyPrice.Add(3, Convert.ToDouble(price));
			else if (month.Contains("Apr"))
				yearlyPrice.Add(4, Convert.ToDouble(price));
			else if (month.Contains("May"))
				yearlyPrice.Add(5, Convert.ToDouble(price));
			else if (month.Contains("June"))
				yearlyPrice.Add(6, Convert.ToDouble(price));
			else if (month.Contains("July"))
				yearlyPrice.Add(7, Convert.ToDouble(price));
			else if (month.Contains("Aug"))
				yearlyPrice.Add(8, Convert.ToDouble(price));
			else if (month.Contains("Sep"))
				yearlyPrice.Add(9, Convert.ToDouble(price));
			else if (month.Contains("Oct"))
				yearlyPrice.Add(10, Convert.ToDouble(price));
			else if (month.Contains("Nov"))
				yearlyPrice.Add(11, Convert.ToDouble(price));
			else if (month.Contains("Dec"))
				yearlyPrice.Add(12, Convert.ToDouble(price));
		}
	}
}
