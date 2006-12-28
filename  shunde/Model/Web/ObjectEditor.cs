using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Shunde.Common;
using Shunde.Framework;
using Shunde.Utilities;
using System.ComponentModel;

[assembly: TagPrefix("Shunde.Web", "Shunde")]

namespace Shunde.Web
{


	/// <summary>
	/// A Control that renders text boxes to edit a DBObject
	/// </summary>
	[ToolboxData("<{0}:ObjectEditor runat=server></{0}:ObjectEditor>")]
	public class ObjectEditor : WebControl, INamingContainer
	{

		private string currentJavascriptVersion = "1.0.1";


		private string updaterName = "Object Editor";

		/// <summary>
		/// An optional Name to record who updated this object
		/// </summary>
		[Browsable(false)]
		public string UpdaterName
		{
			get { return updaterName; }
			set { updaterName = value; }
		}


		private DBObject obj = null;

		/// <summary>The object being edited</summary>
		public DBObject DBObject
		{
			get { return obj; }
			set { obj = value; }
		}

		private List<Control> extraControls = new List<Control>();

		/// <summary>
		/// A list of controls to place between the last input field and the submit buttons
		/// </summary>
		public List<Control> ExtraControls
		{
			get { return extraControls; }
			set { extraControls = value; }
		}
	
		

		private Unit numberTextBoxWidth = new Unit("70px");

		/// <summary>
		/// The default width of number textboxes
		/// </summary>
		[Category("Appearance")]
		[DefaultValue("70px")]
		public Unit NumberTextBoxWidth
		{
			get { return numberTextBoxWidth; }
			set { numberTextBoxWidth = value; }
		}


		private Unit dateTextBoxWidth = new Unit("70px");

		/// <summary>
		/// The default width of date textboxes
		/// </summary>
		[Category("Appearance")]
		[DefaultValue("70px")]
		public Unit DateTextBoxWidth
		{
			get { return dateTextBoxWidth; }
			set { dateTextBoxWidth = value; }
		}


		private Unit stringTextBoxWidth = new Unit("95%");

		/// <summary>
		/// The default width of string textboxes
		/// </summary>
		[Category("Appearance")]
		[DefaultValue("100%")]
		public Unit StringTextBoxWidth
		{
			get { return stringTextBoxWidth; }
			set { stringTextBoxWidth = value; }
		}


		private Unit multilineTextBoxHeight = new Unit("140px");

		/// <summary>
		/// The default width of string textboxes
		/// </summary>
		[Category("Appearance")]
		[DefaultValue("140px")]
		public Unit MultilineTextBoxHeight
		{
			get { return multilineTextBoxHeight; }
			set { multilineTextBoxHeight = value; }
		}

		

		private Unit fileUploaderWidth = new Unit("80%");

		/// <summary>
		/// The default width of file uploaders
		/// </summary>
		[Category("Appearance")]
		[DefaultValue("100%")]
		public Unit FileUploaderWidth
		{
			get { return fileUploaderWidth; }
			set { fileUploaderWidth = value; }
		}		

		/// <summary>
		/// 
		/// </summary>
		private Dictionary<string, ColumnInfo> info = new Dictionary<string, ColumnInfo>();
		
		private bool isForPublic = true;

		/// <summary>If set to false, then extra info is given in dropdown boxes. This should be false almost always.</summary>
		public bool IsForPublic
		{
			get { return isForPublic; }
			set { isForPublic = value; }
		}
		

		private bool showOnlySpecified = true;

		/// <summary>If true, then only those columns that have information in the info hashtable will be shown</summary>
		public bool ShowOnlySpecified
		{
			get { return showOnlySpecified; }
			set { showOnlySpecified = value; }
		}

		private bool autoPopulateSelections = false;

		/// <summary>If true, then selections will be automatically populated, if they aren't already</summary>
		/// <remarks>This simply returns all the (non-deleted) objects of the specified type in the database</remarks>
		public bool AutoPopulateSelections
		{
			get { return autoPopulateSelections; }
			set { autoPopulateSelections = value; }
		}

		private ButtonPanel buttonPanel;

		/// <summary>
		/// The button panel
		/// </summary>
		public ButtonPanel ButtonPanel
		{
			get { return buttonPanel; }
			set { buttonPanel = value; }
		}
	


		private BeforeSaveDelegate beforeSaveDelegate = null;

		/// <summary>If this is set and ObjectSaveDelegate is null, then this will be called after populating the object with the values from the object editor, but before the <see cref="Shunde.Framework.DBObject.Save" /> method is called.</summary>
		public BeforeSaveDelegate BeforeSaveDelegate
		{
			get { return beforeSaveDelegate; }
			set { beforeSaveDelegate = value; }
		}

		private ObjectSaveDelegate objectSaveDelegate = null;

		/// <summary>If this is set, then this will be called when the object editor 'save' button is pressed. If it is null, then the object editor will save the object.</summary>
		public ObjectSaveDelegate ObjectSaveDelegate
		{
			get { return objectSaveDelegate; }
			set { objectSaveDelegate = value; }
		}

		private AfterObjectSavedDelegate afterObjectSavedDelegate = null;

		/// <summary>If the object editor saves the object, then this will be called upon a successful save, upon which the delegate will probably redirect to another page.</summary>
		public AfterObjectSavedDelegate AfterObjectSavedDelegate
		{
			get { return afterObjectSavedDelegate; }
			set { afterObjectSavedDelegate = value; }
		}


		private ObjectDeleteDelegate objectDeleteDelegate = null;

		/// <summary>If this is set, then this will be called when the object editor 'delete' button is pressed. If it is null, then the object editor will delete the object.</summary>
		public ObjectDeleteDelegate ObjectDeleteDelegate
		{
			get { return objectDeleteDelegate; }
			set { objectDeleteDelegate = value; }
		}

		private AfterObjectDeletedDelegate afterObjectDeletedDelegate = null;

		/// <summary>If the object editor deletes the object, then this will be called upon a successful delete, upon which the delegate will probably redirect to another page.</summary>
		public AfterObjectDeletedDelegate AfterObjectDeletedDelegate
		{
			get { return afterObjectDeletedDelegate; }
			set { afterObjectDeletedDelegate = value; }
		}


		private EditCancelledDelegate editCancelledDelegate;

		/// <summary>Called when the 'cancel' button is pressed</summary>
		public EditCancelledDelegate EditCancelledDelegate
		{
			get { return editCancelledDelegate; }
			set { editCancelledDelegate = value; }
		}









		private Label infoMessage;








		/// <summary>
		/// Creates a new Object Editor
		/// </summary>
		public ObjectEditor()
		{
			this.Width = new Unit("100%");
		}



		/// <summary>
		/// The OnInit event
		/// </summary>
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			infoMessage = new Label();
			infoMessage.CssClass = "infoMessage";
			this.Controls.Add(infoMessage);

			buttonPanel = new ButtonPanel();






		}

		void deleteButton_Click(object sender, EventArgs e)
		{

			if (ObjectDeleteDelegate != null)
			{
				ObjectDeleteDelegate();
				return;
			}

			DBObject.IsDeleted = true;
			try
			{
				DBObject.Save();
				if (AfterObjectDeletedDelegate != null)
				{
					AfterObjectDeletedDelegate();
				}
			}
			catch (ValidationException vex)
			{
				infoMessage.Text = vex.Message;
			}
			catch (Exception ex)
			{
				infoMessage.Text = ((WebPage)Page).HandleException(ex, Page.Request, "Error while deleting " + DBObject);
			}
		}

		void cancelButton_Click(object sender, EventArgs e)
		{
			if (EditCancelledDelegate != null)
			{
				EditCancelledDelegate();
			}
			else
			{
				throw new Exception("No delegate for cancelling has been defined");
			}
		}

		void saveButton_Click(object sender, EventArgs e)
		{


			if (!Page.IsValid)
			{
				return;
			}

			if (ObjectSaveDelegate != null)
			{
				ObjectSaveDelegate();
				return;
			}

			DBObject.LastUpdatedBy = UpdaterName;

			try
			{

				SaveUserInputToObject();

			}
			catch (ValidationException vex)
			{
				infoMessage.Text = vex.Message;
				return;
			}




			try
			{

				if (BeforeSaveDelegate != null)
				{
					BeforeSaveDelegate();
				}

				DBObject.Save();
				if (AfterObjectSavedDelegate != null)
				{
					AfterObjectSavedDelegate();
				}
			}
			catch (ValidationException vex)
			{
				infoMessage.Text = vex.Message;
			}
			catch (Exception ex)
			{
				string exceptionHandled = ((WebPage)Page).HandleException(ex, Page.Request, "Error while saving " + DBObject);
				infoMessage.Text = "There was an error while saving that. " + exceptionHandled;
			}
		}


		/// <summary>
		/// Gets the tag that surrounds this control
		/// </summary>
		protected override HtmlTextWriterTag TagKey
		{
			get { return HtmlTextWriterTag.Div; }
		}

		/// <summary>
		/// Renders the begin tag
		/// </summary>
		public override void RenderBeginTag(HtmlTextWriter writer)
		{
			if (this.Width.IsEmpty)
			{
				this.Width = new Unit("95%");
			}
			base.RenderBeginTag(writer);
		}





		private string controlId;

		/// <summary>
		/// Makes the table
		/// </summary>
		public void PopulateTable()
		{

			this.controlId = this.ClientID;

			// write the javascript stuff we need for the calendar dropdowns first
			this.Controls.Add( new LiteralControl(@"
<SCRIPT LANGUAGE=""JavaScript"">

	var " + controlId + @"_scriptLoaded = ('function' == typeof oe_getScriptVersion);

	if (" + controlId + @"_scriptLoaded) {
		if ( oe_getScriptVersion() != '" + currentJavascriptVersion + @"' ) {
			alert( 'Warning, the Object Editor requires Object Editor Script File version " + currentJavascriptVersion + @", however the loaded version is ' + oe_getScriptVersion() );
		}
		document.write(getCalendarStyles());
	} else {
		alert( 'Warning, the Object Editor Javascript File version " + currentJavascriptVersion + @" has not been loaded. Some parts of the editor will not work until this is loaded.' );
	}

 
</SCRIPT>

<select id=""oe_searchListBox"" name=""oe_searchListBox"" ondblclick=""oe_doubleClick(this);"" onmouseover=""oe_mouseOver(this, true);"" onmouseout=""oe_mouseOver(this,false);"" onchange=""oe_selectCurrentlySelected();"" multiple=""multiple"" style=""height:150px;position:absolute;left:0px;top:0px;visibility:hidden;""></select>

"
			) );



			Type objType = DBObject.GetType();
			ObjectInfo oi = ObjectInfo.GetObjectInfo(objType);

			List<ComparableRow> panels = new List<ComparableRow>();


			// a unique number, which is required in various places
			int uniqueNumber = 0;

			foreach (DBTable table in oi.Tables)
			{


				uniqueNumber++;

				foreach (DBColumn col in table.Columns)
				{

					uniqueNumber++;



					ColumnInfo ci;
					if (info.ContainsKey(col.Name))
					{
						ci = info[col.Name];
					}
					else
					{

						ci = new ColumnInfo();


						// by default, put the Name at the top
						if (col.Name.Equals("name") && !showOnlySpecified)
						{
							ci.DisplayOrder = 0;
						}
						else if (col.Name.Equals("updateId"))
						{
							ci.IsInvisible = true;
							info.Add(col.Name, ci);
						}
						else if (col.Name.Equals("lastUpdatedBy") || col.Name.Equals("className") || col.Name.Equals("lastUpdate"))
						{
							ci.ShowThisColumn = false;
							info.Add(col.Name, ci);
						}
						else if (showOnlySpecified)
						{
							ci.ShowThisColumn = false;
						}

					}
					if (ci.FriendlyName.Length == 0)
					{
						ci.FriendlyName = TextUtils.MakeFriendly(col.Name);
					}

					string friendlyName = ci.FriendlyName;

					if (!ci.ShowThisColumn)
					{
						continue;
					}



					ComparableRow row = new ComparableRow();
					row.DisplayOrder = ci.DisplayOrder;


					FieldInfo fi = col.FieldInfo;

					object value = fi.GetValue(DBObject);

					string label = friendlyName;
					if (!col.AllowNulls || ci.RequiredField == RequiredField.Yes)
					{
						label += "<font color=red>*</font>";
					}
					


					if (ci.IsInvisible)
					{
						HtmlInputHidden hih = new HtmlInputHidden();
						hih.ID = col.Name;
						hih.Value = value.ToString();
						this.Controls.Add(hih);
						row.Visible = false;
					}
					else if (col.Type.Equals(typeof(bool)))
					{

						row.Cells.Add(new TableCell());

						CheckBox cb = new CheckBox();
						cb.Checked = System.Convert.ToBoolean(value);
						cb.Text = "<b>" + friendlyName + "</b>";
						cb.ID = col.Name;
						cb.TabIndex = 1;

						Literal lit2 = new Literal();
						lit2.Text = " " + ci.GetMoreInfoAsJavascriptPopupHtml();


						TableCell td = new TableCell();
						td.Controls.Add(cb);
						td.Controls.Add(lit2);
						row.Cells.Add(td);

					}
					else if (col.Type.Equals(typeof(string)) || col.Type.Equals(typeof(short)) || col.Type.Equals(typeof(int)) || col.Type.Equals(typeof(long)) || col.Type.Equals(typeof(float)) || col.Type.Equals(typeof(double)))
					{

						TableCell tc = new TableCell();
						TableCell th = null;

						if (ci.UseRichTextEditor)
						{
							th = tc;
							th.Controls.Add(new LiteralControl("<div><strong>" + label + "</strong></div>"));
							tc.ColumnSpan = 2;
						}
						else
						{
							th = new TableHeaderCell();
							th.Controls.Add(new LiteralControl(label));
							row.Cells.Add(th);
						}
						TextBox tb = new TextBox();
						tb.Text = (DBColumn.IsColumnNull(value)) ? "" : value.ToString();
						tb.ID = col.Name;
						tb.TabIndex = 1;

						bool isNull = DBColumn.IsColumnNull(value);

						if (col.Type.Equals(typeof(string)))
						{
							tb.Width = (ci.TextboxWidth.IsEmpty) ? stringTextBoxWidth : ci.TextboxWidth;
							if (col.MaxLength == -1)
							{
								tb.TextMode = TextBoxMode.MultiLine;
								if (ci.UseRichTextEditor)
								{
									tb.Attributes["UseRichTextEditor"] = "true";
									tb.Height = (ci.TexboxHeight.IsEmpty) ? multilineTextBoxHeight : new Unit(300);
								}
								else
								{
									tb.Height = (ci.TexboxHeight.IsEmpty) ? multilineTextBoxHeight : ci.TexboxHeight;
								}
							}
							else
							{
								tb.MaxLength = col.MaxLength;
								if (col.Name.Equals("password"))
								{
									tb.TextMode = TextBoxMode.Password;
								}
							}

						}
						else if (col.Type.Equals(typeof(DateTime)))
						{
							tb.Width = (ci.TextboxWidth.IsEmpty) ? dateTextBoxWidth : ci.TextboxWidth;
							if (isNull)
							{
								tb.Text = "";
							}
						}
						else
						{
							tb.Width = (ci.TextboxWidth.IsEmpty) ? numberTextBoxWidth : ci.TextboxWidth;

							if (isNull)
							{
								tb.Text = "";
							}

						}


						// add validation
						if (ci.RequiredField == RequiredField.Yes || (ci.RequiredField == RequiredField.UseColumnDefault && !col.AllowNulls && !(col.Type.Equals(typeof(String)) && col.MinLength == 0)))
						{

							// but, don't make it required if it's a password and it already exists
							if (tb.TextMode == TextBoxMode.Password && DBObject.Exists())
							{
								ci.MoreInfo += "<div>You may leave this blank to keep your existing password.</div>";
							}
							else
							{
								RequiredFieldValidator rfv = new RequiredFieldValidator();
								rfv.ControlToValidate = tb.ID;
								rfv.ErrorMessage = "Please enter a value for " + friendlyName;
								rfv.Display = ValidatorDisplay.Static;
								rfv.Text = "*";
								rfv.SetFocusOnError = true;
								th.Controls.Add(rfv);
							}
						}

						if (col.RegularExpression != null)
						{
							RegularExpressionValidator rev = new RegularExpressionValidator();
							rev.ControlToValidate = tb.ID;
							string msg = (col.RegularExpressionErrorMessage != null) ? col.RegularExpressionErrorMessage : "The value for " + ci.FriendlyName + " is not allowed.";
							rev.ErrorMessage = msg;
							rev.Display = ValidatorDisplay.Dynamic;
							rev.Text = "*";
							rev.ValidationExpression = col.RegularExpression;
							rev.SetFocusOnError = true;
							th.Controls.Add(rev);
						}

						if (ci.ValidationRegex.Length > 0)
						{
							RegularExpressionValidator rev = new RegularExpressionValidator();
							rev.ControlToValidate = tb.ID;
							rev.ErrorMessage = ci.RegexErrorMessage;
							rev.Display = ValidatorDisplay.Dynamic;
							rev.Text = "*";
							rev.ValidationExpression = ci.ValidationRegex;
							rev.SetFocusOnError = true;
							th.Controls.Add(rev);
						}

						if (col.MinAllowed != null || col.MaxAllowed != null)
						{
							RangeValidator rv = new RangeValidator();
							rv.ID = col.Name + "RV";

							if (col.Type.Equals(typeof(string)))
							{
								rv.Type = ValidationDataType.String;
							}
							else if (col.Type.Equals(typeof(short)) || col.Type.Equals(typeof(int)) || col.Type.Equals(typeof(long)))
							{
								rv.Type = ValidationDataType.Integer;
							}
							else
							{
								rv.Type = ValidationDataType.Double;
							}

							rv.ControlToValidate = tb.ID;
							rv.Display = ValidatorDisplay.Dynamic;
							rv.Text = "*";
							rv.SetFocusOnError = true;

							if (col.MinAllowed != null && col.MaxAllowed != null)
							{
								rv.MinimumValue = col.MinAllowed.ToString();
								rv.MaximumValue = col.MaxAllowed.ToString();
								rv.ErrorMessage = "The value of " + friendlyName + " must be between " + col.MinAllowed + " and " + col.MaxAllowed + " (inclusive).";
							}
							else if (col.MinAllowed != null)
							{
								rv.MinimumValue = col.MinAllowed.ToString();
								rv.MaximumValue = int.MaxValue.ToString();
								rv.ErrorMessage = "The minimum value allowed for " + friendlyName + " is " + col.MinAllowed;
							}
							else
							{
								rv.MaximumValue = col.MaxAllowed.ToString();
								rv.MinimumValue = int.MinValue.ToString();
								rv.ErrorMessage = "The maximum value allowed for " + friendlyName + " is " + col.MaxAllowed;
							}

							th.Controls.Add(rv);
						}


						tc.Controls.Add(tb);



						if (ci.MoreInfo.Length > 0)
						{
							tc.Controls.Add(new LiteralControl("<div class=\"moreInfo\">" + ci.MoreInfo + "</div>"));
						}

						if (tb.TextMode == TextBoxMode.Password)
						{

							HtmlGenericControl confirmDiv = new HtmlGenericControl("div");
							confirmDiv.Controls.Add(new LiteralControl("Please confirm your password:"));
							confirmDiv.ID = tb.ID + "PasswordConfirmDiv";


							tb.Attributes["onkeyup"] = "document.getElementById(this.id + 'PasswordConfirmDiv').style.display = (this.value.length == 0) ? 'none' : 'block';";
							confirmDiv.Style[HtmlTextWriterStyle.Display] = "none";

							tc.Controls.Add(confirmDiv);
							TextBox passwordConfirmTB = new TextBox();
							passwordConfirmTB.ID = tb.ID + "PasswordConfirm";
							passwordConfirmTB.TextMode = TextBoxMode.Password;
							passwordConfirmTB.Width = tb.Width;
							passwordConfirmTB.TabIndex = 1;

							CompareValidator cv = new CompareValidator();
							cv.ControlToCompare = tb.ID;
							cv.ControlToValidate = passwordConfirmTB.ID;
							cv.ErrorMessage = "Your passwords did not match";
							cv.Text = " * ";
							cv.SetFocusOnError = true;

							if (!DBObject.Exists())
							{
								RequiredFieldValidator rfv = new RequiredFieldValidator();
								rfv.ControlToValidate = passwordConfirmTB.ID;
								rfv.ErrorMessage = "Please enter a value for the confirmation password field";
								rfv.Display = ValidatorDisplay.Static;
								rfv.Text = "*";
								rfv.SetFocusOnError = true;
								confirmDiv.Controls.Add(rfv);
							}

							confirmDiv.Controls.Add(cv);
							confirmDiv.Controls.Add(passwordConfirmTB);
						}

						row.Cells.Add(tc);


					}
					else if (col.Type.Equals(typeof(DateTime)))
					{


						DateTime dt = Convert.ToDateTime(value);

						TableHeaderCell th = new TableHeaderCell();
						th.Controls.Add(new LiteralControl(label + ci.GetMoreInfoAsJavascriptPopupHtml()));

						row.Cells.Add(th);

						TableCell tc = new TableCell();
						row.Cells.Add(tc);

						TextBox tb = new TextBox();

						tb.Text = (dt.Equals(DBColumn.DateTimeNullValue)) ? "" : dt.ToString("dd/MM/yyyy");
						tb.ID = col.Name;
						tb.TabIndex = 1;
						tb.Width = (ci.TextboxWidth.IsEmpty) ? dateTextBoxWidth : ci.TextboxWidth;


						// add validation
						if (!col.AllowNulls)
						{
							RequiredFieldValidator rfv = new RequiredFieldValidator();
							rfv.ControlToValidate = tb.ID;
							rfv.ErrorMessage = "Please enter a value for " + friendlyName;
							rfv.Display = ValidatorDisplay.Static;
							rfv.Text = "*";
							th.Controls.Add(rfv);
						}

						if (col.MinAllowed != null || col.MaxAllowed != null)
						{
							RangeValidator rv = new RangeValidator();
							rv.ID = col.Name + "RV";
							rv.Type = ValidationDataType.Date;
							rv.ControlToValidate = tb.ID;
							rv.Display = ValidatorDisplay.Dynamic;
							rv.Text = "*";

							if (col.MinAllowed != null & col.MaxAllowed != null)
							{
								rv.MinimumValue = ((DateTime)col.MinAllowed).ToString("yyyy/MM/dd");
								rv.MaximumValue = ((DateTime)col.MaxAllowed).ToString("yyyy/MM/dd");
								rv.ErrorMessage = "The value of " + friendlyName + " must be between " + col.MinAllowed + " and " + col.MaxAllowed + " (inclusive).";
							}
							else if (col.MinAllowed != null)
							{
								rv.MinimumValue = ((DateTime)col.MinAllowed).ToString("yyyy/MM/dd");
								rv.MaximumValue = "2500/01/01";
								rv.ErrorMessage = "The minimum value allowed for " + friendlyName + " is " + col.MinAllowed;
							}
							else
							{
								rv.MaximumValue = ((DateTime)col.MaxAllowed).ToString("yyyy/MM/dd");
								rv.MinimumValue = "1900/01/01";
								rv.ErrorMessage = "The maximum value allowed for " + friendlyName + " is " + col.MaxAllowed;
							}

							th.Controls.Add(rv);


						}





						string calName = controlId + "_cal_" + col.Name;
						string tbName = controlId + "_" + tb.ClientID;

						Literal lit2 = new Literal();
						lit2.Text = "<b>Date:</b> <font size=\"1\">(dd/mm/yyyy) </font>";
						tc.Controls.Add(lit2);

						tc.Controls.Add(tb);


						string anchorName = controlId + "_anchor" + uniqueNumber + "xx";
						string divName = controlId + "_caldiv" + uniqueNumber;

						Literal javascriptStuff = new Literal();
						javascriptStuff.Text = @"
<SCRIPT LANGUAGE=""JavaScript"">
if (" + controlId + @"_scriptLoaded) {
	var " + calName + @" = new CalendarPopup(""" + divName + @""");
	" + calName + @".showYearNavigation();
	" + calName + @".showYearNavigationInput();
}
<" + @"/SCRIPT>
					";

						

						Literal calendarDivText = new Literal();
						calendarDivText.Text = "<DIV ID=\"" + divName + "\" STYLE=\"position:absolute;visibility:hidden;background-color:white;\"></DIV>";

						Literal lit3 = new Literal();
						lit3.Text = @"
<A HREF=""javascript:void(null);"" onClick=""" + calName + @".select(document.getElementById('" + tbName + @"'),'" + anchorName + @"','dd/MM/yyyy'); return false;"" NAME=""" + anchorName + @""" ID=""" + anchorName + @"""><img src=""" + Page.Request.ApplicationPath + @"/images/calendar.gif"" height=""21"" width=""21"" border=""0"" style=""border:none;margin:0px 0px 0px 0px;"" align=""absmiddle"" /></A>
					";

						tc.Controls.Add(lit3);

						this.Controls.Add(javascriptStuff);
						this.Controls.Add(calendarDivText);


						if (ci.ShowTimeWithDate)
						{
							Literal timeLit = new Literal();
							timeLit.Text = "&nbsp;&nbsp;<b>Time:</b> <font size=\"1\">(h:mm) </font>";
							tc.Controls.Add(timeLit);
							TextBox timeTB = new TextBox();
							timeTB.Text = (dt.Equals(DBColumn.DateTimeNullValue) || TextUtils.IsMidnight(dt)) ? "" : dt.ToString("h:mm");
							timeTB.ID = col.Name + "_shundeTime";
							timeTB.TabIndex = 1;
							timeTB.Width = new Unit("40px");
							tc.Controls.Add(timeTB);

							DropDownList ampmDDL = new DropDownList();
							ampmDDL.ID = col.Name + "_ampmDDL";
							ampmDDL.Items.Add(new ListItem("am", "0"));
							ampmDDL.Items.Add(new ListItem("pm", "12"));
							ampmDDL.TabIndex = 1;
							ampmDDL.SelectedIndex = (dt.Hour > 12) ? 1 : 0;
							tc.Controls.Add(ampmDDL);

						}



					}
					else if (col.Type.Equals(typeof(BinaryData)))
					{


						TableHeaderCell th = new TableHeaderCell();
						th.Controls.Add(new LiteralControl(label + " " + ci.GetMoreInfoAsJavascriptPopupHtml()));

						
						HtmlInputFile hif = new HtmlInputFile();
						hif.ID = col.Name;
						Unit width = ((ci.TextboxWidth.IsEmpty) ? fileUploaderWidth : ci.TextboxWidth);
						hif.Style["width"] = width.ToString();
						hif.Attributes["TabIndex"] = "1";

						TableCell tc = new TableCell();
						tc.Controls.Add(hif);

						if (DBColumn.IsColumnNull(value))
						{
							// add validation if nulls are not allowed
							if (!col.AllowNulls)
							{
								RequiredFieldValidator rfv = new RequiredFieldValidator();
								rfv.ControlToValidate = hif.ID;
								rfv.ErrorMessage = "Please select a file for " + friendlyName;
								rfv.Display = ValidatorDisplay.Static;
								rfv.Text = "*";
								th.Controls.Add(rfv);
							}

						}
						else
						{
							BinaryData bd = (BinaryData)value;

							Literal curFileLit = new Literal();
							curFileLit.Text = "<br/>Current file type: " + bd.MimeType + " (" + TextUtils.GetFriendlyFileSize(bd.Size) + ") ";

							if (ci.ViewBinaryDataUrl.Length > 0)
							{
								curFileLit.Text += "<a href=\"" + ci.ViewBinaryDataUrl + DBObject.Id + "&fieldName=" + col.Name + "\" target=\"_blank\">[view]</a> ";
							}


							tc.Controls.Add(curFileLit);

							if (col.AllowNulls)
							{
								CheckBox cb = new CheckBox();
								cb.ID = col.Name + "_deleteFile";
								cb.Checked = false;
								cb.Text = " <b>Delete</b>";
								tc.Controls.Add(cb);
							}

						}

						row.Cells.Add(th);
						row.Cells.Add(tc);


					}
					else if (col.Type.Equals(typeof(DBObject)) || col.Type.IsSubclassOf(typeof(DBObject)))
					{

						

						// populate if required
						if (ci.Selections == null && (ci.AutoPopulate || autoPopulateSelections))
						{
							ObjectInfo foreignOI = ObjectInfo.GetObjectInfo(col.Type);
							ci.Selections = DBObject.GetObjects(foreignOI.GetSelectStatement(ci.MaxAllowedInDropDown + 1) + " WHERE isDeleted = 0 ORDER BY displayOrder ASC", col.Type);
						}




						if (ci.Selections != null)
						{

							string cur = (value == null) ? "" : ((DBObject)value).Id.ToString();


							TableHeaderCell th = new TableHeaderCell();
							th.Controls.Add(new LiteralControl(label + ci.GetMoreInfoAsJavascriptPopupHtml()));
							row.Cells.Add(th);

							TableCell inputCell = new TableCell();
							row.Cells.Add(inputCell);

							if (ci.SelectionMode == SelectionMode.TextBox || (ci.SelectionMode == SelectionMode.Default && ci.Selections.Count > ci.MaxAllowedInDropDown && ci.SelectionsPopupUrl.Length > 0))
							{

								// *******************************
								// Use a textbox window to select
								// *******************************

								Literal popupLit = new Literal();
								Literal javascriptLiteral = new Literal();

								string curName = "";
								if (value != null)
								{
									DBObject curObj = DBObject.GetObject(cur);
									curName = curObj.FriendlyName;
								}


								HtmlInputHidden hih = new HtmlInputHidden();
								hih.ID = col.Name;
								hih.Value = cur;

								javascriptLiteral.Text = @"

<script language=""JavaScript"">

	function object" + uniqueNumber + @"Selected( objectId, objectName ) {
		var idField  = document.getElementById( '" + controlId + "_" + hih.ClientID + @"' );
		idField.value = objectId;

		var nameField = document.getElementById( '" + controlId + "_" + col.Name + @"_cosmeticName' );
		nameField.value = objectName;

		if (idField.onchange) {
			idField.onchange();
		}
		if (nameField.onchange) {
			nameField.onchange();
		}
		//object" + uniqueNumber + @"NameDiv.innerHTML = objectName;
	}

</" + @"script>
								";

								if (ci.SelectionsPopupUrl.Length > 0)
								{
									popupLit.Text = @"

<a href=""javascript:popupFor" + this.ID + "( '" + ci.SelectionsPopupUrl + @"object" + uniqueNumber + @"Selected&type=" + HttpUtility.UrlEncode(col.Type.FullName) + @"&assName=" + HttpUtility.UrlEncode(col.Type.Assembly.FullName) + @"&dbName=" + HttpUtility.UrlEncode(DBUtils.GetSqlConnection().Database) + @"', 'selectObjectWindow', 600, 800 );"">[select]</a>

<a href=""javascript:object" + uniqueNumber + @"Selected( '', '' );"">[un-select]</a>

								";
								}

								inputCell.Controls.Add(popupLit);
								this.Controls.Add(javascriptLiteral);
								this.Controls.Add(hih);

								TextBox curNameTB = new TextBox();
								curNameTB.ID = col.Name + "_cosmeticName";
								curNameTB.Text = curName;

								if (ci.SearchBoxUrl.Length == 0)
								{
									curNameTB.Attributes["style"] = "border: none;width: 300px;background: none;";
								}
								else
								{
									curNameTB.Attributes["onfocus"] = "oe_searchBoxUpdate(this, document.getElementById('" + hih.ClientID + "'), '" + ci.SearchBoxUrl + "');";
									curNameTB.Attributes["onkeyup"] = "oe_searchBoxUpdate(this, document.getElementById('" + hih.ClientID + "'), '" + ci.SearchBoxUrl + "');";
									curNameTB.Attributes["onkeydown"] = "return oe_searchKeyDown(event);";
									curNameTB.Attributes["onblur"] = "oe_unselectResultsBox();";
									curNameTB.Attributes["autocomplete"] = "off";
									curNameTB.TabIndex = 1;
									curNameTB.Width = new Unit("200px");
								}

								inputCell.Controls.Add(curNameTB);
















							} else
							{

								// *******************************
								// Use a drop down list
								// *******************************


								ListControl ddl = (ci.SelectionMode == SelectionMode.RadioButtonList) ? (ListControl) new RadioButtonList() : (ListControl) new DropDownList();
								ddl.ID = col.Name;
								ddl.TabIndex = 1;

								// add validation
								if (!col.AllowNulls)
								{
									RequiredFieldValidator rfv = new RequiredFieldValidator();
									rfv.ControlToValidate = ddl.ID;
									rfv.ErrorMessage = "Please select a value for " + friendlyName;
									rfv.Display = ValidatorDisplay.Dynamic;
									rfv.Text = "*";
									th.Controls.Add(rfv);
								}

								if (col.AllowNulls)
								{
									string name = (ddl is DropDownList) ? ci.NoSelectionName : HttpUtility.HtmlEncode(ci.NoSelectionName);
									ListItem li = new ListItem( name, "");
									li.Selected = (cur.Length == 0);
									ddl.Items.Add(li);
								}
								else if (ddl is DropDownList)
								{
									ddl.Items.Add(new ListItem("<Select One>", ""));
								}




								foreach (DBObject selectObj in ci.Selections)
								{

									string selectObjId = selectObj.Id.ToString();


									string oName;
									Type ot = selectObj.GetType();

									if (isForPublic)
									{
										oName = selectObj.FriendlyName;
									}
									else
									{
										oName = selectObj.Id.ToString() + ") " + selectObj.FriendlyName + " (" + ot.FullName + ")";
									}


									ListItem li = new ListItem(oName, selectObjId + "," + ot.FullName);
									if (selectObjId.Equals(cur))
									{
										li.Selected = true;
									}
									ddl.Items.Add(li);
								}

								inputCell.Controls.Add(ddl);

							}






						}
						else
						{

							TableCell tc = new TableCell();
							tc.ColumnSpan = 2;
							tc.Text = "<font color=red><b>" + ci.FriendlyName + "</b></font>";
							row.Cells.Add(tc);
						}
						
												
						
					}
					else
					{
						throw new ShundeException("ObjectEditor has come across this foreign type: " + col.Type);
					}

					panels.Add(row);

					

				}

			}

			// now add the text/hr lines
			foreach (KeyValuePair<string,ColumnInfo> keyValuePair in info)
			{
				if (!keyValuePair.Key.StartsWith("-"))
				{
					continue;
				}
				ComparableRow cr = new ComparableRow();
				TableCell infoCell = new TableCell();
				infoCell.ColumnSpan = 2;

				if (keyValuePair.Value.MoreInfo.Length == 0)
				{
					infoCell.Text = "<hr />";
				}
				else
				{
					infoCell.Text = keyValuePair.Value.MoreInfo;
				}

				cr.Cells.Add(infoCell);
				cr.DisplayOrder = keyValuePair.Value.DisplayOrder;
				panels.Add(cr);
			}

			panels.Sort();

			{
				Table table = new Table();
				table.CssClass = "ObjectEditorTable";
				table.Width = new Unit("100%");
				table.Style["table-layout"] = "fixed";

				{
					TableRow tr = new TableRow();
					table.Rows.Add(tr);
					{
						TableCell tc = new TableCell();
						tc.Style[HtmlTextWriterStyle.Width] = "33%";
						tc.Style[HtmlTextWriterStyle.Height] = "1px";
						tr.Cells.Add(tc);
					}
					{
						TableCell tc = new TableCell();
						tc.Style[HtmlTextWriterStyle.Width] = "67%";
						tc.Style[HtmlTextWriterStyle.Height] = "1px";
						tr.Cells.Add(tc);
					}
				}

				foreach (ComparableRow tr in panels)
				{
					table.Rows.Add(tr);
				}

				if (extraControls.Count > 0)
				{
					TableRow extraControlsRow = new TableRow();
					table.Rows.Add(extraControlsRow);
					TableCell ecc = new TableCell();
					ecc.ColumnSpan = 2;
					extraControlsRow.Cells.Add(ecc);
					foreach (Control c in extraControls)
					{
						ecc.Controls.Add(c);
					}
				}

				TableRow buttonRow = new TableRow();
				buttonRow.Cells.Add(new TableHeaderCell());

				{
					TableCell tc = new TableCell();
					buttonRow.Cells.Add(tc);

					tc.Controls.Add(buttonPanel);
				}

				if (!DBObject.Exists())
				{
					buttonPanel.DeleteButton.Visible = false;
				}

				table.Rows.Add(buttonRow);

				this.Controls.Add(table);




			}



			buttonPanel.SaveButton.Click += new EventHandler(saveButton_Click);
			buttonPanel.CancelButton.Click += new EventHandler(cancelButton_Click);
			buttonPanel.DeleteButton.Click += new EventHandler(deleteButton_Click);

		}








		/// <summary>
		/// Writes the user-inputted values to the object
		/// </summary>
		public void SaveUserInputToObject()
		{


			Type objType = DBObject.GetType();
			ObjectInfo oi = ObjectInfo.GetObjectInfo(objType);


			Control panel = this;

			foreach (DBTable table in oi.Tables)
			{
				
				foreach (DBColumn col in table.Columns)
				{
					
					ColumnInfo ci;
					if (info.ContainsKey(col.Name))
					{
						ci = info[col.Name];
					}
					else
					{
						ci = new ColumnInfo();
						ci.FriendlyName = TextUtils.MakeFriendly(col.Name);
						ci.AddOnTheFly = true;
						if (showOnlySpecified)
						{
							ci.ShowThisColumn = false;
						}

					}


					if (!ci.ShowThisColumn)
					{
						continue;
					}

					FieldInfo fi = col.FieldInfo;

					if (ci.IsInvisible)
					{

						HtmlInputHidden hih = (HtmlInputHidden)panel.FindControl(col.Name);
						fi.SetValue(DBObject, Convert.ChangeType(hih.Value, col.Type));

//						if (col.Name.Equals("updateId") && DBObject.Exists())
//						{
//							hih.Value = (System.Convert.ToInt32(hih.Value) + 1).ToString();
//						}


					}
					else if (col.Type.Equals(typeof(bool)))
					{

						CheckBox cb = (CheckBox)panel.FindControl(col.Name);
						fi.SetValue(DBObject, cb.Checked);

					}
					else if (col.Type.Equals(typeof(string)) || col.Type.Equals(typeof(short)) || col.Type.Equals(typeof(int)) || col.Type.Equals(typeof(long)) || col.Type.Equals(typeof(float)) || col.Type.Equals(typeof(double)))
					{

						TextBox tb = (TextBox)panel.FindControl(col.Name);

						Object val = tb.Text;
						if (tb.Text.Trim().Length == 0)
						{
							if (tb.TextMode == TextBoxMode.Password && DBObject.Exists())
							{
								val = fi.GetValue(DBObject);
							}
							else if (col.Type.Equals(typeof(string)) && col.AllowNulls)
							{
								val = null;
							}
							else if (col.Type.Equals(typeof(int)))
							{
								val = DBColumn.IntegerNullValue;
							}
							else if (col.Type.Equals(typeof(double)))
							{
								val = DBColumn.DoubleNullValue;
							}
							else if (col.Type.Equals(typeof(short)))
							{
								val = DBColumn.ShortNullValue;
							} else if (col.Type.Equals(typeof(long))) {
								val = DBColumn.LongNullValue;
							}
							else if (col.Type.Equals(typeof(float)))
							{
								val = DBColumn.FloatNullValue;
							}
						}
						else if (tb.TextMode == TextBoxMode.Password)
						{
							TextBox confirmTB = (TextBox)panel.FindControl(col.Name + "PasswordConfirm");
							if (confirmTB.Text != tb.Text)
							{
								throw new ValidationException("Your passwords did not match.");
							}
						}

						if (val == null)
						{
							fi.SetValue(DBObject, null);
						}
						else
						{
							fi.SetValue(DBObject, System.Convert.ChangeType(val, col.Type));
						}

					}
					else if (col.Type.Equals(typeof(DateTime)))
					{

						TextBox dateTB = (TextBox)panel.FindControl(col.Name);
						TextBox timeTB = (TextBox)panel.FindControl(col.Name + "_shundeTime");
						DropDownList ampmDDL = (DropDownList)panel.FindControl(col.Name + "_ampmDDL");
						DateTime dt;
						if (col.AllowNulls && dateTB.Text.Trim().Length == 0)
						{
							dt = DBColumn.DateTimeNullValue;
						}
						else
						{
							try
							{
								String timeText = (timeTB == null) ? "" : " " + timeTB.Text.Trim();
								dt = Convert.ToDateTime(dateTB.Text.Trim() + timeText);
								if (dt.Hour < 12 && ampmDDL != null && ampmDDL.SelectedIndex > 0)
								{
									dt = dt.AddHours(12);
								}
							}
							catch
							{
								throw new ValidationException(" Please enter a valid date and time for " + ci.FriendlyName);
							}
						}
						fi.SetValue(DBObject, dt);
					}
					else if (col.Type.Equals(typeof(BinaryData)))
					{

						HtmlInputFile hif = (HtmlInputFile)panel.FindControl(col.Name);

						if (hif.PostedFile == null || hif.PostedFile.ContentLength == 0)
						{
							// see if the delete checkbox was ticked
							CheckBox deleteCB = (CheckBox)panel.FindControl(col.Name + "_deleteFile");
							if (deleteCB != null && deleteCB.Checked)
							{
								fi.SetValue(DBObject, new BinaryData(null, "", ""));
							}
						}
						else
						{
							Stream fileStream = hif.PostedFile.InputStream;
							byte[] bytes = new byte[hif.PostedFile.ContentLength];
							fileStream.Read(bytes, 0, hif.PostedFile.ContentLength);
							String filename = Path.GetFileName(hif.PostedFile.FileName);
							if (filename.Length > 128)
							{
								filename = filename.Substring(0, 128);
							}
							BinaryData bd = new BinaryData(bytes, hif.PostedFile.ContentType, filename);
							fi.SetValue(DBObject, bd);
						}

					}
					else if (col.Type.Equals(typeof(DBObject)) || col.Type.IsSubclassOf(typeof(DBObject)))
					{


						Control control = panel.FindControl(col.Name);

						if (control is DropDownList || control is RadioButtonList)
						{

							ListControl ddl = (ListControl)control;

							TextBox newValueTextBox = (TextBox)panel.FindControl(col.Name + "_addnew");
							if (newValueTextBox != null && ci.AddOnTheFly && newValueTextBox.Text.Trim().Length > 0)
							{

								//							String newValue = newValueTextBox.Text.Trim();
								//							AttribType at = AttribType.getAttribType( sqlConnection, table.Name + "." + col.Name );
								//							Attrib newAttrib = Attrib.createFromValue( sqlConnection, at, newValue );
								//							fi.SetValue( DBObject, newAttrib );

							}
							else
							{

								string value = ddl.SelectedItem.Value;

								if (value.Length == 0)
								{
									fi.SetValue(DBObject, null);
								}
								else
								{
									//ConstructorInfo coninf = (ConstructorInfo) ObjectInfo.constructors[ col.Type.FullName ];
									//DBObject foreignObj = (DBObject) coninf.Invoke(null);

									string[] values = value.Split(new char[] { ',' });
									DBObject foreignObj = DBObject.CreateObject(col.Type.Assembly, values[1]);
									foreignObj.Id = Convert.ToInt32(values[0]);
									fi.SetValue(DBObject, foreignObj);
								}

							}

						}
						else if (control is HtmlInputHidden)
						{

							// use the hidden control
							HtmlInputHidden hih = (HtmlInputHidden)control;
							string value = hih.Value;
							if (value.Length == 0)
							{
								// see if they have typed something in
								string typed = ((TextBox)panel.FindControl(col.Name+"_cosmeticName")).Text;
								if (typed.Length == 0)
								{
									fi.SetValue(DBObject, null);
								}
								else
								{
									if (ci.FindObjectDelegate != null)
									{
										DBObject foreignObj = ci.FindObjectDelegate(typed, ci.AddOnTheFly);
										fi.SetValue(DBObject, foreignObj);
									}
									else
									{
										throw new ValidationException("The value \"" + typed + "\" you entered for " + ci.FriendlyName + " could not be found. Please try again.");
									}
								}
							}
							else
							{
								DBObject foreignObj = DBObject.GetObject(value);

								// make sure the selected object is compatible
								if (foreignObj.GetType().Equals(col.Type) || foreignObj.GetType().IsSubclassOf(col.Type))
								{
									fi.SetValue(DBObject, foreignObj);
								}
								else
								{
									throw new ValidationException("The " + foreignObj.GetType().Name + " you have selected for " + ci.FriendlyName + " cannot be selected. You must choose a " + col.Type.Name + " for this property.");
								}
							}


						}
						else
						{
							throw new Exception("Control for selecting " + col.Name + " not found. Control: " + control);
						}


					}

				}

			}

		}





		/// <summary>Pass an array of strings and column infos for each string will be made with display order in ascending order</summary>
		public void SetOrders(string[] names)
		{
			int disOrd = 100;
			for (int i = 0; i < names.Length; i++)
			{
				string name = names[i];
				if (name.Equals("-"))
				{
					name += disOrd.ToString();
				}
				ColumnInfo ci = GetCI(name);
				ci.DisplayOrder = disOrd;
				ci.AddOnTheFly = true;
				disOrd += 10;
			}
		}

		/// <summary>
		/// Gets the column info of a column
		/// </summary>
		public ColumnInfo GetCI(string columnName)
		{
			if (info.ContainsKey(columnName))
			{
				return info[columnName];
			}
			ColumnInfo ci = new ColumnInfo();
			info[columnName] = ci;
			
			return ci;
		}

		/// <summary>
		/// Sets the friendly Name of a database column
		/// </summary>
		public void SetFriendlyName(string columnName, string friendlyName)
		{
			ColumnInfo ci = GetCI(columnName);
			ci.FriendlyName = friendlyName;
		}























		/// <summary>
		/// This method is created solely for the design view of visual studio
		/// </summary>
		protected override void RenderContents(HtmlTextWriter writer)
		{
			base.RenderContents(writer);

			if (DBObject == null)
			{
				writer.Write(@"
		<div id=""ObjectEditor"">
	<span class=""infoMessage"">Non-client side validation/error messages go here.</span>

					<DIV ID=""caldiv12"" STYLE=""position:absolute;visibility:hidden;background-color:white;layer-background-color:white;""></DIV><table class=""ObjectEditorTable"" border=""0"">
		<tr>
			<th>Person</th><td><select name=""ObjectEditor$person"" id=""ObjectEditor_person"" tabindex=""1"">
				<option value="""">&lt;none&gt;</option>
				<option selected=""selected"" value=""4,TestingGround.Person"">Daniel Flower</option>

			</select></td>
		</tr><tr>
			<th><b>Year<font color=red>*</font></b><span id=""ObjectEditor_ctl04"" style=""color:Red;visibility:hidden;"">*</span></th><td><input name=""ObjectEditor$year"" type=""text"" value=""2005"" id=""ObjectEditor_year"" tabindex=""1"" style=""width:" + NumberTextBoxWidth.ToString() + @""" /></td>
		</tr><tr>
			<th>Picture </th><td><input name=""ObjectEditor$picture"" type=""file"" id=""ObjectEditor_picture""  style=""width:" + fileUploaderWidth.ToString() + @""" TabIndex=""1"" /></td>
		</tr><tr>
			<th>Date Imported</th><td><b>Date:</b> (dd/mm/yyyy) <input name=""ObjectEditor$dateImported"" type=""text"" value=""16/02/2006""  style=""width:" + DateTextBoxWidth.ToString() + @""" id=""ObjectEditor_dateImported"" tabindex=""1"" />
<A HREF=""javascript:void(null);"" onClick=""cal_dateImported.select(document.getElementById('ObjectEditor_dateImported'),'anchor12xx','dd/MM/yyyy'); return false;"" NAME=""anchor12xx"" ID=""anchor12xx""><img src=""images/calendar.gif"" height=""21"" width=""21"" border=""0"" style=""border:none;margin:0px 0px 0px 0px;"" align=""absmiddle"" /></A>
					&nbsp;&nbsp;<b>Time:</b> (h:mm) <input name=""ObjectEditor$dateImported_shundeTime"" type=""text"" value=""2:32"" size=""4"" id=""ObjectEditor_dateImported_shundeTime"" tabindex=""1"" /><select name=""ObjectEditor$dateImported_ampmDDL"" id=""ObjectEditor_dateImported_ampmDDL"" tabindex=""1"">
				<option value=""0"">am</option>
				<option selected=""selected"" value=""12"">pm</option>

			</select></td>
		</tr><tr>
			<th><b>Display Order<font color=red>*</font></b></th><td><input name=""ObjectEditor$displayOrder"" type=""text"" value=""0"" style=""width:" + NumberTextBoxWidth.ToString() + @""" id=""ObjectEditor_displayOrder"" tabindex=""1"" /></td>
		</tr><tr>
			<td></td><td><input id=""ObjectEditor_isDeleted"" type=""checkbox"" name=""ObjectEditor$isDeleted"" tabindex=""1"" /><label for=""ObjectEditor_isDeleted""><b>Is Deleted</b></label> </td>
		</tr><tr>
			<th><b>Make<font color=red>*</font></b><span id=""ObjectEditor_ctl10"" style=""color:Red;visibility:hidden;"">*</span></th><td><input name=""ObjectEditor$make"" type=""text"" value=""Bency"" maxlength=""200"" style=""width:" + StringTextBoxWidth.ToString() + @""" id=""ObjectEditor_make"" tabindex=""1"" /></td>
		</tr><tr>
			<td></td><td><input type=""submit"" name=""ObjectEditor$ctl11"" value=""Save Details"" onclick=""javascript:WebForm_DoPostBackWithOptions(new WebForm_PostBackOptions(&quot;ObjectEditor$ctl11&quot;, &quot;&quot;, true, &quot;&quot;, &quot;&quot;, false, false))"" tabindex=""1"" /><input type=""submit"" name=""ObjectEditor$ctl12"" value=""Delete"" onclick=""javascript:WebForm_DoPostBackWithOptions(new WebForm_PostBackOptions(&quot;ObjectEditor$ctl12&quot;, &quot;&quot;, true, &quot;&quot;, &quot;&quot;, false, false))"" tabindex=""1"" /><input type=""submit"" name=""ObjectEditor$ctl13"" value=""Cancel"" onclick=""javascript:WebForm_DoPostBackWithOptions(new WebForm_PostBackOptions(&quot;ObjectEditor$ctl13&quot;, &quot;&quot;, true, &quot;&quot;, &quot;&quot;, false, false))"" tabindex=""1"" /></td>
		</tr>
	</table>
</div>

				");
			}

		}







	}



	/// <summary>
	/// Called after the object has been saved
	/// </summary>
	public delegate void BeforeSaveDelegate();

	/// <summary>
	/// Called after the object has been saved
	/// </summary>
	public delegate void AfterObjectSavedDelegate();

	/// <summary>
	/// Called after an object is deleted
	/// </summary>
	public delegate void AfterObjectDeletedDelegate();

	/// <summary>
	/// A delegate to take care of saving the object
	/// </summary>
	public delegate void ObjectSaveDelegate();

	/// <summary>
	/// A delegate to take care of saving the object
	/// </summary>
	public delegate void ObjectDeleteDelegate();

	/// <summary>
	/// A delegate to take care of saving the object
	/// </summary>
	public delegate void EditCancelledDelegate();

}
