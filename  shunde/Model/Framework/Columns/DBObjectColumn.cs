using System;
using System.Collections.Generic;
using System.Text;

namespace Shunde.Framework.Columns
{

	/// <summary>
	/// A column which holds a reference to DBObject
	/// </summary>
	public class DBObjectColumn : DBColumn
	{

		/// <summary>
		/// Creates a new DBObjectColumn
		/// </summary>
		public DBObjectColumn(string columnName, Type type, bool allowNulls)
			: base(columnName, type, allowNulls)
		{

			Type dbObjectType = typeof(DBObject);
			if (!type.Equals(dbObjectType) && !type.IsSubclassOf(dbObjectType))
			{
				throw new Exception("The type " + type.FullName + " specified for " + columnName + " is not a DBObject type.");
			}
		}

		/// <summary>
		/// Gets SQL Data Type that corresponds to this column
		/// </summary>
		public override System.Data.SqlDbType GetCorrespondingSqlDBType()
		{
			return System.Data.SqlDbType.Int;
		}

	}
}
