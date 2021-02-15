using System;
using System.Collections.Generic;
using System.Text;

namespace Git_Sandbox.DailyRunJob.Contract
{
	public interface Ioperation
	{
		bool UpdateLatesNAV(string comp, double liveprice);

	}
}
