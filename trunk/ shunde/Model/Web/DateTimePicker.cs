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
	[DefaultProperty("SelectedDate")]
	[ToolboxData("<{0}:DateTimePicker ID=dateTimePicker runat=server />")]
	[ValidationProperty("SelectedDate")]
	public class DateTimePicker : WebControl, INamingContainer, IPostBackDataHandler
	{

		private static readonly char[] ValidTimeSeparators = new char[] { ':', '-', '.', ' ' };

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
				return (DateTime?)ViewState["SelectedDate"];
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

		private DateTime minimumValue = DateTime.MinValue;

		/// <summary>
		/// The minimum value that can be selected
		/// </summary>
		public DateTime MinimumValue
		{
			get { return minimumValue; }
			set { minimumValue = value; }
		}

		private DateTime maximumValue = DateTime.MaxValue;

		/// <summary>
		/// The maximum value that can be selected
		/// </summary>
		public DateTime MaximumValue
		{
			get { return maximumValue; }
			set { maximumValue = value; }
		}
	
	



		/// <summary>
		/// An event called when the selected date is changed
		/// </summary>
		public event DateChangedEventHandler DateChanged;


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

			this.Page.RegisterRequiresPostBack(this);


		}

		/// <summary>
		/// Creates the control
		/// </summary>
		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			DateTime? cur = SelectedDate;


			if (partToPick != DateTimePart.Time) // add the date box
			{

				// write the javascript stuff we need for the calendar dropdowns first
				this.Controls.Add(new LiteralControl(@"
<script type=""text/javascript"">

	document.write(getCalendarStyles());
 
</script>

"
				));


				TextBox dateBox = new TextBox();
				dateBox.Text = (cur == null || DateTimeColumn.DateIsNull(cur.Value)) ? "" : cur.Value.ToString(dateFormat);
				dateBox.ID = "dateTB";
				dateBox.TabIndex = 1;
				dateBox.Columns = 10;


				this.Controls.Add(CreateDateFormatValidator(dateBox));

				this.Controls.Add(dateBox);





				string calName = this.ClientID + "_calName";

				Image calendarImage = new Image();
				calendarImage.ID = "anchor";
				this.Controls.Add(calendarImage);
				calendarImage.Attributes["name"] = calendarImage.ClientID;
				calendarImage.ImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(DateTimePicker), "Shunde.Resources.Calendar.gif");
				calendarImage.Style[HtmlTextWriterStyle.Cursor] = "pointer";
				calendarImage.Style[HtmlTextWriterStyle.VerticalAlign] = "bottom";
				calendarImage.Width = Unit.Pixel(21);
				calendarImage.Height = Unit.Pixel(21);
				calendarImage.BorderStyle = BorderStyle.None;
				calendarImage.Attributes["onclick"] = calName + @".select(document.getElementById('" + dateBox.ClientID + @"'),'" + calendarImage.ClientID + @"','" + dateFormat + @"'); return false;";

				{ // add the div to hold the calendar

					HtmlGenericControl calDiv = new HtmlGenericControl("div");
					calDiv.ID = "calDiv";
					calDiv.Style[HtmlTextWriterStyle.Position] = "absolute";
					calDiv.Style[HtmlTextWriterStyle.Visibility] = "hidden";
					calDiv.Style[HtmlTextWriterStyle.BackgroundColor] = "White";
					this.Controls.Add(calDiv);

					string javascript = @"
	var " + calName + @" = new CalendarPopup('" + calDiv.ClientID + @"');
	" + calName + @".showYearNavigation();
	" + calName + @".showYearNavigationInput();
";
					Page.ClientScript.RegisterStartupScript(typeof(DateTimePicker), this.ID, javascript, true);

				}
			}



			if (partToPick != DateTimePart.Date)
			{

				Literal timeLit = new Literal();
				timeLit.Text = "&nbsp;&nbsp;<b>Time:</b> <font size=\"1\">(h:mm) </font>";
				this.Controls.Add(timeLit);

				TextBox timeTB = new TextBox();

				if (cur == null || DateTimeColumn.TimeIsNull(cur.Value))
				{
					timeTB.Text = "";
				}
				else
				{
					timeTB.Text = cur.Value.ToString("h:mm");
					if (cur.Value.Second > 0)
					{
						timeTB.Text += "." + cur.Value.Second.ToString("00");
					}

				}
				timeTB.ID = "timeTB";
				timeTB.TabIndex = 1;
				timeTB.Width = new Unit("40px");
				this.Controls.Add(timeTB);

				this.Controls.Add(CreateTimeFormatValidator(timeTB));

				DropDownList ampmDDL = new DropDownList();
				ampmDDL.ID = "ampmDDL";
				ampmDDL.Items.Add(new ListItem("am", "0"));
				ampmDDL.Items.Add(new ListItem("pm", "12"));
				ampmDDL.TabIndex = 1;
				ampmDDL.SelectedIndex = (cur != null && cur.Value.Hour > 12) ? 1 : 0;
				this.Controls.Add(ampmDDL);

			}

		}

		private Control CreateTimeFormatValidator(TextBox timeTB)
		{
			RegularExpressionValidator rev = new RegularExpressionValidator();
			rev.ControlToValidate = timeTB.ID;
			rev.Display = ValidatorDisplay.Dynamic;
			rev.EnableClientScript = true;
			rev.ErrorMessage = "The time you have entered is in an invalid format. Please try again.";
			rev.SetFocusOnError = true;
			rev.Text = " * ";

			string separators = "";
			foreach (char c in ValidTimeSeparators)
			{
				separators += "\\" + c;
			}

			rev.ValidationExpression = @"([0-2]|)\d([" + separators + "][0-5][0-9]([" + separators + "][0-5][0-9]|)|)";
			return rev;
		}

		private Control CreateDateFormatValidator(TextBox dateBox)
		{
			RangeValidator rv = new RangeValidator();
			rv.ControlToValidate = dateBox.ID;
			rv.Display = ValidatorDisplay.Dynamic;
			rv.EnableClientScript = true;
			rv.ErrorMessage = "Please enter a date in the format " + this.dateFormat + ".";
			rv.MaximumValue = maximumValue.ToShortDateString();
			rv.MinimumValue = minimumValue.ToShortDateString();
			rv.SetFocusOnError = true;
			rv.Text = "* ";
			rv.Type = ValidationDataType.Date;
			return rv;
		}

		/// <summary>
		/// Gets a SPAN tag
		/// </summary>
		protected override HtmlTextWriterTag TagKey
		{
			get
			{
				return HtmlTextWriterTag.Span;
			}
		}


		#region IPostBackDataHandler Members

		bool dateHasChanged = false;

		/// <summary>
		/// Updates the SelectedDate property on postback of the control
		/// </summary>
		public bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
		{

			DateTime? previous = SelectedDate;

			TextBox dateTB = (TextBox)this.FindControl("dateTB");
			TextBox timeTB = (TextBox)this.FindControl("timeTB");
			DropDownList ampmDDL = (DropDownList)this.FindControl("ampmDDL");

			DateTime? newValue = null;
			if (dateTB != null && dateTB.Text.Length > 0)
			{
				newValue = (DateTime?)DateTimeColumn.MakeTimeNull(DateTime.Parse(dateTB.Text));
			}

			if (timeTB != null && timeTB.Text.Length > 0)
			{
				if (newValue == null)
				{
					newValue = (DateTime?)DateTimeColumn.MakeTimeNull(new DateTime());
				}

				string[] timeParts = timeTB.Text.Split(ValidTimeSeparators);
				int hour = int.Parse(timeParts[0]);
				int minute = (timeParts.Length > 1) ? int.Parse(timeParts[1]) : 0;
				int second = (timeParts.Length > 2) ? int.Parse(timeParts[2]) : 0;

				// adjust for PM time
				if (hour <= 12 && ampmDDL.SelectedIndex == 1)
				{
					hour += 12;
				}

				// make sure we don't have 24 o'clock, etc
				hour = hour % 24;

				DateTime date = newValue.Value;
				newValue = (DateTime?)new DateTime(date.Year, date.Month, date.Day, hour, minute, second);

			}
			else if (newValue != null)
			{
				newValue = DateTimeColumn.MakeTimeNull(newValue.Value);
			}


			if (newValue.Equals(previous))
			{
				dateHasChanged = false;
				return false;
			}

			SelectedDate = newValue;

			return true;
		}

		/// <summary>
		/// Raises the DateChanged event when the date gets changed
		/// </summary>
		public void RaisePostDataChangedEvent()
		{
			if (dateHasChanged && DateChanged != null)
			{
				DateChanged(this, new DateChangedEventArgs(SelectedDate));
			}
		}

		#endregion
	}


	#region Events Stuff

	/// <summary>
	/// The delegate to handle the DateChanged event
	/// </summary>
	public delegate void DateChangedEventHandler(object sender, DateChangedEventArgs e);

	/// <summary>
	/// Event args for the DateChanged event, which includes the new date
	/// </summary>
	public class DateChangedEventArgs : EventArgs
	{

		private DateTime? newDate;

		/// <summary>
		/// The color that has just been selected
		/// </summary>
		public DateTime? NewDate
		{
			get { return newDate; }
			set { newDate = value; }
		}

		/// <summary>
		/// Creates this object and sets the given DateTime
		/// </summary>
		/// <param name="newDate">The DateTime that the control has just changed to</param>
		public DateChangedEventArgs(DateTime? newDate)
		{
			this.newDate = newDate;
		}

	}

	#endregion


}
