using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


[assembly: TagPrefix("Shunde.Web", "Shunde")]


namespace Shunde.Web
{

	/// <summary>
	/// A control which puts a save, delete and cancel button together
	/// </summary>
	[ToolboxData("<{0}:ButtonPanel runat=server></{0}:ButtonPanel>")]
	public class ButtonPanel : Panel, INamingContainer
	{


		private Button saveButton;

		/// <summary>The save button</summary>
		public Button SaveButton
		{
			get { return saveButton; }
		}

		private Button cancelButton;

		/// <summary>The cancel button</summary>
		public Button CancelButton
		{
			get { return cancelButton; }
		}

		private Button deleteButton;

		/// <summary>The delete button</summary>
		public Button DeleteButton
		{
			get { return deleteButton; }
		}

		/// <summary>
		/// Creates a new button panel
		/// </summary>
		public ButtonPanel()
		{
			saveButton = new Button();
			saveButton.ID = "saveButton";
			saveButton.Text = "Save Details";
			saveButton.TabIndex = 1;

			cancelButton = new Button();
			cancelButton.ID = "cancelButton";
			cancelButton.Text = "Cancel";
			cancelButton.TabIndex = 1;
			cancelButton.UseSubmitBehavior = false;
			cancelButton.CausesValidation = false;

			deleteButton = new Button();
			deleteButton.ID = "deleteButton";
			deleteButton.Text = "Delete";
			deleteButton.TabIndex = 1;
			cancelButton.UseSubmitBehavior = false;
			deleteButton.CausesValidation = false;
			deleteButton.Attributes["onclick"] = "return confirm('Are you sure you want to delete this?');";

		}


		/// <summary>
		/// Creates a new button panel
		/// </summary>
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

		}




		/// <summary>
		/// Creates the child controls
		/// </summary>
		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			this.Controls.Add(saveButton);
			this.Controls.Add(deleteButton);
			this.Controls.Add(cancelButton);

			string deleteButtonJs = (deleteButton.Visible) ? ", '" + deleteButton.ClientID + "'" : "";
			string disableJS = "oe_disableControls( new Array('" + saveButton.ClientID + "', '" + cancelButton.ClientID + "'" + deleteButtonJs + ") );";
			saveButton.Attributes["onclick"] = "if (typeof(Page_ClientValidate) == 'function') if (!Page_ClientValidate()) return false; " + disableJS + this.Page.ClientScript.GetPostBackEventReference(saveButton, saveButton.ClientID);
			cancelButton.Attributes["onclick"] = disableJS + this.Page.ClientScript.GetPostBackEventReference(cancelButton, cancelButton.ClientID);
			if (deleteButton.Visible)
			{
				deleteButton.Attributes["onclick"] = "if (!confirm('Are you sure you want to delete this?')) return false;" + disableJS + this.Page.ClientScript.GetPostBackEventReference(deleteButton, deleteButton.ClientID);
			}


		}


	}


}
