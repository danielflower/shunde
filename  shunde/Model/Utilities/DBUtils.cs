using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using Shunde.Framework;
using Shunde.Framework.Columns;

namespace Shunde.Utilities
{
	/// <summary>Database Utilities for the Shunde framework</summary>
	public class DBUtils
	{





		/// <summary>Executes an SQL Query that is known to return just one integer Value</summary>
		/// <remarks>To use this, set the column Name holding the integer to be the first column. If no results are found, or more than 1 result is found, or the first column does not hold an integer, a ShundeException will be thrown</remarks>
		/// <param Name="sqlQuery">The SQL SELECT statement to be queried</param>
		/// <returns>Returns the integer Value returned by the SQL statement</returns>
		public static int GetIntFromSqlSelect(string sqlQuery)
		{
			SqlCommand sqlCommand = new SqlCommand(sqlQuery);
			return GetIntFromSqlSelect(sqlCommand);
		}



		/// <summary>Executes an SQL Query that is known to return just one integer Value</summary>
		/// <remarks>To use this, set the column Name holding the integer to be the first column. If no results are found, or more than 1 result is found, or the first column does not hold an integer, a ShundeException will be thrown</remarks>
		/// <param Name="sqlCommand">The <see cref="SqlCommand" /> to execute</param>
		/// <returns>Returns the integer Value returned by the SQL statement</returns>
		public static int GetIntFromSqlSelect(SqlCommand sqlCommand)
		{

			SqlDataReader myReader = DBManager.ExecuteSqlQuery(sqlCommand);

			int returnValue = -1;
			if (myReader.Read())
			{
				try
				{
					returnValue = System.Convert.ToInt32(myReader[0]);
				}
				catch
				{
					myReader.Close();
					throw new ShundeException("Either the column named intValue does not exist, or it contains a non-integer value\n\nSQL Statement:\n" + sqlCommand.CommandText);
				}
				if (myReader.Read())
				{
					myReader.Close();
					throw new ShundeException("getIntFromSqlSelect(SqlConnection, string) was called with an SQL Statement that returned more than 1 row");
				}
				myReader.Close();
			}
			else
			{
				myReader.Close();
				throw new ShundeException("getIntFromSqlSelect(SqlConnection, string) was called with an SQL Statement that returned no rows");
			}
			return returnValue;
		}



		/// <summary>Executes the given SQL Query and returns a boolean indicating whether 1 or more rows were returned</summary>
		public static bool HasRows(string query)
		{
			bool returnValue = false;
			SqlDataReader sdr = DBManager.ExecuteSqlQuery(query);
			if (sdr.Read())
			{
				returnValue = true;
			}
			sdr.Close();
			return returnValue;
		}


		/// <summary>Escapes all apostrophes in an SQL Statement.</summary>
		/// <param Name="sql">The SQL string to be parsed</param>
		/// <returns>Returns escaped SQL string. This is an empty string if the passed string was null</returns>
		public static string ParseSql(string sql)
		{
			return ParseSql(sql, false);
		}

		/// <summary>Escapes all apostrophes in an SQL Statement.</summary>
		/// <param Name="sql">The SQL string to be parsed</param>
		/// <param Name="encloseInSingleQuotes">If true, this will enclose the return string in single quotes (<i>'</i>), or will set the string to the Value "<i>null</i>" if it is an empty string. This is useful for setting fields to equal NULL in the database where values are empty.</param>
		/// <returns>Returns escaped SQL string.</returns>
		public static string ParseSql(string sql, bool encloseInSingleQuotes)
		{
			if (sql == null)
			{
				sql = string.Empty;
			}
			string returnString = sql.Replace("'", "''");
			if (encloseInSingleQuotes)
			{
				returnString = "N'" + returnString + "'";
			}
			return returnString;
		}

		/// <summary>Parse a date-time, to be put into SQL Server</summary>
		/// <remarks>Sets Value to Null if DateTime.MinValue</remarks>
		public static string ParseSql(DateTime? dateValue)
		{
			if (dateValue == null)
			{
				return "null";
			}
			else
			{
				return "'" + dateValue.Value.ToString("yyyy/MM/dd HH:mm:ss.fff") + "'";
			}
		}

		/// <summary>Converts a boolean into the SQL equivalent</summary>
		public static string ParseSql(bool boolValue)
		{
			return System.Convert.ToInt32(boolValue).ToString();
		}






	}

}
