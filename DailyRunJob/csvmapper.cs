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
			MapProperty(0, x => x.SC_CODE);
			MapProperty(1, x => x.SC_NAME);

		}
	}
}
