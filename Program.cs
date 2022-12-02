﻿using System;
using Confluent.Kafka;
using System.Threading.Tasks;
using DailyRunEquity;
using System.Threading;
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
                // obj.UpdateCompanyDetails();

              component.getBondsObj().LoadBondDetails();
                
                //Update Shares latest NAV -Daily
               obj.UpdateEquityLiveData();		

                //Add new dividend details that are updated in bse in last 90 days
               obj.AddDividendDetails();               
                
               obj.AddBonusTransaction();

               obj.AddPbAndMarketCap();
               
                //Update Monthly asset details
               obj.UpdateAssetHistory();

               obj.UpdatePPFSnapshot();

              _webHelper.GetProcurementDetails();

                Console.ReadKey();
            
            }
            catch(Exception ex)
			{
                Console.WriteLine(ex.Message);
                component.getWebScrappertObj().Dispose(); 
                Console.ReadKey();

            }
         }
    }
}
