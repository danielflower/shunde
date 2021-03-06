using System;
using System.Collections.Generic;
using System.Text;
using Shunde.Framework;
using Shunde.Utilities;
using Shunde.Framework.Columns;

namespace Shunde.Common
{
	/// <summary>This is a generic class for a type, for example a type of event</summary>
	/// <remarks>This allows multiple type objects, normally kept in separate tables, to all be stored in one Table.</remarks>
	public class GenericType : DBObject
	{

		private GenericTypeType genericTypeType;

		/// <summary>The type of generic type that this is</summary>
		public GenericTypeType GenericTypeType
		{
			get { return genericTypeType; }
			set { genericTypeType = value; }
		}

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

		private string formatInfo;

		/// <summary>Optional text field containing format information, such as a colour Code</summary>
		public string FormatInfo
		{
			get { return formatInfo; }
			set { formatInfo = value; }
		}


		private int? code = null;

		/// <summary>
		/// A unique code to identify this GenericType
		/// </summary>
		public int? Code
		{
			get { return code; }
			set { code = value; }
		}

		/// <summary>
		/// Gets the friendly name of the object
		/// </summary>
		public override string FriendlyName
		{
			get { return Name; }
		}


		/// <summary>The objects within this type</summary>
		public IEnumerable<DBObject> objects;

		/// <summary>Sets up the <see cref="Shunde.Framework.ObjectInfo" /> for this class</summary>
		static GenericType()
		{

			DBColumn codeCol = new NumberColumn("code", typeof(int?), 1, null);
			codeCol.IsUnique = true;

			DBTable tbl = new DBTable("GenericType", new DBColumn[] {
				new DBObjectColumn( "genericTypeType", typeof(GenericTypeType), false ),
				new SingleLineString( "name", 1, 100 ),
				new MultiLineString( "notes", true ),
				new SingleLineString( "formatInfo", 0, 200 ),
				codeCol
			});

			ObjectInfo.RegisterObjectInfo(typeof(GenericType), tbl);

		}


		/// <summary>Gets and populates all the GenericTypes for the given type</summary>
		/// <param Name="GenericTypeType">The type to filter by</param>
		/// <returns>Returns an array of 0 or more GenericTypes</returns>
		public static GenericType[] GetGenericTypes(GenericTypeType genericTypeType)
		{

			Type t = typeof(GenericType);

			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);

			String sql = oi.GetSelectStatement() + " WHERE DBObject.isDeleted = 0 AND GenericType.genericTypeTypeId = " + genericTypeType.Id + " ORDER BY DBObject.displayOrder ASC, GenericType.name ASC";

			return (GenericType[]) DBObject.GetObjects(sql, t);

		}

		/// <summary>Gets and populates all the GenericTypes for the given <see cref="Shunde.Common.GenericTypeType.Code" /></summary>
		/// <param Name="genericTypeTypeCode">The <see cref="Shunde.Common.GenericTypeType.Code" /> of the <see cref="GenericTypeType" /> that are to be returned.</param>
		/// <returns>Returns an array of 0 or more GenericTypes</returns>
		public static GenericType[] GetGenericTypes(int genericTypeTypeCode)
		{

			Type t = typeof(GenericType);

			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);

			String sql = oi.GetSelectStatement() + " WHERE DBObject.isDeleted = 0 AND GenericType.genericTypeTypeId = (SELECT id FROM GenericTypeType WHERE code = " + genericTypeTypeCode + ") ORDER BY DBObject.displayOrder ASC, GenericType.name ASC";

			return (GenericType[]) DBObject.GetObjects(sql, t);

		}

		/// <summary>Gets all the generic types in the database, irrespective of the <see cref="GenericTypeType" /></summary>
		/// <returns>Returns an array of 0 or more GenericTypes</returns>
		public static GenericType[] GetGenericTypes()
		{

			Type t = typeof(GenericType);

			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);

			String sql = oi.GetSelectStatement() + " WHERE DBObject.isDeleted = 0 ORDER BY GenericType.genericTypeTypeId ASC, DBObject.displayOrder ASC, GenericType.name ASC";

			return (GenericType[]) DBObject.GetObjects(sql, t);

		}



		/// <summary>Gets the GenericType with the given <see cref="Code" /></summary>
		public static GenericType GetGenericType(int code)
		{

			try
			{
				return (GenericType)DBObject.GetObject("SELECT gt.[id] FROM GenericType gt INNER JOIN DBObject obj ON obj.id = gt.id WHERE obj.isDeleted = 0 AND gt.code = " + code);
			}
			catch (ValidationException vex)
			{
				// a validation exception is thrown if more than one row is returned
				// in this case, we change to a ShundeException because this shouldn't
				// occur on the website
				throw new ShundeException(vex.Message);
			}

		}



	}

}
