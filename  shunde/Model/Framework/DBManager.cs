using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Collections;
using System.Data;
using System.Web;

namespace Shunde.Framework
{

	/// <summary>
	/// The class which manages the connection to the database
	/// </summary>
	public static class DBManager
	{

		/// <summary>Gets the SqlConnection</summary>
		public static SqlConnection SqlConnection
		{
			get
			{
				SqlConnection sqlConnection = (SqlConnection)StorageContainer["sqlConnection"];
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
		}

		/// <summary>Gets the SqlTransaction</summary>
		private static SqlTransaction GetSqlTransaction()
		{
			return (SqlTransaction)StorageContainer["sqlTransaction"];
		}

		/// <summary>This method sets the SqlConnection, ready to be used.</summary>
		/// <remarks>
		/// When used in websites, each HttpContext (i.e. each web-page request) should have its own SqlConnection, and a good storage container to use would be HttpContext.Current.Items
		/// </remarks>
		/// <param name="storageContainer">A storage container, which needs to survive until <see cref="CloseSqlConnection" /> is called, which the Shunde Framework uses to hold necessary objects.</param>
		/// <param name="connectionString">The connection string to the database</param>
		public static void SetSqlConnection(IDictionary storageContainer, string connectionString)
		{
			StorageContainer = storageContainer;
			StorageContainer.Add("sqlConnection", new SqlConnection(connectionString));
		}

		/// <summary>
		/// Starts an Sql Transaction
		/// </summary>
		public static void BeginTransaction()
		{
			StorageContainer.Add("sqlTransaction", SqlConnection.BeginTransaction());
		}

		/// <summary>
		/// Commits the currently started Sql Transaction
		/// </summary>
		public static void CommitTransaction()
		{
			GetSqlTransaction().Commit();
			StorageContainer.Remove("sqlTransaction");
		}

		/// <summary>
		/// Rolls back the current Sql Transaction
		/// </summary>
		public static void RollbackTransaction()
		{
			try
			{
				GetSqlTransaction().Rollback();
				StorageContainer.Remove("sqlTransaction");
			}
			catch { }
		}

		/// <summary>Closes the database connection for the current HttpContext</summary>
		public static void CloseSqlConnection()
		{
			SqlConnection sqlConnection = (SqlConnection)StorageContainer["sqlConnection"];
			if (sqlConnection == null)
			{
				return;
			}
			sqlConnection.Close();
			sqlConnection.Dispose();
			StorageContainer.Remove("sqlConnection");
			StorageContainer.Remove("sqlTransaction");
			storageContainer = null;
		}

		/// <summary></summary>
		[ThreadStatic]
		private static IDictionary storageContainer = null;

		/// <summary></summary>
		public static IDictionary StorageContainer
		{
			get
			{
				return storageContainer;
			}
			set
			{
				storageContainer = value;
			}
		}

		/// <summary>Executes a non-query SQL Command</summary>
		/// <param Name="sqlStatement">The SQL statement to be executed (not a SELECT statement)</param>
		public static void ExecuteSqlCommand(string sqlStatement)
		{

			SqlConnection sqlConnection = SqlConnection;
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

			sqlCommand.Connection = SqlConnection;
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

			SqlCommand myCommand = new SqlCommand(sqlQuery, SqlConnection, GetSqlTransaction());

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

			sqlCommand.Connection = SqlConnection;
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

	}
}
