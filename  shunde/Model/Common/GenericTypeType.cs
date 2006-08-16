using System;
using System.Collections.Generic;
using System.Text;
using Shunde.Framework;
using Shunde.Utilities;

namespace Shunde.Common
{
	/// <summary>This is a type of a <see cref="GenericType" />, for example "Event Type"</summary>
	public class GenericTypeType : DBObject
	{

		private string name;

		/// <summary>The Name of this data type</summary>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		private string notes;

		/// <summary>Any extra Notes about this type</summary>
		public string Notes
		{
			get { return notes; }
			set { notes = value; }
		}

		private int code;

		/// <summary>A unique Code identifying this type</summary>
		public int Code
		{
			get { return code; }
			set { code = value; }
		}

		/// <summary>
		/// Gets the friendly name of the object
		/// </summary>
		public override string FriendlyName
		{
			get
			{
				return Name;
			}
		}

		/// <summary>Sets up the <see cref="Shunde.Framework.ObjectInfo" /> for this class</summary>
		static GenericTypeType()
		{

			DBColumn codeCol = new DBColumn("code", typeof(int), false, 1, null);
			codeCol.IsUnique = true;

			DBTable tbl = new DBTable("GenericTypeType", new DBColumn[] {
				new DBColumn( "name", typeof(string), 1, 100 ),
				new DBColumn( "notes", typeof(string), true ),
				codeCol
			});

			ObjectInfo.RegisterObjectInfo(typeof(GenericTypeType), tbl);

		}


		/// <summary>Gets the GenericTypeType with the given <see cref="Code" /></summary>
		public static GenericTypeType GetGenericTypeType(int code)
		{

			try
			{
				return (GenericTypeType)DBObject.GetObject("SELECT gtt.[id] FROM GenericTypeType gtt INNER JOIN DBObject obj ON obj.id = gtt.id WHERE obj.isDeleted = 0 AND gtt.code = " + code);
			}
			catch (ValidationException vex)
			{
				// a validation exception is thrown if more than one row is returned
				// in this case, we change to a ShundeException because this shouldn't
				// occur on the website
				throw new ShundeException(vex.Message);
			}

		}


		/// <summary>Gets and populates all the GenericTypeTypes</summary>
		/// <returns>Returns an array of 0 or more GenericTypeTypes</returns>
		public static GenericTypeType[] GetGenericTypeTypes()
		{

			Type t = typeof(GenericTypeType);

			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);

			String sql = oi.GetSelectStatement() + " WHERE DBObject.isDeleted = 0 ORDER BY DBObject.displayOrder ASC, GenericTypeType.name ASC";

			return (GenericTypeType[]) DBObject.GetObjects(sql, t);

		}


	}

}
