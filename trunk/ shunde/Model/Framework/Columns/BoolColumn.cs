using System;
using System.Collections.Generic;
using System.Text;

namespace Shunde.Framework.Columns
{

	/// <summary>
	/// A column holding a boolean value
	/// </summary>
	public class BoolColumn : DBColumn
	{

		/// <summary>
		/// Specifies whether or not this column allows NULL values into the database
		/// </summary>
		public override bool AllowNulls
		{
			get
			{
				return base.AllowNulls;
			}
			set
			{
				if (value == true)
				{
					throw new Exception("Booleans cannot be set to null");
				}
				base.AllowNulls = value;
			}
		}

		/// <summary>
		/// Creates a new boolean column
		/// </summary>
		/// <param name="name">The name of the column</param>
		public BoolColumn(string name)
			: base(name, typeof(bool), false)
		{
		}


		/// <summary>
		/// Gets SQL Data Type that corresponds to this column
		/// </summary>
		public override System.Data.SqlDbType GetCorrespondingSqlDBType()
		{
			return System.Data.SqlDbType.Bit;
		}


		/// <summary>Gets the Value of this object in a suitable manner for use in SQL statements</summary>
		/// <returns>Returns 1 if the object is true, otherwise 0</returns>
		public override string GetSqlText(object value)
		{
			return ((bool)value) ? "1" : "0";
		}

	}
}
