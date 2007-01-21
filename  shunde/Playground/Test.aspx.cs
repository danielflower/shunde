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

public partial class Test : System.Web.UI.Page
{
	protected void Page_Load(object sender, EventArgs e)
	{

		if (!IsPostBack)
		{
			comboBox.Items.Add("Part one");
			comboBox.Items.Add("Part two");
			comboBox.Items.Add("Part three");
			comboBox.Items.Add("Part four");
			for (int i = 5; i < 30; i++)
			{
				comboBox.Items.Add("Part " + i);
				comboBox1.Items.Add("Part " + i);
			}

			

		}


	}
}
