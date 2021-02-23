using System;
using System.Collections.Generic;
using System.Text;
using Git_Sandbox.Model;

namespace Git_Sandbox.DailyRunJob.Contract
{
	public interface Ioperation
	{
		bool UpdateLatesNAV(equity eq);

		bool ArchiveBankAccountDetails();

	}
}
