using System;
using System.Collections.Generic;
using System.Text;

namespace Git_Sandbox.Model
{
	public class dividend
	{
		public DateTime dt { get; set; }
		public string companyid { get; set; }
		public double value { get; set; }
		public DateTime lastCrawledDate { get; set; }
	}
}
