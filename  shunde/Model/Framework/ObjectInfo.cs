using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Shunde.Framework
{

	/// <summary>Information for a class. This contains the <see cref="DBTable" />s that make up this object, as well as the from clause and column clause to use in SQL statements, etc.</summary>
	public sealed class ObjectInfo
	{

		/// <summary>The text to appear in an SQL FROM clause, i.e. the joined tables that make up this object.</summary>		
		private string fromClause;


		/// <summary>The column names that are in the SELECT query</summary>
		private string columnClause;

		/// <summary>The base Table Name</summary>
		private string baseTableName = "DBObject";

		/// <summary>The tables that make up this object</summary>
		public DBTable[] Tables
		{
			get { return tables; }
		}

		private DBTable[] tables;


		/// <summary>The Type of the object that this ObjectInfo represents</summary>
		public Type DBObjectType {
			get { return type; }
		}
		private Type type;


		/// <summary>Hash Table containing all the <see cref="ObjectInfo">ObjectInfo</see> objects</summary>
		/// <remarks>Each Class, as it is loaded, has it's ObjectInfo placed into this HashTable - that is object per class. Each DBObject then gets a reference to the ObjectInfo of the class it belongs to, for easy access.</remarks>
		internal static Dictionary<string, ObjectInfo> objectInfoHT = new Dictionary<string, ObjectInfo>();

		/// <summary>Hash Table of constructors</summary>
		/// <remarks>Creating DBObjects is a common occurance in the Shunde Framework. For this reason, the <see cref="ConstructorInfo">ConstructorInfo</see> object for each class is saved in this Hashtable automatically, as classes are loaded.</remarks>
		internal static Dictionary<string, ConstructorInfo> constructors = new Dictionary<string, ConstructorInfo>();



		/// <summary>Creates a new ObjectInfo class and populates the fromClause.</summary>
		public ObjectInfo(Type type, DBTable table)
		{
			this.type = type;

			if (!type.Equals(typeof(DBObject)))
			{
				Type baseType = type.BaseType;
				ObjectInfo oi = ObjectInfo.GetObjectInfo(baseType);

				if (oi == null)
				{

					baseType.TypeInitializer.Invoke(null, null);

					oi = ObjectInfo.GetObjectInfo(baseType);

					if (oi == null)
					{
						throw new ShundeException("ObjectInfo was null for type " + type + " , table " + table);
					}

				}

				tables = new DBTable[oi.tables.Length + 1];
				oi.tables.CopyTo(tables, 0);
				tables[oi.tables.Length] = table;

			}
			else
			{
				tables = new DBTable[] { table };
			}

			GetDirectTable().ObjectInfo = this;

			SetupObjectInfo(this);

		}




		/// <summary>Creates a SELECT statement for this object with no WHERE clause</summary>
		public string GetSelectStatement()
		{
			return GetSelectStatement(-1);
		}

		/// <summary>Creates a SELECT statement for this object with no WHERE clause selecting the TOP numberToGet records, or unlimited if -1</summary>
		public string GetSelectStatement(int numberToGet)
		{
			string top = (numberToGet < 0) ? " " : " TOP " + numberToGet + " ";
			return "SELECT " + top + columnClause + " FROM " + fromClause + " ";
		}



		/// <summary>Returns the columns of this object LEFT joined onto the specified object types</summary>
		public string GetJoinedColumnClause(Type[] types)
		{
			string cc = columnClause;

			//int uniqueNum = 1000;

			foreach (Type type in types)
			{
				ObjectInfo oi = ObjectInfo.GetObjectInfo(type);
				DBTable tab = oi.GetDirectTable();
				string joinTableName = tab.Name;
				foreach (DBColumn col in tab.Columns)
				{

					if (!col.Type.Equals(typeof(BinaryData)))
					{
						cc += ", [" + joinTableName + "].[" + col.GetColumnName() + "]";
					}

					// this is a reference to another object, so here we select the class Name of the object
					if (col.isDBObjectType)
					{
						cc += ", [" + joinTableName + "].[" + col.Name + "ClassName]";
					}
					else if (col.Type.Equals(typeof(BinaryData)))
					{
						// rather than getting the data with each population, we just get the size of the data
						// this is to reduce network bandwidth.
						// A call to DBObject.populatBinaryData() will truly populate the data
						columnClause += ", DATALENGTH([" + joinTableName + "].[" + col.GetColumnName() + "]) AS [" + col.GetColumnName() + "]";
						columnClause += ", [" + joinTableName + "].[" + col.Name + "MimeType]";
						columnClause += ", [" + joinTableName + "].[" + col.Name + "Filename]";
					}


				}
			}

			return cc;
		}

		/// <summary>Returns a FROM clause that is LEFT joined onto the specified object types</summary>
		public string GetJoinedFromClause(Type[] types)
		{

			string fc = fromClause;

			foreach (Type type in types)
			{
				ObjectInfo oi = ObjectInfo.GetObjectInfo(type);
				DBTable tab = oi.GetDirectTable();
				string joinTableName = tab.Name;
				fc = "(" + fc + " LEFT JOIN " + tab.Name + " " + joinTableName + " ON " + joinTableName + ".id = " + baseTableName + ".id) ";
			}

			return fc;

		}


		/// <summary>Sets up an object info object</summary>
		/// <remarks>Creates the FROM and COLUMN clauses. This is called by the constructor.</remarks>
		private static void SetupObjectInfo(ObjectInfo oi)
		{

			SetupFromClause(oi);
			SetupColumnClause(oi);

		}


		/// <summary>Gets a comma separated list of the column names that make up this object</summary>
		private static void SetupColumnClause(ObjectInfo oi)
		{

			DBTable[] dbTables = oi.tables;

			int numOODBObjs = 0;


			string columnClause = "[DBObject].[id]";

			int counter = 1;
			foreach( DBTable table in dbTables )
			{
				
				foreach (DBColumn col in table.Columns)
				{
					
					if (!(col.Type.Equals(typeof(BinaryData))))
					{
						// we handle binary data differently (see below)
						columnClause += ", [" + table.Name + "].[" + col.GetColumnName() + "]";
					}
					col.sdrIndex = counter;
					counter++;

					// this is a reference to another object, so here we select the class Name of the object
					if (col.isDBObjectType)
					{
						columnClause += ", [" + table.Name + "].[" + col.Name + "ClassName]";
						counter++;
						numOODBObjs++;
					}
					else if (col.Type.Equals(typeof(BinaryData)))
					{
						// rather than getting the data with each population, we just get the size of the data
						// this is to reduce network bandwidth.
						// A call to DBObject.populatBinaryData() will truly populate the data
						columnClause += ", DATALENGTH([" + table.Name + "].[" + col.GetColumnName() + "]) AS [" + col.GetColumnName() + "]";
						columnClause += ", [" + table.Name + "].[" + col.Name + "MimeType]";
						columnClause += ", [" + table.Name + "].[" + col.Name + "Filename]";
						counter += 2;
					}

				}

			}
			oi.columnClause = columnClause;
		}

		/// <summary>Gets the Table that this object added - eg. For a Product it would be the Product Table</summary>
		internal DBTable GetDirectTable()
		{
			return tables[tables.Length - 1];
		}


		/// <summary>Gets a comma separated list of the Table names that make up this object</summary>
		private static void SetupFromClause(ObjectInfo oi)
		{

			DBTable[] dbTables = oi.tables;
			string fromClause = dbTables[0].Name;
			string lastTableName = fromClause;

			
			for (int i = 1; i < dbTables.Length; i++) // skip the first Table, which is the oodbobject Table
			{
				DBTable table = dbTables[i];
				fromClause = " (" + fromClause + " INNER JOIN [dbo].[" + table.Name + "] ON [" + table.Name + "].[id] = [" + lastTableName + "].[id]) ";
				lastTableName = table.Name;
			}

			// now add any joins for foreign keys
			/*
			int numOODBObjs = 0;
			for (int i = 1; i < dbTables.Length; i++) {
				DBTable Table = dbTables[i];
				for (int j = 0; j < Table.columns.Length; j++) {
					DBColumn col = Table.columns[j];
					
					if (false && col.isDBObjectType) {
						string joinType = (col.allowNulls) ? "LEFT" : "INNER";
						
						fromClause = "(" + fromClause + " " + joinType + " JOIN DBObject DBObject" + numOODBObjs + " ON DBObject" + numOODBObjs + ".id = " + Table.Name + "." + col.getColumnName() + ") ";
						
						numOODBObjs++;
						
					}
					
				}
			}*/

			oi.fromClause = fromClause;
		}






		/// <summary>Registers an <see cref="ObjectInfo">ObjectInfo</see> into the hash Table</summary>
		public static void RegisterObjectInfo(Type type, DBTable table)
		{
			objectInfoHT[type.FullName] = new ObjectInfo(type, table);
		}

		/// <summary>Registers an <see cref="ObjectInfo" /> into the hash Table</summary>
		public static void RegisterObjectInfo(ObjectInfo oi)
		{
			objectInfoHT[oi.type.FullName] = oi;
		}

		/// <summary>Gets an ObjectInfo for the given Type</summary>
		public static ObjectInfo GetObjectInfo(Type type)
		{

			try
			{
				return objectInfoHT[type.FullName];
			}
			catch (KeyNotFoundException) { }

			type.TypeInitializer.Invoke(null, null);
			return objectInfoHT[type.FullName];

		}


	}

}
