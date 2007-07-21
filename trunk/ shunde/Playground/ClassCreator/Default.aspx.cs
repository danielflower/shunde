using System;
using System.Text;
using System.Configuration;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Shunde.Framework;
using Shunde.Utilities;
using Shunde.Web;

public partial class ClassCreator_Default : PageBase
{
	public override void Start()
	{
		

		Page.Title = "Create C# Class File";


		int[] dummyArray = new int[Convert.ToInt32(this.numberToCreateTB.Text)];

		this.changeNumberButton.Click += new EventHandler(changeNumberButton_Click);
		this.textboxRepeater.ItemDataBound += new RepeaterItemEventHandler(textboxRepeater_ItemDataBound);

		if (!IsPostBack)
		{
			this.textboxRepeater.DataSource = dummyArray;
			this.textboxRepeater.DataBind();
		}

		textareaId.Text = this.outputTB.ClientID;

	}


	void changeNumberButton_Click(object sender, EventArgs e)
	{
		int[] dummyArray = new int[Convert.ToInt32(this.numberToCreateTB.Text)];
		this.textboxRepeater.DataSource = dummyArray;
		this.textboxRepeater.DataBind();

	}

	void textboxRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
	{
		if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
		{
			TextBox fieldNameTB = (TextBox)e.Item.FindControl("fieldNameTB");
			ComboBox typeTB = (ComboBox)e.Item.FindControl("typeTB");
			CheckBox allowNullsCB = (CheckBox)e.Item.FindControl("allowNullsCB");
			TextBox minAllowedTB = (TextBox)e.Item.FindControl("minAllowedTB");
			TextBox maxAllowedTB = (TextBox)e.Item.FindControl("maxAllowedTB");

			//typeTB.Attributes["onkeyup"] = "typeChanged( '" + fieldNameTB.ClientID + "', '" + typeTB.ClientID + "', '" + allowNullsCB.ClientID + "', '" + minAllowedTB.ClientID + "', '" + maxAllowedTB.ClientID + "' );";
			typeTB.DataSource = ColumnTypes.DataSource;
			typeTB.DataBind();

		}
	}

	protected void submitButton_Click(object sender, EventArgs e)
	{

		infoLabel.Text = "";
		outputTB.Text = "";

		string fields = "";
		string properties = "";
		List<string> dbcols = new List<string>();

		List<string> enumColTypes = new List<string>();
		List<string> enumColNames = new List<string>();

		List<string> dbObjectColTypes = new List<string>();
		List<string> dbObjectColNames = new List<string>();

		foreach (RepeaterItem item in this.textboxRepeater.Items)
		{
			if (!(item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem))
			{
				continue;
			}
			TextBox fieldNameTB = (TextBox)item.FindControl("fieldNameTB");
			ComboBox typeTB = (ComboBox)item.FindControl("typeTB");
			CheckBox allowNullsCB = (CheckBox)item.FindControl("allowNullsCB");
			TextBox minAllowedTB = (TextBox)item.FindControl("minAllowedTB");
			TextBox maxAllowedTB = (TextBox)item.FindControl("maxAllowedTB");
			TextBox summaryTB = (TextBox)item.FindControl("summaryTB");

			if (fieldNameTB.Text.Length == 0 || typeTB.Text.Length == 0)
			{
				continue;
			}


			string name = fieldNameTB.Text;

			char first = Convert.ToChar(name.Substring(0, 1));
			if (!char.IsLetter(first))
			{
				infoLabel.Text = "Field names must start with a letter - check field " + name;
				return;
			}
			if (!char.IsLower(first))
			{
				infoLabel.Text = "Please use camel notation for field names - check field " + name;
				return;
			}

			if (TextUtils.RemoveNonAlphaNumeric(name).Length != name.Length)
			{
				infoLabel.Text = "Please only use alphanumeric field names - check field " + name;
			}
			

			string propertyName = name.Substring(0,1).ToUpper() + name.Substring(1);


			bool allowNulls = allowNullsCB.Checked;
			string type = typeTB.Text.Trim();
			string dotNetType = type;
			if (allowNulls && Array.IndexOf(ColumnTypes.NullablePrimitives, type) >= 0)
			{
				dotNetType += "?";
			}
			else if (type == ColumnTypes.SingleLineString || type == ColumnTypes.MultilineString)
			{
				dotNetType = "string";
			}
			else if (IsEnumType(type))
			{
				dotNetType = type.Substring(5);
				enumColNames.Add(fieldNameTB.Text);
				enumColTypes.Add(dotNetType);
			}






			fields += @"		private " + dotNetType + " " + fieldNameTB.Text + @";
";

			properties += @"
		/// <summary>" + Server.HtmlEncode(summaryTB.Text) + @"</summary>
		public " + dotNetType + " " + propertyName + @"
		{
			get { return this." + name + @"; }
			set { this." + name + @" = value; }
		}
";


			if (IsDBObjectType(type, dotNetType))
			{
				dbObjectColTypes.Add(type);
				dbObjectColNames.Add(name);
			}

			string minLength = minAllowedTB.Text;
			
			string maxLength = maxAllowedTB.Text;


			if (type == ColumnTypes.DateTime)
			{
				if (minAllowedTB.Text.Length > 0)
				{
					try
					{
						DateTime date = DateTime.Parse(minAllowedTB.Text);
						minLength = "new DateTime(" + date.Ticks + ")";
					}
					catch
					{
						infoLabel.Text = "Please make sure your dates are in the correct format, particularly for " + name;
						return;
					}
				}
				if (maxAllowedTB.Text.Length > 0)
				{
					try
					{
						DateTime date = DateTime.Parse(maxAllowedTB.Text);
						maxLength = "new DateTime(" + date.Ticks + ")";
					}
					catch
					{
						infoLabel.Text = "Please make sure your dates are in the correct format, particularly for " + name;
						return;
					}
				}
			}


			if (minLength.Length > 0 ^ maxLength.Length > 0)
			{
				if (minLength.Length == 0)
				{
					minLength = "null";
				}
				else
				{
					maxLength = "null";
				}
			}

			if (minLength.Length > 0 || maxLength.Length > 0)
			{
				minLength = ", " + minLength;
				maxLength = ", " + maxLength;
			}



			switch (type)
			{
				case ColumnTypes.Int:
				case ColumnTypes.Short:
				case ColumnTypes.Long:
				case ColumnTypes.Float:
				case ColumnTypes.Double:
					{
						if (minLength.Length > 0 || maxLength.Length > 0)
						{
							minLength = (minLength == "null") ? ", null" : ", (" + dotNetType + ")" + minLength;
							maxLength = (maxLength == "null") ? ", null" : ", (" + dotNetType + ")" + maxLength;
						}
						dbcols.Add("new NumberColumn(\"" + name + "\", typeof(" + dotNetType + @")" + minLength + maxLength + ")");
						break;
					}
				case ColumnTypes.SingleLineString:
					{
						dbcols.Add("new SingleLineString(\"" + name + "\"" + minLength + maxLength + ")");
						break;
					}

				case ColumnTypes.MultilineString:
					{
						dbcols.Add("new MultiLineString(\"" + name + "\", " + allowNulls.ToString().ToLower() + ")");
						break;
					}
				case ColumnTypes.BinaryData:
					{
						dbcols.Add("new BinaryDataColumn(\"" + name + "\", " + allowNulls.ToString().ToLower() + ")");
						break;
					}
				case ColumnTypes.Color:
					{
						dbcols.Add("new ColorColumn(\"" + name + "\", " + allowNulls.ToString().ToLower() + ")");
						break;
					}
				case ColumnTypes.DateTime:
					{
						if (minLength.Length > 0 || maxLength.Length > 0)
						{
							minLength = (minLength == "null") ? ", null" : ", (" + dotNetType + ")" + minLength;
							maxLength = (maxLength == "null") ? ", null" : ", (" + dotNetType + ")" + maxLength;
						}
						dbcols.Add("new DateTimeColumn(\"" + name + "\", " + allowNulls.ToString().ToLower() + minLength + maxLength + ", DateTimePart.DateAndOptionallyTime)");
						break;
					}
					
				default:
					{
						if (IsEnumType(type))
						{
							dbcols.Add("new EnumColumn(\"" + name + "\", typeof(" + dotNetType + "))");
						}
						else
						{
							dbcols.Add("new DBObjectColumn(\"" + name + "\", typeof(" + dotNetType + "), " + allowNulls.ToString().ToLower() + ")");
						}
						break;
					}
			}


		}

		string className = classNameTB.Text;

		StringBuilder s = new StringBuilder();





		s.Append(@"using System;
using Shunde.Framework;
using Shunde.Framework.Columns;

namespace " + namespaceTB.Text + @" {

	/// <summary>
	/// A " + className + @" object
	/// </summary>
	public class " + className + @" : DBObject
	{

		#region Fields
");

		s.Append(fields);

		s.Append(@"		#endregion


		#region Properties
");

		s.Append(properties);

		s.Append(@"
		#endregion
");

		s.Append(@"

		#region Static Constructor

		/// <summary>Sets up the <see cref=""ObjectInfo"" /> for this class</summary>
		static " + className + @"()
		{

			DBTable tbl = new DBTable(""" + className + @""", new DBColumn[] {
"		);

		for (int i = 0; i < dbcols.Count; i++ )
		{
			string col = dbcols[i];
			string comma = (i < dbcols.Count - 1) ? "," : "";
			s.Append("\t\t\t\t" + col + comma + "\n");
		}


		s.Append(@"			});

			ObjectInfo.RegisterObjectInfo(typeof(" + className + @"), tbl);

		}

		#endregion


		#region Static Methods

		/// <summary>Gets all the " + className + @" objects in the database</summary>
		public static " + className + @"[] Get" + Pluralise(className) + @"()
		{

			Type t = typeof(" + className + @");
			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);
			string sql = oi.GetSelectStatement() + "" WHERE [DBObject].[isDeleted] = 0 ORDER BY [DBObject].[displayOrder] ASC"";
			return (" + className + @"[])DBObject.GetObjects(sql, t);

		}
");


		for (int i = 0; i < dbObjectColNames.Count; i++) 
		{
			string type = dbObjectColTypes[i];
			string name = dbObjectColNames[i];

			s.Append(@"
		/// <summary>Gets all the " + className + @" objects in the database filtered by the given <see cref=""" + type + @""">" + name + @"</see></summary>
		/// <param name=""" + name + @""">The " + type + @" to filter by</param>
		public static " + className + @"[] Get" + Pluralise(className) + @"(" + type + @" " + name + @")
		{

			Type t = typeof(" + className + @");
			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);
			string sql = oi.GetSelectStatement() + "" WHERE [DBObject].[isDeleted] = 0 AND [" + className + @"].[" + name + @"Id] = "" + " + name + @".Id + "" ORDER BY [DBObject].[displayOrder] ASC"";
			return (" + className + @"[])DBObject.GetObjects(sql, t);

		}
");
		}

		for (int i = 0; i < enumColTypes.Count; i++)
		{
			string type = enumColTypes[i];
			string name = enumColNames[i];

			s.Append(@"
		/// <summary>Gets all the " + className + @" objects in the database filtered by the given <see cref=""" + type + @""">" + name + @"</see></summary>
		/// <param name=""" + name + @""">The " + type + @" to filter by</param>
		public static " + className + @"[] Get" + Pluralise(className) + @"(" + type + @" " + name + @")
		{

			Type t = typeof(" + className + @");
			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);
			string sql = oi.GetSelectStatement() + "" WHERE [DBObject].[isDeleted] = 0 AND [" + className + @"].[" + name + @"] = "" + (int)" + name + @" + "" ORDER BY [DBObject].[displayOrder] ASC"";
			return (" + className + @"[])DBObject.GetObjects(sql, t);

		}
");
		}

		s.Append(@"
		#endregion

	}

	");

		for (int i = 0; i < enumColTypes.Count; i++)
		{
			string type = enumColTypes[i];
			string name = enumColNames[i];
			s.Append(@"

	/// <summary>The " + name + @" of a " + className + @"</summary>
	public enum " + type + @"
	{

	}
");
		}

		s.Append(@"

}

");




		outputTB.Text = s.ToString();
	
	}






	bool IsEnumType(string enteredName)
	{
		return enteredName.StartsWith("enum ");
	}

	bool IsDBObjectType(string enteredName, string name)
	{
		string[] primitives = new string[] {
			"multiline", "string", "int", "short", "long", "float", "double", "bool", "DateTime", "BinaryData", "Color"
		};

		foreach (string primitive in primitives)
		{
			if (primitive.Equals(name))
			{
				return false;
			}
		}

		if (enteredName.StartsWith("enum "))
		{
			return false;
		}

		return true;
	}


	string Pluralise(string singular)
	{
		if (singular.EndsWith("y"))
		{
			return singular.Substring(0, singular.Length - 1) + "ies";
		}
		if (singular.EndsWith("s"))
		{
			return singular + "es";
		}
		return singular + "s";
	}

	private static class ColumnTypes
	{
		public const string Int = "int";
		public const string Short = "short";
		public const string Long = "long";
		public const string Float = "float";
		public const string Double = "double";
		public const string SingleLineString = "Single Line String";
		public const string MultilineString = "Multiline String";
		public const string DateTime = "DateTime";
		public const string Color = "Color";
		public const string BinaryData = "BinaryData";
		public const string Enumeration = "enum ";
//		public const string DBObject = "DBObject";


		public static readonly string[] NullablePrimitives = new string[] { Int, Short, Long, Float, Double, DateTime };
		public static readonly string[] DataSource;

		static ColumnTypes()
		{
			string[] types = new string[] { Int, Short, Long, Float, Double, SingleLineString, MultilineString, DateTime, Color, BinaryData, Enumeration };
			Array.Sort(types);
			DataSource = types;
		}

	}

}






