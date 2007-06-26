using System;
using Shunde.Framework;
using Shunde.Framework.Columns;
using TestingGround;
using System.Drawing;

namespace Test.Test {

	/// <summary>
	/// A Test object
	/// </summary>
	public class Test : DBObject
	{


		private int? testInt;

		/// <summary></summary>
		public int? TestInt
		{
			get { return this.testInt; }
			set { this.testInt = value; }
		}

		private Car car;

		/// <summary></summary>
		public Car Car
		{
			get { return this.car; }
			set { this.car = value; }
		}

		private Car car2;

		/// <summary></summary>
		public Car Car2
		{
			get { return this.car2; }
			set { this.car2 = value; }
		}

		private DateTime yeah;

		/// <summary></summary>
		public DateTime Yeah
		{
			get { return this.yeah; }
			set { this.yeah = value; }
		}

		private string singleStr;

		/// <summary></summary>
		public string SingleStr
		{
			get { return this.singleStr; }
			set { this.singleStr = value; }
		}

		private string multi;

		/// <summary></summary>
		public string Multi
		{
			get { return this.multi; }
			set { this.multi = value; }
		}

		private Color col;

		/// <summary></summary>
		public Color Col
		{
			get { return this.col; }
			set { this.col = value; }
		}


		/// <summary>Sets up the <see cref="ObjectInfo" /> for this class</summary>
		static Test()
		{

			DBTable tbl = new DBTable("Test", new DBColumn[] {
				new NumberColumn("testInt", typeof(int?), (int?)3, (int?)3),
				new SingleLineString("singleStr", 3, 6),
				new MultiLineString("multi", true),
				new ColorColumn("col", false)
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

		/// <summary>Gets all the Test objects in the database filtered by the given <see cref="Car">car</see></summary>
		/// <param name="car">The Car to filter by</param>
		public static Test[] GetTests(Car car)
		{

			Type t = typeof(Test);
			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);
			string sql = oi.GetSelectStatement() + " WHERE [DBObject].[isDeleted] = 0 AND [Test].[carId] = " + car.Id + " ORDER BY [DBObject].[displayOrder] ASC";
			return (Test[])DBObject.GetObjects(sql, t);

		}




	}

}

