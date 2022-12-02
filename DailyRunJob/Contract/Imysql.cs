using System;
using System.Collections.Generic;
using System.Text;
using Git_Sandbox.Model;
using myfinAPI.Model;

namespace Git_Sandbox.DailyRunJob.Contract
{
	public interface Ioperation
	{
		bool UpdateLatesNAV(EquityBase eq);

		bool ArchiveBankAccountDetails();

	}
}
