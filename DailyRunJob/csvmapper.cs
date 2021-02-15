using System;
using System.Collections.Generic;
using System.Text;
using Git_Sandbox.Model;
using TinyCsvParser.Mapping;

namespace Git_Sandbox.DailyRunJob
{
	public class CsvUserDetailsMapping : CsvMapping<equity>
	{
		public CsvUserDetailsMapping()
			: base()
		{
			MapProperty(0, x => x.Symbol);
			MapProperty(1, x => x.Companyname);
			MapProperty(6, x => x.ISIN);

		}
	}
}
