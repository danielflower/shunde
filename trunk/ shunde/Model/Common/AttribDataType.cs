using System;
using System.Collections.Generic;
using System.Text;
using Shunde.Framework;
using Shunde.Framework.Columns;

namespace Shunde.Common
{
	/// <summary>The type of data that an <see cref="Attrib" /> holds</summary>
	public class AttribDataType : DBObject
	{

		private string name;

		/// <summary>The Name of this data type</summary>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		private string dataClassName;

		/// <summary>The C# class Name of this data type</summary>
		public string DataClassName
		{
			get { return dataClassName; }
			set { dataClassName = value; }
		}


		/// <summary>
		/// Gets the name of this object
		/// </summary>
		public override string FriendlyName
		{
			get { return this.name; }
		}


		/// <summary>Sets up the <see cref="Shunde.Framework.ObjectInfo" /> for this class</summary>
		static AttribDataType()
		{

			DBTable tbl = new DBTable("AttribDataType", new DBColumn[] {
				new SingleLineString( "name", 1, 100 ),
				new SingleLineString( "dataClassName", 1, 200 )
			});

			ObjectInfo.RegisterObjectInfo(typeof(AttribDataType), tbl);

		}

		/// <summary>Gets the data Type of the attribute</summary>
		public Type GetDataType()
		{
			return Type.GetType(DataClassName);
		}



		/// <summary>Gets and populates all the AttribDataTypes</summary>
		/// <returns>Returns an array of 0 or more AttribDataTypes</returns>
		public static AttribDataType[] GetAttribDataTypes()
		{

			Type t = typeof(AttribDataType);

			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);

			string sql = oi.GetSelectStatement() + " WHERE DBObject.isDeleted = 0 ORDER BY DBObject.displayOrder ASC, AttribDataType.name ASC";

			return (AttribDataType[])DBObject.GetObjects(sql, t);

		}




	}
}
