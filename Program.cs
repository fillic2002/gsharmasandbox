using System;
using Confluent.Kafka;
using System.Threading.Tasks;
using DailyRunEquity;

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

                //Update Shares latest NAV -Daily
                obj.UpdateShareCurrentPrice();		

                //Add new dividend details that are updated in bse in last 90 days
                obj.AddDividendDetails();

                //Update Monthly asset details
                obj.UpdateAssetHistory();

                _webHelper.GetProcurementDetails();

                Console.ReadKey();
            }
            catch(Exception ex)
			{

			}
         }
    }
}
