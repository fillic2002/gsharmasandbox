using System;
using System.Collections.Generic;
using System.Text;
using TinyCsvParser.Mapping;

namespace Git_Sandbox.Model
{
	public class equity
	{
		public string Symbol { get; set; }
		public string Companyname { get; set; }
		//public string Series;
		//public string Date;
		//public string DATEOFLISTING;
		//public string PAIDUPVALUE;
		//public string MARKETLOT;
		public string ISIN { get; set; }
		//public string FACEVALUE;
		public double LivePrice;

		public string sourceurl;
		//public string NO_OF_SHRS;
		//public string NET_TURNOV;
		//public string TDCLOINDI;
		//public string ISIN;
		//public string TRADING_DATE;
		//public string FILLER2;
		//public string FILLER3;
	}
	
}
