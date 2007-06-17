using System;
using Shunde.Framework;
using System.Web.UI.WebControls;
using Shunde.Framework.Columns;


namespace TestingGround
{

	/// <summary>
	/// A ImportedCar object
	/// </summary>
	public class ImportedCar : Car
	{


		private BinaryData manual;

		/// <summary>The manual</summary>
		public BinaryData Manual
		{
			get { return this.manual; }
			set { this.manual = value; }
		}

		private HorizontalAlign? horizontalAlign = null;

		/// <summary>
		/// The horizontal alignment of the stereo
		/// </summary>
		public HorizontalAlign? HorizontalAlign
		{
			get { return horizontalAlign; }
			set { horizontalAlign = value; }
		}
	


		/// <summary>Sets up the <see cref="ObjectInfo" /> for this class</summary>
		static ImportedCar()
		{

			DBTable tbl = new DBTable("ImportedCar", new DBColumn[] {
				new BinaryDataColumn( "manual", false ),
				new EnumColumn( "horizontalAlign", typeof(HorizontalAlign?))
			});

			ObjectInfo.RegisterObjectInfo(typeof(ImportedCar), tbl);

		}


		/// <summary>Gets all the ImportedCar objects in the database</summary>
		public static ImportedCar[] GetImportedCars()
		{

			Type t = typeof(ImportedCar);
			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);
			string sql = oi.GetSelectStatement() + " WHERE [DBObject].[isDeleted] = 0 ORDER BY [DBObject].[displayOrder] ASC";
			return (ImportedCar[])DBObject.GetObjects(sql, t);

		}


	}

}

