using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Shunde.Framework.Columns
{

	/// <summary>
	/// A number, or a nullable number
	/// </summary>
	public class NumberColumn : DBColumn, IRangeValidatedColumn
	{


		private IComparable minimumAllowed = null;

		/// <summary>Represents the minimum Value that this column is allowed.</summary>
		/// <remarks>
		/// 	<para>If there is no minimum, then this should be null.</para>
		/// 	<para>If a column has a Value lower than the set Value, then a <see cref="ValidationException" /> will be thrown.</para>
		/// </remarks>
		IComparable IRangeValidatedColumn.MinimumAllowed
		{
			get { return minimumAllowed; }
			set { minimumAllowed = value; }
		}


		private IComparable maximumAllowed = null;

		/// <summary>Represents the maximum Value that this column is allowed.</summary>
		/// <remarks>
		/// 	<para>If a column has a Value higher than the set Value, then a <see cref="ValidationException" /> will be thrown.</para>
		/// 	<para>If there is no maximum, then this should be null.</para>
		/// </remarks>
		IComparable IRangeValidatedColumn.MaximumAllowed
		{
			get { return maximumAllowed; }
			set { maximumAllowed = value; }
		}



		/// <summary>
		/// Creates a new number column
		/// </summary>
		/// <param name="columnName"></param>
		/// <param name="type"></param>
		public NumberColumn(string columnName, Type type)
			: this(columnName, type, null, null)
		{
		}

		/// <summary>
		/// Creates a new number column with maximum and minimum allowed values specified
		/// </summary>
		/// <param name="columnName"></param>
		/// <param name="type"></param>
		/// <param name="minimumAllowed"></param>
		/// <param name="maximumAllowed"></param>
		public NumberColumn(string columnName, Type type, IComparable minimumAllowed, IComparable maximumAllowed)
			: base(columnName, type, DBColumn.IsNullableType(type))
		{
			this.minimumAllowed = minimumAllowed;
			this.maximumAllowed = maximumAllowed;

			if (!IsNumberOrNullableNumber(type))
			{
				throw new Exception("The type " + type.FullName + " specified for " + columnName + " is not supported in a number column.");
			}

			Type underlyingType = (DBColumn.IsNullableType(type)) ? Nullable.GetUnderlyingType(type) : type;

			if (minimumAllowed != null && !minimumAllowed.GetType().Equals(underlyingType))
			{
				throw new Exception("The type specified for the minimum allowed value of " + columnName + " is " + minimumAllowed.GetType().FullName + ", however it must be the same as the column type, which is " + underlyingType.FullName + ".");
			}

			if (maximumAllowed != null && !maximumAllowed.GetType().Equals(underlyingType))
			{
				throw new Exception("The type specified for the maximum allowed value of " + columnName + " is " + maximumAllowed.GetType().FullName + ", however it must be the same as the column type, which is " + underlyingType.FullName + ".");
			}

		}




		
		/// <summary>
		/// Gets SQL Data Type that corresponds to this column
		/// </summary>
		public override SqlDbType GetCorrespondingSqlDBType()
		{

			Type t = (IsNullableType(this.Type)) ? Nullable.GetUnderlyingType(this.Type) : this.Type;


			if (t.Equals(typeof(int)))
			{
				return SqlDbType.Int;
			}

			if (t.Equals(typeof(short)))
			{
				return SqlDbType.SmallInt;
			}

			if (t.Equals(typeof(long)))
			{
				return SqlDbType.BigInt;
			}

			if (t.Equals(typeof(float)))
			{
				return SqlDbType.Float;
			}

			if (t.Equals(typeof(double)))
			{
				return SqlDbType.Float;
			}

			throw new Exception("No SQL DB Type known for " + t.FullName + " in column " + this.FriendlyName + ".");

		}


		/// <summary>
		/// Returns true if the given type is a number, or a nullable number
		/// </summary>
		public static bool IsNumberOrNullableNumber(Type t)
		{
			if (t.IsClass || t == typeof(string))
			{
				return false;
			}
			return t == typeof(int) || t == typeof(short?) || t == typeof(int?) || t == typeof(long?) || t == typeof(float?) || t == typeof(double?) || t == typeof(short) || t == typeof(int) || t == typeof(long) || t == typeof(float) || t == typeof(double);
		}


	}
}
