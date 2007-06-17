using System;
using System.Collections.Generic;
using System.Text;

namespace Shunde.Framework.Columns
{

	/// <summary>
	/// A column which holds binary data
	/// </summary>
	public class BinaryDataColumn : DBColumn
	{

		private int minimumFileSize = 0;

		/// <summary>
		/// The minimum file size, in bytes
		/// </summary>
		public int MinimumFileSize
		{
			get { return minimumFileSize; }
			set { minimumFileSize = value; }
		}

		private int maximumFileSize = int.MaxValue;

		/// <summary>
		/// The maximum file size allowed for this column
		/// </summary>
		public int MaximumFileSize
		{
			get { return maximumFileSize; }
			set { maximumFileSize = value; }
		}
	
	

		/// <summary>
		/// Creates a new binary data column
		/// </summary>
		/// <param name="columnName">The name of the column</param>
		/// <param name="allowNulls">Specifies whether or not nulls are allowed</param>
		public BinaryDataColumn(string columnName, bool allowNulls)
			: base(columnName, typeof(BinaryData), allowNulls)
		{

		}

		/// <summary>
		/// Throws an exception, as binary data cannot be converted to a string
		/// </summary>
		public override string GetSqlText(object value)
		{
			throw new Exception("Binary data cannot be converted to a string");
		}

		/// <summary>
		/// Gets SQL Data Type that corresponds to this column
		/// </summary>
		public override System.Data.SqlDbType GetCorrespondingSqlDBType()
		{
			return System.Data.SqlDbType.Image;
		}


		/// <summary>
		/// Validates the object
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="value"></param>
		public override void Validate(DBObject obj, object value)
		{
			base.Validate(obj, value);

			if (value != null)
			{
				BinaryData bd = (BinaryData)value;
				if (bd.Size < minimumFileSize)
				{
					throw new ValidationException("The minimum size allowed for " + this.Name + " is " + this.minimumFileSize + " bytes.  The uploaded file is " + bd.Size + " bytes.");
				}
				if (bd.Size > maximumFileSize)
				{
					throw new ValidationException("The maximum size allowed for " + this.Name + " is " + this.maximumFileSize + " bytes.  The uploaded file is " + bd.Size + " bytes.");
				}
			}

		}

	}
}
