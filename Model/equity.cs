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
		 
		public double LivePrice;

		public string sourceurl { get; set; }
		public string divUrl { get; set; }
 
	}
	public enum AssetType
	{
		Shares=1,
		EquityMF=2,
		PF,
		PPF,
		DebtMF,
		Bank,
		Plot,
		Flat,
		Gold
	}
	
}
