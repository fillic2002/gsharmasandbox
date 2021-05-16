using System;
using System.Collections.Generic;
using System.Text;

namespace Git_Sandbox.Model
{
	public class EquityTransaction
	{
		public int portfolioId { get; set; }
		public equity equity { get; set; }
		public int qty { get; set; }		
		public double price { get; set; }		
		public DateTime TransactionDate { get; set; }
		public char TypeofTransaction{ get; set; }

	}
}
