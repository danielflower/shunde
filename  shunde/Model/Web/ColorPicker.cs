using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Web.UI.HtmlControls;
using Shunde.Framework;


namespace Shunde.Web
{

	/// <summary>
	/// A control to allow colors to be chosen from a swatch
	/// </summary>
	[DefaultProperty("SelectedColor")]
	[ToolboxData("<{0}:ColorPicker ID=colorPicker runat=server></{0}:ColorPicker>")]
	[ValidationProperty("SelectedColorCode")]
	public class ColorPicker : WebControl, INamingContainer, IPostBackDataHandler
	{

		#region Fields and properties

		/// <summary>
		/// The coloured div which displays the current colour
		/// </summary>
		private HtmlGenericControl sampleDiv = null;

		/// <summary>
		/// An event called when the selected color is changed
		/// </summary>
		public event SelectedColorChangedEventHandler SelectedColorChanged;

		private string onClientSelectedColorChanged;

		/// <summary>
		/// Javascript to be called when the color is changed.  The javascript can access the variable 'color' which will hold the HTML code of the color
		/// </summary>
		/// <example>alert('You have selected: ' + color);</example>
		[Category("Behavior")]
		public string OnClientSelectedColorChanged
		{
			get { return onClientSelectedColorChanged; }
			set { onClientSelectedColorChanged = value; }
		}
	

		/// <summary>
		/// The selected color
		/// </summary>
		[Browsable(true)]
		[Category("Data")]
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
		[Browsable(false)]
		public string SelectedColorCode
		{
			get
			{
				Color color = SelectedColor;
				if (color.IsEmpty)
				{
					return "";
				}
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
		public bool AColorIsSelected
		{
			get
			{
				return SelectedColor.IsEmpty;
			}
		}

		#endregion

		/// <summary>
		/// Creates a new ColorPicker object, and sets the default width and height
		/// </summary>
		public ColorPicker()
		{
			this.sampleDiv = new HtmlGenericControl("div");
			this.Width = new Unit(35, UnitType.Pixel);
			this.Height = new Unit(17, UnitType.Pixel);
		}

		/// <summary>
		/// The method to raise the event
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnSelectedColorChanged(SelectedColorChangedEventArgs e)
		{
			if (SelectedColorChanged != null)
			{
				SelectedColorChanged(this, e);
			}
		}


		/// <summary>
		/// Registers the Javascript files
		/// </summary>
		/// <param name="e"></param>
		protected override void OnInit(EventArgs e)
		{

			base.OnInit(e);

			this.Page.ClientScript.RegisterClientScriptInclude(typeof(DBObject), "ShundeScripts", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "Shunde.Resources.ShundeScripts.js"));
			this.Page.ClientScript.RegisterClientScriptInclude(typeof(DBObject), "ColorPickerScripts", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "Shunde.Resources.ColorPickerScripts.js"));

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

			

			this.Controls.Add(sampleDiv);
			sampleDiv.ID = colourHF.ID + "_sample";
			sampleDiv.Style[HtmlTextWriterStyle.Cursor] = "pointer";
			sampleDiv.Style[HtmlTextWriterStyle.Width] = this.Width.ToString();
			sampleDiv.Style[HtmlTextWriterStyle.Height] = this.Height.ToString();
			sampleDiv.Style["border"] = "1px solid black";
			SetBackground();
			sampleDiv.Attributes["title"] = "Select a colour";
			string clientJs = (string.IsNullOrEmpty(onClientSelectedColorChanged)) ? "" : Shunde.Utilities.TextUtils.JavascriptStringEncode(this.onClientSelectedColorChanged);
			sampleDiv.Attributes["onclick"] = "cp_pick(this, this.id.replace('_sample', ''), '" + clientJs + "');return false;";
			sampleDiv.InnerHtml = "&nbsp;";

		}


		private void SetBackground()
		{
			if (SelectedColor == Color.Empty)
			{
				sampleDiv.Style.Remove(HtmlTextWriterStyle.BackgroundColor);
				sampleDiv.Style[HtmlTextWriterStyle.BackgroundImage] = this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "Shunde.Resources.TransparencyIndicator.gif");
			}
			else
			{
				sampleDiv.Style.Remove(HtmlTextWriterStyle.BackgroundImage);
				sampleDiv.Style[HtmlTextWriterStyle.BackgroundColor] = SelectedColorCode;
			}
		}




		#region IPostBackDataHandler Members

		private bool colorHasChanged = false;

		/// <summary>
		/// Loads the postback data
		/// </summary>
		bool IPostBackDataHandler.LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
		{
			string previous = SelectedColorCode;
			HiddenField colorHF = (HiddenField)this.FindControl("selectedColorHF");
			
			string newValue = colorHF.Value;
			if (newValue.Equals(previous))
			{
				colorHasChanged = false;
				return false;
			}

			SelectedColorCode = newValue;
			SetBackground();
			
			colorHasChanged = true;
			return true;
		}

		/// <summary>
		/// Data Changed Event
		/// </summary>
		void IPostBackDataHandler.RaisePostDataChangedEvent()
		{
			if (colorHasChanged)
			{
				OnSelectedColorChanged(new SelectedColorChangedEventArgs(SelectedColor));
			}
		}

		#endregion


		/// <summary>
		/// Gets a DIV tag
		/// </summary>
		protected override HtmlTextWriterTag TagKey
		{
			get
			{
				return HtmlTextWriterTag.Div;
			}
		}

	}

	#region Events Stuff

	/// <summary>
	/// The delegate to handle the SelectedColorChanged event
	/// </summary>
	public delegate void SelectedColorChangedEventHandler(object sender, SelectedColorChangedEventArgs e);

	/// <summary>
	/// Event args for the SelectedColorChanged event, which includes the new color
	/// </summary>
	public class SelectedColorChangedEventArgs : EventArgs
	{

		private Color newColor;

		/// <summary>
		/// The color that has just been selected
		/// </summary>
		public Color NewColor
		{
			get { return newColor; }
			set { newColor = value; }
		}

		/// <summary>
		/// Creates this object and sets the given color
		/// </summary>
		/// <param name="newColor">The color that the picker has just changed to</param>
		public SelectedColorChangedEventArgs(Color newColor)
		{
			this.newColor = newColor;
		}

	}

	#endregion

}
