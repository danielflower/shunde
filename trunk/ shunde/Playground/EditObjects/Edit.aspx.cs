using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Shunde.Framework;
using Shunde.Utilities;
using Shunde.Web;
using Shunde.Framework.Columns;

public partial class EditObjects_Edit : PageBase
{

	DBObject obj;
	string dbName;
	string returnUrl;

	private const int MaxAllowedInDropDown = 50;

	public override void Start()
	{


		dbName = GetStringParam("dbName");
		ShundeContext.Current.DbConnection.ChangeDatabase(dbName);

		Assembly ass = Assembly.Load(GetStringParam("assName"));
		obj = DBObject.CreateObject(ass, GetStringParam("className"));



		returnUrl = "Default.aspx?dbName=" + Server.UrlEncode(dbName) + "&assName=" + Server.UrlEncode(GetAssName(obj.GetType().Assembly)) + "&className=" + Server.UrlEncode(obj.GetType().FullName);



		obj.Id = GetIntParam("id");
		if (obj.Exists())
		{
			obj.Populate();
			obj.PopulateBinaryData();
		}

		Page.Title = "Edit " + obj.FriendlyName;

		this.ObjectEditor.DBObject = obj;
		this.ObjectEditor.EditCancelledDelegate = RedirectorDelegate;
		this.ObjectEditor.AfterObjectDeletedDelegate = RedirectorDelegate;
		this.ObjectEditor.AfterObjectSavedDelegate = RedirectorDelegate;
		this.ObjectEditor.UpdaterName = "Shunde Playground";




		ObjectInfo oi = ObjectInfo.GetObjectInfo(obj.GetType());
		foreach (DBTable table in oi.Tables)
		{
			foreach (DBColumn col in table.Columns)
			{
				if (col.Name.Equals("className") || col.Name.Equals("updateId") || col.Name.Equals("lastUpdatedBy") || col.Name.Equals("lastUpdate"))
				{
					continue;
				}

				ObjectEditorRow row = this.ObjectEditor[col.Name];

				if (col.Type.Equals(typeof(DBObject)) || col.Type.IsSubclassOf(typeof(DBObject)))
				{
					ObjectInfo foreignOI = ObjectInfo.GetObjectInfo(col.Type);
					DBObject[] selections = DBObject.GetObjects(foreignOI.GetSelectStatement() + " WHERE isDeleted = 0 ORDER BY displayOrder ASC", col.Type);
					row.SetListItems(selections, (DBObject)obj.Get(col.Name), "FriendlyName");


					row.SelectionsPopupUrl = "SelectObject.aspx?functionName=";
					row.SearchBoxUrl = "xmlSearch.aspx?type=" + col.Type.FullName;
					row.InputMode = InputMode.ComboBox;
					row.AddOnTheFly = true;
					row.FindObjectDelegate = FindPerson;
				
				}
				else if (col.Type.Equals(typeof(BinaryData)))
				{
					row.ViewBinaryDataUrl = "ViewBinaryData.aspx?objectId=";
				}
				else if (EnumColumn.IsEnumOrNullableEnum(col.Type))
				{
					row.SetEnumListItems(col.Type, (Enum)obj.Get(col.Name));
					row.InputMode = InputMode.RadioButtonList;
				}


			}

		}




		this.ObjectEditor.PopulateTable();

		headerTag.InnerHtml = "Edit a " + obj.GetType().Name;
	}





	DBObject FindPerson(string name, bool allowCreation)
	{
		try
		{
			return (TestingGround.Person)DBObject.GetObject("SELECT [id] FROM person WHERE (firstName + ' ' + surname) = '" + DBUtils.ParseSql(name) + "'");
		}
		catch (Shunde.ObjectDoesNotExistException)
		{
			if (!allowCreation)
			{
				throw new Shunde.ValidationException("Sorry, the value \"" + name + "\" you specified was not found. Please try another value.");
			}
		}

		int spaceIndex = name.IndexOf(' ');
		if (spaceIndex < 1 || spaceIndex > name.Length - 1)
		{
			throw new Shunde.ValidationException("You must enter a first name and surname for a person");
		}
		TestingGround.Person p = new TestingGround.Person();
		p.FirstName = name.Substring(0, spaceIndex);
		p.Surname = name.Substring(spaceIndex + 1);
		p.Save();
		return p;

	}

	public void RedirectorDelegate()
	{
		Redirect(returnUrl);
	}

	private string GetAssName(Assembly a)
	{
		return a.FullName.Substring(0, a.FullName.IndexOf(","));
	}


}
