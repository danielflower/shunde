using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Shunde.Framework;

[assembly: TagPrefix("Shunde.Web", "Shunde")]

namespace Shunde.Web
{


	/// <summary>
	/// A combo box, allowing the selection of an existing value in a drop-down list, or typing in a new value
	/// </summary>
	[ToolboxData("<{0}:ComboBox runat=server></{0}:ComboBox>")]
	public class ComboBox : ListControl, INamingContainer
	{

		/// <summary>
		/// Specifies that new values can be added by the user
		/// </summary>
		[Category("Behaviour")]
		[DefaultValue("False")]
		public bool AllowNewValues
		{
			get
			{
				object o = ViewState["AllowNewValues"];
				return ((o == null) ? false : (bool)o);
			}

			set
			{
				ViewState["AllowNewValues"] = value;
			}
		}

		/// <summary>
		/// Checks whether or not a new value was entered in by the user, rather than selecting one from the drop-down list
		/// </summary>
		public bool NewValueEntered
		{
			get
			{
				return SelectedIndex == -1;
			}
		}

		/// <summary>
		/// Gets or sets the selected index of the combo box, or -1 if a new value has been entered
		/// </summary>
		public override int SelectedIndex
		{
			get
			{
				string boxValue = textBox.Text.Trim().ToLower();
				int index = 0;
				foreach (ListItem li in this.Items)
				{
					if (li.Text.ToLower().Trim().Equals(boxValue))
					{
						return index;
					}
					index++;
				}
				return -1;
			}
			set
			{
				base.SelectedIndex = value;
				textBox.Text = this.Items[value].Text;
			}
		}


		/// <summary>
		/// Gets the value of the ListItem that is selected
		/// </summary>
		public override string SelectedValue
		{
			get
			{
				return SelectedItem.Value;
			}
			set
			{
				foreach (ListItem li in this.Items)
				{
					if (li.Value.Equals(value))
					{
						li.Selected = true;
						textBox.Text = li.Text;
						return;
					}
				}
			}
		}

		/// <summary>
		/// Gets the text of the ListItem that is selected
		/// </summary>
		public override string Text
		{
			get
			{
				return SelectedItem.Text;
			}
			set
			{
				textBox.Text = value;
			}
		}

		/// <summary>
		/// Gets the selected item, or a ListItem with no value if text was entered
		/// </summary>
		public override ListItem SelectedItem
		{
			get
			{
				int selectedIndex = SelectedIndex;
				if (selectedIndex == -1)
				{
					return new ListItem(textBox.Text.Trim(), "");
				}
				else
				{
					return this.Items[selectedIndex];
				}
			}
		}


		TextBox textBox = new TextBox();

		/// <summary>
		/// Registers the Javascript files
		/// </summary>
		/// <param name="e"></param>
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			this.Page.ClientScript.RegisterClientScriptInclude(typeof(DBObject), "ShundeScripts", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "Shunde.Resources.ShundeScripts.js"));
			this.Page.ClientScript.RegisterClientScriptInclude(typeof(DBObject), "ComboBoxScripts", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "Shunde.Resources.ComboBoxScripts.js"));

		}

		/// <summary>
		/// Creates the child controls
		/// </summary>
		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			ListBox lb = new ListBox();
			TextBox tb = textBox;

			ApplyStyles(tb, lb);

			if (!Page.IsPostBack)
			{
				{
					int index = 0;
					foreach (ListItem li in this.Items)
					{
						lb.Items.Add(li);
						if (li.Selected)
						{
							this.SelectedIndex = index;
						}
						index++;
					}
				}

				if (this.SelectedIndex > -1)
				{
					tb.Text = this.SelectedItem.Text;
				}
				else if (this.Text.Length > 0)
				{
					tb.Text = this.Text;
				}

			}


			lb.Style[HtmlTextWriterStyle.Visibility] = "hidden";
			lb.Style[HtmlTextWriterStyle.Position] = "absolute";

			this.Controls.Add(lb);
			this.Controls.Add(tb);






			lb.TabIndex = this.TabIndex;


			//tb.Attributes["onfocus"] = "oe_searchBoxUpdate(this, document.getElementById('" + hih.ClientID + "'), '" + ci.SearchBoxUrl + "');";
			//tb.Attributes["onkeyup"] = "oe_searchBoxUpdate(this, document.getElementById('" + hih.ClientID + "'), '" + ci.SearchBoxUrl + "');";
			//tb.Attributes["onkeydown"] = "return oe_searchKeyDown(event);";
			//tb.Attributes["onblur"] = "oe_unselectResultsBox();";

			string tbVar = "document.getElementById('" + tb.ClientID + "')";
			string lbVar = "document.getElementById('" + lb.ClientID + "')";

			tb.Attributes["onfocus"] = "ShundeComboBox.showListBox(this, " + lbVar + ");";
			tb.Attributes["onblur"] = "ShundeComboBox.hideListBox(this, " + lbVar + ");";
			tb.Attributes["onkeydown"] = "return ShundeComboBox.onKeyDown(this, " + lbVar + ", event.keyCode);";
			tb.Attributes["onkeyup"] = "ShundeComboBox.updateBox(this, " + lbVar + ");";

			lb.Attributes["onchange"] = "ShundeComboBox.selectCurrentlySelected(" + tbVar + ", this);";
			lb.Attributes["onmouseover"] = "ShundeComboBox.onMouseOverListbox(" + tbVar + ", this);";
			lb.Attributes["onmouseout"] = "ShundeComboBox.onMouseOutListbox(" + tbVar + ", this);";
			lb.Attributes["ondblclick"] = "ShundeComboBox.onMouseDoubleClickListbox(" + tbVar + ", this);";

			tb.Attributes["autocomplete"] = "off";
			tb.TabIndex = this.TabIndex;

			Page.ClientScript.RegisterStartupScript(typeof(ComboBox), this.ID, "ShundeComboBox.initialise(" + tbVar + ", " + lbVar + ");", true);



		}

		private void ApplyStyles(TextBox tb, ListBox lb)
		{
			foreach (HtmlTextWriterStyle key in this.Style.Keys)
			{
				tb.Style[key] = this.Style[key];
				lb.Style[key] = this.Style[key];
			}

			lb.Height = this.Height;
			lb.Width = this.Width;
			tb.Width = this.Width;
		}

		/// <summary>
		/// Renders the control to HTML
		/// </summary>
		/// <param name="writer"></param>
		public override void RenderControl(HtmlTextWriter writer)
		{
			RenderChildren(writer);
		}

	}

	/// <summary>
	/// 
	/// </summary>
	public enum ItemSource
	{
	}

}
