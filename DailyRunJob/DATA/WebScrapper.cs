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
		string _webScrapperUrl = "https://www.nseindia.com/get-quotes/equity?symbol=";
		public void Dispose()
		{
			if(_driver !=null)
				_driver.Quit();
		}
		public void GetDividend(equity e)
		{

			if (e.sourceurl.Contains("mutual-funds"))
				return;
			
			 
				dividend item = new dividend();
				item.companyid = e.ISIN;
				_driver = new ChromeDriver();
				_driver.Navigate().GoToUrl(e.sourceurl);
				Thread.Sleep(150);
				IList<IWebElement> links = _driver.FindElements(By.TagName("a"));
				foreach(IWebElement link in links)
				{
				try
				{
					string s = link.GetAttribute("href");
					if (e.sourceurl.Contains("moneycontrol"))
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
		 
		 
				int i = 1;
				//Past 2 dividend details
				while (i <= 2)
				{
					try
					{
						if (e.sourceurl.Contains("moneycontrol"))
						{
							GetDividendFromMoneyConterol(item, i);
						}
						else
						{
							GetDividendFromBse(item, i);
						}
						component.getMySqlObj().AddDividendDetails(item);
						i++;
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
