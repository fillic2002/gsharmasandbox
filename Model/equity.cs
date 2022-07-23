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
		public AssetType assetType { get; set; }		 
		public string ISIN { get; set; }
		
		public double? LivePrice;
		public string sourceurl { get; set; }
		public string divUrl { get; set; }
		public long noOfShare{ get; set; }
		public double PB { get; set; }
		public double MC { get; set; }
		public DateTime lastUpdated { get; set; }
	}
	public class equityBase
	{
		public string ISIN { get; set; }

		public double LivePrice;
		public string sourceurl { get; set; }
		public string divUrl { get; set; }
		public string Symbol { get; set; }
		public string Companyname { get; set; }
		public string lastUpdated { get; set; }
	}
	public enum AssetType
	{
		Shares=1,
		EquityMF=2,
		PF,
		PPF,
		DebtMF,
		Bank=6,
		Plot=7,
		Flat=8,
		Gold=12
	}
	
}
