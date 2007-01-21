using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Web.UI.HtmlControls;

[assembly: TagPrefix("Shunde.Web", "Shunde")]

namespace Shunde.Web
{

	/// <summary>
	/// A control to allow colors to be chosen from a swatch
	/// </summary>
	[DefaultProperty("SelectedColor")]
	[ToolboxData("<{0}:ColorPicker runat=server></{0}:ColorPicker>")]
	public class ColorPicker : WebControl, INamingContainer, IPostBackDataHandler
	{

		/// <summary>
		/// The selected color
		/// </summary>
		public Color SelectedColor
		{
			get
			{
				object c = ViewState["SelectedColor"];
				return (c == null) ? Color.Empty : (Color)c;
			}

			set
			{
				ViewState["SelectedColor"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the HTML code for the selected color
		/// </summary>
		public string SelectedColorCode
		{
			get
			{
				Color color = SelectedColor;
				return string.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
			}
			set
			{
				try
				{
					SelectedColor = ColorTranslator.FromHtml(value);
				}
				catch
				{
					SelectedColor = Color.Empty;
				}
			}
		}

		/// <summary>
		/// Checks to see whether or not a color has been selected
		/// </summary>
		public bool ColorSelected
		{
			get
			{
				return SelectedColor.IsEmpty;
			}
		}


		/// <summary>
		/// Registers the Javascript files
		/// </summary>
		/// <param name="e"></param>
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			this.Page.ClientScript.RegisterClientScriptInclude(typeof(ColorPicker), "ShundeScripts", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "Shunde.Resources.ShundeScripts.js"));
			this.Page.ClientScript.RegisterClientScriptInclude(typeof(ColorPicker), "ColorPickerScripts", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "Shunde.Resources.ColorPickerScripts.js"));

		}

		/// <summary>
		/// Creates the control
		/// </summary>
		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			Page.RegisterRequiresPostBack(this);

			HiddenField colourHF = new HiddenField();
			this.Controls.Add(colourHF);
			colourHF.ID = "selectedColorHF";
			if (!Page.IsPostBack)
			{
				colourHF.Value = SelectedColorCode;
			}


			HtmlGenericControl sample = new HtmlGenericControl("div");
			this.Controls.Add(sample);
			sample.ID = colourHF.ID + "_sample";
			sample.Style[HtmlTextWriterStyle.Cursor] = "pointer";
			sample.Style[HtmlTextWriterStyle.Width] = "40px";
			sample.Style["border"] = "1px solid black";
			sample.Style[HtmlTextWriterStyle.BackgroundColor] = SelectedColorCode;
			sample.Attributes["title"] = "Select a colour";
			sample.Attributes["onclick"] = "cp_pick(this, this.id.replace('_sample', ''));return false;";
			sample.InnerHtml = "&nbsp;";

		}






		#region IPostBackEventHandler Members

		/// <summary>
		/// Sets the selected-color property
		/// </summary>
		/// <param name="eventArgument"></param>
		public void RaisePostBackEvent(string eventArgument)
		{
			HiddenField colorHF = (HiddenField)this.FindControl("selectedColorHF");
			SelectedColorCode = colorHF.Value;
			throw new Exception(SelectedColorCode);
		}

		#endregion

		#region IPostBackDataHandler Members

		/// <summary>
		/// Loads the postback data
		/// </summary>
		public bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
		{
			string previous = SelectedColorCode;
			HiddenField colorHF = (HiddenField)this.FindControl("selectedColorHF");
			
			string newValue = colorHF.Value;
			if (newValue.Equals(previous))
			{
				return false;
			}

			HtmlGenericControl sampleControl = (HtmlGenericControl)this.FindControl("selectedColorHF_sample");
			sampleControl.Style[HtmlTextWriterStyle.BackgroundColor] = newValue;
			

			SelectedColorCode = newValue;
			return true;
		}

		/// <summary>
		/// Data Changed Event
		/// </summary>
		public void RaisePostDataChangedEvent()
		{
		}

		#endregion
	}
}
