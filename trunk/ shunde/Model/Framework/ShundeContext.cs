using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace Shunde.Framework
{
	/// <summary>
	/// Provides settings, such as database connections, which Shunde requires in order to operate.
	/// Each thread which uses Shunde has a reference to an instance of this class.
	/// </summary>
	public class ShundeContext : IDisposable
	{


		private string dbConnectionString;

		private SqlConnection dbConnection = null;
		private int dbConnectionTimeout = 30;
		private SqlTransaction transaction = null;

		/// <summary>
		/// The time, in seconds, for database calls to timeout.
		/// The default is 30 seconds.
		/// </summary>
		public int DbConnectionTimeout
		{
			get { return dbConnectionTimeout; }
			set { dbConnectionTimeout = value; }
		}

		/// <summary>
		/// Sets the connection string to the database.
		/// </summary>
		public string DbConnectionString
		{
			get { return dbConnectionString; }
			set { dbConnectionString = value; }
		}


		/// <summary>
		/// The currently invoked transaction, or null if there is no current transaction.
		/// </summary>
		public SqlTransaction Transaction
		{
			get { return transaction; }
		}
	


		/// <summary>
		/// A connection to the database
		/// </summary>
		public SqlConnection DbConnection
		{
			get
			{
				if (dbConnection == null)
				{

					if (string.IsNullOrEmpty(this.dbConnectionString))
					{
						throw new Exception("The database connection string has not been set yet.  Please set ShundeContext.Current.DbConnectionString before making calls to the database.");
					}
					this.dbConnection = new SqlConnection(this.dbConnectionString);
					this.dbConnectionString = null;
					this.dbConnection.Open();
				}
				return this.dbConnection;
			}
		}

		private ShundeContext()
		{
		}

		/// <summary>
		/// Sets the database connection string for the current thread.
		/// </summary>
		/// <remarks>Does not attempt to connection to or open the database until a database call is made.</remarks>
		/// <param name="connectionString">The connection string to the database.</param>
		public void SetDbConnectionString(string connectionString)
		{
			if (string.IsNullOrEmpty(connectionString))
			{
				throw new ArgumentException("No value was specified for connectionString", "connectionString");
			}
			this.dbConnectionString = connectionString;
		}

		#region Transaction methods

		/// <summary>
		/// Starts an Sql Transaction
		/// </summary>
		public void BeginTransaction()
		{
			this.transaction = this.DbConnection.BeginTransaction();
		}

		/// <summary>
		/// Commits the currently started Sql Transaction
		/// </summary>
		public void CommitTransaction()
		{
			this.transaction.Commit();
			this.transaction = null;
		}

		/// <summary>
		/// Rolls back the current Sql Transaction
		/// </summary>
		public void RollbackTransaction()
		{
			try
			{
				this.transaction.Rollback();
				this.transaction = null;
			}
			catch { }
		}


		#endregion


		#region Context reference members

		[ThreadStatic]
		private static ShundeContext context = null;

		/// <summary>
		/// Gives access to the current ShundeContext.
		/// </summary>
		/// <remarks>There is one ShundeContext per thread.</remarks>
		public static ShundeContext Current
		{
			get
			{
				if (context == null)
				{
					context = new ShundeContext();
				}
				return context;
			}
		}


		/// <summary>
		/// Closes and releases all resources used by the current ShundeContext.
		/// </summary>
		public static void CloseContext()
		{
			if (context != null)
			{
				context.Dispose();
				context = null;
			}
		}

		#endregion


		#region IDisposable Members

		/// <summary>
		/// Disposes the resources used by this context.
		/// </summary>
		public void Dispose()
		{
			if (dbConnection != null)
			{
				dbConnection.Close();
				dbConnection.Dispose();
				dbConnection = null;
			}
		}

		#endregion
	}



}
