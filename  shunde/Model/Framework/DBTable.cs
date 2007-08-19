using System;
using System.Collections.Generic;
using System.Text;
using Shunde.Framework.Columns;

namespace Shunde.Framework
{

	/// <summary>Represents a single Database Table</summary>
	public sealed class DBTable
	{

		private string name;

		/// <summary>The Name of the Table</summary>
		public string Name {
			get
			{
				return name;
			}
		}

		/// <summary>The columns that make up the Table</summary>
		public DBColumn[] Columns
		{
			get
			{
				return columns;
			}
		}
		
		private DBColumn[] columns;


		private ObjectInfo objectInfo;

		/// <summary>
		/// The object info corresponding to this Table
		/// </summary>
		public ObjectInfo ObjectInfo
		{
			get { return objectInfo; }
			set { objectInfo = value; }
		}


		/// <summary>A comma separated list of columns that, held together, need to be unique</summary>
		/// <remarks>The comma separated list must have no spaces between the column names. Leave it as an empty string to have no unique index created.</remarks>
		public string UniqueIndexColumns
		{
			get { return uniqueIndexColumns; }
			set { uniqueIndexColumns = value; }
		}

		private string uniqueIndexColumns = "";

		/// <summary>Creates a new DBTable with the specified Name and columns</summary>
		public DBTable(string name, params DBColumn[] columns)
		{
			this.name = name;
			this.columns = columns;
			foreach (DBColumn col in columns)
			{
				col.DBTable = this;
			}
		}

	}
}
