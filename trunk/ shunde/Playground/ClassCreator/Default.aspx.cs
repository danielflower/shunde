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
			TextBox typeTB = (TextBox)e.Item.FindControl("typeTB");
			CheckBox allowNullsCB = (CheckBox)e.Item.FindControl("allowNullsCB");
			TextBox minAllowedTB = (TextBox)e.Item.FindControl("minAllowedTB");
			TextBox maxAllowedTB = (TextBox)e.Item.FindControl("maxAllowedTB");

			typeTB.Attributes["onkeyup"] = "typeChanged( '" + fieldNameTB.ClientID + "', '" + typeTB.ClientID + "', '" + allowNullsCB.ClientID + "', '" + minAllowedTB.ClientID + "', '" + maxAllowedTB.ClientID + "' );";

		}
		else if (e.Item.ItemType == ListItemType.Footer)
		{
			Button submitButton = (Button)e.Item.FindControl("submitButton");
			//submitButton.Click += new EventHandler(submitButton_Click);
		}
	}

	protected void submitButton_Click(object sender, EventArgs e)
	{

		infoLabel.Text = "";
		outputTB.Text = "";

		string fields = "";
		List<string> dbcols = new List<string>();

		List<string> dbObjectColTypes = new List<string>();
		List<string> dbObjectColNames = new List<string>();

		foreach (RepeaterItem item in this.textboxRepeater.Items)
		{
			if (!(item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem))
			{
				continue;
			}
			TextBox fieldNameTB = (TextBox)item.FindControl("fieldNameTB");
			TextBox typeTB = (TextBox)item.FindControl("typeTB");
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


			string type = typeTB.Text.Trim();


			string allowNulls = (type == "string") ? "" : ", " + allowNullsCB.Checked.ToString().ToLower();

			if (type.Equals("multiline"))
			{
				type = "string";
			}



			fields += @"
		private " + type + " " + fieldNameTB.Text + @";

		/// <summary>" + Server.HtmlEncode(summaryTB.Text) + @"</summary>
		public " + type + " " + propertyName + @"
		{
			get { return this." + name + @"; }
			set { this." + name + @" = value; }
		}
";


			if (IsDBObjectType(type))
			{
				dbObjectColTypes.Add(type);
				dbObjectColNames.Add(name);
			}

			string minLength = minAllowedTB.Text;
			
			string maxLength = maxAllowedTB.Text;


			if (type == "DateTime")
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

			if (allowNulls.Length == 0 && (minLength.Length == 0 || maxLength.Length == 0))
			{
				infoLabel.Text = "Please specify a minimum and maximum value for " + name;
				return;
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


			dbcols.Add( "new DBColumn( \"" + name + "\", typeof(" + type + @")" + allowNulls + minLength + maxLength + " )" );

		}

		string className = classNameTB.Text;

		StringBuilder s = new StringBuilder();





		s.Append(@"using System;
using Shunde.Framework;


namespace " + namespaceTB.Text + @" {

	/// <summary>
	/// A " + className + @" object
	/// </summary>
	public class " + className + @" : DBObject
	{

");

		s.Append(fields);

		s.Append( @"

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


		/// <summary>Gets all the " + className + @" objects in the database</summary>
		public static " + className + @"[] Get" + Pluralise(className) + @"()
		{

			Type t = typeof(" + className + @");
			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);
			String sql = oi.GetSelectStatement() + "" WHERE [DBObject].[isDeleted] = 0 ORDER BY [DBObject].[displayOrder] ASC"";
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
			String sql = oi.GetSelectStatement() + "" WHERE [DBObject].[isDeleted] = 0 AND [" + className + @"].[" + name + @"Id] = "" + " + name + @".Id + "" ORDER BY [DBObject].[displayOrder] ASC"";
			return (" + className + @"[])DBObject.GetObjects(sql, t);

		}
");
		}

		s.Append(@"

	}

}

");




		outputTB.Text = s.ToString();
	
	}








	bool IsDBObjectType(string name)
	{
		string[] primitives = new string[] {
			"multiline", "string", "int", "short", "long", "float", "double", "bool", "DateTime", "BinaryData"
		};

		foreach (string primitive in primitives)
		{
			if (primitive.Equals(name))
			{
				return false;
			}
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



}






