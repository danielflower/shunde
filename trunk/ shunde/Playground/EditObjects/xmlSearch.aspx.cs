using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml;
using Shunde.Framework;
using Shunde.Common;

public partial class EditObjects_xmlSearch : PageBase
{

	public override void Start()
	{
		Response.ContentType = "text/xml";

		string name = GetStringParam("name");

		string type = GetStringParam("type");

		if (type.Equals("TestingGround.Person" ))
		{
			Shunde.Utilities.ObjectUtils.WriteObjectsToXml(Response.OutputStream, TestingGround.Person.SearchPeople(name));
		}
		else if (type.Equals(typeof(GenericTypeType)))
		{
			Shunde.Utilities.ObjectUtils.WriteObjectsToXml(Response.OutputStream, GenericTypeType.GetGenericTypeTypes());
		}



	}



}
