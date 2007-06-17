using System;
using System.Collections.Generic;
using System.Text;

namespace Shunde.Framework.Columns
{
	/// <summary>
	/// A class to hold enumerated values
	/// </summary>
	public class EnumColumn : DBColumn
	{


		/// <summary>
		/// Creates a an enum column
		/// </summary>
		/// <param name="columnName">The name of the column</param>
		/// <param name="type">The type of enumeration, which can be nullable</param>
		public EnumColumn(string columnName, Type type)
			: base (columnName, type, DBColumn.IsNullableType(type))
		{
			if (!IsEnumOrNullableEnum(type))
			{
				throw new Exception("The type " + type.FullName + " specified for " + columnName + " is not an enumeration.");
			}
		}

		/// <summary>
		/// Gets SQL Data Type that corresponds to this column
		/// </summary>
		public override System.Data.SqlDbType GetCorrespondingSqlDBType()
		{
			return System.Data.SqlDbType.Int;
		}


		/// <summary>
		/// Returns true if the specified type is an enumeration, or a nullable enumeration
		/// </summary>
		public static bool IsEnumOrNullableEnum(Type type)
		{
			if (type.IsEnum)
			{
				return true;
			}

			if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
			{
				type = Nullable.GetUnderlyingType(type);
				return type.IsEnum;
			}

			return false;

		}

	}
}
