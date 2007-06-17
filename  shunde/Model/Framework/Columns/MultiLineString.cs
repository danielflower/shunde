using System;
using System.Collections.Generic;
using System.Text;
using Shunde.Utilities;

namespace Shunde.Framework.Columns
{
	/// <summary>
	/// A multi-line string
	/// </summary>
	public class MultiLineString : StringColumn
	{

		/// <summary>
		/// Creates a new multi line string column
		/// </summary>
		/// <param name="name">The name of the database column that this column corresponds to</param>
		/// <param name="allowNulls">Specifies whether or not this column can hold null (empty) strings</param>
		public MultiLineString(string name, bool allowNulls)
			: this(name, allowNulls, null, null)
		{
		}

		/// <summary>
		/// Creates a new multi line string column
		/// </summary>
		/// <param name="name">The name of the database column that this column corresponds to</param>
		/// <param name="allowNulls">Specifies whether or not this column can hold null (empty) strings</param>
		/// <param name="regularExpression">A regular expression that the value of this column must match</param>
		/// <param name="regularExpressionErrorMessage">The error message to display if the value of this column does not match the given regular expression</param>
		public MultiLineString(string name, bool allowNulls, string regularExpression, string regularExpressionErrorMessage)
			: base(name, allowNulls)
		{
			this.RegularExpression = regularExpression;
			this.RegularExpressionErrorMessage = regularExpressionErrorMessage;
		}


		/// <summary>
		/// Converts the string to a string which can be used in an SQL statement
		/// </summary>
		/// <param name="value">The value, which is a string, to convert</param>
		public override string GetSqlText(object value)
		{
			string str = value as string;
			if (str == null)
			{
				str = string.Empty;
			}
			return DBUtils.ParseSql(str, true);
		}

		/// <summary>
		/// Gets SQL Data Type that corresponds to this column
		/// </summary>
		public override System.Data.SqlDbType GetCorrespondingSqlDBType()
		{
			return System.Data.SqlDbType.NText;
		}




	}
}
