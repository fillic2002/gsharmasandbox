using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Git_Sandbox.Model;

namespace Git_Sandbox.DailyRunJob.DATA
{
	public class WebScrapper : IDisposable
	{
		IWebDriver _driver;
		ChromeOptions chromeOptions;
		string _webScrapperUrl= string.Empty;

		public WebScrapper()
		{
			_webScrapperUrl = "https://www.nseindia.com/get-quotes/equity?symbol=";
			chromeOptions = new ChromeOptions();
			chromeOptions.AddArguments("headless");			 
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
	}
}
