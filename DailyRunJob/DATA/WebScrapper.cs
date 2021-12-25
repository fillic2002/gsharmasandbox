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

namespace Git_Sandbox.DailyRunJob.DATA
{
	public class WebScrapper : IDisposable
	{
		IWebDriver _driver;
		ChromeOptions chromeOptions;
		string _webScrapperUrl= string.Empty;
		IDictionary<int, double> yearlyPrice; 
		
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
		public void GetDividend(dividend d,equity e)
		{
			if (e.divUrl.Contains("mutual-funds"))
			{
				return;
			}
			if(string.IsNullOrEmpty(e.divUrl))
			{
				return;
			}
			dividend div = new dividend();
			div.companyid = e.ISIN;
			
			GetChromeINstance();
			_driver.Navigate().GoToUrl(e.divUrl);
			Thread.Sleep(150);
			IList<IWebElement> links = _driver.FindElements(By.TagName("a"));
			foreach(IWebElement link in links)
			{
			try
			{
				string s = link.GetAttribute("href");
				if (e.divUrl.Contains("moneycontrol"))
				{
					if (s != null && s.Contains("dividend"))
					{
						link.Click();
						var newTabHandle = _driver.WindowHandles[0];
						var newTab = _driver.SwitchTo().Window(newTabHandle).PageSource;
						break;
					}
				}
				else
				{			
					if (s != null && s.Contains("corp-actions"))
					{
						link.Click();
						break;
					}
				}
			}
			catch (Exception ex)
			{
				string msg = ex.StackTrace;
				continue;
			}
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
						//if (DateTime.UtcNow.Subtract(item.dt).Days>90)
						//	continue;
						
					}
					Console.WriteLine("Dividend Added:: Companyid:"+ div.companyid +" Date::"+ div.dt +" Value::"+div.value);
 					if(previoudDivDate == div.dt && previousDivValue != div.value)
					{
						div.value += previousDivValue;
					}
					div.lastCrawledDate = DateTime.Now;
					component.getMySqlObj().ReplaceDividendDetails(div);
					previoudDivDate = div.dt;
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
				//return Convert.ToDouble(dividend);					 
			
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
				//price.Add(month, GetHistoricalMFPrice(name, month, year));
				//This is to read from a file
				GetHistoricalMFPrice(assetType);
				return price;
			}
			//return GetHistoricalSharePrice(name, month, year);
		}
		CsvConfiguration csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
		{
			HasHeaderRecord = false
		};

		private void GetHistoricalMFPrice(AssetType assetType)
		{
			try
			{
				using var streamReader = File.OpenText("..\\..\\..\\AxisLongTerm.csv");
				using var csvReader = new CsvReader(streamReader, csvConfig);

				string value;

				while (csvReader.Read())
				{
					int month= 0;
					int year = 0;
					for (int i = 0; csvReader.TryGetField<string>(i, out value); i++)
					{
						
						switch(i)
						{
							case 0:
								if (value.Contains("Date"))
									break;
								if (value.Contains("January"))
									month = 1;
								if (value.Contains("February"))
									month = 2;
								if (value.Contains("March"))
									month = 3;
								if (value.Contains("April"))
									month = 4;
								if (value.Contains("May"))
									month = 5;
								if (value.Contains("June"))
									month = 6;
								if (value.Contains("July"))
									month = 7;
								if (value.Contains("August"))
									month = 8;
								if (value.Contains("September"))
									month = 9;
								if (value.Contains("October"))
									month = 10;
								if (value.Contains("November"))
									month = 11;
								if (value.Contains("December"))
									month = 12;
								if (value.Contains("2018"))
									year = 2018;
								if (value.Contains("2017"))
									year = 2017;
								if (value.Contains("2019"))
									year = 2019;
								if (value.Contains("2020"))
									year = 2020;
								if (value.Contains("2021"))
									year = 2021;
								if (value.Contains("2016"))
									year = 2016;
								if (value.Contains("2015"))
									year = 2015;
								break;
							case 1:
								break;
							case 2:
								component.getMySqlObj().UpdateEquityMonthlyPrice(new equityHistory()
								{
									equityid = "4",
									month = month,
									price = Convert.ToDouble(value),
									year = year,
									assetType = Convert.ToInt32(AssetType.EquityMF)
								});
								break;
						}						
					}

					Console.WriteLine();
				}
			}
			catch(Exception ex)
			{
				string s = ex.Message;
			}
		}
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
				Thread.Sleep(3500);
				_driver.FindElement(By.XPath("//*[@id='mc_mainWrapper']/div[2]/div[1]/div[5]/div[2]/div[6]/table/tbody/tr/td[3]/form/div[4]/select[1]/option[12]")).Click();
				Thread.Sleep(3500);
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
		private double GetHistoricalMFPrice(string name, int month, int year)
		{
			try
			{
				int length = 10;
				if (name == null)
					return 0;
				Thread.Sleep(1500);
				GetChromeINstance();
				_driver.Navigate().GoToUrl(_webScrapperUrl);

				Thread.Sleep(1500);
				IList<IWebElement> option = _driver.FindElements(By.XPath("//form[@id='navhistory']"));
				IList<IWebElement> tableRow = option[0].FindElements(By.TagName("input"));
				tableRow[3].Click();

				
				//option[2].Click();
				string sub = name.Substring(0, length);
				IWebElement input = _driver.FindElement(By.XPath("//*[@id='mfSearchInput']"));
				input.SendKeys(sub);

				IList<IWebElement> suggest = _driver.FindElements(By.XPath("/html/body/div[1]/div[8]/section[2]/div/div[1]/div[2]/div[2]/div/ul/li[2]/div/div[1]/div/ul/li"));

				while (suggest.Count > 1 || suggest.Count == 0)
				{
					if (name.Length != length)
					{

						string c = name.Substring(length, 1);
						length++;
						//sub += c;
						Thread.Sleep(1500);
						input.Clear();
						Thread.Sleep(1500);
						input.SendKeys(c);
						Thread.Sleep(2500);
						suggest = _driver.FindElements(By.XPath("/html/body/div[1]/div[8]/section[2]/div/div[1]/div[2]/div[2]/div/ul/li[2]/div/div[1]/div/ul/li"));
					}
					else
						break;

				}
				suggest[0].Click();
				Thread.Sleep(3500);
				WebDriverWait wait = new WebDriverWait(_driver,new TimeSpan(1500));
				 
				//IWebElement from = _driver.FindElement(By.XPath("//*[@id='ui-item-0']"));
			 
				//from.Click();
				//Thread.Sleep(1500);
				IWebElement dayStart = _driver.FindElement(By.XPath("//*[@id='fromDate']/div/input[1]"));
			 
				dayStart.SendKeys("25");
				IWebElement monthStart = _driver.FindElement(By.XPath("//*[@id='fromDate']/div/input[2]"));
				monthStart.SendKeys(month.ToString());
				IWebElement yrStart = _driver.FindElement(By.XPath("//*[@id='fromDate']/div/input[3]"));
				yrStart.SendKeys(year.ToString());

				//wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("//*[@id='toDate']/div/input[1]")));
				wait.Until(x => x.FindElement(By.XPath("//*[@id='toDate']/div/input[1]")));
				IWebElement today = _driver.FindElement(By.XPath("//*[@id='toDate']/div/input[1]"));
				today.SendKeys("30");
				IWebElement toMonth = _driver.FindElement(By.XPath("//*[@id='toDate']/div/input[2]"));
				toMonth.SendKeys(month.ToString());
				IWebElement toYear = _driver.FindElement(By.XPath("//*[@id='toDate']/div/input[3]"));
				toYear.SendKeys(year.ToString());

				//todate.SendKeys(Keys.Tab);
				 
				Thread.Sleep(1500);
				_driver.FindElement(By.XPath("//*[@id='submit']")).Click();
				Thread.Sleep(1500);
				IWebElement result = _driver.FindElement(By.XPath("//*[@id='mf_nav_table']/table/tbody/tr[1]/td[3]"));

				return Convert.ToDouble(result.Text.Substring(1));
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
			var dtt = _driver.FindElements(By.XPath("//*[@id='tblinsidertrd']/tbody/tr[" + yr + "]/td[3]"))[0].Text;
			item.dt = Convert.ToDateTime(dtt);
			item.value = Convert.ToDouble(data);
		}
		private void GetDividendFromMoneyConterol(dividend item, int yr)
		{   //From MoneyControl
			var data = _driver.FindElements(By.XPath("//*[@id='mc_content']/div[2]/section[2]/div/div[2]/table/tbody/tr["+yr+"]/td[5]"))[0].Text;			
			var dtt = _driver.FindElements(By.XPath("//*[@id='mc_content']/div[2]/section[2]/div/div[2]/table/tbody/tr["+yr+"]/td[1]"))[0].Text;			
			item.dt = Convert.ToDateTime(dtt);
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
