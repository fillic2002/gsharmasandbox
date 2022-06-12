using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Git_Sandbox.DailyRunJob.Contract;
using Git_Sandbox.Model;
using MySql.Data.MySqlClient;

namespace Git_Sandbox.DailyRunJob.DATA
{
	public class MysqlHelper : Ioperation
	{
		string connString = "Server = localhost; Database = myfin; Uid = root; Pwd = Welcome@1; ";
		public bool AddAssetDetails(Model.equity item)
		{
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				var command = new MySqlCommand(@"INSERT INTO myfin.assetdetails ( ISIN,name, symbol) 
												VALUES ( '" + item.ISIN + "','" + item.Companyname + "','" + item.Symbol + "');", _conn);
				int result = command.ExecuteNonQuery();

				return true;
			}

		}

		public bool UpdateLatesNAV(equity eq)
		{
			if (eq.LivePrice <= 0)
				return false;
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				using var command = new MySqlCommand(@"UPDATE myfin.equitydetails SET liveprice = " + eq.LivePrice + "," +
									" dtupdated ='" + DateTime.UtcNow.ToString("yyyy-MM-dd") + "' WHERE (ISIN = '" + eq.ISIN + "');", _conn);

				int result = command.ExecuteNonQuery();
			}
			return true;
		}
		public bool UpdateCompanyDetail(equity eq)
		{
			
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				using var command = new MySqlCommand(@"UPDATE myfin.equitydetails SET divLink = '" + eq.divUrl+ "'," +
									" description='" + eq.sourceurl + "' WHERE (ISIN = '" + eq.ISIN + "');", _conn);

				int result = command.ExecuteNonQuery();
			}
			return true;
		}
		public double GetLatesNAV(string ISIN)
		{
			double result = 0;
			
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				using var command = new MySqlCommand(@"SELECT liveprice FROM myfin.equitydetails" +
									" WHERE ISIN = '" + ISIN + "';", _conn);

				 result = (double)command.ExecuteScalar();
			}
			return result;
		}

		public IList<equity> GetEquityNavUrl()
		{
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				using var command = new MySqlCommand(@"select isin, divlink,assettypeid,description from myfin.equitydetails where myfin.equitydetails.description IS NOT NULL;", _conn);
				using var reader = command.ExecuteReader();
				IList<equity> eq = new List<equity>();
				while (reader.Read())
				{
					eq.Add(new equity()
					{
						ISIN = reader["isin"].ToString(),
						sourceurl = reader["description"].ToString(),
						assetType = (AssetType)((int)reader["assettypeid"]),
						divUrl = reader["divlink"].ToString()
					});
				}
				return eq;
			}

		}

		bool Ioperation.ArchiveBankAccountDetails()
		{
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				using var command = new MySqlCommand(@"select isin, description,assettypeid from myfin.equitydetails where myfin.equitydetails.description IS NOT NULL;", _conn);
				using var reader = command.ExecuteReader();
				IList<equity> eq = new List<equity>();
				while (reader.Read())
				{
					eq.Add(new equity()
					{
						ISIN = reader["isin"].ToString(),
						sourceurl = reader["description"].ToString(),
						assetType = (AssetType)(int)reader["assettypeid"]
					});
				}
				return true;
			}
		}

		public bool ReplaceDividendDetails(dividend item)
		{
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				using var command = new MySqlCommand(@"REPLACE into myfin.dividend(isin,dividend,dtupdated,lastcrawleddt) values('" + item.companyid+"','"+item.value+"','"+item.dtUpdated.ToString("yyyy-MM-dd") + "','" + item.lastCrawledDate.ToString("yyyy-MM-dd") + "');", _conn);
				int  reader = command.ExecuteNonQuery();
				 
				return true;
			}
		}
		 
		public void GetCompaniesID(IList<dividend> item)
		{
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				//using var command = new MySqlCommand(@"Select distinct(isin) from myfin.dividend
				//						where isin not in(
				//						SElect distinct(isin)
				//						from myfin.dividend
				//						where datediff(curdate() ,dtupdated )<=90);", _conn);
				using var command = new MySqlCommand(@"select * from myfin.equitydetails where description is not null and Assettypeid=1;", _conn);
				using var reader = command.ExecuteReader();

				while (reader.Read())
				{
					item.Add(new dividend()
					{						
						companyid= reader["isin"].ToString()
					});
				}
			}
		}
		public void GetTransactions(IList<EquityTransaction> p, int portfolioId)
		{
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				string query;
			//	using var command = new MySqlCommand(@"SELECT et.*,ed.liveprice,ed.assettypeid,ed.name FROM myfin.equitytransactions et
			//				Join myfin.equitydetails ed
			//				ON ed.isin=et.isin
			//				where portfolioid=" + portfolioId + ";", _conn);
				if (portfolioId != 0)
				{
					query = @"SELECT et.*,ed.liveprice,ed.assettypeid,ed.name,ed.divlink FROM myfin.equitytransactions et
							Join myfin.equitydetails ed
							ON ed.isin=et.isin
							where portfolioid=" + portfolioId + ";";
				}
				else
				{
					query = @"SELECT et.*,ed.liveprice,ed.assettypeid,ed.name,ed.divlink FROM myfin.equitytransactions et
							Join myfin.equitydetails ed
							ON ed.isin=et.isin;";
				}
				using MySqlCommand command = new MySqlCommand(query, _conn);
				using var reader = command.ExecuteReader();

				while (reader.Read())
				{
					p.Add(new EquityTransaction()
					{
						equity = new equity()
						{
							ISIN = reader["isin"].ToString(),
							LivePrice = Convert.ToDouble(reader["liveprice"]),
							assetType = (AssetType)Convert.ToInt32(reader["assettypeid"]),
							Companyname = reader["name"].ToString(),
							divUrl = reader["divlink"].ToString()

						},
						price = Convert.ToDouble(reader["price"]),
						portfolioId = Convert.ToInt16(reader["portfolioid"]),
						TransactionDate = Convert.ToDateTime(reader["TransactionDate"]),
						TypeofTransaction = Convert.ToChar(reader["action"]),
						qty = Convert.ToInt32(reader["qty"]),
						PB = Convert.ToDouble(reader["pb"]),
						MC= Convert.ToDouble(reader["marketcap"])

					}); 
				}
			}
		}
		public void UpdateCompanyDetails(IList<EquityTransaction> res, double pb, double mv, long noShare)
		{
			try
			{
				foreach (EquityTransaction tran in res)
				{
					if (DateTime.Now.Subtract(tran.TransactionDate).TotalDays <= 190)
					{
						if (tran.PB == 0 || tran.MC == 0 || tran.equity.noOfShare == 0)
						{
							//tran.PB = (pb / eq.LivePrice) * tran.price;
							//tran.MC = (mv / eq.LivePrice) * tran.price;
							component.getMySqlObj().UpdateTransaction(tran);
							tran.equity.noOfShare = noShare;
							component.getMySqlObj().UpdateTransaction(tran);
						}
					}
				}

			}
			catch (Exception ex)
			{

			}
		}

		public bool UpdateTransaction(EquityTransaction tran)
		{
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				string dt = tran.TransactionDate.ToString("yyyy-MM-dd");
				using var command = new MySqlCommand(@"UPDATE myfin.equitytransactions 
								SET pb= "+tran.PB+" , marketcap="+tran.MC+", OpenShare="+tran.equity.noOfShare+" WHERE ISIN='"+tran.equity.ISIN+"' AND transactiondate='"+dt+"';", _conn);
				int result = command.ExecuteNonQuery();
			}
			return true;
		}

		public void GetPropertyTransactions(IList<propertyTransaction> p, int portfolioId)
		{
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				using var command = new MySqlCommand(@"SELECT * FROM myfin.propertytransaction 
							where portfolioid=" + portfolioId + ";", _conn);
				using var reader = command.ExecuteReader();

				while (reader.Read())
				{
					p.Add(new propertyTransaction()
					{					 
						astType = (AssetType)Convert.ToInt32(reader["assettype"]),					 
						astvalue = Convert.ToDouble(reader["currentvalue"]),
						portfolioId = Convert.ToInt16(reader["portfolioid"]),
						TransactionDate = Convert.ToDateTime(reader["dtofpurchase"]),
						investment = Convert.ToDouble(reader["purchaseprc"]),
						TypeofTransaction = Convert.ToChar(reader["tranmode"])
					}); ;
				}
			}
		}
		public void GetPFSnapshot(IList<AssetHistory> h, int portfolioId)
		{
			//using (MySqlConnection _conn = new MySqlConnection(connString))
			//{
			//	_conn.Open();
			//	using var command = new MySqlCommand(@"SELECT myfin.assetsnapshot(portfolioid,year,month,assettype,assetvalue,dividend,invstmt)
			//				values( select " + portfolioId + ",YEAR(dateoftransaction),MONTH(dateoftransaction),useracctid ,amt,0,0 from myfin.bankdetail;", _conn);
			//	using var reader = command.ExecuteReader();

			//	while (reader.Read())
			//	{
			//		p.Add(new propertyTransaction()
			//		{
			//			astType = (AssetType)Convert.ToInt32(reader["assettype"]),
			//			astvalue = Convert.ToDouble(reader["currentvalue"]),
			//			portfolioId = Convert.ToInt16(reader["portfolioid"]),
			//			TransactionDate = Convert.ToDateTime(reader["dtofpurchase"]),
			//			investment = Convert.ToDouble(reader["purchaseprc"]),
			//			TypeofTransaction = Convert.ToChar(reader["tranmode"])
			//		}); ;
			//	}
			//}
		}
		public void GetCompanyDetails(equity e)
		{
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				using var command = new MySqlCommand(@"select * from myfin.equitydetails where isin='"+e.ISIN+"';", _conn);

				using var reader = command.ExecuteReader();

				while (reader.Read())
				{
					e.Companyname = reader["Name"].ToString();
					e.LivePrice = Convert.ToDouble(reader["liveprice"]);
				}
			}
		}
		public bool UpdatePFSnapshot(int m,int y,int portfolioId,double invst)
		{
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				using var command = new MySqlCommand(@"REPLACE INTO myfin.assetsnapshot(portfolioid,year,month,assettype,assetvalue,dividend,invstmt)
							 select " + portfolioId + ","+y+","+m+ ",ba.accttype ,amt,0,"+ invst+ " from myfin.AccoutBalance bt " +
							 "join myfin.bankaccounttype ba on ba.accttypeid = bt.accttypeId " +
							 " where folioid = "+ portfolioId + " and bt.accttypeid in (16, 17);", _conn);
				 
				int result = command.ExecuteNonQuery();

				return true;

			}
		}
		public bool UpdatePFSnapshot(AssetHistory astHstry)
		{
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				using var command = new MySqlCommand(@"REPLACE myfin.assetsnapshot
							 set  portfolioid=" + astHstry.portfolioId + ", year=" + astHstry.year + ",month=" + astHstry.month + ",assettype=" + (int)astHstry.assetType + ",assetvalue=" + astHstry.AssetValue + ",dividend="+ astHstry.Dividend +
							 ",invstmt=" + astHstry.Investment +";", _conn);

				int result = command.ExecuteNonQuery();

				return true;

			}
		}
		public bool UpdateBankSnapshot(int m, int y, int portfolioId)
		{
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				using var command = new MySqlCommand(@"REPLACE INTO myfin.assetsnapshot(portfolioid,year,month,assettype,assetvalue,dividend,invstmt)
							 select " + portfolioId + "," + y + "," + m + ",6, sum(amt),0,0 from myfin.AccoutBalance bt join myfin.bankaccounttype ba " +
							 "on ba.accttypeid = bt.accttypeId" +
							 " where folioid="+portfolioId+ " and ba.accttype=6  group by folioid;", _conn);

				int result = command.ExecuteNonQuery();

				return true;

			}
		}


		public void GetCompaniesDividendDetails(IList<dividend> d, int portfolioId, int month, int year)
		{
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				using var command = new MySqlCommand(@" SELECT * FROM myfin.dividend 
					Where isin in (SELECT isin FROM myfin.equitytransactions where portfolioid="+ portfolioId + ") and year(dtupdated)="+year+" and month(dtupdated)="+ month+ "" +
					"	order by isin,dtupdated desc;", _conn);
				using var reader = command.ExecuteReader();

				while (reader.Read())
				{
					d.Add(new dividend()
					{
						companyid=reader["isin"].ToString() ,
						dtUpdated= Convert.ToDateTime(reader["dtupdated"]),
						value = Convert.ToDouble(reader["dividend"])
					});
				}
			}
		}
		public void GetPortFolio(IList<Portfolio> p)
		{
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				using var command = new MySqlCommand(@"SELECT * FROM myfin.portfolio;", _conn);
				using var reader = command.ExecuteReader();

				while (reader.Read())
				{
					p.Add(new Portfolio()
					{
						FolioName = reader["folioname"].ToString(),
						folioId = Convert.ToInt32(reader["portfolioid"]),
						userid = Convert.ToInt16(reader["userid"])
					});
				}
			}
		}
		public bool AddAssetSnapshot(AssetHistory item)
		{
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				using var command = new MySqlCommand(@"REPLACE into myfin.assetsnapshot(portfolioid,month,year,assetvalue,dividend,invstmt,assettype) 
										values('" + item.portfolioId + "','" + item.month+ "','" + item.year + "','" + 
											item.AssetValue +"','"+ item.Dividend + "','" + item.Investment + "','"+(int)item.assetType+"' );", _conn);
				int reader = command.ExecuteNonQuery();

				return true;
			}
		}

		public void GetAssetSnapshot(AssetHistory hstry)
		{
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				using var command = new MySqlCommand(@"Select portfolioid,month,year,assetvalue,dividend,invstmt,assettype 
										from myfin.assetsnapshot where portfolioid=" + hstry.portfolioId + " AND month=" + hstry.month + 
										" AND year=" + hstry.year + " AND assettype=" + (int)hstry.assetType + ";", _conn);
				using var reader = command.ExecuteReader();

				while (reader.Read())
				{
					hstry.AssetValue = Convert.ToDouble(reader["assetvalue"]);
					hstry.Dividend = Convert.ToDouble(reader["dividend"]);
					hstry.Investment = Convert.ToDouble(reader["invstmt"]);
				
				}

			}

		}

		public void getLastDividendOfCompany(dividend d)
		{
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				using var command = new MySqlCommand(@"SELECT * FROM myfin.dividend where isin='"+d.companyid+"' order by dtupdated desc limit 1;", _conn);
				using var reader = command.ExecuteReader();

				while (reader.Read())
				{
					d.dtUpdated = Convert.ToDateTime(reader["dtupdated"]);
					d.lastCrawledDate = Convert.ToDateTime(reader["lastcrawleddt"]);					
				}
			}
		}

		public bool UpdateEquityMonthlyPrice(equityHistory equityItem)
		{
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				using var command = new MySqlCommand(@"REPLACE INTO myfin.equitymonthlyprice (id, month,year,price,assetType)
				values('" + equityItem.equityid+ "'," + equityItem.month+ "," + equityItem.year+ "," + equityItem.price+","+equityItem.assetType+");", _conn);
				using var reader = command.ExecuteReader();

				return true;
			}
		}
 
		public double GetHistoricalSharePrice(string id,int month, int year)
			{
				using (MySqlConnection _conn = new MySqlConnection(connString))
				{
					_conn.Open();
					using var command = new MySqlCommand(@"SELECT * FROM myfin.equitymonthlyprice where id='" + id + "' AND year="+year+" AND month=" +
						month+";", _conn);
					using var reader = command.ExecuteReader();

					while (reader.Read())
					{
						return Convert.ToDouble(reader["price"]);						
					}
				return 0;
				}
			}

		public void GetPf_PPFTransaction(int folioId, IList<pf> pftran,AssetType type)
		{
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				using var command = new MySqlCommand(@"select * from myfin.pf where folioid=" + folioId + " AND acttype="+Convert.ToInt32( type)+" order by dtofchange asc;", _conn);
				using var reader = command.ExecuteReader();

				while (reader.Read())
				{
					pftran.Add(new pf(){  
						dtOfChange = Convert.ToDateTime(reader["dtofchange"]),
						empCont = Convert.ToDouble(reader["emp"]),
						emplyrCont= Convert.ToDouble(reader["employer"]),
						pension= Convert.ToDouble(reader["pension"]),
						type= reader["typeofcredit"].ToString()
					});			
				}
			}
		}

		public void GetCompaniesMissingInformation(IList<equity> itemlist)
		{
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				using var command = new MySqlCommand(@"select name,isin from myfin.equitydetails where (description is null )AND assettypeid=1;", _conn);
				//using var command = new MySqlCommand(@"select name,isin from myfin.equitydetails where (divlink is null OR description is null )AND assettypeid=1;", _conn);

				using var reader = command.ExecuteReader();

				while (reader.Read())
				{
					itemlist.Add(
						new equity()
						{
						 Companyname=reader["name"].ToString(),
						 ISIN = reader["ISIN"].ToString()
						});
				}
 			}
		}
	}
}