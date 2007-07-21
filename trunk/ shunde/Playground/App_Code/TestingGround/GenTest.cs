using System;
using Shunde.Framework;
using Shunde.Framework.Columns;

namespace Test
{

	/// <summary>
	/// A Test object
	/// </summary>
	public class Test : DBObject
	{


		private Status status;

		/// <summary></summary>
		public Status Status
		{
			get { return this.status; }
			set { this.status = value; }
		}


		/// <summary>Sets up the <see cref="ObjectInfo" /> for this class</summary>
		static Test()
		{

			DBTable tbl = new DBTable("Test", new DBColumn[] {
				new EnumColumn("status", typeof(Status))
			});

			ObjectInfo.RegisterObjectInfo(typeof(Test), tbl);

		}


		/// <summary>Gets all the Test objects in the database</summary>
		public static Test[] GetTests()
		{

			Type t = typeof(Test);
			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);
			string sql = oi.GetSelectStatement() + " WHERE [DBObject].[isDeleted] = 0 ORDER BY [DBObject].[displayOrder] ASC";
			return (Test[])DBObject.GetObjects(sql, t);

		}

		/// <summary>Gets all the Test objects in the database filtered by the given <see cref="Status">status</see></summary>
		/// <param name="status">The Status to filter by</param>
		public static Test[] GetTests(Status status)
		{

			Type t = typeof(Test);
			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);
			string sql = oi.GetSelectStatement() + " WHERE [DBObject].[isDeleted] = 0 AND [Test].[status] = (int)status ORDER BY [DBObject].[displayOrder] ASC";
			return (Test[])DBObject.GetObjects(sql, t);

		}


	}



	/// <summary>The status of a Test</summary>
	[Flags]
	public enum Status
	{
		Up = 1,
		Down = 2,
		Around = 4,
		There = 8
	}


}

