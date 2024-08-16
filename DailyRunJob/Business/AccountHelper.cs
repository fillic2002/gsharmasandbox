using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Git_Sandbox.Model;
using myfinAPI.Factory;
using myfinAPI.Model;
using static myfinAPI.Model.AssetClass;

namespace Git_Sandbox.DailyRunJob.Business
{
	public class AccountHelper
	{

		IList<Portfolio> folioDetail;

		public AccountHelper()
		{
			folioDetail = new List<Portfolio>();
			component.getMySqlObj().GetPortFolio(folioDetail);
		}
		/// <summary>
		/// Step1: Get the 
		/// </summary>
		public void UpdatePPFSnapshot()
		{			
			foreach (Portfolio p in folioDetail)
			{
				IList<AssetHistory> asstHistory = new List<AssetHistory>();				 
				GetAccountSnapshotMiss(asstHistory, AssetType.PPF,p.folioId);
				asstHistory = new List<AssetHistory>();
				GetAccountSnapshotMiss(asstHistory, AssetType.PF,p.folioId);				 		
			}
		}

		private void GetAccountSnapshotMiss(IList<AssetHistory> asstHistory, AssetType asttype, int folioId)
		{
			ComponentFactory.GetSnapshotObj().GetMonthlyAssetSnapshot(folioId, asttype, asstHistory);
			var s = asstHistory.Where(x => x.Assettype == asttype);
			if (asstHistory.FirstOrDefault(x => x.Assettype == asttype) != null)
			{
				var hst = asstHistory.FirstOrDefault(x => x.Assettype == asttype);
				DateTime dt = new DateTime(hst.year, hst.month, DateTime.UtcNow.Day);
				while (dt < DateTime.Today)
				{
					dt = dt.AddMonths(1);
					UpdatePPFSnapshot(hst, dt);					
				}
			}
		}
		/// <summary>
		/// This function is stable enough now. Need to modify for only current year PF snapshot update and not from 
		/// First day of investment	
		/// </summary>
		/// <param name="p"></param>
		/// <param name="astType"></param>
		public void UpdatePPFSnapshot(AssetHistory? lastMonthAssetHsty, DateTime tranDt)
		{
			AssetHistory curMonthAssetHstry = new AssetHistory();
			curMonthAssetHstry.Assettype = lastMonthAssetHsty.Assettype;
			curMonthAssetHstry.portfolioId = lastMonthAssetHsty.portfolioId;
			curMonthAssetHstry.month = tranDt.Month;
			curMonthAssetHstry.year = tranDt.Year;
			curMonthAssetHstry.AssetValue = lastMonthAssetHsty.AssetValue;
			curMonthAssetHstry.Investment = lastMonthAssetHsty.Investment;
			

			IList<PFAccount> ppfTransaction = new List<PFAccount>();
			component.getMySqlObj().GetPf_PPFTransaction(lastMonthAssetHsty.portfolioId, ppfTransaction, 
												lastMonthAssetHsty.Assettype);
			if (ppfTransaction.Count == 0)
				return;

			foreach (PFAccount ppf in ppfTransaction.Where(x=>x.DateOfTransaction.Year==tranDt.Year && 
						x.DateOfTransaction.Month== tranDt.Month))
			{			     
				if (ppf.TypeOfTransaction == TranType.Deposit || ppf.TypeOfTransaction == TranType.Carry)
				{
					curMonthAssetHstry.Investment = lastMonthAssetHsty.Investment+ppf.InvestmentEmp + ppf.InvestmentEmplr + ppf.Pension;
					curMonthAssetHstry.AssetValue = lastMonthAssetHsty.AssetValue+ ppf.InvestmentEmp + ppf.InvestmentEmplr + ppf.Pension;
				}
				else
				{
					curMonthAssetHstry.AssetValue =lastMonthAssetHsty.AssetValue+ ppf.InvestmentEmp + ppf.InvestmentEmplr;
				}				 
			}
			component.getMySqlObj().UpdatePFSnapshot(curMonthAssetHstry);
		}
	}
}
