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
             
          
            Eqhelper obj = new Eqhelper();

            //Update Shares latest NAV - Daily
           obj.UpdateShareCurrentPrice();

           // Add new dividend details that are updated in bse in last 90 days
           obj.AddDividendDetails();

            //Update Monthly asset details
            obj.UpdateAssetHistory();          
            

            //obj.ReadNewExcel();

            //Procurement objPro = new Procurement();
            //var result = objPro.ShowProcurementInfo();
            //Console.Write("DO YOU SEE ANY PROPERTY?");
            //Console.ReadKey();
        }
    }
}
