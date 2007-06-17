using System;
using System.Collections.Generic;
using Shunde.Framework;
using System.Drawing;
using Shunde.Framework.Columns;

/// <summary>
/// Summary description for Car
/// </summary>
namespace TestingGround {

	public class Car : DBObject
	{

		private string make;

		public string Make
		{
			get { return make; }
			set { make = value; }
		}

		private short? year;

		public short? Year
		{
			get { return year; }
			set { year = value; }
		}

		private Person person = null;

		public Person Person
		{
			get { return person; }
			set { person = value; }
		}

		private DateTime? dateImported;

		public DateTime? DateImported
		{
			get { return dateImported; }
			set { dateImported = value; }
		}

		private BinaryData picture;

		public BinaryData Picture
		{
			get { return picture; }
			set { picture = value; }
		}

		private Color color = Color.Empty;

		public Color Color
		{
			get { return color; }
			set { color = value; }
		}


		/// <summary>Sets up the <see cref="ObjectInfo" /> for this class</summary>
		static Car()
		{

			DBTable tbl = new DBTable("Car", new DBColumn[] {
				new SingleLineString( "make", 1, 200 ),
				new NumberColumn( "year", typeof(short?), (short?)1000, (short?)2000 ),
				new DBObjectColumn( "person", typeof(Person), true ),
				new DateTimeColumn( "dateImported", true, DateTimePart.Date ),
				new BinaryDataColumn( "picture", true ),
				new ColorColumn("color", true)
			});

			ObjectInfo.RegisterObjectInfo(typeof(Car), tbl);

		}


		/// <summary>Gets all the cars in the database</summary>
		public static Car[] GetCars()
		{

			Type t = typeof(Car);
			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);
			String sql = oi.GetSelectStatement() + " WHERE DBObject.isDeleted = 0";

			return (Car[]) DBObject.GetObjects(sql, t);

		}

	}

}
