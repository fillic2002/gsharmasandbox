using System;
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
using myfinAPI.Model;
using static myfinAPI.Model.AssetClass;
using myfinAPI.Model.DTO;
using System.Collections.Concurrent;

namespace Git_Sandbox.DailyRunJob.DATA
{
 

	public class WebScrapper : IDisposable
	{
		IWebDriver _driver;
		ChromeOptions chromeOptions;
		string _webScrapperUrl= string.Empty;
		IDictionary<int, decimal> yearlyPrice;
		const string _mc = "https://www.moneycontrol.com/india/stockpricequote/";
		const string _bondPrice = "https://www.nseindia.com/market-data/bonds-traded-in-capital-market";
		const string _nseBondPriceLink ="https://www.nseindia.com/get-quotes/bonds?symbol=";
		const string _bondFrequencyPriceLink = "https://www.indiabondinfo.nsdl.com/bds-web/homePortal.do?action=searchDtls&isinno=";
		//Specific to MF INDIA NAMING CONVENTION
		const string _idfcMfCBF = "IDFC Corporate Bond Fund - Direct Growth";
		const string _idfcMfNiftyMF = "IDFC Nifty 50 Index Fund-Direct Plan-Growth";
		const string _idfcMfMRF = "IDFC Bond Fund - Medium Term Plan-Direct Plan-Growth";
		const string _idfcMfCRF = "IDFC Credit Risk Fund-Direct Plan-Growth";
		const string _bondSymbol = "NHAI,NHIT,IRFC,PFC";

		 
	 


		public WebScrapper()
		{
		 
			chromeOptions = new ChromeOptions();
			//chromeOptions.AddArguments("headless");	
			//chromeOptions.AddArguments("User-Agent:  Chrome/107.0.0.0 Safari/537.36 ");
			chromeOptions.AddArguments("--safebrowsing-disable-download-protection");
			_driver = new ChromeDriver(chromeOptions);
			
		 
		}
	 

		private void GetChromeINstance()
		{
			Dispose();
			_driver = new ChromeDriver(chromeOptions);
		}
		
	
	public void Dispose()
		{
			if (_driver != null)
			{
				Console.WriteLine("Closing ThreadID::" + Thread.CurrentThread.ManagedThreadId);
				_driver.Close();
				_driver.Quit();
			}
		}
		public async Task<bool> GetMFDetails(EquityBase eq)
		{
			if (string.IsNullOrEmpty(eq.sourceurl))
			{
				return false;
			}
			
			try
			{
				Console.WriteLine("Access URL");
				//GetChromeINstance();
				_driver.Navigate().GoToUrl(eq.sourceurl);
				Thread.Sleep(4000);

				Console.WriteLine("URL Opened");
				IList<IWebElement> pb = _driver.FindElements(By.XPath("//div/span[@class='amt']"));

				Console.WriteLine("Access AMT detail");
				//Thread.Sleep(1000);
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Price for::"+ eq.equityName  +"::"+pb[0].Text);
				Console.ResetColor();
				//Thread.Sleep(1000);
				var price = pb[0].Text.Replace(" ", "").Replace("?", "");
				//Thread.Sleep(1000);
				eq.livePrice = Convert.ToDecimal(price.Substring(1, price.Length - 1));
				//Thread.Sleep(1000);
				IList<IWebElement> fundSize = _driver.FindElements(By.XPath("//span[@class='amt']"));
				Thread.Sleep(1000);
				var fundS = fundSize[1].Text;
				eq.MarketCap= Convert.ToDecimal(fundS.Substring(1, fundS.Length-3));
				eq.lastUpdated = DateTime.Now;
				Thread.Sleep(100);
				//Dispose();
				return true;
			}
			catch(Exception ex)
			{
				Dispose();
				return false;
			}
			//Dispose();
		}
		public async Task<bool> GetEquityDetails(EquityBase eq)
		{			
			if (string.IsNullOrEmpty(eq.divUrl))
			{
			//	Console.WriteLine("DivURL is empty::"+eq.equityName);
				return false;
			}
			//GetChromeINstance();
			try {
				_driver.Navigate().GoToUrl(eq.divUrl);
				 
			}
			catch(Exception ex)
			{
				string s = ex.Message;
				GetChromeINstance();
			}
		 
			Thread.Sleep(450);
			try
			{
				IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
				IList<IWebElement> title = new List<IWebElement>();

				IList<IWebElement> pb = _driver.FindElements(By.XPath("//div[@class='whitebox']"));
				var pricetobook = pb[2].FindElements(By.XPath("//td[@class='textvalue ng-binding']"))[18].Text;
				var mc = pb[2].FindElements(By.XPath("//td[@class='textvalue ng-binding']"))[12].Text;
				IList<IWebElement> prc = _driver.FindElements(By.XPath("//strong[@id='idcrval']"));
				var pr = prc[0].Text;
				eq.livePrice = Convert.ToDecimal(pr);
				title = _driver.FindElements(By.XPath("//h1[@class='panel-title']"));
				if (pricetobook != "-")
				{
					eq.PB = Convert.ToDecimal(pricetobook);
					eq.MarketCap = Convert.ToDecimal(mc);					 
				}
				if (eq.lastUpdated.AddDays(1) >= DateTime.Now)
					return true;
				//Freefloat details
				Thread.Sleep(200);
				title = _driver.FindElements(By.XPath("//h1[@class='panel-title']/a"));
				Thread.Sleep(2000);
				//IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
				js.ExecuteScript("arguments[0].scrollIntoView();", title[4]);
				Thread.Sleep(1000);
				title[4].Click();
				Thread.Sleep(1000);
				UInt64 noOfShare = 0;
				IList<IWebElement> shrHld = _driver.FindElements(By.XPath("//div[@class='largetable']//td"));
				IList<IWebElement> rows = _driver.FindElements(By.XPath("//div[@class='largetable']//tr"));
				foreach (IWebElement row in rows)
				{
					//Console.WriteLine(row.Text);
					if (row.Text.Contains("Grand Total") && row.Text.StartsWith("Grand"))
					{
						//Console.WriteLine(row.Text);
						if (row.Text.StartsWith("Grand"))
						{
							var s = row.FindElements(By.TagName("td"));
							Console.ForegroundColor = ConsoleColor.Yellow;
							Console.Write("TotalShare::"+s[3].Text);
							Console.ResetColor();
							noOfShare = UInt64.Parse(s[3].Text.Replace(",", ""));
							eq.freefloat = noOfShare;
							break;
						}
					}
				}
			}
			catch(Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Some problem fetching company details:"+eq.equityName);
				Console.WriteLine(ex.StackTrace);
				Console.ResetColor();
			}
			 
			//Dispose();
			return true;
		}
		public void GetEquityDivAndBonusDetail(dividend d, EquityBase e, string flag)
		{
			if (e.divUrl.Contains("mutual-funds"))
			{
				return;
			}
			if (string.IsNullOrEmpty(e.divUrl))
			{
				Console.WriteLine("DivURL missing for company:" + e.assetId);
				return;
			}
			 
			//GetChromeINstance();
			Thread.Sleep(2000);
			_driver.Navigate().GoToUrl(e.divUrl);
			IList<IWebElement> title = new List<IWebElement>();
			IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
			Thread.Sleep(2000);
			title = _driver.FindElements(By.XPath("//h1[@class='panel-title']"));
			js.ExecuteScript("arguments[0].scrollIntoView();", title[3]);
			Thread.Sleep(4500);
			title[3].Click();
			Thread.Sleep(4500);
			var lin = _driver.FindElements(By.XPath("//*[@id='tblinsidertrd']/tbody/tr[6]/td/a/i"));

			IList<IWebElement> lin2 = _driver.FindElements(By.XPath("//*[@id='tblinsidertrd']/tbody/tr"));
			foreach(IWebElement ele in lin2)
			{
				if(ele.FindElements(By.TagName("a")).Count>0)
				{
					//var s = lin2[1].FindElements(By.TagName("a"));
					ele.FindElement(By.TagName("a")).Click();
					break;
				}
			}
			Thread.Sleep(4500);
			//GetBonusDetailsFromBse(div);
			
			var divAndBonusRows = _driver.FindElements(By.TagName("tr"));
			int rowCount = 5;
			foreach(IWebElement row in divAndBonusRows)
			{
				if (divAndBonusRows.Count  >= rowCount+2) // last 10 records
				{
					rowCount++;
					continue;
				}
					
				dividend div = new dividend();
				div.companyid = e.assetId;
				var cell=row.FindElements(By.TagName("td"));
				if(cell.Count>=10 && cell.Count<13)
				{
					if(cell[3].Text.Contains("Dividend"))
					{
						string divi = cell[3].Text.Substring(cell[3].Text.IndexOf("Rs")+6,4);
						if(cell[3].Text.Contains("Special"))
							div.creditType = TypeOfCredit.SpclDividend;
						else if(cell[3].Text.Contains("Final"))
							div.creditType = TypeOfCredit.FDividend;
						else if (cell[3].Text.Contains("Interim"))
							div.creditType = TypeOfCredit.IntDividend;
						else 
							div.creditType = TypeOfCredit.IntDividend;

						div.value = Convert.ToDecimal(divi);
						div.dtUpdated = Convert.ToDateTime(cell[2].Text);
						div.lastCrawledDate = DateTime.Parse(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

						component.getMySqlObj().ReplaceDividendDetails(div);

					}else if(cell[3].Text.Contains("Bonus"))
					{
						Console.WriteLine(cell[3].Text);
						string b = cell[3].Text.Replace("Bonus issue","");
						div.creditType = TypeOfCredit.Bonus;
						div.value = Convert.ToDecimal(b.Replace(':','.'));
						div.dtUpdated = Convert.ToDateTime(cell[2].Text);
						div.lastCrawledDate = DateTime.Parse(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
						component.getMySqlObj().ReplaceDividendDetails(div);
					}
					rowCount++;
				}
				 
				
			}

			//Dispose();
		}
		public void GetDividendAndTotalShare(dividend d,EquityBase e, string flag)
		{
			if (e.divUrl.Contains("mutual-funds"))
			{
				return;
			}
			if(string.IsNullOrEmpty(e.divUrl))
			{
				Console.WriteLine("DivURL missing for company:"+e.assetId);
				return;
			}
			dividend div = new dividend();
			div.companyid = e.assetId;
			
			//GetChromeINstance();
			Thread.Sleep(2000);
			_driver.Navigate().GoToUrl(e.divUrl);
			 
			
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
					e.livePrice = Convert.ToDecimal(pr);
					title = _driver.FindElements(By.XPath("//h1[@class='panel-title']"));
					if (pricetobook != "-")
						{
						e.PB = Convert.ToDecimal(pricetobook);
						e.MarketCap = Convert.ToDecimal(mc);
						if (flag == "PB")
							return;
						title = _driver.FindElements(By.XPath("//h1[@class='panel-title']/a"));
						Thread.Sleep(2500);
						//IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
						js.ExecuteScript("arguments[0].scrollIntoView();", title[4]);
						Thread.Sleep(2000);
						title[4].Click();
						Thread.Sleep(1500);
						int counter = 0;
						UInt64 noOfShare =0;
						IList<IWebElement> shrHld = _driver.FindElements(By.XPath("//div[@class='largetable']//td"));
						IList<IWebElement> rows = _driver.FindElements(By.XPath("//div[@class='largetable']//tr"));
						foreach(IWebElement row in rows)
						{
							//Console.WriteLine(row.Text);
							if(row.Text.Contains("Grand Total") && row.Text.StartsWith("Grand"))
							{	
								Console.WriteLine(row.Text);								
								if(row.Text.StartsWith("Grand"))
								{
									var s = row.FindElements(By.TagName("td"));
									Console.ForegroundColor = ConsoleColor.Yellow;
									Console.WriteLine(s[3].Text);
									Console.ResetColor();
									noOfShare = UInt64.Parse(s[3].Text.Replace(",", ""));
									e.freefloat = noOfShare;
									break;
								}								
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
			decimal previousDivValue=0;
			GetBonusDetailsFromBse(div);
			//Update Bonus here
			//component.getMySqlObj().ReplaceDividendDetails(div);

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
						//GetBonusDetailsFromBse(div);
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
			//Dispose();
		}

		public IDictionary<int,decimal> GetHistoricalAssetPrice(string name, int month, int year, AssetType assetType)
		{
			
			if (assetType== AssetType.Shares)
			{
				_webScrapperUrl= "https://www.moneycontrol.com/stocks/histstock.php?classic=true";
				return GetHistoricalSharePrice(name, month, year);
			}
			else
			{
				_webScrapperUrl = "https://www.amfiindia.com/net-asset-value/nav-history";				
				Dictionary<int,decimal> price= new Dictionary<int, decimal>();
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
		private IDictionary<int,decimal> GetHistoricalSharePrice(string name, int month, int year)
		{			
			try
			{
				int length = 3;
				yearlyPrice = new Dictionary<int, decimal>();

				Thread.Sleep(2500);				 
				//GetChromeINstance();
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
						Thread.Sleep(500);
						suggest = _driver.FindElements(By.XPath("//*[@id='suggest']/ul/li/a"));
					}
					else
						break;
					
				}
				suggest[0].Click();
				Thread.Sleep(1500);
				_driver.FindElement(By.XPath("//*[@id='mc_mainWrapper']/div[2]/div[1]/div[5]/div[2]/div[6]/table/tbody/tr/td[3]/form/div[4]/select[1]/option[12]")).Click();
				Thread.Sleep(1500);
				IWebElement selYear= _driver.FindElement(By.XPath("//select[@name='mth_to_yr']"));
				SelectElement el = new SelectElement(selYear);
				el.SelectByValue(year.ToString());
				Thread.Sleep(1500);
				IWebElement selFrmYear = _driver.FindElement(By.XPath("//select[@name='mth_frm_yr']"));
				SelectElement elFrm = new SelectElement(selFrmYear);
				elFrm.SelectByValue(year.ToString());
				Thread.Sleep(1500);

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

				//Dispose();
				return yearlyPrice;
			}
			catch(Exception e)
			{
				return yearlyPrice;
				//Dispose();
			} 
		}
		public string GetActualFundName(string DBSavedMFName)
		{
			if (DBSavedMFName.Contains("IDFC Corporate"))
				return _idfcMfCBF;
			else if (DBSavedMFName.Contains("IDFC nifty"))
				return _idfcMfNiftyMF;
			else if (DBSavedMFName.Contains("IDFC Credit"))
				return _idfcMfCRF;
			else if (DBSavedMFName.Contains("IDFC Bond"))
				return _idfcMfCRF;
			else if (DBSavedMFName.Contains("Axis Long"))
				return "Axis Long Term Equity Fund - Direct Plan - Growth Option";
			else if (DBSavedMFName.Contains("Kotak Corporate"))
				return "Kotak Corporate Bond Fund- Direct Plan- Growth Option";
			else if (DBSavedMFName.Contains("SBI Long"))
				return "SBI LONG TERM ADVANTAGE FUND - SERIES I - DIRECT PLAN - GROWTH";
			return DBSavedMFName;
		}
		private decimal GetHistoricalMFPrice(string name, int month, int year)
		{
			try
			{
				int length = 10;
				if (name == null)
					return 0;
				Thread.Sleep(1500);
				string mfiFundName=GetActualFundName(name);
				//GetChromeINstance();
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
				tableRow[9].SendKeys("22-"+getMonth(month)+"-"+year);
				//dayStart.SendKeys("25");
				tableRow[10].Click();
				tableRow[10].SendKeys("25-"+getMonth(month)+"-"+year);
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
				//Dispose();
				return Convert.ToDecimal(result.Text);
				//var result = _driver.FindElement(By.XPath("//*[@id='mc_mainWrapper']/div[2]/div[1]/div[4]/div[4]/table/tbody/tr[3]/td[5]")).Text;

				
			}
			catch (Exception e)
			{
				return 0;
			}
			//Dispose();
		}
		private void GetDividendFromBse(dividend item, int yr)
		{ 
			var data = _driver.FindElements(By.XPath("//*[@id='tblinsidertrd']/tbody/tr[" + yr + "]/td[2]"))[0].Text;
			Thread.Sleep(2000);
			var dtt = _driver.FindElements(By.XPath("//*[@id='tblinsidertrd']/tbody/tr[" + yr + "]/td[3]"))[0].Text;
			item.dtUpdated = Convert.ToDateTime(dtt);
			item.value = Convert.ToDecimal(data);
			item.creditType = TypeOfCredit.FDividend;
		}
		private void GetBonusDetailsFromBse(dividend item)
		{
			try
			{
				var data = _driver.FindElements(By.XPath("//*[@id='tblinsidertrd']/tbody/tr"));

				int counter = 8;
				int i = 0;
				while (i <= counter && i< data.Count)
				{
					Console.WriteLine("Result TR::" + data[i].Text);
					//Thread.Sleep(500);
					if (data[i].Text.Contains("Bonus"))
					{
						var res = data[i].FindElements(By.TagName("td"));
						Console.WriteLine("Result TD::" + res[0].Text);
						string[] bonus = res[0].Text.Split(' ');
						item.value =  Convert.ToDecimal(bonus[2].Replace(':','.'));
						item.creditType = TypeOfCredit.Bonus;
						var dtt = res[2].Text;
						item.dtUpdated = Convert.ToDateTime(dtt);
						item.lastCrawledDate = DateTime.UtcNow;
						break;						
					}
					i++;
				}
			}
			catch(Exception ex)
			{
				string mess = ex.Message;
			}

		}
		private void GetDividendFromMoneyConterol(dividend item, int yr)
		{   //From MoneyControl
			var data = _driver.FindElements(By.XPath("//*[@id='mc_content']/div[2]/section[2]/div/div[2]/table/tbody/tr["+yr+"]/td[5]"))[0].Text;			
			var dtt = _driver.FindElements(By.XPath("//*[@id='mc_content']/div[2]/section[2]/div/div[2]/table/tbody/tr["+yr+"]/td[1]"))[0].Text;			
			item.dtUpdated = Convert.ToDateTime(dtt);
			item.value = Convert.ToDecimal(data.Substring(data.IndexOf("Rs.") + 3, 6));
		}
		private void GetYearlyPrice(string month, string price)
		{
			if (month.Contains("Jan"))
				yearlyPrice.Add(1, Convert.ToDecimal(price));
			else if (month.Contains("Feb"))
				yearlyPrice.Add(2, Convert.ToDecimal(price));
			else if (month.Contains("Mar"))
				yearlyPrice.Add(3, Convert.ToDecimal(price));
			else if (month.Contains("Apr"))
				yearlyPrice.Add(4, Convert.ToDecimal(price));
			else if (month.Contains("May"))
				yearlyPrice.Add(5, Convert.ToDecimal(price));
			else if (month.Contains("June"))
				yearlyPrice.Add(6, Convert.ToDecimal(price));
			else if (month.Contains("July"))
				yearlyPrice.Add(7, Convert.ToDecimal(price));
			else if (month.Contains("Aug"))
				yearlyPrice.Add(8, Convert.ToDecimal(price));
			else if (month.Contains("Sep"))
				yearlyPrice.Add(9, Convert.ToDecimal(price));
			else if (month.Contains("Oct"))
				yearlyPrice.Add(10, Convert.ToDecimal(price));
			else if (month.Contains("Nov"))
				yearlyPrice.Add(11, Convert.ToDecimal(price));
			else if (month.Contains("Dec"))
				yearlyPrice.Add(12, Convert.ToDecimal(price));
		}
		public void getListOfCompany(out List<string> companyList)
		{
			companyList = new List<string>();
			_driver.Navigate().GoToUrl(_bondPrice);
			Thread.Sleep(7000);
			WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(25));
			IList<IWebElement> listOfBonds = new List<IWebElement>();
			var downloadLink = _driver.FindElements(By.LinkText("Download (.csv)"));
			
			//return;
			listOfBonds = _driver.FindElements(By.XPath("//table[@id='liveTCMTable']/tbody/tr"));
			while (listOfBonds.Count <=1)
			{
				try
				{
					Dispose();
					GetChromeINstance();
					_driver.Navigate().GoToUrl(_bondPrice);
					Thread.Sleep(9800);
					wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(25));
					IWebElement myDynamicElement = wait.Until(d => d.FindElement(By.Id("liveTCMTable")));

					listOfBonds = _driver.FindElements(By.XPath("//table[@id='liveTCMTable']/tbody/tr"));
					//Thread.Sleep(1800);
				}
				catch (Exception ex)
				{
					var s = ex.Message;
				}
			}
			downloadLink[0].Click();
			return;

			Console.WriteLine("Adding to Company List");
			foreach (IWebElement ele in listOfBonds)
			{
				Console.Write(".");												 
				var bondDetailURL = ele.FindElements(By.TagName("td"));
				if(companyList.Find(x => x == bondDetailURL[0].Text) is null)
				{
					companyList.Add(bondDetailURL[0].Text);
				}						
			}
			Dispose();
		}
		public void GetlistOfBonds(IList<Bond> obj, string company)
		{
			int threshold = 0;
			//GetChromeINstance();
			_driver.Navigate().GoToUrl(_nseBondPriceLink+company);			
			
			Thread.Sleep(6000);
			WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(25));
			IList<IWebElement> listOfBonds = new List<IWebElement>();
			listOfBonds =_driver.FindElements(By.XPath("//div[@id='bondsAllSecurityTable']/table/tbody/tr"));
			while(listOfBonds.Count == 0)
			{
				try
				{ 
					//Dispose();
					//GetChromeINstance();
					_driver.Navigate().GoToUrl(_nseBondPriceLink + company);
					Thread.Sleep(8800);
					wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(250));
					IWebElement myDynamicElement = wait.Until(d => d.FindElement(By.Id("bondsAllSecurityTable")));

					listOfBonds = _driver.FindElements(By.XPath("//div[@id='bondsAllSecurityTable']/table/tbody/tr"));
					//Thread.Sleep(2800);					 
					threshold++;
					if(threshold>=5)
					{
						return;
					}
				}
				catch(Exception ex)
				{
					var s = ex.Message;
				}
				
			}

			foreach (IWebElement ele in listOfBonds)
			{
				Console.WriteLine(ele.Text);
				string line = ele.Text;
				var list = line.Split(' ');
				var bondDetailURL = ele.FindElements(By.TagName("td"));
				string s = bondDetailURL[4].Text;
				string price = bondDetailURL[10].Text;			 
				try
				{
					obj.Add(new Bond()
					{							
						LivePrice = Convert.ToDecimal(price),							
						dateOfMaturity = Convert.ToDateTime(s),
						BondId = bondDetailURL[2].Text,
						symbol = company+bondDetailURL[1].Text,
					});					 
				}
				catch (Exception ex)
				{
					continue;
					//Dispose();
				}
				
			}
			//Dispose();
		}
		public void GetLiveBondOnSale(IList<Bond> obj)
		{
			try
			{
				_driver.Navigate().GoToUrl(_bondPrice);
				Thread.Sleep(8600);
				WebDriverWait wait = new WebDriverWait(_driver,  TimeSpan.FromSeconds(25));
				
				IList<IWebElement> listOfBonds =_driver.FindElements(By.XPath("//table[@id='liveTCMTable']/tbody/tr"));
				foreach(IWebElement ele in listOfBonds)
				{
					Console.WriteLine(ele.Text);
					var bondDetailURL = ele.FindElements(By.TagName("td"));
					var bondLink = ele.FindElements(By.TagName("a"));
					 
					string s= bondDetailURL[11].Text;
					string bond = ele.Text;
					var list= bond.Split(' ');
					try
					{
						obj.Add(new Bond()
						{
							couponRate = Convert.ToDecimal(list[3]),
							faceValue = Convert.ToDecimal(list[4]),
							LivePrice = Convert.ToDecimal(list[5]),
							//YTM = Convert.ToDouble(list[3]) * Convert.ToDouble(list[4]) / Convert.ToDouble(list[5]),
							BondName= list[0] + list[1],
							dateOfMaturity = Convert.ToDateTime(s)
							//BondLink= bondDetailURL
						});
						//Console.WriteLine("Current YTM::"+ Convert.ToDouble(list[3]) * Convert.ToDouble(list[4]) / Convert.ToDouble(list[5]));
						Thread.Sleep(5000);
						//bondLink[0].Click();
					}
					catch(Exception ex)
					{
					
						continue;
						
					}
					//Thread.Sleep(100);				 
				}
				
			}
			catch(Exception ex)
			{
				//Dispose();
			}
		}

		public void GetBondFrequency(Bond b)
		{
			_driver.Navigate().GoToUrl(_bondFrequencyPriceLink+b.BondId);
			Thread.Sleep(5000);
			WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(25));

			IList<IWebElement> listOfBonds = _driver.FindElements(By.XPath("//h3/span"));
			listOfBonds[0].Click();

		}
		public string getMonth(int mnth)
		{
			string month = string.Empty;
			switch (mnth)
			{
				case 1:
					month = "Jan";
					break;

				case 2:
					month = "Feb";
					break;

				case 3:
					month = "Mar";
					break;

				case 4:
					month = "Apr";
					break;

				case 5:
					month = "May";
					break;

				case 6:
					month = "Jun";
					break;
				case 7:
					month = "Jul";
					break;

				case 8:
					month = "Aug";
					break;

				case 9:
					month = "Sep";
					break;

				case 10:
					month = "Oct";
					break;

				case 11:
					month = "Nov";
					break;

				case 12:
					month = "Dec";
					break;
			}
			return month;
		}
	}
}
