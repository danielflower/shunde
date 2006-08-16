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

public partial class Template : System.Web.UI.MasterPage
{
	protected void Page_Load(object sender, EventArgs e)
	{

		HtmlGenericControl js = new HtmlGenericControl("script");
		js.Attributes["language"] = "javascript";
		js.Attributes["type"] = "text/javascript";
		js.Attributes["src"] = Page.ResolveUrl("~/includes/ObjectEditorScripts.js");

		head.Controls.Add(js);

	}
}
