using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Shunde.Utilities;

namespace Shunde.Framework
{
	/// <summary>Represents a Database column</summary>
	public sealed class DBColumn
	{

		/// <summary>The C# short Value that represents the database NULL Value</summary>
		/// <remarks>Setting a column's Value to this Value will cause this column to be set to NULL when saving to the database</remarks>
		public const int ShortNullValue = short.MinValue;

		/// <summary>The C# integer Value that represents the database NULL Value</summary>
		/// <remarks>Setting a column's Value to this Value will cause this column to be set to NULL when saving to the database</remarks>
		public const int IntegerNullValue = int.MinValue;

		/// <summary>The C# long Value that represents the database NULL Value</summary>
		/// <remarks>Setting a column's Value to this Value will cause this column to be set to NULL when saving to the database</remarks>
		public const long LongNullValue = long.MinValue;

		/// <summary>The C# DateTime Value that represents the database NULL Value</summary>
		/// <remarks>Setting a column's Value to this Value will cause this column to be set to NULL when saving to the database</remarks>
		public static DateTime DateTimeNullValue = DateTime.MinValue;

		/// <summary>The C# double Value that represents the database NULL Value</summary>
		/// <remarks>Setting a column's Value to this Value will cause this column to be set to NULL when saving to the database</remarks>
		public const double DoubleNullValue = double.MinValue;

		/// <summary>The C# float Value that represents the database NULL Value</summary>
		/// <remarks>Setting a column's Value to this Value will cause this column to be set to NULL when saving to the database</remarks>
		public const float FloatNullValue = float.MinValue;

		/// <summary>The Name of this column</summary>
		public string Name
		{
			get
			{
				return name;
			}
		}
		private string name;

		/// <summary>The Table that this column is a part of</summary>
		public DBTable DBTable
		{
			get { return dbTable; }
			set { dbTable = value; }
		}

		private DBTable dbTable;

		/// <summary>The C# Type of the column</summary>
		public Type Type
		{
			get { return type; }
		}

		private Type type;

		private FieldInfo fieldInfo = null;

		/// <summary>
		/// Gets the field info that corresponds to this column
		/// </summary>
		public FieldInfo FieldInfo
		{
			get
			{
				
				if (fieldInfo == null)
				{
					Type t = this.dbTable.ObjectInfo.DBObjectType;
					fieldInfo = t.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
				}
				if (fieldInfo == null)
				{
					Type t = this.dbTable.ObjectInfo.DBObjectType;
					FieldInfo[] fields = t.GetFields();
					string h = "";
					foreach (FieldInfo fi in fields)
					{
						h += fi.Name + ": " + fi.ToString() + "\n";
					}
					throw new ShundeException("Field info not found for column " + this.name + ".Type: " + t.FullName + "\n\nAll fields:\n" + h);
				}
				return fieldInfo;
			}
		}


		private bool isUnique = false;

		/// <summary>Specifies that this column should be unique in the Table</summary>
		/// <remarks>If IsUnique is set to true, a <see cref="ValidationException" /> is thrown when saving the object if there is another row with the same Value in the database for this column. Unlike in Sql Server, two null values are considered Unique in this context.</remarks>
		public bool IsUnique
		{
			get { return isUnique; }
			set { isUnique = value; }
		}


		private IComparable minAllowed = null;

		/// <summary>Represents the minimum Value that this column is allowed.</summary>
		/// <remarks>
		/// 	<para>If there is no minimum, then this should be null.</para>
		/// 	<para>If a column has a Value lower than the set Value, then a <see cref="ValidationException" /> will be thrown.</para>
		/// </remarks>
		public IComparable MinAllowed
		{
			get { return minAllowed; }
			set { minAllowed = value; }
		}


		private IComparable maxAllowed = null;

		/// <summary>Represents the maximum Value that this column is allowed.</summary>
		/// <remarks>
		/// 	<para>If a column has a Value higher than the set Value, then a <see cref="ValidationException" /> will be thrown.</para>
		/// 	<para>If there is no maximum, then this should be null.</para>
		/// </remarks>
		public IComparable MaxAllowed
		{
			get { return maxAllowed; }
			set { maxAllowed = value; }
		}

		/// <summary>This is the minimum number of characters allowed for a string in this column</summary>
		/// <remarks>
		/// 	<para>For a string Value, this should always be at least 0. For other types, this Value is ignored.</para>
		/// 	<para>If a string column has a Value shorter than the set Value, then a <see cref="ValidationException" /> will be thrown.</para>
		/// </remarks>
		public int MinLength
		{
			get { return minLength;}
			set { minLength = value;}
		}

		private int minLength = 0;

		/// <summary>This is the maximum number of characters allowed for a string in this column</summary>
		/// <remarks>
		/// 	<para>If this is set to a positive Value then the type of column is assumed to be nvarchar. A Value of -1 means that the column is assumed to be an ntext column.</para>
		/// 	<para>If a string column has a Value longer than the set Value and the set Value is positive, then a <see cref="ValidationException" /> will be thrown.</para>
		/// </remarks>
		public int MaxLength
		{
			get { return maxLength; }
			set { maxLength = value; }
		}

		private int maxLength = -1;

		/// <summary>Specifies whether or not this column allows NULL values into the database.</summary>
		/// <remarks>If a this is set to false and the Value being saved is null, then a <see cref="ValidationException" /> will be thrown.</remarks>
		public bool AllowNulls
		{
			get { return allowNulls; }
			set { allowNulls = value; }
		}

		private bool allowNulls = false;
	

		/// <summary>This specifies the index into the SqlDataReader that this column's Value will be in.</summary>
		internal int sdrIndex;

		/// <summary>Specifies that this column is a foreign key to an DBObject.</summary>
		internal bool isDBObjectType = false;

		/// <summary>Any CHECK CONSTRAINTS that aren't covered by other fields in the DBColumn class can be specified manually.</summary>
		/// <example>The className field of the <see cref="DBObject" /> class requires that the Value start with 1 or more characters, followed by a period, followed by one or more characters. This is achieved with the following way:
		/// 	<Code>
		///			DBColumn cnCol = new DBColumn( "className", typeof(string), 1, 100);
		/// 		cnCol.Constraints = "className LIKE '_%._%'";
		/// 	</Code>
		/// </example>
		public string Constraints
		{
			get { return constraints; }
			set { constraints = value; }
		}

		private string constraints = "";

		/// <summary>Creates a new DBColumn with the values specified. Also sets the Value of <see cref="isDBObjectType" />.</summary>
		public DBColumn(string name, Type type, bool allowNulls)
		{
			this.name = name;
			this.type = type;
			this.allowNulls = allowNulls;

			if (type.IsClass)
			{
				if (type.Equals(typeof(DBObject)) || type.IsSubclassOf(typeof(DBObject)))
				{
					isDBObjectType = true;
				}
			}

		}

		/// <summary>Gets the database column Name of this column</summary>
		/// <remarks>This is normally the same as the Name specified in <see cref="Name" />, however for DBObject types it has "Id" appended.</remarks>
		public string GetColumnName()
		{
			if (!isDBObjectType)
			{
				return name;
			}
			return name + "Id";
		}

		/// <summary>Creates a new DBColumn with the values specified. Also sets the Value of <see cref="isDBObjectType" />.</summary>
		/// <remarks>This constructor is normally used for DateTime, double, or int types where a minimum and/or maximum Value is set.</remarks>
		public DBColumn(string name, Type type, bool allowNulls, IComparable minAllowed, IComparable maxAllowed)
			: this(name, type, allowNulls)
		{
			this.minAllowed = minAllowed;
			this.maxAllowed = maxAllowed;
		}

		/// <summary>Creates a new DBColumn with the values specified. Also sets the Value of <see cref="isDBObjectType" />.</summary>
		/// <remarks>This constructor is normally used for string types where a minimum and/or maximum string length is needed. It sets <see cref="allowNulls" /> to false.</remarks>
		public DBColumn(string name, Type type, int minLength, int maxLength)
			: this(name, type, false)
		{
			this.minLength = minLength;
			this.maxLength = maxLength;
			this.allowNulls = (minLength == 0);
		}

		/// <summary>Gets the Value of this object in a suitable manner for use in SQL statements</summary>
		/// <remarks>string values will have apostrophes escaped, and be enclosed by apostrophes. Varchar columns will also have the <see cref="string.Trim()">string.Trim</see> method called on them. Null values will be returned as "null", and all other types are converted to an appropriate string representation.</remarks>
		public string GetSqlText(Object value)
		{
			if (IsColumnNull(value))
			{
				return "null";
			}

			if ( type.Equals(typeof(int)) || type.Equals(typeof(short)) || type.Equals(typeof(long)) || type.Equals(typeof(float)) || type.Equals(typeof(double)))
			{
				return value.ToString();
			}

			if (type.Equals(typeof(string)))
			{
				string tempVal = value.ToString().Replace("'", "''");
				if (maxLength > 0)
				{
					return "N'" + tempVal.Trim() + "'";
				}
				else
				{
					return "N'" + tempVal + "'";
				}
			}

			if (type.Equals(typeof(bool)))
			{
				return ((bool)value) ? "1" : "0";
			}

			if (type.Equals(typeof(DateTime)))
			{
				return "'" + ((DateTime)value).ToString("yyyy/MM/dd HH:mm:ss.fff") + "'";
			}

			if (type.IsSubclassOf(typeof(DBObject)) || type.Equals(typeof(DBObject)))
			{
				return ((DBObject)value).Id.ToString();
			}

			if (type.Equals(typeof(BinaryData)))
			{
				return "@" + name;
			}

			return value.ToString();

		}

		/// <summary>Determines whether the given object should be considered to be null by the database.</summary>
		/// <remarks>This is true if the Value is null, but also if an int has the Value <see cref="IntegerNullValue" /> etc.</remarks>
		public static bool IsColumnNull(Object value)
		{
			if (value is BinaryData)
			{
				return !((BinaryData)value).Exists;
			}
			else
			{
				return (value == null || value.Equals(DBNull.Value) || value.Equals(DBColumn.ShortNullValue) || value.Equals(DBColumn.IntegerNullValue) || value.Equals(DBColumn.LongNullValue) || value.Equals(DBColumn.DateTimeNullValue) || value.Equals(DBColumn.FloatNullValue) || value.Equals(DBColumn.DoubleNullValue) || value.Equals(""));
			}
		}

		/// <summary>Checks that the given Value is within the constraints placed upon it by this column.</summary>
		/// <remarks>This does not check the specific constraints specified in the <see cref="constraints" /> field. A Value violating those constraints will be found when attempting to save the object.</remarks>
		/// <exception cref="ValidationException">Thrown if the Value violates the constraints of this column. The Message property contains a friendly error message, suitable to show to end users, on why the validation failed.</exception>
		public void Validate(DBObject obj, Object value)
		{

			string friendlyName = TextUtils.MakeFriendly(name);

			bool isNull = IsColumnNull(value);

			if (isNull)
			{
				if (!allowNulls)
				{
					throw new ValidationException("You must specify a value for \"" + friendlyName + "\".");
				}
			}
			else
			{

				if (isUnique && !IsUniqueInDB(obj, value))
				{
					throw new ValidationException("The value " + value + " you specified for \"" + friendlyName + "\" already exists in our system. You must specify a unique value for " + friendlyName + ".");
				}

				if (MinAllowed != null || MaxAllowed != null)
				{
					IComparable compVal = (IComparable)value;

					if (MinAllowed != null)
					{
						if (compVal.CompareTo(MinAllowed) < 0)
						{
							throw new ValidationException("The smallest value allowed for \"" + friendlyName + "\" is " + MinAllowed + ".");
						}
					}

					if (MaxAllowed != null)
					{
						if (compVal.CompareTo(MaxAllowed) > 0)
						{
							throw new ValidationException("The largest value allowed for \"" + friendlyName + "\" is " + MaxAllowed + ".");
						}
					}

				}

				if (maxLength > 0)
				{
					int len = value.ToString().Length;
					if (len > maxLength)
					{
						throw new ValidationException("The maximum length allowed for \"" + friendlyName + "\" is " + maxLength + " characters. You have written " + len + " characters.");
					}
				}

				if (minLength > 0)
				{
					int len = value.ToString().Length;
					if (len < minLength)
					{
						throw new ValidationException("The minimum length allowed for \"" + friendlyName + "\" is " + minLength + " character" + ((minLength == 1) ? "" : "s") + ". You have written " + len + " characters.");
					}
				}
			}

		}



		/// <summary>Checks whether this column for the given object holds a unique Value in the database</summary>
		/// <remarks>This check is not made if this is a deleted object. Uniqueness only holds among non deleted objects.</remarks>
		private bool IsUniqueInDB(DBObject obj, Object value)
		{
			if (obj.IsDeleted)
			{
				return true;
			}
			string sql = "SELECT 1 FROM [" + dbTable.Name + "] INNER JOIN DBObject ON DBObject.[ID] = [" + dbTable.Name + "].[ID] WHERE [" + dbTable.Name + "]." + GetColumnName() + " = " + GetSqlText(value) + " AND [" + dbTable.Name + "].id <> " + obj.Id + " AND DBObject.isDeleted = 0";
			return !DBUtils.HasRows(sql);
		}

	}
}
