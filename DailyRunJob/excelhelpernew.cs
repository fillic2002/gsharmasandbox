using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Git_Sandbox.Model;
using myfinAPI.Model;
using TinyCsvParser;

namespace Git_Sandbox.DailyRunJob
{
	public static class excelhelpernew
	{
       public static void ReadExcelFile()
        {
            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
            CsvUserDetailsMapping csvMapper = new CsvUserDetailsMapping();
            CsvParser<EquityBase> csvParser = new CsvParser<EquityBase>(csvParserOptions, csvMapper);
            var result = csvParser
                         .ReadFromFile(@"C:\Users\fillic\Downloads\EQUITY_L.CSV", Encoding.ASCII)
                        .ToList();
            //Console.WriteLine("Name " + "ID   " + "City  " + "Country");
			foreach (var details in result)
			{
                try
                {
                    //Console.WriteLine("Adding to db:" + details.Result + " " + details.Result.SC_CODE);
                    component.getMySqlObj().AddAssetDetails(new EquityBase() { symbol = details.Result.symbol, 
                        equityName = details.Result.equityName,
                        assetId = details.Result.assetId
                    });
                }
                catch(Exception ex)
				{
                    Console.WriteLine(ex.Message);
				}
			}

		}
    }
}
