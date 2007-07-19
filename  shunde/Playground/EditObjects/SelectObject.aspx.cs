using System;
using System.Data;
using System.Configuration;
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

public partial class EditObjects_SelectObject : PageBase
{

	public override void Start()
	{

		Page.Title = "Select an object";

		ShundeContext.Current.DbConnection.ChangeDatabase(GetStringParam("dbName"));

		functionName.Text = GetStringParam("functionName");


		Assembly ass = Assembly.Load(GetStringParam("assName"));
		Type objType = ass.GetType(GetStringParam("type"));


		ObjectInfo oi = ObjectInfo.GetObjectInfo(objType);
		DBObject[] objs = DBObject.GetObjects(oi.GetSelectStatement() + " WHERE isDeleted = 0 ORDER BY displayOrder ASC", objType);

		String h = "";
		foreach (DBObject obj in objs)
		{

			string objName = obj.FriendlyName;
			if (objName == null)
			{
				objName = obj.ToString();
			}
			h += "<li><a href=\"javascript:selectObject(" + obj.Id + ", '" + TextUtils.JavascriptStringEncode(objName) + "');\">" + objName + "</a></li>";

		}


		output.Text = h;


	}

}
