using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Git_Sandbox.Model;
using TinyCsvParser;

namespace Git_Sandbox.DailyRunJob
{
	public static class excelhelpernew
	{
       public static void ReadExcelFile()
        {
            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
            CsvUserDetailsMapping csvMapper = new CsvUserDetailsMapping();
            CsvParser<equity> csvParser = new CsvParser<equity>(csvParserOptions, csvMapper);
            var result = csvParser
                         .ReadFromFile(@"C:\Users\fillic\Downloads\EQUITY_L.CSV", Encoding.ASCII)
                        .ToList();
            //Console.WriteLine("Name " + "ID   " + "City  " + "Country");
			foreach (var details in result)
			{
                try
                {
                    //Console.WriteLine("Adding to db:" + details.Result + " " + details.Result.SC_CODE);
                    component.getMySqlObj().AddAssetDetails(new equity() { Symbol = details.Result.Symbol, Companyname = details.Result.Companyname,ISIN=details.Result.ISIN });
                }
                catch(Exception ex)
				{
                    Console.WriteLine(ex.Message);
				}
			}

		}
    }
}
