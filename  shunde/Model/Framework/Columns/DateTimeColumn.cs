using System;
using System.Collections.Generic;
using System.Text;

namespace Shunde.Framework.Columns
{

	/// <summary>
	/// A column holding a date-time object
	/// </summary>
	public class DateTimeColumn : DBColumn
	{

		private const int MillisecondsToRepresentNullTime = 100;

		/// <summary>
		/// Represents a DateTime which hasn't been set
		/// </summary>
		public static readonly DateTime NotSetValue = MakeDateNull(MakeTimeNull(new DateTime()));

		private DateTimePart part;

		/// <summary>
		/// The part of the date and/or time which is being saved
		/// </summary>
		public DateTimePart Part
		{
			get { return part; }
			set { part = value; }
		}
	

		/// <summary>
		/// Creates a new DateTime column
		/// </summary>
		/// <param name="columnName">The name of the column in the database</param>
		/// <param name="allowNulls">This should be true if the type is Nullable (i.e. a DateTime? type), otherwise false</param>
		/// <param name="part">The part of the date to be saving</param>
		public DateTimeColumn(string columnName, bool allowNulls, DateTimePart part)
			: base(columnName, (allowNulls ? typeof(DateTime?) : typeof(DateTime)), allowNulls)
		{
			this.part = part;
		}

		/// <summary>
		/// Converts the given DateTime to a string which can be put into SQL
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override string GetSqlText(object value)
		{
			return "'" + ((DateTime)value).ToString("yyyy/MM/dd HH:mm:ss.fff") + "'";
		}

		/// <summary>
		/// Validates this object
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="value"></param>
		public override void Validate(DBObject obj, object value)
		{
			base.Validate(obj, value);

			if (!IsNull(value))
			{
				DateTime dt = (AllowNulls) ? ((DateTime?)value).Value : (DateTime)value;
				if ((this.part == DateTimePart.DateAndTime || this.part == DateTimePart.Time) && TimeIsNull(dt))
				{
					throw new ValidationException("Please specify a time for " + FriendlyName);
				}
				if ((this.part == DateTimePart.DateAndTime || this.part == DateTimePart.Date) && DateIsNull(dt))
				{
					throw new ValidationException("Please specify a date for " + FriendlyName);
				}
			}
		}

		/// <summary>
		/// Gets SQL Data Type that corresponds to this column
		/// </summary>
		public override System.Data.SqlDbType GetCorrespondingSqlDBType()
		{
			return System.Data.SqlDbType.DateTime;
		}



		#region Static Utility Methods

		/// <summary>
		/// Returns true if the given DateTime object is considered to have its time not set
		/// </summary>
		/// <remarks>A date is considered to have a "null time" if it's time component is 100 milliseconds past midnight</remarks>
		public static bool TimeIsNull(DateTime date)
		{
			return date.Hour == 0 && date.Minute == 0 && date.Second == 0 && date.Millisecond == MillisecondsToRepresentNullTime;
		}

		/// <summary>
		/// Converts the given DateTime object to a DateTime which does not have its time set
		/// </summary>
		public static DateTime MakeTimeNull(DateTime date)
		{
			return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, MillisecondsToRepresentNullTime);
		}

		/// <summary>
		/// Returns true if the given DateTime object is considered to have its date not set
		/// </summary>
		/// <remarks>A date is considered to have a "null date" if it's date component is 1/1/1</remarks>
		public static bool DateIsNull(DateTime date)
		{
			return date.Year == 1 && date.Month == 1 && date.Day == 1;
		}

		/// <summary>
		/// Converts the given DateTime object to a DateTime which does not have its date set
		/// </summary>
		public static DateTime MakeDateNull(DateTime date)
		{
			return new DateTime(1, 1, 1, date.Hour, date.Minute, date.Second, date.Millisecond);
		}

		#endregion

	}


	/// <summary>
	/// Specifies what part of the date or time is of interest
	/// </summary>
	public enum DateTimePart
	{
		/// <summary>
		/// Only the time is being saved (the date will not be entered by the user)
		/// </summary>
		Time,

		/// <summary>
		/// Only the date is being saved (the time will not be entered by the user)
		/// </summary>
		Date,

		/// <summary>
		/// Both the date and time are to be saved
		/// </summary>
		DateAndTime,

		/// <summary>
		/// The date, and optionally the time, should be saved
		/// </summary>
		DateAndOptionallyTime
	}



}
