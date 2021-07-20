using System;
using System.Collections.Generic;
using System.Text;

namespace Git_Sandbox.Model
{
	public class AssetHistory
	{
		public int portfolioId { get; set; }
		public double AssetValue{ get; set; }				
		public double Dividend{ get; set; }
		public double Investment{ get; set; }
		public int qurarter { get; set; }
		public int year { get; set; }
		public int qty { get; set; }
		public int assetType { get; set; }
		
	}
	
}
