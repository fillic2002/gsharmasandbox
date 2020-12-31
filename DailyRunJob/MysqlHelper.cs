using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;

namespace Git_Sandbox.DailyRunJob
{
	public class MysqlHelper
	{
		MySqlConnection connection = new MySqlConnection("Server = localhost; Database = myfin; Uid = root; Pwd = Welcome@1; ");
		public bool AddAssetDetails(Model.equity item)
		{

			if (connection.State != ConnectionState.Open)
				connection.Open();			
			var command = new MySqlCommand(@"INSERT INTO myfin.assetdetail ( id,assetTypeID, assetName,assetDetail) 
												VALUES ( " + item.SC_CODE+ ",1,'" +item.SC_NAME+ "','EMPTY');", connection);
			int result = command.ExecuteNonQuery();

			return true;

		}
	}
}
