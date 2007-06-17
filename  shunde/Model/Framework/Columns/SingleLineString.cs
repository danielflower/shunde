using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Shunde.Utilities;

namespace Shunde.Framework.Columns
{

	/// <summary>
	/// A single-line string
	/// </summary>
	public class SingleLineString : StringColumn
	{




				/// <summary>
		/// Creates a new single line string column
		/// </summary>
		/// <param name="name">The name of the database column that this column corresponds to</param>
		/// <param name="minLength">The minimum length that this string is allowed</param>
		/// <param name="maxLength">The maximum length that this string is allowed</param>
		public SingleLineString(string name, int minLength, int maxLength)
			: this(name, minLength, maxLength, null, null)
		{
		}

		/// <summary>
		/// Creates a new single line string column
		/// </summary>
		/// <param name="name">The name of the database column that this column corresponds to</param>
		/// <param name="minLength">The minimum length that this string is allowed</param>
		/// <param name="maxLength">The maximum length that this string is allowed</param>
		/// <param name="regularExpression">A regular expression that the value of this column must match</param>
		/// <param name="regularExpressionErrorMessage">The error message to display if the value of this column does not match the given regular expression</param>
		public SingleLineString(string name, int minLength, int maxLength, string regularExpression, string regularExpressionErrorMessage)
			: base(name, (minLength == 0))
		{
			if (minLength < 0)
			{
				throw new ArgumentException("Minimum length cannot be less than 0", "minimumLengthAllowed");
			}
			if (maxLength < 1)
			{
				throw new ArgumentException("Maximum length cannot be less than 1", "maximumLengthAllowed");
			}
			if (minLength > maxLength)
			{
				throw new ArgumentException("Minimum length cannot be greater than maximum length");
			}
			this.MinLength = minLength;
			this.MaxLength = maxLength;
			this.RegularExpression = regularExpression;
			this.RegularExpressionErrorMessage = regularExpressionErrorMessage;
		}


		/// <summary>
		/// Gets SQL Data Type that corresponds to this column
		/// </summary>
		public override System.Data.SqlDbType GetCorrespondingSqlDBType()
		{
			return System.Data.SqlDbType.NVarChar;
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
			return DBUtils.ParseSql(str.Trim(), true);
		}


	}
}
