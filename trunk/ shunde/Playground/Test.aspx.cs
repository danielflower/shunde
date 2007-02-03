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
using TestingGround;
using Shunde.Utilities;

public partial class Test : PageBase
{
	public override void Start()
	{


		//Enum val = (Enum)Convert.ChangeType((int)1, typeof(HorizontalAlign));
		Type t = typeof(HorizontalAlign?);
		Enum val = (HorizontalAlign?)Enum.Parse(Nullable.GetUnderlyingType(t), "1");

		Response.Write("enum: " + val);

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
			comboBox.SelectedValue = "Part ffour";
			

		}

		goButton.Click += new EventHandler(goButton_Click);
	}

	void goButton_Click(object sender, EventArgs e)
	{
		ListItem li = comboBox.SelectedItem;
		selectedItem.Text = li.Text + "/" + li.Value;

		selectedIndex.Text = comboBox.SelectedIndex.ToString();
		selectedText.Text = comboBox.Text;
		selectedValue.Text = comboBox.SelectedValue;
	}



}
