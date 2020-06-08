using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyRunEquity
{
	class Program
	{
		static void Main(string[] args)
		{
			Eqhelper obj = new Eqhelper();
			obj.fillShareDetails();

			Procurement objPro = new Procurement();
			var result = objPro.ShowProcurementInfo();
			Console.Write("DO YOU SEE ANY PROPERTY?");
			Console.ReadKey();
		}
	}
}
