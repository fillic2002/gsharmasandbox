using System;
using System.Collections.Generic;
using System.Text;

namespace Git_Sandbox.Model
{
	public class propertyTransaction
	{
		public int portfolioId { get; set; }
		public myfinAPI.Model.AssetClass.AssetType astType { get; set; }
		public decimal astvalue { get; set; }
		public decimal investment { get; set; }
		public DateTime TransactionDate { get; set; }
		public char TypeofTransaction { get; set; }
		public decimal qty { get; set; }
	}
}
