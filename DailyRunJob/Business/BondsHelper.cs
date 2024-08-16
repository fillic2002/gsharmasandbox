using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Equity;
using myfinAPI.Model.DTO;
using static myfinAPI.Business.Xirr;

namespace Git_Sandbox.DailyRunJob.Business
{
	public class BondsHelper
	{
		StringBuilder faultyISIN = new StringBuilder();
		List<Bond> bondMasterlist = new List<Bond>();
		const string bondLivePriceFile = @"C:\Users\fillic\Downloads\MW-Bonds-on-CM-"; //27-Nov-2022.csv

		public void CalculateBondIntrest()
		{
			IList<BondTransaction> bondTran = new List<BondTransaction>();
			IList<BondIntrest> bondIntrst;

			component.getBondBusinessHelperObj().GetBondTransaction(0, bondTran);
			bondTran.ToList().ForEach(x =>
			{
				
				bondIntrst = new List<BondIntrest>();
				component.getBondContextObj().getBondIntrestForTransaction(bondIntrst, x, x.folioId);
				if (bondIntrst.Count <= 0)
					component.getBondBusinessHelperObj().UpdateBondyIntrestPayment(x);
			});
		}

		public void ReadBondLivePriceFromExcel(List<Bond> bondFromAllExcel)
		{
			try
			{
				string fullName = bondLivePriceFile + DateTime.Now.ToString("dd-MMM-yyyy") + ".csv";
				if (File.Exists(fullName))
				{
					GetBondLivePriceFromNSEFile(bondFromAllExcel);
				}
				else
					return;
				//foreach(Bond b in bondFromAllExcel)
				//{
				//	double livePr= b.LivePrice;
				//	component.getBondBusinessHelperObj().SearchBondDetails(b);
				//	b.LivePrice = livePr;
				//	//frmDB.issuer = b.issuer;
				//	if(b.YTM <= 0)
				//		component.getBondBusinessHelperObj().CalculateYTMXirr(b);
				//	component.getBondBusinessHelperObj().SaveBondDetails(b);
				//}
				//component.getBondBusinessHelperObj().SaveBondDetails(bondFromAllExcel);
				//foreach (Bond b in bondFromAllExcel)
				//{
				//	Console.ForegroundColor = ConsoleColor.DarkYellow;
				//	Console.WriteLine(b.BondName+"-"+b.BondId +"::"+b.YTM);
				//	Console.ResetColor();
				//}
				//Thread.Sleep(5000);
				//Console.ReadKey();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				//component.getWebScrappertObj().Dispose();
			}
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
					faultyISIN.Append(b.BondId + ",");
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
			List<Bond> bondDetails = new List<Bond>();
			IList<CashItem> cashFlow = new List<CashItem>();
			if (DateTime.UtcNow.Day < 15)
				GetBondMasterListFromNSDL();

			ReadBondLivePriceFromExcel(bondDetails);

			if (bondDetails.Count == 0)
				return;

			foreach (Bond b in bondDetails)
			{
				decimal livePr = b.LivePrice;
				component.getBondBusinessHelperObj().SearchBondDetails(b);
				if (b.LivePrice == livePr) //If the price already updated no need to update again
					continue;
				b.LivePrice = livePr;

				component.getBondBusinessHelperObj().CalculateYTMXirr(b, cashFlow);
				component.getBondContextObj().SaveliveBondDetails(b);

				Console.ForegroundColor = ConsoleColor.DarkYellow;
				Console.WriteLine(b.BondName + "-" + b.BondId + "::" + b.YTM);
				Console.ResetColor();
			}
			//component.getBondBusinessHelperObj().SaveBondDetails(bondDetails);
			//foreach (Bond b in bondDetails)
			//{
			//	Console.ForegroundColor = ConsoleColor.DarkYellow;
			//	Console.WriteLine(b.BondName + "-" + b.BondId + "::" + b.YTM);
			//	Console.ResetColor();
			//}

		}



		private void GetBondLivePriceFromNSEFile(List<Bond> bondDetails)
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
					int pointer = 0; int startIndex = 0;
					decimal liveP = 0;
					decimal coupon = 0;
					decimal fv = 0;
					string sym = "";
					string ser = "";
					DateTime maturity = new DateTime();
					while (Index <= 10)
					{
						pointer = temp.TrimStart(',').IndexOf("\"", startIndex + 1);
						var vla = temp.TrimStart(',').Substring(startIndex + 1, pointer - startIndex - 1);
						if (Index == 0)
						{
							sym = vla;
						}
						else if (Index == 5)
						{
							liveP = Convert.ToDecimal(vla);
						}
						else if (Index == 3)
						{
							coupon = Convert.ToDecimal(vla);
						}
						else if (Index == 4)
						{
							fv = Convert.ToDecimal(vla);
						}
						else if (Index == 1)
						{
							ser = vla;
						}
						else if (Index == 10)
						{
							maturity = Convert.ToDateTime(vla.ToString());
						}

						Index++;
						temp = temp.Substring(pointer + 2);

					}

					bondDetails.Add(new Bond()
					{
						LivePrice = liveP,
						BondName = sym,
						BondId = sym + "-" + ser,
						couponRate = coupon,
						faceValue = fv,
						dateOfMaturity = maturity,
						issuer = sym,
						symbol = ser

					});
				}
				catch (Exception ex)
				{
					var sa = ex.Message;
					continue;
				}
			}
		}
		private static object SanitizeData(string fieldData, Type t)
		{
			if (t.Name == "DateTime")
			{
				var s = Regex.Replace(fieldData.ToString(), @"[^0-9a-zA-Z]+", "-");
				if (s == "-")
					s = s.Replace('-', ' ');
				fieldData = string.IsNullOrEmpty(s.Trim()) ? new DateTime(1001, 1, 1).ToString() : s;
				return fieldData;
			}
			if (t.Name == "String")
			{
				var s = Regex.Replace(fieldData.ToString(), @"[^0-9a-zA-Z]+", "");
				return s;
			}
			return Regex.Replace(fieldData.ToString(), @"[^0-9a-zA-Z]+", ""); ;
		}
		public bool IsNumeric(string input)
		{
			double test;
			return double.TryParse(input, out test);
		}
		private void GetBondMasterListFromNSDL()
		{
			IList<string> data = new List<string>();
			component.getExcelHelperObj().ReadBondData(data, component.getExcelHelperObj().BOND_FILE_PATH, ExcelHelper.bondColumnName);
			var s = ExcelHelper.bondColumnName;
			int i = 0;
			foreach (string line in data)
			{
				try
				{
					string[] item = line.Split(',');
					if (item.Count() >= 19)
					{
						if (item[19] == "Active")
						{
							string isin = SanitizeData(item[s["ISIN"]], typeof(string)).ToString();
							DateTime doa = Convert.ToDateTime(SanitizeData(item[s["Date of Allotment"]], typeof(DateTime)));
							DateTime dom = Convert.ToDateTime(SanitizeData(item[s["Date of Redemption/Conversion"]], typeof(DateTime)));
							string bondName = item[s["Security Description"]].ToString().Replace('\'', ' ');
							bool num = IsNumeric(item[s["Coupon Rate (%)"]].Replace('%', ' '));
							string coupon = num == true ? item[s["Coupon Rate (%)"]].Replace('%', ' ') : "0";
							string intrestCycle = item[s["Frequency of Interest Payment"]].ToString();
							string rating = item[s["Credit Rating"]].ToString();
							double faceVal = Convert.ToDouble(item[s["Face Value (In Rs.)"]]);

							if (Convert.ToDateTime(dom) >= DateTime.UtcNow)
							{
								Bond bondNew = new Bond()
								{
									BondId = isin,
									couponRate = Convert.ToDecimal(coupon),
									dateOfMaturity = dom,
									intrestCycle = intrestCycle.ToString(),
									rating = rating,
									BondName = bondName.ToString(),
									faceValue = Convert.ToDecimal(faceVal.ToString() == "" ? "0" : faceVal.ToString()),
									firstIPDate = doa
									//firstIPDate = DateTime.Parse(firstIPDt.ToString())
								};
								bondMasterlist.Add(bondNew);
								if (component.getBondBusinessHelperObj().SearchBondDetails(bondNew) == null)
								{
									if (component.getBondContextObj().SaveliveBondDetails(bondNew) != true)
									{
										Console.Write("Failed to Save");
										Console.WriteLine(line);
									}
								}
							}
							Console.Write(".");
						}
						else
							continue;
					}
				}
				catch (Exception ex)
				{
					var sa = ex.Message;
					continue;
				}
			}
		}
	}
}
