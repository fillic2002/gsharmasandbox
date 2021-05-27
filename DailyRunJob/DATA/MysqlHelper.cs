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
				using var command = new MySqlCommand(@"REPLACE into myfin.dividend(isin,dividend,dtupdated,lastcrawleddt) values('" + item.companyid+"','"+item.value+"','"+item.dt.ToString("yyyy-MM-dd") + "','" + item.lastCrawledDate.ToString("yyyy-MM-dd") + "');", _conn);
				int  reader = command.ExecuteNonQuery();
				 
				return true;
			}
		}
		 

		public void GetStaleDividendCompanies(IList<dividend> item)
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
				using var command = new MySqlCommand(@"SELECT et.*,ed.liveprice,ed.assettypeid FROM myfin.equitytransactions et
							Join myfin.equitydetails ed
							ON ed.isin=et.isin
							where portfolioid=" + portfolioId + ";", _conn);
				using var reader = command.ExecuteReader();

				while (reader.Read())
				{
					p.Add(new EquityTransaction()
					{
						equity= new equity(){ISIN = reader["isin"].ToString(), LivePrice= Convert.ToDouble(reader["liveprice"]),assetType= (AssetType)Convert.ToInt32(reader["assettypeid"]) },						
						price = Convert.ToDouble(reader["price"]),
						portfolioId = Convert.ToInt16(reader["portfolioid"]),
						TransactionDate= Convert.ToDateTime(reader["TransactionDate"]),
						TypeofTransaction =Convert.ToChar(reader["action"]),
						qty = Convert.ToInt32(reader["qty"]),					

					});
				}
			}
		}

		public void GetCompaniesDividendDetails(IList<dividend> d, int portfolioId)
		{
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				using var command = new MySqlCommand(@" SELECT * FROM myfin.dividend 
					Where isin in (SELECT isin FROM myfin.equitytransactions where portfolioid="+ portfolioId + ")" +
					"	order by isin,dtupdated desc;", _conn);
				using var reader = command.ExecuteReader();

				while (reader.Read())
				{
					d.Add(new dividend()
					{
						companyid=reader["isin"].ToString() ,
						dt= Convert.ToDateTime(reader["dtupdated"]),
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
				using var command = new MySqlCommand(@"REPLACE into myfin.assetsnapshot(portfolioid,qtr,year,assetvalue,dividend,invstmt) 
										values('" + item.portfolioId + "','" + item.qurarter+ "','" + item.year + "','" + 
											item.AssetValue +"','"+ item.Dividend + "','" + item.Investment + "' );", _conn);
				int reader = command.ExecuteNonQuery();

				return true;
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
					d.dt = Convert.ToDateTime(reader["dtupdated"]);
					d.lastCrawledDate = Convert.ToDateTime(reader["lastcrawleddt"]);					
				}
			}
		}



	}
}