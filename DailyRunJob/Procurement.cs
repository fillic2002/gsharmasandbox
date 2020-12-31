using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Equity;


namespace DailyRunEquity
{
	public class Procurement
	{
		//private string _eprocUrl ="https://eproc.karnataka.gov.in/eprocurement/common/eproc_tenders_list.seam";
		private GenericFunc _htmlHelper = new GenericFunc();
		public string ShowProcurementInfo()
		{
			var result = _htmlHelper.GetBDAProcurementDetails();
			foreach(string s in result)
			{
				if (s.ToUpper().Contains("SURYANAGAR"))
				{
					Console.Write("SURYANAGAR LIST CAME OUT!");
				}
			}
			return result.ToString();
		}
	}
}
