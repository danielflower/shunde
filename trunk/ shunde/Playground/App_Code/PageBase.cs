using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Shunde.Web;
/// <summary>
/// Summary description for PageBase
/// </summary>
public abstract class PageBase : ShundePageBase
{
	public override sealed void Page_Load(object Sender, EventArgs e)
	{
		base.Page_Load(Sender, e);
		Start();
	}

	public abstract void Start();

	public override string ConnectionString
	{
		get { return ConfigurationManager.ConnectionStrings["PlaygroundConnectionString"].ConnectionString; }
	}

	public override string ErrorsEmail
	{
		get { return "errors@danielflower.com"; }
	}

	public override string WebsiteName
	{
		get { return "Training Ground"; }
	}

	public override string SmtpServer
	{
		get { return "localhost"; }
	}

	public override int SmtpPortNumber
	{
		get { return 25; }
	}
}
