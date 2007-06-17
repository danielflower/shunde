using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Shunde.Framework;
using Shunde.Framework.Columns;


namespace Shunde.Web
{

	/// <summary>
	/// A control to allow dates and/or times to be selected
	/// </summary>
	[DefaultProperty("SelectedColor")]
	[ToolboxData("<{0}:ColorPicker ID=colorPicker runat=server></{0}:ColorPicker>")]
	[ValidationProperty("SelectedColorCode")]
	public class DateTimePicker : Control, INamingContainer
	{

		#region Fields and properties




		/// <summary>
		/// The selected date
		/// </summary>
		[Browsable(true)]
		[Category("Data")]
		public DateTime? SelectedDate
		{
			get
			{
				object dt = ViewState["SelectedDate"];
				return (dt == null) ? null : (DateTime?)dt;
			}

			set
			{
				ViewState["SelectedDate"] = value;
			}
		}


		private DateTimePart partToPick = DateTimePart.DateAndOptionallyTime;

		/// <summary>
		/// The part of the date/time to pick
		/// </summary>
		[DefaultValue(DateTimePart.DateAndOptionallyTime)]
		public DateTimePart PartToPick
		{
			get { return partToPick;}
			set { partToPick = value;}
		}


		private string dateFormat = "dd/MM/yyyy";

		/// <summary>
		/// The format that dates should be displayed in
		/// </summary>
		[DefaultValue("dd/MM/yyyy")]
		public string DateFormat
		{
			get { return dateFormat; }
			set { dateFormat = value; }
		}
	

		#endregion

		/// <summary>
		/// Creates a new DateTimePicker object
		/// </summary>
		public DateTimePicker()
		{
		}



		/// <summary>
		/// Registers the Javascript files
		/// </summary>
		/// <param name="e"></param>
		protected override void OnInit(EventArgs e)
		{

			base.OnInit(e);

			this.Page.ClientScript.RegisterClientScriptInclude(typeof(DBObject), "ShundeScripts", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "Shunde.Resources.ShundeScripts.js"));
			this.Page.ClientScript.RegisterClientScriptInclude(typeof(DBObject), "DateTimePickerScripts", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "Shunde.Resources.DateTimePickerScripts.js"));

		}

		/// <summary>
		/// Creates the control
		/// </summary>
		protected override void CreateChildControls()
		{
			base.CreateChildControls();


			if (partToPick != DateTimePart.Time)
			{


				TextBox dateBox = new TextBox();
				DateTime? cur = SelectedDate;
				dateBox.Text = (cur == null) ? "" : cur.Value.ToString(dateFormat);
				dateBox.ID = "dateTB";
				this.Controls.Add(dateBox);

				



				string calName = this.ClientID + "_calName";
				string calDivName = this.ClientID + "_calDiv";

				Image calendarImage = new Image();
				calendarImage.ID = "anchor";
				this.Controls.Add(calendarImage);
				calendarImage.Attributes["name"] = calendarImage.ClientID;
				calendarImage.ImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(DateTimePicker), "Shunde.Resources.Calendar.gif");
				calendarImage.Style[HtmlTextWriterStyle.Cursor] = "pointer";
				calendarImage.Style[HtmlTextWriterStyle.VerticalAlign] = "middle";
				calendarImage.Width = Unit.Pixel(21);
				calendarImage.Height = Unit.Pixel(21);
				calendarImage.BorderStyle = BorderStyle.None;
				calendarImage.Attributes["onclick"] = calName + @".select(document.getElementById('" + dateBox.ClientID + @"'),'" + calendarImage.ClientID + @"','" + dateFormat + @"'); return false;";

				string javascript = @"
	var " + calDivName + @" = document.createElement('div');
	" + calDivName + @".id = '" + calDivName + @"';
	" + calDivName + @".style.position = 'absolute';
	" + calDivName + @".style.visibility = 'hidden';
	" + calDivName + @".style.backgroundColor = 'White';
	document.body.appendChild(" + calDivName + @");
	var " + calName + @" = new CalendarPopup('" + calDivName + @"');
	" + calName + @".showYearNavigation();
	" + calName + @".showYearNavigationInput();

";
				Page.ClientScript.RegisterStartupScript(typeof(DateTimePicker), this.ID, javascript, true);

			}

		}



	}


}
