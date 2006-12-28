using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using Shunde.Framework;

namespace Shunde.Utilities
{
	/// <summary>Database Utilities for the Shunde framework</summary>
	public class DBUtils
	{

		/// <summary>Gets the SqlConnection</summary>
		public static SqlConnection GetSqlConnection()
		{
			SqlConnection sqlConnection = (SqlConnection) GetStorageContainer()["sqlConnection"];
			if (sqlConnection == null)
			{
				throw new ShundeException("The SqlConnection was not found in the current HttpContext.");
			}
			if (sqlConnection.State == ConnectionState.Closed)
			{
				sqlConnection.Open();
			}

			return sqlConnection;
		}

		/// <summary>Gets the SqlTransaction</summary>
		private static SqlTransaction GetSqlTransaction()
		{
			return (SqlTransaction) GetStorageContainer()["sqlTransaction"];
		}

		/// <summary>Each HttpContext has it's own SqlConnection. This method sets the SqlConnection, ready to be used.</summary>
		public static void SetSqlConnection(string connectionString)
		{
			GetStorageContainer().Add("sqlConnection", new SqlConnection(connectionString));
		}

		/// <summary>
		/// Starts an Sql Transaction
		/// </summary>
		public static void BeginTransaction()
		{
			GetStorageContainer().Add("sqlTransaction", DBUtils.GetSqlConnection().BeginTransaction());
		}

		/// <summary>
		/// Commits the currently started Sql Transaction
		/// </summary>
		public static void CommitTransaction()
		{
			DBUtils.GetSqlTransaction().Commit();
			GetStorageContainer().Remove("sqlTransaction");
		}

		/// <summary>
		/// Rolls back the current Sql Transaction
		/// </summary>
		public static void RollbackTransaction()
		{
			try
			{
				DBUtils.GetSqlTransaction().Rollback();
				GetStorageContainer().Remove("sqlTransaction");
			}
			catch { }
		}

		/// <summary>Closes the database connection for the current HttpContext</summary>
		public static void CloseSqlConnection()
		{
			SqlConnection sqlConnection = (SqlConnection) GetStorageContainer()["sqlConnection"];
			if (sqlConnection == null)
			{
				return;
			}
			sqlConnection.Close();
			sqlConnection.Dispose();
			GetStorageContainer().Remove("sqlConnection");
			GetStorageContainer().Remove("sqlTransaction");
			storageContainer = null;
		}

		/// <summary></summary>
		[ThreadStatic]
		public static IDictionary storageContainer = null;

		/// <summary></summary>
		private static IDictionary GetStorageContainer()
		{
			return (storageContainer == null) ? HttpContext.Current.Items : storageContainer;
		}

		/// <summary>Executes a non-query SQL Command</summary>
		/// <param Name="sqlStatement">The SQL statement to be executed (not a SELECT statement)</param>
		public static void ExecuteSqlCommand(string sqlStatement)
		{

			SqlConnection sqlConnection = GetSqlConnection();
			SqlCommand myCommand = new SqlCommand(sqlStatement, sqlConnection);
			myCommand.Transaction = GetSqlTransaction();
			try
			{
				myCommand.ExecuteNonQuery();
			}
			catch (Exception se)
			{
				throw new ShundeSqlException(sqlStatement, se);
			}
		}

		/// <summary>Executes a non-query SQL Command using the supplied SqlCommand object (useful for transactions)</summary>
		/// <param Name="sqlCommand">An SqlCommand object</param>
		public static void ExecuteSqlCommand(SqlCommand sqlCommand)
		{

			sqlCommand.Connection = GetSqlConnection();
			sqlCommand.Transaction = GetSqlTransaction();



			try
			{
				sqlCommand.ExecuteNonQuery();
			}
			catch (SqlException se)
			{
				if (se.Message.StartsWith("CONCURRENCY"))
				{
					throw new ConcurrencyException();
				}
				throw new ShundeSqlException(sqlCommand.CommandText, se);
			}
		}

		/// <summary>Executes an SQL Query</summary>
		/// <param Name="sqlQuery">The SQL SELECT statement to be queried</param>
		/// <returns>Returns an SqlDataReader object with the results from the query</returns>
		public static SqlDataReader ExecuteSqlQuery(string sqlQuery)
		{

			SqlCommand myCommand = new SqlCommand(sqlQuery, GetSqlConnection(), GetSqlTransaction());

			SqlDataReader myReader = null;
			try
			{
				myReader = myCommand.ExecuteReader();
			}
			catch (Exception se)
			{
				throw new ShundeSqlException(sqlQuery, se);
			}

			return myReader;
		}

		/// <summary>Executes an SQL Query using the specified SqlCommand object (useful in Transactions)</summary>
		/// <param Name="sqlCommand">The SqlCommand object to use</param>
		/// <returns>Returns an SqlDataReader object with the results from the query</returns>
		public static SqlDataReader ExecuteSqlQuery(SqlCommand sqlCommand)
		{

			sqlCommand.Connection = GetSqlConnection();
			sqlCommand.Transaction = GetSqlTransaction();

			SqlDataReader myReader = null;
			try
			{
				myReader = sqlCommand.ExecuteReader();
			}
			catch (Exception se)
			{
				if (myReader != null)
				{
					myReader.Close();
				}
				throw new ShundeSqlException(sqlCommand.CommandText, se);
			}
			return myReader;
		}




		/// <summary>Executes an SQL Query that is known to return just one integer Value</summary>
		/// <remarks>To use this, set the column Name holding the integer to <i>intValue</i>. If no results are found, or more than 1 result is found, or <i>intValue</i> is an invalid column Name or does not hold an integer, a MiscException will be thrown</remarks>
		/// <param Name="sqlQuery">The SQL SELECT statement to be queried</param>
		/// <returns>Returns the integer Value returned by the SQL statement</returns>
		public static int GetIntFromSqlSelect(string sqlQuery)
		{
			SqlCommand sqlCommand = new SqlCommand(sqlQuery);
			return GetIntFromSqlSelect(sqlCommand);
		}



		/// <summary>Executes an SQL Query that is known to return just one integer Value</summary>
		/// <remarks>To use this, set the column Name holding the integer to <i>intValue</i>. If no results are found, or more than 1 result is found, or <i>intValue</i> is an invalid column Name or does not hold an integer, a MiscException will be thrown</remarks>
		/// <param Name="sqlCommand">The <see cref="SqlCommand" /> to execute</param>
		/// <returns>Returns the integer Value returned by the SQL statement</returns>
		public static int GetIntFromSqlSelect(SqlCommand sqlCommand)
		{

			SqlDataReader myReader = ExecuteSqlQuery(sqlCommand);

			int returnValue = -1;
			if (myReader.Read())
			{
				try
				{
					returnValue = System.Convert.ToInt32(myReader["intValue"]);
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
			SqlDataReader sdr = DBUtils.ExecuteSqlQuery(query);
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
			string returnString = (sql + "").Replace("'", "''");
			if (encloseInSingleQuotes)
			{
				if (returnString.Length == 0)
					returnString = "null";
				else
					returnString = "N'" + returnString + "'";
			}
			return returnString;
		}

		/// <summary>Parse a date-time, to be put into SQL Server</summary>
		/// <remarks>Sets Value to Null if DateTime.MinValue</remarks>
		public static string ParseSql(DateTime dateValue)
		{
			if (dateValue.Equals(DBColumn.DateTimeNullValue))
			{
				return "null";
			}
			else
			{
				return "'" + dateValue.ToString("yyyy/MM/dd HH:mm:ss") + "'";
			}
		}

		/// <summary>Converts a boolean into the SQL equivalent</summary>
		public static string ParseSql(bool boolValue)
		{
			return "" + System.Convert.ToInt32(boolValue);
		}

		/// <summary>Converts an int Value into the SQL equivalent, or NULL if int = int.MinValue</summary>
		public static string ParseSql(int intValue)
		{
			return (intValue == int.MinValue) ? "null" : intValue.ToString();
		}


		/// <summary>Gets an int Value from the given SqlDataReader</summary>
		public static int GetIntValue(SqlDataReader sdr, string columnName)
		{
			return Convert.ToInt32(sdr[columnName]);
		}

		/// <summary>Gets an int Value from the given SqlDataReader, where the int Value may be null</summary>
		public static int getIntValueMayBeNull(SqlDataReader sdr, string columnName)
		{
			Object tempObj = sdr[columnName];
			if (tempObj == DBNull.Value)
			{
				return DBColumn.IntegerNullValue;
			}
			return Convert.ToInt32(tempObj);
		}

		/// <summary>Gets a boolean Value from the given SqlDataReader</summary>
		public static bool GetBoolValue(SqlDataReader sdr, String columnName)
		{
			return Convert.ToInt32(sdr[columnName]) == 1;
		}

		/// <summary>Gets a DateTime Value from the given SqlDataReader</summary>
		public static DateTime GetDateTimeValue(SqlDataReader sdr, String columnName)
		{
			return Convert.ToDateTime(sdr[columnName]);
		}

		/// <summary>Gets the SqlDataType for the given column</summary>
		public static SqlDbType GetSqlDbType(DBColumn col)
		{
			Type t = col.Type;

			if (t.Equals(typeof(string)))
			{
				if (col.MaxLength < 0)
				{
					return SqlDbType.NText;
				}
				else
				{
					return SqlDbType.NVarChar;
				}
			}

			if (t.Equals(typeof(int)))
			{
				return SqlDbType.Int;
			}

			if (t.Equals(typeof(short)))
			{
				return SqlDbType.SmallInt;
			}

			if (t.Equals(typeof(long)))
			{
				return SqlDbType.BigInt;
			}

			if (t.Equals(typeof(float)))
			{
				return SqlDbType.Float;
			}

			if (t.Equals(typeof(double)))
			{
				return SqlDbType.Float;
			}

			if (t.Equals(typeof(bool)))
			{
				return SqlDbType.Bit;
			}

			if (t.Equals(typeof(DateTime)))
			{
				return SqlDbType.DateTime;
			}

			if (col.isDBObjectType)
			{
				return SqlDbType.Int;
			}

			if (t.Equals(typeof(BinaryData)))
			{
				return SqlDbType.Image;
			}

			throw new ShundeException("No suitable SqlDbType found for the column " + col.Name + " which has a data type of " + col.Type);

		}

	}

}
