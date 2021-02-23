using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

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
		public void GetDividend(string url)
		{

			if (url.Contains("mutual-funds"))
				return;
			
			
			try
			{

					_driver = new ChromeDriver();
					//if (folio.equityType == 1)			
					//{

						_driver.Navigate().GoToUrl(url);
						Thread.Sleep(1500);
				IList<IWebElement> links = _driver.FindElements(By.TagName("a"));
				foreach(IWebElement link in links)
				{
					string s= link.GetAttribute("href");
					if(s!=null && s.Contains("dividend"))
					{
						link.Click();
						//break;
					}
				}
				var data= _driver.FindElements(By.XPath("//*[@id='mc_content']/div[2]/section[2]/div/div[2]/table/tbody"));
																		  
				 
				var dividend = Convert.ToDouble(_driver.FindElements(By.Id("quoteLtp"))[0].Text);						

					//}
					// else
					//{	
					//	_driver.Navigate().GoToUrl(_eq.desctiption);
					//	Thread.Sleep(1000);
					//	//_eq.livePrice = Convert.ToDouble(
					//	_eq.livePrice=Convert.ToDouble(_driver.FindElements(By.ClassName("amt"))[0].Text.Substring(1));

					//}
					//ComponentFactory.GetMySqlObject().UpdateLivePrice(folio.EquityId, _eq.livePrice);
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
