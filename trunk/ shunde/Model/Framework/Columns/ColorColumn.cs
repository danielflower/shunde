using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Shunde.Framework.Columns
{

	/// <summary>
	/// A column which holds a color value
	/// </summary>
	/// <remarks>Nullable colors are not allowed; instead, an empty color is represented using <see cref="Color.Empty"/>.</remarks>
	public class ColorColumn : DBColumn
	{

		/// <summary>
		/// Creates a new color column
		/// </summary>
		/// <param name="columnName">The name of the column</param>
		/// <param name="allowNulls">If true, then <see cref="Color.Empty" /> is an allowed value, otherwise it is not. Note that nullable colors are not allowed.</param>
		public ColorColumn(string columnName, bool allowNulls)
			: base(columnName, typeof(Color), allowNulls)
		{
		}

		/// <summary>
		/// Returns true if the color is <see cref="Color.Empty" />
		/// </summary>
		public override bool IsNull(object value)
		{
			return ((Color)value).Equals(Color.Empty);
		}

		/// <summary>
		/// Gets SQL Data Type that corresponds to this column
		/// </summary>
		public override System.Data.SqlDbType GetCorrespondingSqlDBType()
		{
			return System.Data.SqlDbType.VarChar;
		}


	}
}
