using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Git_Sandbox.Model;
using OpenQA.Selenium.Support.UI;

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
			//_webScrapperUrl = "https://www.nseindia.com/get-quotes/equity?symbol=";
			
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
		 
			int i= 1;
			//Past 2 dividend details
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
					div.lastCrawledDate = DateTime.Now;
					component.getMySqlObj().ReplaceDividendDetails(div);
				
				}
				catch(Exception ex)
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
			}
			//else
			//{
			//	_webScrapperUrl = "https://www.etmoney.com/mutual-funds/filter/historical-mutual-fund-nav";
			//	return GetHistoricalMFPrice(name, month, year);
			//}
			return GetHistoricalSharePrice(name, month, year);
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
				IList<IWebElement> button = _driver.FindElements(By.XPath("//*[@id='mc_mainWrapper']/div[2]/div[1]/div[5]/div[2]/div[6]/table/tbody/tr/td[3]/form/div[4]/input[1]"));

				button[0].Click();
				//TODO- Need to change the logic to check month name here
				int row = 9 - month;
				int items = _driver.FindElements(By.XPath("//*[@id='mc_mainWrapper']/div[2]/div[1]/div[4]/div[4]/table/tbody/tr")).Count;
				for(int i=2; i<= items;i++)
				{
					string m = _driver.FindElement(By.XPath("//*[@id='mc_mainWrapper']/div[2]/div[1]/div[4]/div[4]/table/tbody/tr[" + i.ToString() + "]/td[1]")).Text;
					string price = _driver.FindElement(By.XPath("//*[@id='mc_mainWrapper']/div[2]/div[1]/div[4]/div[4]/table/tbody/tr[" + i.ToString() + "]/td[5]")).Text;
					GetYearlyPrice(m, price);
				} 
				var result = _driver.FindElement(By.XPath("//*[@id='mc_mainWrapper']/div[2]/div[1]/div[4]/div[4]/table/tbody/tr["+ row.ToString() +"]/td[5]")).Text;
							
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
				int length = 3;
				
				Thread.Sleep(1500);
				GetChromeINstance();
				_driver.Navigate().GoToUrl(_webScrapperUrl);

				Thread.Sleep(1500);
				IList<IWebElement> option = _driver.FindElements(By.XPath("//*[@id='nav_page_link']/li[2]/label"));
				option[0].Click();
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
				 
				wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("//*[@id='toDate']/div/input[1]")));
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
			else if (month.Contains("Jun"))
				yearlyPrice.Add(6, Convert.ToDouble(price));
			else if (month.Contains("July"))
				yearlyPrice.Add(7, Convert.ToDouble(price));
			else if (month.Contains("Aug"))
				yearlyPrice.Add(8, Convert.ToDouble(price));
			else if (month.Contains("Sep"))
				yearlyPrice.Add(9, Convert.ToDouble(price));
			else if (month.Contains("oct"))
				yearlyPrice.Add(10, Convert.ToDouble(price));
			else if (month.Contains("nov"))
				yearlyPrice.Add(11, Convert.ToDouble(price));
			else if (month.Contains("dec"))
				yearlyPrice.Add(12, Convert.ToDouble(price));
		}
	}
}
