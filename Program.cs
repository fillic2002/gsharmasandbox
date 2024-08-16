using System;
using DailyRunEquity;
using Git_Sandbox.DailyRunJob;

namespace Git_Sandbox
{
	class Program
	{
		public static void Main(string[] args)
		{
			try
			{
				Eqhelper obj = new Eqhelper();
				WebHelper _webHelper = new WebHelper();

				//Update any missing URL
				//obj.UpdateCompanyDetails();

				// component.getBondsObj().LoadBondDetails();
				component.getBondsObj().CalculateBondIntrest();

				component.getExpenseHelperObj().getExpense();
				obj.UpdateEquityLiveData();

				
				obj.AddCorpActionDetails();

				obj.AddBonusTransaction();

				obj.AddTransactionPbAndMarketCap();

				//obj.UpdatePPFSnapshot();
				component.getAccountObj().UpdatePPFSnapshot();

				obj.UpdateAssetHistory();

				_webHelper.GetProcurementDetails();

				_webHelper.GetProcurementDetails();


				Console.ReadKey();
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(ex.Message);
				Console.ResetColor();
				component.getWebScrappertObj().Dispose();
				Console.ReadKey();

			}
		}
	}
}
