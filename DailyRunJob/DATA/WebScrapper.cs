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
			
			try
			{
				dividend item = new dividend();
				item.companyid = e.ISIN;
				_driver = new ChromeDriver();
				_driver.Navigate().GoToUrl(e.sourceurl);
				Thread.Sleep(150);
				IList<IWebElement> links = _driver.FindElements(By.TagName("a"));
				foreach(IWebElement link in links)
				{
					string s= link.GetAttribute("href");
					if(s!=null && s.Contains("dividend"))
					{
						link.Click();					 
					}
				}
				var newTabHandle = _driver.WindowHandles[1];
				var newTab = _driver.SwitchTo().Window(newTabHandle).PageSource;

				var data= _driver.FindElements(By.XPath("//*[@id='mc_content']/div[2]/section[2]/div/div[2]/table/tbody/tr[1]/td[5]"))[0].Text;
				var dtt= _driver.FindElements(By.XPath("//*[@id='mc_content']/div[2]/section[2]/div/div[2]/table/tbody/tr[1]/td[1]"))[0].Text;
				item.dt = Convert.ToDateTime(dtt);
				item.value = Convert.ToDouble(data.Split(' ')[0].Replace("Rs.", ""));


				component.getMySqlObj().AddDividendDetails(item);
				Dispose();
				//return Convert.ToDouble(dividend);
				}

			
			catch (Exception ex)

			{
				string s = ex.Message;
				Dispose();

			}
			
		}
	}
}
