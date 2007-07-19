using System;
using System.Data.SqlClient;
using System.Text;
using Shunde.Framework;
using Shunde.Utilities;
using Shunde.Framework.Columns;
using System.Data.Common;

namespace Shunde.Common
{
	/// <summary>Some text, stored in the database</summary>
	public class TextSection : DBObject
	{


		private string code = "";

		/// <summary>Unique Code identifying this object</summary>
		/// <remarks>This can be any unique identify, however, it is recommended that the URL of a page is used, ignoring the querystring.</remarks>
		public string Code
		{
			get { return code; }
			set { code = value; }
		}

		private string header = "";

		/// <summary>An optional Header</summary>
		public string Header
		{
			get { return header; }
			set { header = value; }
		}

		private string content = "";

		/// <summary>The Content of the text</summary>
		public string Content
		{
			get { return content; }
			set { content = value; }
		}

		private string nameForAdministrators = "";

		/// <summary>
		/// The name for this textblock for administrators, for editing purposes, for example 'Homepage'
		/// </summary>
		public string NameForAdministrators
		{
			get { return nameForAdministrators; }
			set { nameForAdministrators = value; }
		}




		/// <summary>Default Constructor</summary>
		/// <remarks>This constructor exists only because all <see cref="DBObject" />s must have a constructor that takes no paramaters. You should use the other constructor when creating a TextSection object.</remarks>
		public TextSection() { }

		/// <summary>Creates a new TextSection object and populates it using the specified Code</summary>
		/// <param Name="code">The Code to get</param>
		public TextSection(string code)
		{
			this.code = code;
			DbDataReader sdr = DBUtils.ExecuteSqlQuery("SELECT id FROM TextSection WHERE code = '" + DBUtils.ParseSql(code) + "'");
			if (sdr.Read())
			{
				Id = Convert.ToInt32(sdr["id"]);
				sdr.Close();
				Populate();
			}
			else
			{
				sdr.Close();
			}
		}

		/// <summary>Sets up the <see cref="Shunde.Framework.ObjectInfo" /> for this class</summary>
		static TextSection()
		{

			DBTable tbl = new DBTable("TextSection", new DBColumn[] {
				new SingleLineString( "code", 1, 400 ),
				new SingleLineString( "header", 0, 200 ),
				new MultiLineString( "content", true ),
				new SingleLineString( "nameForAdministrators", 0, 200 )
			});
			 
			ObjectInfo.RegisterObjectInfo(typeof(TextSection), tbl);

		}

		/// <summary>
		/// Returns the header name if one exists; otherwise the default friendly name is returned
		/// </summary>
		public override string FriendlyName
		{
			get
			{
				if (header.Length > 0)
				{
					return header;
				}
				else
				{
					return base.FriendlyName;
				}
			}
		}

		/// <summary>
		/// Gets all the text sections that have a value specified for administrators
		/// </summary>
		public static TextSection[] GetTextSectionsForAdministration()
		{
			Type t = typeof(TextSection);
			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);
			string sql = oi.GetSelectStatement() + " WHERE nameForAdministrators IS NOT NULL ORDER BY nameForAdministrators ASC";
			return (TextSection[])GetObjects(sql, t);
		}

	}
}
