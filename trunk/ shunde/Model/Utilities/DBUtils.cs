using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using Shunde.Framework;
using Shunde.Framework.Columns;
using System.Data.Common;

namespace Shunde.Utilities
{
	/// <summary>Database Utilities for the Shunde framework</summary>
	public class DBUtils
	{


		#region Database execution methods

		/// <summary>Executes a non-query SQL Command</summary>
		/// <param Name="sqlStatement">The SQL statement to be executed (not a SELECT statement)</param>
		public static void ExecuteSqlCommand(string sqlStatement)
		{
			ShundeContext context = ShundeContext.Current;

			SqlConnection sqlConnection = context.DbConnection;
			SqlCommand myCommand = sqlConnection.CreateCommand();
			myCommand.CommandText = sqlStatement;
			myCommand.CommandTimeout = context.DbConnectionTimeout;
			myCommand.CommandType = CommandType.Text;
			myCommand.Transaction = context.Transaction;


			try
			{
				myCommand.ExecuteNonQuery();
			}
			catch (Exception se)
			{
				if (se.Message.StartsWith("CONCURRENCY"))
				{
					throw new ConcurrencyException();
				}
				throw new ShundeSqlException(sqlStatement, se);
			}
		}

		/// <summary>Executes a non-query SQL Command using the supplied SqlCommand object (useful for transactions)</summary>
		/// <param Name="dbCommand">A DbCommand object</param>
		/// <remarks>The Connection, CommandTimeout, and Transaction properties are set - and so will be overridden - in this method. All other properties of the DbCommand object are left as is.</remarks>
		public static void ExecuteSqlCommand(SqlCommand dbCommand)
		{
			ShundeContext context = ShundeContext.Current;

			dbCommand.Connection = context.DbConnection;
			dbCommand.CommandTimeout = context.DbConnectionTimeout;
			dbCommand.Transaction = context.Transaction;

			try
			{
				dbCommand.ExecuteNonQuery();
			}
			catch (SqlException se)
			{
				if (se.Message.StartsWith("CONCURRENCY"))
				{
					throw new ConcurrencyException();
				}
				throw new ShundeSqlException(dbCommand.CommandText, se);
			}
		}

		/// <summary>Executes an SQL Query</summary>
		/// <param Name="sqlQuery">The SQL SELECT statement to be queried</param>
		/// <returns>Returns a DbDataReader object with the results from the query</returns>
		public static SqlDataReader ExecuteSqlQuery(string sqlQuery)
		{
			ShundeContext context = ShundeContext.Current;

			SqlConnection sqlConnection = context.DbConnection;
			SqlCommand myCommand = sqlConnection.CreateCommand();
			myCommand.CommandText = sqlQuery;
			myCommand.CommandTimeout = context.DbConnectionTimeout;
			myCommand.Transaction = context.Transaction;


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
		/// <param Name="dbCommand">The DbCommand object to use</param>
		/// <remarks>The Connection, CommandTimeout, and Transaction properties are set - and so will be overridden - in this method. All other properties of the DbCommand object are left as is.</remarks>
		/// <returns>Returns a DbDataReader object with the results from the query</returns>
		public static SqlDataReader ExecuteSqlQuery(SqlCommand dbCommand)
		{

			ShundeContext context = ShundeContext.Current;

			dbCommand.Connection = context.DbConnection;
			dbCommand.CommandTimeout = context.DbConnectionTimeout;
			dbCommand.Transaction = context.Transaction;

			SqlDataReader myReader = null;
			try
			{
				myReader = dbCommand.ExecuteReader();
			}
			catch (Exception se)
			{
				if (myReader != null)
				{
					myReader.Close();
				}
				throw new ShundeSqlException(dbCommand.CommandText, se);
			}
			return (SqlDataReader)myReader;
		}

		#endregion


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

			DbDataReader myReader = DBUtils.ExecuteSqlQuery(sqlCommand);

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
			DbDataReader sdr = DBUtils.ExecuteSqlQuery(query);
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

	/// <summary>
	/// Facilitates the execution of a database transaction.
	/// </summary>
	public class TransactionScope : IDisposable
	{

		private bool isCommitted = false;

		/// <summary>
		/// Starts a new transaction
		/// </summary>
		public TransactionScope()
		{

			ShundeContext.Current.BeginTransaction();

		}

		/// <summary>
		/// Commits the transaction
		/// </summary>
		public void Commit()
		{
			ShundeContext.Current.CommitTransaction();
			isCommitted = true;
		}


		#region IDisposable Members

		/// <summary>
		/// Disposes of the object.
		/// </summary>
		public void Dispose()
		{
			if (!isCommitted)
			{
				ShundeContext.Current.RollbackTransaction();
			}
		}

		#endregion
	}

}
