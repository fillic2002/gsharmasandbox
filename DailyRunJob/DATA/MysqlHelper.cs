﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Git_Sandbox.DailyRunJob.Contract;
using Git_Sandbox.Model;
using MySql.Data.MySqlClient;

namespace Git_Sandbox.DailyRunJob
{
	public class MysqlHelper: Ioperation
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

	 

		public bool UpdateLatesNAV(string company, double liveprice)
		{
			if (liveprice <= 0)
				return false;
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				using var command = new MySqlCommand(@"UPDATE myfin.equitydetails SET liveprice = " + liveprice + "," +
									" dtupdated ='" + DateTime.UtcNow.ToString("yyyy-MM-dd") + "' WHERE (ISIN = '" + company + "');", _conn);

				int result = command.ExecuteNonQuery();
			}
			return true;
		}

		public IList<equity> GetPortfolioAssetUrl()
		{
			using (MySqlConnection _conn = new MySqlConnection(connString))
			{
				_conn.Open();
				using var command = new MySqlCommand(@"select isin, description from myfin.equitydetails where myfin.equitydetails.description IS NOT NULL;", _conn);
				using var reader = command.ExecuteReader();
				IList<equity> eq = new List<equity>();
				while (reader.Read())
				{
					eq.Add(new equity()
					{
						ISIN = reader["isin"].ToString(),
						sourceurl = reader["description"].ToString()
					});
				}
				return eq;
			}

		}
	}
}
