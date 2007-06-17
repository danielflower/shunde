using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Shunde.Utilities;
using System.Data;

namespace Shunde.Framework.Columns
{
	/// <summary>Represents a Database column</summary>
	public abstract class DBColumn
	{


		/// <summary>The Name of this column</summary>
		public string Name
		{
			get
			{
				return name;
			}
		}
		private string name;

		/// <summary>
		/// Gets the human-readable name of this column
		/// </summary>
		public string FriendlyName
		{
			get
			{
				return TextUtils.MakeFriendly(this.name);
			}
		}

		/// <summary>The Table that this column is a part of</summary>
		public DBTable DBTable
		{
			get { return dbTable; }
			set { dbTable = value; }
		}

		private DBTable dbTable;

		/// <summary>The .NET Type of the column</summary>
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
				if (this.fieldInfo == null)
				{
					fieldInfo = GetDBObjectField(this.dbTable.ObjectInfo.DBObjectType, this.name);
				}
				return fieldInfo;
			}
		}

		/// <summary>
		/// Gets the field with the given name for the given type, and throws a ShundeException if not found
		/// </summary>
		public static FieldInfo GetDBObjectField(Type type, string fieldName)
		{
			FieldInfo fi = null;
			Type t = type;
			while (fi == null && t != null)
			{
				fi = t.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
				if (fi != null) {
					return fi;
				}
				t = t.BaseType;
			}

			throw new ShundeException("Field " + fieldName + " not found for type " + type.FullName);

		}


		private bool isUnique = false;

		/// <summary>Specifies that this column should be unique in the Table</summary>
		/// <remarks>If IsUnique is set to true, a <see cref="ValidationException" /> is thrown when saving the object if there is another row with the same Value in the database for this column. Unlike in Sql Server, two null values are considered Unique in this context.</remarks>
		public bool IsUnique
		{
			get { return isUnique; }
			set { isUnique = value; }
		}




		/// <summary>Specifies whether or not this column allows NULL values into the database.</summary>
		/// <remarks>If a this is set to false and the Value being saved is null, then a <see cref="ValidationException" /> will be thrown.</remarks>
		public virtual bool AllowNulls
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
		///			SingleLineColumn cnCol = new SingleLineColumn( "className", 1, 100);
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




		/// <summary>Gets the Value of this object in a suitable manner for use in SQL statements</summary>
		/// <remarks>string values will have apostrophes escaped, and be enclosed by apostrophes. Varchar columns will also have the <see cref="string.Trim()">string.Trim</see> method called on them. Null values will be returned as "null", and all other types are converted to an appropriate string representation.</remarks>
		public virtual string GetSqlText(object value)
		{
			if (IsNull(value))
			{
				return "null";
			}

			if ( type.Equals(typeof(int)) || type.Equals(typeof(short)) || type.Equals(typeof(long)) || type.Equals(typeof(float)) || type.Equals(typeof(double)))
			{
				return value.ToString();
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


		/// <summary>
		/// Gets the SqlDBType which corresponds to this column type
		/// </summary>
		public abstract SqlDbType GetCorrespondingSqlDBType();


		/// <summary>Determines whether the given string is considered to be null.</summary>
		/// <remarks>This is true if value equals <see cref="string.Empty" />.</remarks>
		public virtual bool IsNull(object value)
		{
			return value == null || value.Equals(DBNull.Value);
		}


		/// <summary>Checks that the given Value is within the constraints placed upon it by this column.</summary>
		/// <remarks>This does not check the specific constraints specified in the <see cref="constraints" /> field. A Value violating those constraints will be found when attempting to save the object.</remarks>
		/// <exception cref="ValidationException">Thrown if the Value violates the constraints of this column. The Message property contains a friendly error message, suitable to show to end users, on why the validation failed.</exception>
		public virtual void Validate(DBObject obj, object value)
		{

			string friendlyName = TextUtils.MakeFriendly(name);

			bool isNull = IsNull(value);

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

				IRangeValidatedColumn rangeCol = this as IRangeValidatedColumn;
				if (rangeCol != null)
				{
					IComparable minimumAllowed = rangeCol.MinimumAllowed;
					IComparable maximumAllowed = rangeCol.MaximumAllowed;
					if (minimumAllowed != null || maximumAllowed != null)
					{
						IComparable compVal = (IComparable)value;

						if (minimumAllowed != null)
						{
							if (compVal.CompareTo(minimumAllowed) < 0)
							{
								throw new ValidationException("The smallest value allowed for \"" + FriendlyName + "\" is " + minimumAllowed + ".");
							}
						}

						if (maximumAllowed != null)
						{
							if (compVal.CompareTo(maximumAllowed) > 0)
							{
								throw new ValidationException("The largest value allowed for \"" + FriendlyName + "\" is " + maximumAllowed + ".");
							}
						}

					}
				}


			}

		}

		/// <summary>
		/// Returns true if the specified type is nullable
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsNullableType(Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
		}

		/// <summary>Checks whether this column for the given object holds a unique Value in the database</summary>
		/// <remarks>This check is not made if this is a deleted object. Uniqueness only holds among non deleted objects.</remarks>
		private bool IsUniqueInDB(DBObject obj, object value)
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
