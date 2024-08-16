using myfinAPI.Model;

namespace Git_Sandbox.DailyRunJob.Contract
{
	public interface Ioperation
	{
		bool UpdateLatesNAV(EquityBase eq);

		bool ArchiveBankAccountDetails();

	}
}
