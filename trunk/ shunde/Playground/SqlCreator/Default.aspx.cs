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
using Shunde;
using Shunde.Framework;
using Shunde.Utilities;

public partial class SqlCreator_Default : PageBase
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

		if (!IsPostBack)
		{
			setupAssemblies();
		}
		else
		{

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


	protected void clickAssembly(Object Sender, EventArgs e)
	{
		listbox.Items.Clear();
		for (int i = 0; i < assembliesCB.Items.Count; i++)
		{
			int index = System.Convert.ToInt32(assembliesCB.Items[i].Value);
			if (assembliesCB.Items[i].Selected)
			{
				addToSelectable(index);
			}
		}
	}

	protected void addToSelectable(int index)
	{
		Assembly a = assemblies[Convert.ToInt32(assembliesCB.SelectedValue)];
		

		// get all the objects of the current assembly, and add them to the list box

		Type ooDBObjType = typeof(DBObject);

		
		Type[] types = a.GetTypes();

		Comparison<Type> comparer = new Comparison<Type>(delegate(Type t1, Type t2) { return t1.FullName.CompareTo(t2.FullName); });
		Array.Sort<Type>(types, comparer);

		for (int i = 0; i < types.Length; i++)
		{
			Type t = types[i];
			if (t.IsSubclassOf(ooDBObjType) || t.Equals(ooDBObjType))
			{
				ListItem li = new ListItem(t.FullName);
				if (!listbox.Items.Contains(li))
				{
					listbox.Items.Add(li);
				}
			}
		}

	}

	private string getAssName(Assembly a)
	{
		return a.FullName.Substring(0, a.FullName.IndexOf(","));
	}

	protected void createSql(Object Sender, EventArgs e)
	{

		String tbls = "";
		String fks = "";

		List<string> checker = new List<string>();

		for (int i = 0; i < listbox.Items.Count; i++)
		{
			ListItem li = listbox.Items[i];
			if (li.Selected)
			{
				Type t = getType(li.Value);

				try
				{
					tbls += DBCreation.GetCreateTable(t, checker, tablesAlreadyExist.Checked);
				}
				catch (ShundeException se)
				{
					sqlLiteral.Text = se.Message;
					return;
				}

				fks += DBCreation.GetForeignKeyConstraints(t);
			}
		}

		sqlLiteral.Text = Server.HtmlEncode(tbls + "\n" + fks + "\n\n");

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
