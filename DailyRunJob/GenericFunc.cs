using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Git_Sandbox.DailyRunJob;
using Git_Sandbox.Model;
//using System.Web.Script.Serialization;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Equity
{
	public class GenericFunc
	{
		HtmlWeb web;
		private const string RESULTQUARTER = "http://www.bseindia.com/stock-share-price/stockreach_financials.aspx?scripcode=";
		private const string PEER_WATCH = "http://www.bseindia.com/stock-share-price/Peergroupdisplay.aspx?scripcode=";
		private const string PORTFOLIO_MONEYCONTROL = "http://www.moneycontrol.com/bestportfolio/wealth-management-tool/investments#port_top";
		private const string MONEYCONTROM_STOCK = "http://www.moneycontrol.com/india/stockpricequote/";
		private const string EPROCUMENT_KARNATKA = "https://eproc.karnataka.gov.in/eprocurement/common/eproc_tenders_list.seam";
		private Dictionary<int, string> MF_WATCH = new Dictionary<int, string>();
		private Dictionary<int, string> SHARE_WATCH = new Dictionary<int, string>();
		private const string _baseURL = "http://www.moneycontrol.com/mutual-funds/nav/";

		public enum MFName
		{
			IDFC_DEBT = 1,
			SBI_EQUITY=2,
			AXIS_EQUITY=3,
			IDFC_CORP_EQ_FUND=4,
			IDFC_SUPER_SAVER_INCOME_MEDIUM=5,
			KOTAK_CORP_BOND=6,

		};
		public enum CompanyName
		{
			BEL = 0,
			GAIL ,
			HAL ,
			SRIKALAHASTI,
			PETRONETLNG,
			BALMERLAWRIE,
			KOVAIMEDICAL,
			TATACHEMICAL,
			MAHANAGAR,
			NESCO,
			POWERGRID=10,
			ONGC,
			GIC,
			NIACL,
			BPCL
		}

		public GenericFunc()
		{
			if (web == null)
				web = new HtmlWeb();

			MF_WATCH.Add((int)MFName.IDFC_DEBT, _baseURL+ "idfc-corporate-bond-fund-direct-plan/MAG1720");
			MF_WATCH.Add((int)MFName.SBI_EQUITY, _baseURL+"sbi-long-term-advantage-fund-series-i-direct-plan/MSB1065");
			MF_WATCH.Add((int)MFName.AXIS_EQUITY, _baseURL+"axis-long-term-equity-fund-direct-plan/MAA192");
			MF_WATCH.Add((int)MFName.IDFC_CORP_EQ_FUND, _baseURL+"idfc-credit-opportunities-fund-direct-plan/MAG1765");
			MF_WATCH.Add((int)MFName.IDFC_SUPER_SAVER_INCOME_MEDIUM, _baseURL +"idfc-super-saver-income-fund-medium-term-plan-direct-plan/MAG775");
			MF_WATCH.Add((int)MFName.KOTAK_CORP_BOND, _baseURL+"kotak-corporate-bond-fund-direct-plan/MAI083");

		
			SHARE_WATCH.Add((int)CompanyName.GAIL, "https://www.moneycontrol.com/india/stockpricequote/oil-drilling-and-exploration/gailindia/GAI");
			SHARE_WATCH.Add((int)CompanyName.BEL, "https://www.moneycontrol.com/india/stockpricequote/electricals/bharatelectronics/BE03");
			SHARE_WATCH.Add((int)CompanyName.NESCO, "https://www.moneycontrol.com/india/stockpricequote/diversified/nesco/NES");
			SHARE_WATCH.Add((int)CompanyName.MAHANAGAR, "https://www.moneycontrol.com/india/stockpricequote/refineries/mahanagargas/MG02");
			SHARE_WATCH.Add((int)CompanyName.KOVAIMEDICAL, "https://www.moneycontrol.com/india/stockpricequote/hospitals-medical-services/kovaimedicalcenterhospital/KMC02");
			SHARE_WATCH.Add((int)CompanyName.TATACHEMICAL, "https://www.moneycontrol.com/india/stockpricequote/chemicals/tatachemicals/TC");
			SHARE_WATCH.Add((int)CompanyName.GIC, "https://www.moneycontrol.com/india/stockpricequote/diversified/generalinsurancecorporationindia/GIC12");

		}
		public void GetCompanyListWithEarningRatio(Dictionary<double, Dictionary<int, string>> CompanyMasterList, List<double> listofcompanyid)
		{
			int columnnumber = 3;
			foreach (double key in listofcompanyid)
			{
				try
				{
					string url = RESULTQUARTER + key.ToString() + "&expandable=0";
					HtmlAgilityPack.HtmlDocument doc = web.Load(url);

					double lastfourqresult = 0.0;
					string Promotor = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_quatre')]//table//tr//td//table//tr[12]//td[1]")[0].InnerText;
					if (Promotor == "Net Profit")
					{
						var q1 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_quatre')]//table//tr//td//table/tr[12]//td[2]")[0].InnerText;
						lastfourqresult += Convert.ToDouble(q1);
						var q2 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_quatre')]//table//tr//td//table/tr[12]//td[3]")[0].InnerText;
						lastfourqresult += Convert.ToDouble(q2);
						var q3 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_quatre')]//table//tr//td//table/tr[12]//td[4]")[0].InnerText;
						lastfourqresult += Convert.ToDouble(q3);
						var q4 = doc.DocumentNode.SelectNodes("//div[contains(@id, 'ctl00_ContentPlaceHolder1_quatre')]//table//tr//td//table/tr[12]//td[5]")[0].InnerText;
						lastfourqresult += Convert.ToDouble(q4);
					}

					Dictionary<int, string> RowValues = new Dictionary<int, string>();
					RowValues.Add(columnnumber, lastfourqresult.ToString());
					CompanyMasterList[key] = RowValues;
				}
				catch (Exception ex)
				{
					Console.WriteLine("Exception for item:" + ex);
					continue;
				}

			}

		}

		public void GetCompanyShareHolding(List<double> listOfCompanyId, Dictionary<double, Dictionary<int, string>> CompanyMasterList)
		{
			foreach (double item in listOfCompanyId)
			{
				try
				{
					string url = PEER_WATCH + item.ToString() + "&scripcomare=";
					HtmlDocument doc = web.Load(url);

					string promoterShare = doc.DocumentNode.SelectSingleNode("//div[@id='quatre']//table//tr//td//table//tr[22]//td[2]").InnerText;
					string publicShare = doc.DocumentNode.SelectSingleNode("//div[@id='quatre']//table//tr//td//table//tr[25]//td[2]").InnerText;

					if (promoterShare == "--")
						promoterShare = "0";
					if (publicShare == "--")
						publicShare = "0";
					double TotalShare = Convert.ToDouble(promoterShare) + Convert.ToDouble(publicShare);

					var LTP = Convert.ToDouble(doc.DocumentNode.SelectSingleNode("//div[@id='quatre']//table//tr[2]//td[2]").InnerText);

					Dictionary<int, string> TotalShareColumn = new Dictionary<int, string>();

					//Column 4 is Total Share in Revenue sheet
					TotalShareColumn.Add(4, TotalShare.ToString());
					TotalShareColumn.Add(5, LTP.ToString());

					CompanyMasterList[item] = TotalShareColumn;

				}
				catch (Exception ex)
				{
					Console.WriteLine("Exception for item:" + ex);
					continue;
				}

			}
		}
		public void GetCompanyShareHolding(double CompanyId, Dictionary<double, Dictionary<int, string>> CompanyMasterList)
		{

			try
			{
				string url = PEER_WATCH + CompanyId.ToString() + "&scripcomare=";
				HtmlDocument doc = web.Load(url);

				string promoterShare = doc.DocumentNode.SelectSingleNode("//div[@id='quatre']//table//tr//td//table//tr[22]//td[2]").InnerText;
				string publicShare = doc.DocumentNode.SelectSingleNode("//div[@id='quatre']//table//tr//td//table//tr[25]//td[2]").InnerText;

				if (promoterShare == "--")
					promoterShare = "0";
				if (publicShare == "--")
					publicShare = "0";
				double TotalShare = Convert.ToDouble(promoterShare) + Convert.ToDouble(publicShare);

				var LTP = Convert.ToDouble(doc.DocumentNode.SelectSingleNode("//div[@id='quatre']//table//tr[2]//td[2]").InnerText);

				Dictionary<int, string> TotalShareColumn = new Dictionary<int, string>();
				//Column 4 is Total Share in Revenue sheet
				TotalShareColumn.Add(4, TotalShare.ToString());
				TotalShareColumn.Add(5, LTP.ToString());

				CompanyMasterList[CompanyId] = TotalShareColumn;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception for item:" + ex);
			}
		}
		
		public async Task<double> GetAssetNAVAsync(equity eq)
		{
			try
			{
				HtmlDocument doc = web.Load( eq.sourceurl);
				Thread.Sleep(2000);
				string result="0";
				if (eq.assetType == AssetType.Shares)
				{
					try
					{
						HtmlNodeCollection node = doc.DocumentNode.SelectNodes("//*[@id='nsecp']");
						if (node == null)
							node = doc.DocumentNode.SelectNodes("//*[@id='bsecp']");
						
						result = node[0].InnerText;
						Console.WriteLine("Price for :" + eq.divUrl.Split('/')[5] + " Is:" + result.ToString());
					}
					catch(Exception ex)
					{
						result = doc.DocumentNode.SelectNodes("//*[@id='bsecp']")[0].InnerText;
						Console.WriteLine("Price for :" + eq.divUrl.Split('/')[5] + " Is:" + result.ToString());
					}
				}
				else
				{
					result = doc.DocumentNode.SelectNodes("//span[@class='amt']")[0].InnerText.Split(' ')[1];
					Console.WriteLine("Price for id:" + eq.ISIN + " Is:" + result.ToString());
				}
				return Convert.ToDouble(result);
			}
			catch(Exception ex)
			{
				return 0;
			}
		}
		public void GetDividend(dividend d,string url)
		{
			try
			{
				HtmlDocument doc = web.Load(url);
				string result = "0";
				Thread.Sleep(3000);
				result = doc.DocumentNode.SelectNodes("//table[@class='mctable1']")[0].InnerText.Split(' ')[1];				 
			}
			catch (Exception ex)
			{
				string s = ex.StackTrace;
			}
		}

		public IList<equity> GetEquityLinks()
		{
			IEnumerable<equity> listPortfolio= component.getMySqlObj().GetEquityNavUrl().Distinct<equity>();
			return listPortfolio.ToList();
		}

		public IList<string> GetBDAProcurementDetails()
		{
			IList<string> listOfProc = new List<string>();
			try
			{
//				IList<string> listOfProc = new List<string>();
				HtmlDocument doc = web.Load(EPROCUMENT_KARNATKA);
				var listOfOffer = doc.DocumentNode.SelectNodes("//tbody[@id='eprocTenders:browserTableEprocTenders:tbody_element']//tr[@class='trobg1']//td[4]").ToList();
				foreach (HtmlAgilityPack.HtmlNode node in listOfOffer)
				{
					listOfProc.Add(node.InnerText);
				}
				var listOfOffer2 = doc.DocumentNode.SelectNodes("//tbody[@id='eprocTenders:browserTableEprocTenders:tbody_element']//tr[@class='trobg2']//td[4]").ToList();
				foreach (HtmlAgilityPack.HtmlNode node in listOfOffer2)
				{
					listOfProc.Add(node.InnerText);
				}

				return listOfProc;
			}
			catch
			{
				Console.WriteLine("An error occurred on accessing EPROC site");
				return listOfProc;
			}

		}

		public void GetROCE(Dictionary<double, Dictionary<int, string>> CompanyMasterList, string companycode, string group)
		{
			try
			{
				if (!string.IsNullOrEmpty(group))
				{
					HtmlDocument doc = web.Load(MONEYCONTROM_STOCK + "U");
					HtmlNodeCollection companys = doc.DocumentNode.SelectNodes("//table[@class='pcq_tbl MT10']//tr//td//a[@class='bl_12']");//[0].Attributes["href"].Value;
					foreach (HtmlNode nodeCompany in companys)
					{
						string companyurl = nodeCompany.Attributes["href"].Value;
						string[] a = companyurl.Split('/');
						if (a.Length == 1)
							continue;
						var companyid = a[a.Length - 1];
						var companyname = a[a.Length - 2];

						GetROCEForCompany(CompanyMasterList, companyid, companyname);
					}
				}
				else
				{

					string url2 = "http://www.moneycontrol.com/mccode/common/autosuggesion.php?query=" + companycode + "&type=1&format=json&callback=suggest1";
					HttpWebRequest request = WebRequest.Create(url2) as HttpWebRequest;

					using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
					{
						if (response.StatusCode != HttpStatusCode.OK)
							throw new Exception(String.Format(
							"Server error (HTTP {0}: {1}).",
							response.StatusCode,
							response.StatusDescription));
						var s = new StreamReader(response.GetResponseStream()).ReadToEnd();
						s = s.Replace("suggest1([", "");
						s = s.Replace("])", "");

						//JavaScriptSerializer js = new JavaScriptSerializer();
						//object obj = js.DeserializeObject(s);
						//object companyid = "", companyname = "";
						//foreach (string key in ((Dictionary<string, object>)obj).Keys)
						//{
						//	if (key == "sc_id")
						//		companyid = ((Dictionary<string, object>)obj)[key];
						//	if (key == "stock_name")
						//		companyname = ((Dictionary<string, object>)obj)[key];

						//}
						//GetROCEForCompany(CompanyMasterList, companyid.ToString(), companyname.ToString());
					}
				}
			}catch(Exception ex)
			{
				throw ex;
			}
		}

		public string GetCompanyMarketValue(int companyId)
		{
			HtmlDocument doc = web.Load(SHARE_WATCH[companyId]);
			return doc.DocumentNode.SelectNodes("//div[@class='value_txtfr']")[0].InnerHtml;
		}


		public void GetPortFolioDetails()
		{
			var document = LoadURL(PORTFOLIO_MONEYCONTROL);
			var s=document.DocumentNode.SelectSingleNode("//div[@class='bgbl']//span[@id='networth_disp']");
		} 

		private HtmlDocument LoadURL(string url)
		{			 
			return web.Load(url);
		}

		private void GetROCEForCompany(Dictionary<double, Dictionary<int, string>> CompanyMasterList, string companyid, string companyname)
		{
			string url2 = "http://www.moneycontrol.com/financials/" + companyname + "/ratiosVI/" + companyid;
			HtmlDocument doc2 = web.Load(url2);
			try
			{
				var roe = doc2.DocumentNode.SelectNodes("//div[@class='boxBg']//table[@class='table4']//tr//td[@class='det']");
				bool flagFound = false;
				int count = 0;
				Dictionary<int, string> roce = new Dictionary<int, string>();
				foreach (HtmlNode node in roe)
				{
					if (flagFound)
					{
						roce.Add(count + 3, node.InnerHtml);
						count++;
						if (count == 5)
							break;
					}
					if (node.InnerHtml == "Return on Networth / Equity (%)")
					{
						flagFound = true;
					}
				}

				var id = doc2.DocumentNode.SelectSingleNode("//div[@class='FL gry10']");
				CompanyMasterList[Convert.ToDouble(id.InnerText.Split('|')[0].Split(':')[1].Trim())] = roce;
			}
			catch (Exception ex)
			{
				string s = ex.ToString();
				//continue;
			}

		}
		
		
	}
}
