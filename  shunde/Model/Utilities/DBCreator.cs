using System;
using System.Collections.Generic;
using System.Text;
using Shunde.Framework;
using Shunde.Utilities;

namespace Shunde.Utilities
{
	/// <summary>A class for creating database tables for <see cref="DBObject" />s</summary>
	/// <remarks>This class creates SQL Code to create database tables and views, along with foreign key constraints etc, by using the information in the <see cref="ObjectInfo" /> class for a given type.</remarks>
	public class DBCreation
	{


		/// <summary>Creates constraints for a Table</summary>
		/// <remarks>Does not create any constraints for DBObjects</remarks>
		public static string GetForeignKeyConstraints(Type type)
		{

			if (type.Equals(typeof(DBObject)))
			{
				return "";
			}

			ObjectInfo oi = ObjectInfo.GetObjectInfo(type);

			DBTable table = oi.GetDirectTable();

			string fks = "ALTER TABLE [dbo].[" + table.Name + "] WITH CHECK ADD\n";
			fks += GetForeignKeyText(table.Name, "id", ObjectInfo.GetObjectInfo(type.BaseType).GetDirectTable().Name);

			foreach (DBColumn col in table.Columns)
			{
				if (col.isDBObjectType)
				{
					string colName = col.GetColumnName();
					string ftName = ObjectInfo.GetObjectInfo(col.Type).GetDirectTable().Name;
					fks += "\t,\n" + GetForeignKeyText(table.Name, colName, ftName);
				}
			}

			string cs = "";
			if (table.UniqueIndexColumns.Length > 0)
			{
				cs += "\n\nALTER TABLE [dbo].[" + table.Name + "] WITH CHECK ADD CONSTRAINT IX_" + TextUtils.RemoveNonAlphaNumeric(table.Name) + "_" + table.UniqueIndexColumns.Replace(",", "_") + "\n";
				cs += "\tUNIQUE NONCLUSTERED (" + table.UniqueIndexColumns + ")\n";
			}


			return fks + cs + "\nGO\n\n";
		}

		/// <summary>Creates part of a FOREIGN KEY constraint</summary>
		private static string GetForeignKeyText(string tableName, string columnName, string foreignTable)
		{
			string fk = "\tCONSTRAINT [FK_" + TextUtils.RemoveNonAlphaNumeric(tableName) + "_" + TextUtils.RemoveNonAlphaNumeric(columnName) + "] FOREIGN KEY (\n";
			fk += "\t\t[" + columnName + "]\n";
			fk += "\t) REFERENCES [dbo].[" + foreignTable + "] (\n";
			fk += "\t\t[id]\n";
			fk += "\t)\n";
			return fk;
		}

		/// <summary>Creates SQL Code to create an indexed view</summary>
		public static string GetCreateView(Type type)
		{

			string t = "";

			ObjectInfo oi = ObjectInfo.GetObjectInfo(type);

			if (!oi.UseView)
			{
				return t;
			}

			String viewName = oi.ViewName;

			oi.UseView = false;
			ObjectInfo.SetupObjectInfo(oi);


			t = "SET ANSI_NULLS ON\nSET ANSI_PADDING ON\nSET ANSI_WARNINGS ON\nSET ARITHABORT ON\nSET CONCAT_NULL_YIELDS_NULL ON\nSET QUOTED_IDENTIFIER ON\nGO\n\n";

			t += "CREATE VIEW [dbo].[" + viewName + "] WITH SCHEMABINDING AS\n" + oi.GetSelectStatement() + "\nGO\n\n";

			t += "CREATE UNIQUE CLUSTERED INDEX IX_" + TextUtils.RemoveNonAlphaNumeric(viewName) + "_id ON [dbo].[" + viewName + "](id)\nGO\n\n";

			oi.UseView = true;
			ObjectInfo.SetupObjectInfo(oi);

			return t;

		}

		/// <summary>Creates SQL Code to create a Table for the given type</summary>
		/// <remarks>Pass a list of already-created Table names into checker to make sure that no duplicate tables are being created</remarks>
		public static string GetCreateTable(Type type, List<string> checker, bool tableAlreadyExists)
		{

			ObjectInfo oi = ObjectInfo.GetObjectInfo(type);
			return GetCreateTable(type, oi.GetDirectTable(), checker, tableAlreadyExists);

		}



		/// <summary>Creates the SQL Code for a single DBTable</summary>
		private static string GetCreateTable(Type type, DBTable table, List<string> checker, bool tableAlreadyExists)
		{


			if (checker.Contains(table.Name))
			{
				int first = checker.IndexOf(table.Name) + 1;
				int second = checker.Count + 1;
				throw new ValidationException("The table " + table.Name + " is a duplicate (the duplicate tables are the " + first + TextUtils.GetOrdinalSuffix(first) + " and " + second + TextUtils.GetOrdinalSuffix(second) + " tables that you selected)");
			}
			checker.Add(table.Name);


			string t;

			if (tableAlreadyExists)
			{
				t = "ALTER TABLE [dbo].[" + table.Name + "] ADD";
			}
			else
			{
				t = "CREATE TABLE [dbo].[" + table.Name + "] (";
				t += "\n\t[id] INT PRIMARY KEY CLUSTERED,";
			}

			// used to check for duplicate column names
			List<string> columnNames = new List<string>();

			int columnCounter = 0;
			foreach (DBColumn col in table.Columns)
			{
				
				if (columnNames.Contains(col.Name))
				{
					throw new ValidationException("The table " + table.Name + " contains a duplicate column (" + col.Name + ")");
				}


				{ // check to see if this column is part of the class
					System.Reflection.FieldInfo fi = col.FieldInfo;
					if (fi == null)
					{
						throw new ValidationException("The column " + col.Name + " could not be found in the class " + type.FullName);
					}
				}


				columnNames.Add(col.Name);

				if (columnCounter > 0)
				{
					t += ",";
				}

				t += "\n\t[" + col.GetColumnName() + "] " + GetColumnText(col, table.Name);

				if (col.isDBObjectType)
				{
					string colName = col.Name + "ClassName";
					string nullConstraint = (col.AllowNulls) ? "NULL" : "NOT NULL";
					t += ",\n\t[" + colName + "] varchar(100) " + nullConstraint + " CONSTRAINT CK_" + table.Name + "_" + colName + " CHECK ( " + colName + " LIKE '_%._%' )";
				}
				else if (col.Type.Equals(typeof(BinaryData)))
				{
					String colName = col.Name + "MimeType";
					String nullConstraint = (col.AllowNulls) ? "NULL" : "NOT NULL";
					t += ",\n\t[" + colName + "] varchar(50) " + nullConstraint;

					colName = col.Name + "Filename";
					nullConstraint = (col.AllowNulls) ? "NULL" : "NOT NULL";
					t += ",\n\t[" + colName + "] varchar(128) " + nullConstraint;
				}

				columnCounter++;

			}

			if (!tableAlreadyExists)
			{
				t += "\n)";
			}
			t += "\nGO\n\n";

			return t;
		}

		/// <summary>Gets the SQL Code to create a single column in a CREATE TABLE statement</summary>
		private static string GetColumnText(DBColumn col, string tableName)
		{
			string t = GetSqlServerType(col);

			string nullVal = (col.AllowNulls) ? "NULL" : "NOT NULL";

			// only specify unique in the db is nulls are not allowed. This is because in in Sql Server, two nulls are considered equal when comparing for unique. We want unique to mean "Unique unless null"
			string uniqueVal = (col.IsUnique && !col.AllowNulls) ? " UNIQUE NONCLUSTERED " : "";

			t += " " + nullVal + uniqueVal;

			string cks = "";
			if (col.MinLength > 0)
			{
				cks = " LEN([" + col.Name + "]) >= " + col.MinLength;
			}


			if (col.MinAllowed != null)
			{
				if (cks.Length > 0)
				{
					cks += " AND";
				}
				cks += " [" + col.Name + "] >= " + col.GetSqlText(col.MinAllowed);
			}

			if (col.MaxAllowed != null)
			{
				if (cks.Length > 0)
				{
					cks += " AND";
				}
				cks += " [" + col.Name + "] <= " + col.GetSqlText(col.MaxAllowed);
			}

			if (col.Constraints.Length > 0)
			{
				if (cks.Length > 0)
				{
					cks += " AND";
				}
				cks += " " + col.Constraints;
			}

			if (cks.Length > 0)
			{
				cks = " CONSTRAINT CK_" + tableName + "_" + col.Name + " CHECK (" + cks + ")";
			}

			return t + cks;
		}

		/// <summary>Gets the SQL Server type equivalent to the C# type that the given column has.</summary>
		private static string GetSqlServerType(DBColumn col)
		{

			Type t = col.Type;

			if (t.Equals(typeof(string)))
			{
				if (col.MaxLength < 0)
				{
					return "NTEXT";
				}
				else
				{
					return "NVARCHAR(" + col.MaxLength + ")";
				}
			}

			if (t.Equals(typeof(int)))
			{
				return "INT";
			}

			if (t.Equals(typeof(short)))
			{
				return "SMALLINT";
			}

			if (t.Equals(typeof(long)))
			{
				return "BIGINT";
			}

			if (t.Equals(typeof(float)))
			{
				return "FLOAT(24)";
			}

			if (t.Equals(typeof(double)))
			{
				return "FLOAT(53)";
			}

			if (t.Equals(typeof(bool)))
			{
				return "BIT";
			}

			if (col.isDBObjectType)
			{
				return "INT";
			}

			if (t.Equals(typeof(BinaryData)))
			{
				return "IMAGE";
			}

			return t.Name;

		}

	}

}
