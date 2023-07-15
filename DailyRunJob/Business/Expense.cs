using System;
using System.Collections.Generic;
using System.Text;

namespace Git_Sandbox.DailyRunJob.Business
{
	public class Expense
	{
		public void getExpense()
		{
			try
			{
				component.getExcelHelperObj().ReadExpenseDetails();
			}
			catch {
				Console.WriteLine("NO BANK ACCOUNT FILE FOUND.");
			}
		}
	}
}
