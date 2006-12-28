using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Shunde.Common;
using Shunde.Utilities;

[assembly: TagPrefix("Shunde.Web", "Shunde")]

namespace Shunde.Web
{
	/// <summary>
	/// A control to render a TextSection to an HTML page
	/// </summary>
	[ToolboxData("<{0}:TextSectionDisplayer runat=server />")]
	public class TextSectionDisplayer : WebControl, INamingContainer
	{

		#region Properties

		private bool allowEditing = false;

		/// <summary>
		/// Specifies that the user viewing this has authorisation to edit the text.
		/// </summary>
		[Browsable(false)]
		public bool AllowEditing
		{
			get { return allowEditing; }
			set { allowEditing = value; }
		}


		private TextSection textSection;

		/// <summary>
		/// The text section to be written to the page
		/// </summary>
		public TextSection TextSection
		{
			get { return textSection; }
			set { textSection = value; }
		}


		private bool showHeader = true;

		/// <summary>
		/// Specifies whether or not to show the header, which if true will be wrapped in &lt;h1&gt; tags.
		/// </summary>
		[Category("Behavior")]
		[DefaultValue(true)]
		public bool ShowHeader
		{
			get { return showHeader; }
			set { showHeader = value; }
		}

		#endregion





		/// <summary>
		/// Gets the tag that surrounds this control
		/// </summary>
		protected override HtmlTextWriterTag TagKey
		{
			get { return HtmlTextWriterTag.Div; }
		}

		/// <summary>
		/// Checks to see if rendering should start
		/// </summary>
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);


		}

		/// <summary>
		/// Renders the control as HTML
		/// </summary>
		protected override void RenderContents(HtmlTextWriter output)
		{

			if (textSection == null)
			{
				this.textSection = new TextSection();
				this.textSection.Header = "Header";
				this.textSection.Content = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean ligula. Nulla sollicitudin, est ut rutrum mollis, elit velit adipiscing ipsum, vitae tempus lacus leo non pede.";
			}

			if (showHeader && textSection.Header.Length > 0)
			{
				output.Write( "<h1>" + textSection.Header + "</h1>\n" );
			}

			if (textSection.Content.Length > 0)
			{
				output.Write( TextUtils.ToHtml( textSection.Content ) );
			}


			if (allowEditing)
			{
				TextBox headerBox = (TextBox)this.FindControl("headerBox");
				headerBox.Text = textSection.Header;
				TextBox contentBox = (TextBox)this.FindControl("contentBox");
				contentBox.Text = textSection.Content;
			}

			base.RenderContents(output);
			
		}

		/// <summary>
		/// Creates the child controls of this control
		/// </summary>
		protected override void CreateChildControls()
		{


			if (true)
			{
				Button editButton = new Button();
				editButton.ID = "editButton";
				editButton.Text = "Edit";
				editButton.Visible = this.allowEditing;
				editButton.Click += new EventHandler(editButton_Click);
				this.Controls.Add(editButton);

				PlaceHolder editPlaceHolder = new PlaceHolder();
				editPlaceHolder.ID = "editPlaceHolder";
				editPlaceHolder.Visible = false;

				TextBox headerBox = new TextBox();
				headerBox.ID = "headerBox";
				headerBox.Width = this.Width;
				headerBox.Style["display"] = "block";
				editPlaceHolder.Controls.Add(headerBox);

				TextBox contentBox = new TextBox();
				contentBox.TextMode = TextBoxMode.MultiLine;
				contentBox.Rows = 20;
				contentBox.Width = this.Width;
				contentBox.ID = "contentBox";
				contentBox.Style["display"] = "block";
				editPlaceHolder.Controls.Add(contentBox);

				Button saveButton = new Button();
				saveButton.Text = "Update Text";
				saveButton.Click += new EventHandler(saveButton_Click);
				saveButton.Style["margin"] = "5px";
				editPlaceHolder.Controls.Add(saveButton);

				Button cancelButton = new Button();
				cancelButton.Text = "Cancel";
				cancelButton.Style["margin"] = "5px";
				cancelButton.Click += new EventHandler(cancelButton_Click);
				editPlaceHolder.Controls.Add(cancelButton);

				this.Controls.Add(editPlaceHolder);


			}
			base.CreateChildControls();
			
		
		}

		void cancelButton_Click(object sender, EventArgs e)
		{
			PlaceHolder editPlaceHolder = (PlaceHolder)this.FindControl("editPlaceHolder");
			editPlaceHolder.Visible = false;
			Button editButton = (Button)this.FindControl("editButton");
			editButton.Visible = true;
		}

		void saveButton_Click(object sender, EventArgs e)
		{
			TextBox headerBox = (TextBox)this.FindControl("headerBox");
			TextBox contentBox = (TextBox)this.FindControl("contentBox");
			if (this.showHeader)
			{
				textSection.Header = headerBox.Text;
			}
			textSection.Content = contentBox.Text;
			textSection.Save();

			PlaceHolder editPlaceHolder = (PlaceHolder)this.FindControl("editPlaceHolder");
			editPlaceHolder.Visible = false;

			Button editButton = (Button)this.FindControl("editButton");
			editButton.Visible = true;
		
		}

		void editButton_Click(object sender, EventArgs e)
		{
			PlaceHolder editPlaceHolder = (PlaceHolder)this.FindControl("editPlaceHolder");
			editPlaceHolder.Visible = true;

			
			((Button)sender).Visible = false;
		}

		
	}
}
