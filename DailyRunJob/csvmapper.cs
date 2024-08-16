using myfinAPI.Model;
using TinyCsvParser.Mapping;

namespace Git_Sandbox.DailyRunJob
{
	public class CsvUserDetailsMapping : CsvMapping<EquityBase>
	{
		public CsvUserDetailsMapping()
			: base()
		{
			MapProperty(0, x => x.symbol);
			MapProperty(1, x => x.equityName);
			MapProperty(6, x => x.assetId);

		}
	}
}
