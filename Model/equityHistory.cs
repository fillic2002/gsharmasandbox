using System;
using System.Collections.Generic;
using System.Text;

namespace Git_Sandbox.Model
{
	public class equityHistory
	{
		public string equityid{ get; set; }
		public double? price{ get; set; }	
		public int month{ get; set; }
		public int year { get; set; }		
		public int assetType { get; set; }
	}
}
