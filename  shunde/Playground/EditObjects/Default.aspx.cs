using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Reflection;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Shunde;
using Shunde.Framework;
using Shunde.Utilities;

public partial class EditObjects_Default : PageBase
{


	List<Assembly> assemblies = new List<Assembly>();



	public override void Start()
	{

		Assembly[] asms = System.AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly a in asms)
		{
			Type[] types = a.GetTypes();
			foreach (Type t in types)
			{
				if (t.IsSubclassOf(typeof(DBObject)))
				{
					assemblies.Add(a);
					break;
				}
			}
		}

		assembliesCB.SelectedIndexChanged += new EventHandler(assembliesCB_SelectedIndexChanged);

		if (!IsPostBack)
		{
			setupAssemblies();
			setupDatabaseNames();

			if (assembliesCB.SelectedIndex > -1)
			{
				assembliesCB_SelectedIndexChanged(null, null);

				if (listbox.SelectedIndex > -1)
				{
					getInstances(null, null);
				}

			}

		}
		else
		{

		}

	}

	void assembliesCB_SelectedIndexChanged(object sender, EventArgs e)
	{
		listbox.Items.Clear();

		Assembly a = assemblies[Convert.ToInt32(assembliesCB.SelectedValue)];

		Type ooDBObjType = typeof(DBObject);

		string curTypeName = GetStringParam("className");

		Type[] types = a.GetTypes();
		for (int i = 0; i < types.Length; i++)
		{
			Type t = types[i];
			if (t.IsSubclassOf(ooDBObjType) || t.Equals(ooDBObjType))
			{
				ListItem li = new ListItem(t.FullName);
				if (!listbox.Items.Contains(li))
				{
					if (curTypeName.Equals(t.FullName))
					{
						li.Selected = true;
					}
					listbox.Items.Add(li);
				}
			}
		}

	}


	private void setupAssemblies()
	{

		string curAss = GetStringParam("assName");

		for (int i = 0; i < assemblies.Count; i++)
		{
			Assembly a = assemblies[i];
			string assName = getAssName(a);
			ListItem li = new ListItem(assName, i.ToString());
			if (curAss.Equals(assName))
			{
				li.Selected = true;
			}
			assembliesCB.Items.Add(li);
		}

	}



	void setupDatabaseNames()
	{

		String curDBName = GetStringParam("dbName");
		if (curDBName.Length == 0)
		{
			curDBName = DBUtils.GetSqlConnection().Database;
		}

		SqlDataReader sdr = DBUtils.ExecuteSqlQuery("sp_helpdb");
		//SqlDataReader sdr = DBUtils.executeSqlQuery("SELECT * FROM Master..sysdatabases" );


		while (sdr.Read())
		{
			String dbName = sdr["name"].ToString();
			String owner = sdr["owner"].ToString();
			//if (!owner.Equals("sa")) {
			ListItem li = new ListItem(dbName, dbName);
			if (dbName.Equals(curDBName))
			{
				li.Selected = true;
			}
			dbDropDown.Items.Add(li);
			//}
		}
		sdr.Close();
	}


	private string getAssName(Assembly a)
	{
		return a.FullName.Substring(0, a.FullName.IndexOf(","));
	}

	protected void getInstances(Object Sender, EventArgs e)
	{

		ListItem typeLi = listbox.SelectedItem;
		if (typeLi == null)
		{
			infoMessage.Text = "Please select a type";
			return;
		}

		DBUtils.GetSqlConnection().ChangeDatabase(dbDropDown.SelectedItem.Value);

		objectsBox.Items.Clear();

		Type t = getType(typeLi.Value);

		ObjectInfo oi = ObjectInfo.GetObjectInfo(t);

		DBObject[] objs = null;

		//		try {
		objs = DBObject.GetObjects(oi.GetSelectStatement(System.Convert.ToInt32(numToGet.Text)) + " WHERE isDeleted = " + System.Convert.ToInt32(getDeleted.Checked) + " ORDER BY displayOrder ASC, lastUpdate DESC", t);
		infoMessage.Text = "";
		//		} catch (Exception ex) {
		//			if (ex.Message.StartsWith( "Invalid object name" )) {
		//				infoMessage.Text = "Database table does not exist. Have you selected the correct database?";
		//				return;
		//			}
		//			throw ex;
		//		}

		for (int i = 0; i < objs.Length; i++)
		{
			DBObject o = objs[i];

			string oName = o.Id.ToString() + ") ";

			Type ot = o.GetType();


			oName += o.FriendlyName + " (" + ot.FullName + ") - last updated " + o.LastUpdate + " by " + o.LastUpdatedBy;


			ListItem li = new ListItem(oName, o.Id.ToString() + "," + ot.FullName + "," + dbDropDown.SelectedItem.Value);
			objectsBox.Items.Add(li);
		}

	}




	protected void createInstance(Object Sender, EventArgs e)
	{

		ListItem li = listbox.SelectedItem;

		if (li == null)
		{
			infoMessage.Text = "Please select a type";
			return;
		}

		Type t = getType(li.Value);

		if (t.IsAbstract)
		{
			infoMessage.Text = t.FullName + " is an abstract class, so you cannot create a new object of this type.";
			return;
		}

		String assName = t.Assembly.FullName;

		Redirect("Edit.aspx?id=-1&className=" + Server.UrlEncode(t.FullName) + "&dbName=" + Server.UrlEncode(dbDropDown.SelectedItem.Value) + "&assName=" + Server.UrlEncode(assName));

	}

	protected void editInstance(Object Sender, EventArgs e)
	{

		ListItem objLi = objectsBox.SelectedItem;
		if (objLi == null)
		{
			infoMessage.Text = "Please select an instance";
			return;
		}

		string[] strings = objLi.Value.Split(new char[] { ',' });

		string assName = getType(listbox.SelectedItem.Value).Assembly.FullName;

		Redirect("Edit.aspx?id=" + strings[0] + "&className=" + Server.UrlEncode(strings[1]) + "&dbName=" + Server.UrlEncode(strings[2]) + "&assName=" + Server.UrlEncode(assName));

	}



	private Type getType(string classname)
	{
		foreach (Assembly a in assemblies)
		{

			Type t = a.GetType(classname);
			if (t != null)
			{
				return t;
			}
		}

		throw new Exception("The class " + classname + " could not be found.");
	}

}
