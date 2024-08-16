using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Git_Sandbox.DailyRunJob.DATA
{

	public class Procurement : IDisposable
	{
		//private string _eprocUrl ="https://eproc.karnataka.gov.in/eprocurement/common/eproc_tenders_list.seam";
		//private string _eprocUrl = "https://eproc.karnataka.gov.in/eprocportal/pages/index.jsp";
		private string _eprocUrl = "https://khb.karnataka.gov.in/info-3/E-AUCTION/en";
		private string _atlasianUrl = "https://kppp.karnataka.gov.in/#/portal/searchTender/live";
		IWebDriver _driver;
		ChromeOptions chromeOptions;
		string _webScrapperUrl = string.Empty;
		IDictionary<int, double> yearlyPrice;
		public void Dispose()
		{
			if (_driver != null)
				_driver.Quit();
		}
		public Procurement()
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
		public void ShowProcurementInfo()
		{
			_driver.Navigate().GoToUrl(_eprocUrl);
			Thread.Sleep(1000);
			_driver.FindElement(By.XPath("//*[@id='englishid']")).Click();
			Thread.Sleep(500);
			_driver.FindElements(By.XPath("//*[@id='btnstyl']//h2"))[1].Click();
			Thread.Sleep(2000);
			_driver.SwitchTo().Window(_driver.WindowHandles[1]);
			Thread.Sleep(500);
			SelectElement sec = new SelectElement(_driver.FindElements(By.XPath("//select"))[2]);
			sec.SelectByText("KHB - Karnataka Housing Board");
			Thread.Sleep(500);
			_driver.FindElement(By.XPath("//input[@value='Search']")).Click();
			Thread.Sleep(2000);
			int i = 1;
			while (true)
			{
				Console.WriteLine("Entering Page:" + i);
				IList<IWebElement> txt = _driver.FindElements(By.XPath("//tr[@class='trobg1']//td[3]"));
				IList<IWebElement> txt2 = _driver.FindElements(By.XPath("//tr[@class='trobg2']//td[3]"));
				if (txt.Count == 0 && txt2.Count == 0)
					break;
				foreach (IWebElement ele in txt)
				{
					if (ele.GetAttribute("innerText").Contains("SURYANAGAR") || ele.GetAttribute("innerText").Contains("BLR") ||
						ele.GetAttribute("innerText").Contains("BANGALORE"))
					{
						Console.ForegroundColor = ConsoleColor.Cyan;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.White;
					}
					Thread.Sleep(500);
					Console.WriteLine(ele.GetAttribute("innerText"));
				}
				foreach (IWebElement ele in txt2)
				{
					if (ele.GetAttribute("innerText").Contains("SURYANAGAR") || ele.GetAttribute("innerText").Contains("BLR") ||
						ele.GetAttribute("innerText").Contains("BANGALORE"))
					{
						Console.ForegroundColor = ConsoleColor.Cyan;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.White;
					}
					Thread.Sleep(500);
					Console.WriteLine(ele.GetAttribute("innerText"));
				}
				i++;
				IList<IWebElement> next = _driver.FindElements(By.XPath("//td[@class='dr-dscr-inact rich-datascr-inact null']"));
				foreach (IWebElement ele in next)
				{
					//Console.WriteLine(ele.GetAttribute("innerHTML"));
					var page = ele.GetAttribute("innerText");
					if (Convert.ToInt16(page) == i)
					{
						ele.Click();
						Thread.Sleep(3000);
						break;
					}
					if (i > next.Count)
					{
						Dispose();
						return;

					}

				}

			}

		}
		public void ShowJobNotification()
		{
			_driver.Navigate().GoToUrl(_eprocUrl);
			Thread.Sleep(1000);
		}
		public void ShowProcurementInfoNew()
		{

			_driver.Navigate().GoToUrl(_eprocUrl);
			Thread.Sleep(1000);
			ReadOnlyCollection<IWebElement> elements = _driver.FindElements(By.XPath("//div[contains(@class, 'language') and contains(@class, 'language-ka')]"));
			foreach (IWebElement element in elements)
			{
				Console.WriteLine(element.Text);
				element.Click();

			}
			Thread.Sleep(15000);
			ReadOnlyCollection<IWebElement> selectElements = _driver.FindElements(By.Id("departmentstatusSelect"));

			IWebElement selectElement = _driver.FindElement(By.Id("departmentstatusSelect"));
			SelectElement select = new SelectElement(selectElement);
			select.SelectByText("KHB - Karnataka Housing Board");
			ReadOnlyCollection<IWebElement> optionElements = selectElement.FindElements(By.TagName("option"));

			IWebElement selectElement1 = _driver.FindElement(By.Id("tenderstatusdropdownSelect"));
			SelectElement select1 = new SelectElement(selectElement1);
			select.SelectByText("Published");

			IWebElement searchButton = _driver.FindElement(By.Id("sendButton"));
			searchButton.Click();

			// Loop through the select elements
			foreach (IWebElement sel in optionElements)
			{
				// Perform actions on each select element if needed
				Console.WriteLine(sel.Text); // Print the ID of each select element
			}


			Thread.Sleep(500);
			_driver.FindElements(By.XPath("//*[@id='btnstyl']//h2"))[1].Click();
			Thread.Sleep(2000);
			_driver.SwitchTo().Window(_driver.WindowHandles[1]);
			Thread.Sleep(500);
			SelectElement sec = new SelectElement(_driver.FindElements(By.XPath("//select"))[2]);
			sec.SelectByText("KHB - Karnataka Housing Board");
			Thread.Sleep(500);
			_driver.FindElement(By.XPath("//input[@value='Search']")).Click();
			Thread.Sleep(2000);
			int i = 1;
			while (true)
			{
				Console.WriteLine("Entering Page:" + i);
				IList<IWebElement> txt = _driver.FindElements(By.XPath("//tr[@class='trobg1']//td[3]"));
				IList<IWebElement> txt2 = _driver.FindElements(By.XPath("//tr[@class='trobg2']//td[3]"));
				if (txt.Count == 0 && txt2.Count == 0)
					break;
				foreach (IWebElement ele in txt)
				{
					if (ele.GetAttribute("innerText").Contains("SURYANAGAR") || ele.GetAttribute("innerText").Contains("BLR") ||
						ele.GetAttribute("innerText").Contains("BANGALORE"))
					{
						Console.ForegroundColor = ConsoleColor.Cyan;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.White;
					}
					Thread.Sleep(500);
					Console.WriteLine(ele.GetAttribute("innerText"));
				}
				foreach (IWebElement ele in txt2)
				{
					if (ele.GetAttribute("innerText").Contains("SURYANAGAR") || ele.GetAttribute("innerText").Contains("BLR") ||
						ele.GetAttribute("innerText").Contains("BANGALORE"))
					{
						Console.ForegroundColor = ConsoleColor.Cyan;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.White;
					}
					Thread.Sleep(500);
					Console.WriteLine(ele.GetAttribute("innerText"));
				}
				i++;
				IList<IWebElement> next = _driver.FindElements(By.XPath("//td[@class='dr-dscr-inact rich-datascr-inact null']"));
				foreach (IWebElement ele in next)
				{
					//Console.WriteLine(ele.GetAttribute("innerHTML"));
					var page = ele.GetAttribute("innerText");
					if (Convert.ToInt16(page) == i)
					{
						ele.Click();
						Thread.Sleep(3000);
						break;
					}
					if (i > next.Count)
					{
						Dispose();
						return;

					}

				}

			}

		}
	}
}
