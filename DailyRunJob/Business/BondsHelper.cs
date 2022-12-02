using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Equity;
using myfinAPI.Model.DTO;

namespace Git_Sandbox.DailyRunJob.Business
{
	public class BondsHelper
	{
		StringBuilder faultyISIN = new StringBuilder();
		List<Bond> bondMasterlist;
		const string bondLivePriceFile = @"C:\Users\fillic\Downloads\MW-Bonds-on-CM-"; //27-Nov-2022.csv

		public void GetLiveBondPrice(List<Bond> bondFromAllExcel)
		{
			List<Bond> bondDetailsFromWeb = new List<Bond>();
			List<Bond> existingBondDetails = new List<Bond>();
			bondMasterlist = bondFromAllExcel;

			IList<string> companyList = new List<string>();

			try
			{
				string fullName = bondLivePriceFile + DateTime.Now.ToString("dd-MMM-yyyy") + ".csv";
				List<Bond> bondFrmWeb = new List<Bond>();
				if (File.Exists(fullName))
				{
					GetBondLivePriceFromFile(bondFrmWeb);
				}
				component.getBondBusinessHelperObj().CalculateYTMXirr(bondFrmWeb);
				foreach (Bond b in bondFrmWeb)
				{
					Console.ForegroundColor = ConsoleColor.DarkYellow;
					Console.WriteLine(b.BondName+"-"+b.BondId +"::"+b.YTM);
					Console.ResetColor();
				}
				Thread.Sleep(5000);
				Console.ReadKey();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				component.getWebScrappertObj().Dispose();
			}

			//while (bondDetailsFromWeb.Count == 0)
			//{				 
			//	component.getWebScrappertObj().GetLiveBondOnSale(bondFromAllExcel);
			//}
		}
		 
		private bool ValidateBondAndSave(IList<Bond> bondDetailsFromWeb, List<Bond> bondFromAllExcel)
		{
			foreach (Bond b in bondDetailsFromWeb)
			{
				Bond result = bondFromAllExcel.Find(x => x.BondId == b.BondId);
				if (result != null)
				{
					result.LivePrice = b.LivePrice;
					result.symbol = b.symbol;
					
					component.getBondContextObj().UpdateBondLivePrice(result);
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine(result.BondId + " Added Successfully");
					Console.ResetColor();
					//Thread.Sleep(1500);
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Red;
					faultyISIN.Append(b.BondId+",");
					Console.WriteLine("Bond Does not exist in master list" + b.BondId);
					Console.ResetColor();
					//Thread.Sleep(1500);
				}
				//component.getBondContextObj().UpdateBondLivePrice(result);
			}
			return true;
			//component.getBondBusinessHelperObj().UpdateBondPrice(existingBondDetails);
		}
		public void LoadBondDetails()
		{
			var s = DateTime.Now.ToString("dd-MMM-yyyy");
			List<Bond> bondDetails = new List<Bond>();
			//Load this data every three times a month from an excel file hosted in NSE portal
			//GetBondMasterList(bondDetails);
			//Get this master list from DB from now onwards


			//var bonds=component.getBondBusinessHelperObj().GetBondDetails();
			//foreach(Bond b in bondDetails)
			//{
			//	var result=bonds.Where(x => x.BondId == b.BondId);
			//	if (result != null)
			//		continue;
			//	else
			//		component.getBondBusinessHelperObj().SaveBondDetails(bondDetails);
			//}
			
			GetLiveBondPrice(bondDetails);
		}
		private void GetBondLivePriceFromFile(List<Bond> bondDetails)
		{
			IList<string> data = new List<string>();
			component.getExcelHelperObj().ReadBondData(data, ExcelHelper.BOND_LIVE_PRICE, ExcelHelper.bondLivePriceMapping);
			var s = ExcelHelper.bondLivePriceMapping;
			int i = 0;
			foreach (string line in data)
			{
				try
				{
					string temp = line;
					Console.WriteLine(line);
					int Index = 0;
					int pointer = 0;int startIndex = 0;
					double liveP = 0;
					double coupon = 0;
					double fv = 0;
					string sym = "";
					string ser = "";
					DateTime maturity= new DateTime();
					while (Index <= 10)
					{
						pointer = temp.TrimStart(',').IndexOf("\"", startIndex + 1);
						var vla = temp.TrimStart(',').Substring(startIndex+1, pointer - startIndex-1);
						if(Index==0)
						{
							sym = vla;
						}else if(Index==5)
						{
							liveP = Convert.ToDouble(vla);
						}else if (Index == 3)
						{
							coupon = Convert.ToDouble(vla);
						}else if (Index == 4)
						{
							fv = Convert.ToDouble(vla);
						}else if (Index == 1)
						{
							ser = vla;
						}else if (Index == 10)
						{
							maturity = Convert.ToDateTime(vla.ToString());
						}

						Index++;
						temp = temp.Substring(pointer+2);
						
					}				 
					 
					bondDetails.Add(new Bond()
					{
						LivePrice = liveP,
						BondName = sym,
						 BondId =  ser,
						  couponRate = coupon,
						  faceValue = fv,
						  dateOfMaturity = maturity

					});
				}
				catch (Exception ex)
				{
					var sa = ex.Message;
					continue;
				}
			}
		}
		private static object SanitizeData(string fieldData,Type t)
		{
			if(t.Name =="DateTime")
			{				
				var s = Regex.Replace(fieldData.ToString(), @"[^0-9a-zA-Z]+", "-");
				if (s == "-")
					s = s.Replace('-',' ');
				fieldData =string.IsNullOrEmpty( s.Trim())? new DateTime(1001, 1, 1).ToString() : s;
				return fieldData;
			}
			if (t.Name == "String")
			{
				var s = Regex.Replace(fieldData.ToString(), @"[^0-9a-zA-Z]+", "");
				return s;
			}
			return Regex.Replace(fieldData.ToString(), @"[^0-9a-zA-Z]+", ""); ;
		}
		 
		private void GetBondMasterList(IList<Bond> bondDetails)
		{
			IList<string> data = new List<string>();
			component.getExcelHelperObj().ReadBondData(data,component.getExcelHelperObj().BOND_FILE_PATH, ExcelHelper.bondColumnName);
			var s= ExcelHelper.bondColumnName;
			int i = 0;
			foreach (string line in data)
			{				
				try
				{
				string[] item = line.Split(',');
//				var dt = item[s["Maturity date"]]==string.Empty?new DateTime(1001,1,1).ToString(): item[s["Maturity date"]];
					var dt = SanitizeData(item[s["Maturity date"]],typeof(DateTime));
				var coupon=item[s["Coupon Rate(IP rate)"]]==string.Empty?"0": item[s["Coupon Rate(IP rate)"]];
				string coup = Regex.Replace(coupon, @"[^0-9a-zA-Z]+", "");
				if(coup==string.Empty)
				{
					coup = "0";
				}

				var intr = item[s["Coupon Frequency"]] == " " ? "U" : item[s["Coupon Frequency"]];
				string intrestCycle = Regex.Replace(intr, @"[^0-9a-zA-Z]+", "");

					string rt = item[s["Credit Ratings Agency (Multiple)"]] == " " ? "" : item[s["Credit Ratings Agency (Multiple)"]];
					rt+= item[s["Rating (Multiple)"]] == " " ? "" : item[s["Rating (Multiple)"]];
					rt= SanitizeData(rt,typeof(string)).ToString();
					//var dat = Regex.Replace(dt.ToString(), @"[^0-9a-zA-Z]+", ",").Split(',');
					var isin = SanitizeData(item[s["ISIN Code"]],typeof(string));
					 
						
				//dat.ToString().Replace(',','-');
					//string newDate = dat[0] +"-"+ dat[1] +"-"+ dat[2];
					var bondName = SanitizeData(item[6].ToString(),typeof(string));
					var faceVal = SanitizeData(item[s["Face Value (Rs.)"]], typeof(string))  ;
					var firstIPDt = SanitizeData(item[s["First IP date"]], typeof(DateTime));
					DateTime d=	DateTime.Parse(dt.ToString());
				 
					if (Convert.ToDateTime(d) >= DateTime.UtcNow)
					{

						bondDetails.Add(new Bond()
						{
							BondId = isin.ToString(),
							couponRate = Convert.ToDouble(coup)/100,
							dateOfMaturity =  d,
							intrestCycle = intrestCycle.ToString(),
							rating = rt,
							BondName = bondName.ToString(),
							faceValue = Convert.ToDouble(faceVal.ToString()==""?"0":faceVal.ToString()),
							firstIPDate= DateTime.Parse(firstIPDt.ToString())
						});
					}
					Console.Write(".");
					//Thread.Sleep(20);
				}
				catch(Exception ex)
				{
					var sa= ex.Message;
					continue;
				}
			}
		}
	}
}
