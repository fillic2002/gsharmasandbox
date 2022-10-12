using System;
using System.Collections.Generic;
using System.Text;

namespace Git_Sandbox.Model
{
	public class propertyTransaction
	{
		public int portfolioId { get; set; }
		public AssetType astType { get; set; }
		public double astvalue { get; set; }
		public double investment { get; set; }
		public DateTime TransactionDate { get; set; }
		public char TypeofTransaction { get; set; }
		public double qty { get; set; }
	}
}
