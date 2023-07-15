using System;
using System.Collections.Generic;
using System.Text;

namespace Git_Sandbox.Model
{
	public class dividend
	{
		public DateTime dtUpdated { get; set; }
		public string companyid { get; set; }
		public double value { get; set; }
		public DateTime lastCrawledDate { get; set; }
		public TypeOfCredit creditType { get; set; }
	}

	public enum TypeOfCredit
	{
		IntDividend =1,
		Bonus=2,
		SpclDividend=3,
		FDividend=4

	}
}
