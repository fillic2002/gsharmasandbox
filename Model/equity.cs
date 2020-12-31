using System;
using System.Collections.Generic;
using System.Text;
using TinyCsvParser.Mapping;

namespace Git_Sandbox.Model
{
	public class equity
	{
		public string SC_CODE { get; set; }
		public string SC_NAME { get; set; }
		public string SC_GROUP;
		public string SC_TYPE;
		public string OPEN;
		public string HIGH;
		public string LOW;
		public string CLOSE;
		public string LAST;
		public string PREVCLOSE;
		public string NO_TRADES;
		public string NO_OF_SHRS;
		public string NET_TURNOV;
		public string TDCLOINDI;
		public string ISIN_CODE;
		public string TRADING_DATE;
		public string FILLER2;
		public string FILLER3;
	}
	
}
