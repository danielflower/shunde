using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Shunde.Framework;
using Shunde.Utilities;
using System.Web;
using System.IO;
using System.Drawing;

namespace Shunde.Web
{
	/// <summary>
	/// Creates Web Controls for inputting data
	/// </summary>
	internal class ControlCreator
	{


		/// <summary>
		/// Creates the cells for the given TableRow, based on info gleened from the ObjectEditorRow
		/// </summary>
		internal static void CreateObjectEditorTableRow(ObjectEditorRow row, TableRow tableRow, object initialValue)
		{
			if (row.InputMode == InputMode.HiddenField)
			{
				CreateHiddenField(row, tableRow, initialValue);
			}
			else if (row.InputMode == InputMode.Checkbox)
			{
				bool selected = (initialValue == null) ? false : (bool)initialValue;
				CreateCheckbox(row, tableRow, selected);
			}
			else if (row.InputMode == InputMode.TextBox || row.InputMode == InputMode.Password)
			{
				CreateSingleLineTextControl(row, tableRow, (string)initialValue);
			}
			else if (row.InputMode == InputMode.MultilineTextBox)
			{
				CreateMultiLineTextControl(row, tableRow, (string)initialValue);
			}
			else if (row.InputMode == InputMode.NumberTextBox)
			{
				CreateNumberTextControl(row, tableRow, initialValue);
			}
			else if (row.InputMode == InputMode.DateTimePicker)
			{
				DateTime? date = (initialValue == null) ? (DateTime?)null : (DateTime?)Convert.ToDateTime(initialValue);
				CreateDateTimePicker(row, tableRow, date);
			}
			else if (row.InputMode == InputMode.FileUpload)
			{
				CreateFileUploader(row, tableRow, (BinaryData)initialValue);
			}
			else if (row.InputMode == InputMode.DropDownList || row.InputMode == InputMode.RadioButtonList || row.InputMode == InputMode.ComboBox)
			{
				CreateListBox(row, tableRow);
			}
			else if (row.InputMode == InputMode.ColorPicker)
			{
				CreateColorPickerControl(row, tableRow, (Color)initialValue);
			}
			else
			{
				throw new ShundeException("Row could not be created as the InputMode was not set to a valid value (set value was " + row.InputMode + ").");
			}
		}




		internal static object GetValueFromControl(ObjectEditorRow row)
		{

			object value;

			if (row.InputMode == InputMode.HiddenField)
			{
				value = GetHiddenFieldValue(row);
			}
			else if (row.InputMode == InputMode.Checkbox)
			{
				value = GetCheckboxValue(row);
			}
			else if (row.InputMode == InputMode.TextBox)
			{
				value = GetSingleLineTextControlValue(row);
			}
			else if (row.InputMode == InputMode.MultilineTextBox)
			{
				value = GetMultiLineTextControlValue(row);
			}
			else if (row.InputMode == InputMode.NumberTextBox)
			{
				value = GetNumberTextControlValue(row);
			}
			else if (row.InputMode == InputMode.DateTimePicker)
			{
				value = GetDateTimePickerValue(row);
			}
			else if (row.InputMode == InputMode.FileUpload)
			{
				value = GetFileUploaderValue(row);
			}
			else if (row.InputMode == InputMode.DropDownList || row.InputMode == InputMode.RadioButtonList || row.InputMode == InputMode.ComboBox)
			{
				value = GetListControlValue(row);
			}
			else if (row.InputMode == InputMode.Password)
			{
				value = GetPasswordValue(row);
			}
			else if (row.InputMode == InputMode.ColorPicker)
			{
				value = GetColorPickerValue(row);
			}
			else
			{
				throw new ShundeException("Value could not be found as the InputMode was not set to a valid value (set value was " + row.InputMode + ").");
			}


			// We now have a value from a web control, however, this does not necessarily correspond
			// to a suitable value.  For example, a reference to another DBObject will have a value
			// being its ID as a string; we need the actual object.

			Type t = row.DBColumn.Type;

			bool isListItemAndIsNull = false;
			if (value is ListItem)
			{
				string liValue = ((ListItem)value).Value;
				if (liValue.Equals(string.Empty))
				{
					isListItemAndIsNull = true;
				}
				else if (t.Equals(typeof(DBObject)) || t.IsSubclassOf(typeof(DBObject)))
				{
					if (Convert.ToInt32(liValue) < 1)
					{
						return null;
					}
				}
			}

			if (value == null || isListItemAndIsNull || DBColumn.IsColumnNull(value))
			{
				if (t.Equals(typeof(string)))
				{
					return "";
				}
				else
				{
					return null;
				}
			}
			

			if (t == typeof(string))
			{
				if (value is string || value is DateTime || value is bool)
				{
					return value.ToString();
				}
				if (value is ListItem)
				{
					return ((ListItem)value).Value;
				}
				if (value is BinaryData)
				{
					string fileContents; // assumes the file is UTF-8
					BinaryData bd = (BinaryData)value;
					using (StreamReader sr = new StreamReader(new MemoryStream(bd.Data), Encoding.UTF8))
					{
						// Read and display lines from the file until the end of 
						// the file is reached.
						fileContents = sr.ReadToEnd();
					}
					return fileContents;

				}
			}
			else if (t == typeof(bool))
			{
				if (value is bool)
				{
					return value;
				}
			}
			else if (t == typeof(DateTime) || t == typeof(DateTime?))
			{
				if (value is DateTime)
				{
					return value;
				}
				try
				{
					return DateTime.Parse(value.ToString());
				}
				catch
				{
					throw new ValidationException("The value " + value + " is not a valid date for " + row.Header);
				}
			}
			else if (DBColumn.IsNumberOrNullableNumber(t))
			{
				object intermediateValue = value;
				if (value is ListItem)
				{
					intermediateValue = ((ListItem)value).Value;
				}
				else if (value is bool)
				{
					intermediateValue = Convert.ToInt32(value);
				}
				else if (value is DateTime)
				{
					intermediateValue = ((DateTime)value).Ticks;
				}

				try
				{
					return FrameworkUtils.ChangeType(intermediateValue, t);
				}
				catch
				{
					throw new ValidationException("The value " + value + " is not a valid number for " + row.Header);
				}

			}
			else if (FrameworkUtils.IsEnumOrNullableEnum(t))
			{
				string intermediateValue;
				if (value is ListItem)
				{
					intermediateValue = ((ListItem)value).Value;
				}
				else
				{
					intermediateValue = value.ToString();
				}

				try
				{
					Type enumType = (t.IsEnum) ? t : Nullable.GetUnderlyingType(t);
					return (Enum)Enum.Parse(enumType, intermediateValue);
				}
				catch
				{
					throw new ValidationException("The value " + value + " is not a valid for " + row.Header);
				}
			}
			else if (t == typeof(BinaryData))
			{
				if (value is BinaryData)
				{
					return value;
				}
			}
			else if (t.Equals(typeof(DBObject)) || t.IsSubclassOf(typeof(DBObject)))
			{
				if (value is ListItem)
				{
					int id = Convert.ToInt32(((ListItem)value).Value);
					if (id < 0)
					{
						return null;
					}

					DBObject foreignObj = DBObject.CreateObject(t.Assembly, t.FullName);
					foreignObj.Id = id;
					return foreignObj;
				}
			}
			else if (t.Equals(typeof(Color)))
			{
				if (value is Color)
				{
					return (Color)value;
				}
				else if (value is string)
				{
					try
					{
						return ColorTranslator.FromHtml((string)value);
					}
					catch
					{
						throw new ValidationException("The value " + value + " is not a valid color.");
					}
				}
			}

			throw new Exception("No way to handle a value of type " + value.GetType().FullName + " for column " + row.Header + " (" + t.FullName + ")");

		}



		static void CreateStandardRow(ObjectEditorRow row, List<Control> headerControls, TableRow tableRow, Control control)
		{
			TableHeaderCell th = new TableHeaderCell();
			th.Controls.Add(new LiteralControl(row.Header));


			if (row.RequiredField)
			{
				th.Controls.Add(new LiteralControl("<span style=\"color:Red;\">*</span>"));
			}

			tableRow.Cells.Add(th);

			if (headerControls != null)
			{
				foreach (Control c in headerControls)
				{
					th.Controls.Add(c);
				}
			}

			TableCell tc = new TableCell();
			tc.Controls.Add(control);
			tableRow.Cells.Add(tc);

			if (row.MoreInfo.Length > 0)
			{
				HtmlGenericControl moreInfo = new HtmlGenericControl("p");
				moreInfo.InnerHtml = row.MoreInfo;
				tc.Controls.Add(moreInfo);
			}
		}




		static void CreateListBox(ObjectEditorRow row, TableRow tableRow)
		{
			HtmlGenericControl span = new HtmlGenericControl("span");
			ListControl lc;
			if (row.InputMode == InputMode.RadioButtonList) {
				lc = new RadioButtonList();
			}
			else if (row.InputMode == InputMode.DropDownList)
			{
				lc = new DropDownList();
			}
			else
			{
				lc = new ComboBox();
			}
			lc.ID = row.Id;
			lc.TabIndex = 1;

			// add validation
			if (row.RequiredField)
			{
				RequiredFieldValidator rfv = new RequiredFieldValidator();
				rfv.ControlToValidate = lc.ID;
				rfv.ErrorMessage = "Please select a value for " + row.Header;
				rfv.Display = ValidatorDisplay.Dynamic;
				rfv.Text = "*";
				span.Controls.Add(rfv);
			}

			span.Controls.Add(lc);

			if (!row.RequiredField && !(lc is ComboBox))
			{
				string name = (lc is DropDownList) ? row.NoSelectionName : HttpUtility.HtmlEncode(row.NoSelectionName);
				ListItem li = new ListItem(name, "");
				li.Selected = true;
				lc.Items.Add(li);

			}
			else if (lc is DropDownList)
			{
				lc.Items.Add(new ListItem("<Select One>", ""));
			}

			List<Control> headerControls = new List<Control>();
			if (row.RequiredField)
			{
				headerControls.Add(CreateRequiredFieldValidator(lc.ID, "Please select an item for " + row.Header, row.ObjectEditor.ValidationGroup));
			}


			if (row.ListItems != null)
			{
				lc.Items.AddRange(row.ListItems.ToArray());
			}


			span.Controls.Add(lc);

			CreateStandardRow(row, headerControls, tableRow, span);

		}



		static void CreateMultiLineTextControl(ObjectEditorRow row, TableRow tableRow, string initialValue)
		{
			TextBox tb = new TextBox();
			tb.Text = initialValue;
			tb.ID = row.Id;
			tb.TabIndex = 1;
			tb.Width = (row.TextboxWidth.IsEmpty) ? row.ObjectEditor.StringTextBoxWidth : row.TextboxWidth;
			tb.TextMode = TextBoxMode.MultiLine;

			if (row.UseRichTextEditor)
			{
				tb.Attributes["UseRichTextEditor"] = "true";
				tb.Height = (row.TexboxHeight.IsEmpty) ? row.ObjectEditor.MultilineTextBoxHeight : new Unit(300);

				TableCell tc = new TableCell();
				tableRow.Cells.Add(tc);
				tc.ColumnSpan = 2;

				HtmlGenericControl header = new HtmlGenericControl("strong");
				header.Style[HtmlTextWriterStyle.Display] = "block";
				header.InnerHtml = row.Header;
				tc.Controls.Add(header);

				if (row.MoreInfo.Length > 0)
				{
					HtmlGenericControl moreInfo = new HtmlGenericControl("p");
					moreInfo.InnerHtml = row.MoreInfo;
					tc.Controls.Add(moreInfo);
				}
				
				tc.Controls.Add(tb);

			}
			else
			{
				tb.Height = (row.TexboxHeight.IsEmpty) ? row.ObjectEditor.MultilineTextBoxHeight : row.TexboxHeight;
				
							List<Control> headerControls = new List<Control>();
			if (row.RequiredField)
			{
				headerControls.Add(CreateRequiredFieldValidator(tb.ID, "Please enter some text for " + row.Header, row.ObjectEditor.ValidationGroup));
			}
				
				CreateStandardRow(row, headerControls, tableRow, tb);
			}
		}




		static void CreateNumberTextControl(ObjectEditorRow row, TableRow tableRow, object initialValue)
		{
			TextBox tb = new TextBox();
			if (Shunde.Framework.DBColumn.IsColumnNull(initialValue))
			{
				tb.Text = "";
			}
			else
			{
				tb.Text = initialValue.ToString();
			}
			tb.ID = row.Id;
			tb.TabIndex = 1;
			tb.Width = (row.TextboxWidth.IsEmpty) ? row.ObjectEditor.NumberTextBoxWidth : row.TextboxWidth;
			
			List<Control> headerControls = new List<Control>();
			if (row.RequiredField)
			{
				headerControls.Add(CreateRequiredFieldValidator(tb.ID, "Please enter a number for " + row.Header, row.ObjectEditor.ValidationGroup));
			}

			if (row.DBColumn.MinAllowed != null || row.DBColumn.MaxAllowed != null)
			{
				ValidationDataType dataType = (row.DBColumn.Type == typeof(float) || row.DBColumn.Type == typeof(double)) ? ValidationDataType.Double : ValidationDataType.Integer;
				headerControls.Add(CreateRangeValidator(tb.ID, row.Header, dataType, row.DBColumn.MinAllowed, row.DBColumn.MaxAllowed, row.ObjectEditor.ValidationGroup));
			}


			CreateStandardRow(row, headerControls, tableRow, tb);
		}


		static void CreateSingleLineTextControl(ObjectEditorRow row, TableRow tableRow, string initialValue)
		{
			TextBox tb = new TextBox();
			tb.Text = initialValue;
			tb.ID = row.Id;
			tb.TabIndex = 1;
			tb.Width = (row.TextboxWidth.IsEmpty) ? row.ObjectEditor.StringTextBoxWidth : row.TextboxWidth;
			tb.MaxLength = row.DBColumn.MaxLength;

			List<Control> headerControls = new List<Control>();
			if (row.RequiredField)
			{
				headerControls.Add(CreateRequiredFieldValidator(tb.ID, "Please enter a some text for " + row.Header, row.ObjectEditor.ValidationGroup));
			}

			if (row.DBColumn.RegularExpression != null)
			{
				string msg = (row.DBColumn.RegularExpressionErrorMessage != null) ? row.DBColumn.RegularExpressionErrorMessage : "The value for " + row.Header + " is not allowed.";
				headerControls.Add(CreateRegularExpressionValidator(tb.ID, row.DBColumn.RegularExpression, msg, row.ObjectEditor.ValidationGroup));
			}

			if (row.ValidationRegex.Length > 0)
			{
				headerControls.Add(CreateRegularExpressionValidator(tb.ID, row.ValidationRegex, row.RegexErrorMessage, row.ObjectEditor.ValidationGroup));
			}

			if (row.InputMode == InputMode.Password)
			{
				HtmlGenericControl container = new HtmlGenericControl("div");
				container.Controls.Add(tb);

				tb.TextMode = TextBoxMode.Password;

				HtmlGenericControl confirmDiv = new HtmlGenericControl("div");
				confirmDiv.Controls.Add(new LiteralControl("Please confirm your password:"));
				confirmDiv.ID = tb.ID + "PasswordConfirmDiv";

				if (row.ObjectEditor.DBObject.Exists()) {
					tb.Attributes["onkeyup"] = "document.getElementById(this.id + 'PasswordConfirmDiv').style.display = (this.value.length == 0) ? 'none' : 'block';";
					confirmDiv.Style[HtmlTextWriterStyle.Display] = "none";
					if (!row.RequiredField)
					{
						container.Controls.Add(new LiteralControl("<div>You may leave this blank to keep your existing password</div>"));
					}
				}

				container.Controls.Add(confirmDiv);


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
				headerControls.Add(cv);

				if (row.RequiredField)
				{
					RequiredFieldValidator rfv = new RequiredFieldValidator();
					rfv.ControlToValidate = passwordConfirmTB.ID;
					rfv.ErrorMessage = "Please enter a value for the confirmation password field";
					rfv.Display = ValidatorDisplay.Static;
					rfv.Text = "*";
					rfv.SetFocusOnError = true;
					headerControls.Add(rfv);
				}

				confirmDiv.Controls.Add(passwordConfirmTB);
				

			

				CreateStandardRow(row, headerControls, tableRow, container);
			}
			else
			{
				CreateStandardRow(row, headerControls, tableRow, tb);
			}
		}


		static void CreateColorPickerControl(ObjectEditorRow row, TableRow tableRow, Color initialValue)
		{
			ColorPicker cp = new ColorPicker();
			cp.SelectedColor = initialValue;
			cp.ID = row.Id;
			cp.TabIndex = 1;
			cp.Width = new Unit(70, UnitType.Pixel);
			cp.Height = new Unit(20, UnitType.Pixel);

			List<Control> headerControls = new List<Control>();
			if (row.RequiredField)
			{
				headerControls.Add(CreateRequiredFieldValidator(cp.ID, "Please select a color for " + row.Header, row.ObjectEditor.ValidationGroup));
			}

			CreateStandardRow(row, headerControls, tableRow, cp);
		}


		static string GetNumberTextControlValue(ObjectEditorRow row)
		{
			TextBox tb = (TextBox)row.ObjectEditor.FindControl(row.Id);
			return tb.Text;
		}


		static Color GetColorPickerValue(ObjectEditorRow row)
		{
			ColorPicker cp = (ColorPicker)row.ObjectEditor.FindControl(row.Id);
			return cp.SelectedColor;
		}

		static DateTime? GetDateTimePickerValue(ObjectEditorRow row)
		{
			TextBox dateTB = (TextBox)row.ObjectEditor.FindControl(row.Id);
			TextBox timeTB = (TextBox)row.ObjectEditor.FindControl(row.Id + "_shundeTime");
			DropDownList ampmDDL = (DropDownList)row.ObjectEditor.FindControl(row.Id + "_ampmDDL");
			
			if (row.DBColumn.AllowNulls && dateTB.Text.Trim().Length == 0)
			{
				return null;
			}
			else
			{
				try
				{
					string timeText = (timeTB == null || timeTB.Text.Length == 0) ? "" : " " + timeTB.Text.Trim();
					if (timeText.IndexOf(':') == -1 && timeText.Length > 0)
					{
						timeText = timeText.Replace('.', ':');
						if (timeText.IndexOf(':') == -1)
						{
							timeText += ":00";
						}
					}

					DateTime dt = Convert.ToDateTime(dateTB.Text.Trim() + timeText);
					if (dt.Hour < 12 && ampmDDL != null && ampmDDL.SelectedIndex > 0)
					{
						dt = dt.AddHours(12);
					}
					return dt;
				}
				catch
				{
					throw new ValidationException(" Please enter a valid date and time for " + row.Header);
				}
			}
		}

		static string GetSingleLineTextControlValue(ObjectEditorRow row)
		{
			TextBox tb = (TextBox)row.ObjectEditor.FindControl(row.Id);
			return tb.Text.Trim();
		}

		static string GetPasswordValue(ObjectEditorRow row)
		{
			TextBox tb = (TextBox)row.ObjectEditor.FindControl(row.Id);
			string val = tb.Text.Trim();
			if (val.Length == 0)
			{
				val = (string) row.DBColumn.FieldInfo.GetValue(row.ObjectEditor.DBObject);
			}
			TextBox confirmTb = (TextBox)row.ObjectEditor.FindControl(row.Id + "PasswordConfirm");
			if (!confirmTb.Text.Equals(tb.Text))
			{
				throw new Shunde.ValidationException("Your passwords did not match!");
			}
			return val;
		}

		static ListItem GetListControlValue(ObjectEditorRow row)
		{
			ListControl lc = (ListControl)row.ObjectEditor.FindControl(row.Id);
			return lc.SelectedItem;
		}

		static string GetMultiLineTextControlValue(ObjectEditorRow row)
		{
			TextBox tb = (TextBox)row.ObjectEditor.FindControl(row.Id);
			return tb.Text.Trim();
		}

		

		static void CreateCheckbox(ObjectEditorRow row, TableRow tableRow, bool selected)
		{
			CheckBox cb = new CheckBox();
			cb.Checked = selected;
			cb.Text = "<strong>" + row.DBColumn.Name + "</strong>";
			cb.ID = row.Id;
			cb.TabIndex = 1;
			
			TableHeaderCell th = new TableHeaderCell();
			th.Text = "";
			tableRow.Cells.Add(th);

			TableCell tc = new TableCell();
			tc.Controls.Add(cb);
			tableRow.Cells.Add(tc);

			if (row.MoreInfo.Length > 0)
			{
				HtmlGenericControl moreInfo = new HtmlGenericControl("p");
				moreInfo.InnerHtml = row.MoreInfo;
				tc.Controls.Add(moreInfo);
			}
		}

		static bool GetCheckboxValue(ObjectEditorRow row)
		{
			CheckBox cb = (CheckBox)row.ObjectEditor.FindControl(row.Id);
			return cb.Checked;
		}

		static void CreateHiddenField(ObjectEditorRow row, TableRow tableRow, object initialValue)
		{
			HtmlInputHidden hih = new HtmlInputHidden();
			hih.ID = row.Id;
			if (initialValue != null)
			{
				hih.Value = initialValue.ToString();
			}

			TableCell tc = new TableCell();
			tc.ColumnSpan = 2;
			tc.Controls.Add(hih);
			tableRow.Cells.Add(tc);
	
			tableRow.Style[HtmlTextWriterStyle.Display] = "none";
		}

		static string GetHiddenFieldValue(ObjectEditorRow row)
		{

			HtmlInputHidden hih = (HtmlInputHidden)row.ObjectEditor.FindControl(row.Id);
			return hih.Value;
		}


		static BinaryData GetFileUploaderValue(ObjectEditorRow row)
		{

			FileUpload fileUpload = (FileUpload)row.ObjectEditor.FindControl(row.Id);



			if (!fileUpload.HasFile)
			{
				// see if the delete checkbox was ticked
				CheckBox deleteCB = (CheckBox)row.ObjectEditor.FindControl(row.Id + "_deleteFile");
				if (deleteCB != null && deleteCB.Checked)
				{
					return null;
				}
				return (BinaryData)row.DBColumn.FieldInfo.GetValue(row.ObjectEditor.DBObject); // return the existing file
			}
			else
			{
				Stream fileStream = fileUpload.FileContent;
				byte[] bytes = new byte[fileUpload.PostedFile.ContentLength];
				fileStream.Read(bytes, 0, fileUpload.PostedFile.ContentLength);
				string filename = TextUtils.RemoveIllegalCharactersFromFilename(Path.GetFileName(fileUpload.FileName));
				if (filename.Length > 128)
				{
					filename = filename.Substring(filename.Length - 128, 128);
				}
				BinaryData bd = new BinaryData(bytes, fileUpload.PostedFile.ContentType, filename);
				return bd;
			}


		}
		

		static void CreateFileUploader(ObjectEditorRow row, TableRow tableRow, BinaryData initialValue)
		{

			HtmlGenericControl span = new HtmlGenericControl("span");

			FileUpload fileUpload = new FileUpload();
			span.Controls.Add(fileUpload);

			fileUpload.ID = row.Id;
			fileUpload.Width = ((row.TextboxWidth.IsEmpty) ? row.ObjectEditor.FileUploaderWidth : row.TextboxWidth);
			fileUpload.TabIndex = 1;

			List<Control> headerControls = new List<Control>();


			if (DBColumn.IsColumnNull(initialValue))
			{
				// add validation if nulls are not allowed
				if (row.RequiredField)
				{
					if (row.RequiredField)
					{
						headerControls.Add(CreateRequiredFieldValidator(fileUpload.ID, "Please upload a file for " + row.Header, row.ObjectEditor.ValidationGroup));
					}
				}

			}
			else
			{

				Literal curFileLit = new Literal();
				string filename;
				if (row.ViewBinaryDataUrl.Length > 0)
				{
					filename = "<a href=\"" + row.ViewBinaryDataUrl + row.ObjectEditor.DBObject.Id + "&fieldName=" + row.DBColumn.Name + "\" target=\"_blank\">" + initialValue.Filename + "</a> ";
				}
				else
				{
					filename = initialValue.Filename;
				}

				curFileLit.Text = "<br/>Current file: " + filename + " (" + TextUtils.GetFriendlyFileSize(initialValue.Size) + ")";

				

				span.Controls.Add(curFileLit);

				if (!row.RequiredField)
				{
					CheckBox cb = new CheckBox();
					cb.ID = row.Id + "_deleteFile";
					cb.Checked = false;
					cb.Text = " <b>Delete</b>";
					span.Controls.Add(cb);
				}

			}

			CreateStandardRow(row, headerControls, tableRow, span);

		}




		static void CreateDateTimePicker(ObjectEditorRow row, TableRow tableRow, DateTime? initialValue)
		{


			string controlId = row.ObjectEditor.ClientID;
			DBColumn col = row.DBColumn;

			TextBox tb = new TextBox();
			tb.Text = (initialValue == null) ? "" : initialValue.Value.ToString("dd/MM/yyyy");
			tb.ID = row.Id;
			tb.TabIndex = 1;
			tb.Width = (row.TextboxWidth.IsEmpty) ? new Unit("70px") : row.TextboxWidth;

			HtmlGenericControl span = new HtmlGenericControl("span");

			// add validation
			List<Control> headerControls = new List<Control>();
			if (row.RequiredField)
			{
				headerControls.Add(CreateRequiredFieldValidator(tb.ID, "Please enter a date for " + row.Header, row.ObjectEditor.ValidationGroup));
			}

			if (row.DBColumn.MinAllowed != null || row.DBColumn.MaxAllowed != null)
			{
				headerControls.Add(CreateRangeValidator(tb.ID, row.Header, ValidationDataType.Date, row.DBColumn.MinAllowed, row.DBColumn.MaxAllowed, row.ObjectEditor.ValidationGroup));
			}



			string calName = controlId + "_cal_" + row.Id;
			string tbName = controlId + "_" + tb.ClientID;

			Literal lit2 = new Literal();
			lit2.Text = "<b>Date:</b> <font size=\"1\">(dd/mm/yyyy) </font>";
			span.Controls.Add(lit2);

			span.Controls.Add(tb);


			string anchorName = controlId + "_anchor" + row.Id + "xx";
			string divName = controlId + "_caldiv" + row.Id;

			Literal javascriptStuff = new Literal();
			javascriptStuff.Text = @"
<SCRIPT LANGUAGE=""JavaScript"">
	var " + calName + @" = new CalendarPopup(""" + divName + @""");
	" + calName + @".showYearNavigation();
	" + calName + @".showYearNavigationInput();
<" + @"/SCRIPT>
";



			Literal calendarDivText = new Literal();
			calendarDivText.Text = "<DIV ID=\"" + divName + "\" STYLE=\"position:absolute;visibility:hidden;background-color:white;\"></DIV>";

			string calendarImageUrl = row.ObjectEditor.Page.ClientScript.GetWebResourceUrl(typeof(ObjectEditor), "Shunde.Resources.Calendar.gif");

			Literal lit3 = new Literal();
			lit3.Text = @"
<A HREF=""javascript:void(null);"" onClick=""" + calName + @".select(document.getElementById('" + tbName + @"'),'" + anchorName + @"','dd/MM/yyyy'); return false;"" NAME=""" + anchorName + @""" ID=""" + anchorName + @"""><img src=""" + calendarImageUrl + @""" height=""21"" width=""21"" border=""0"" style=""border:none;margin:0px 0px 0px 0px;"" align=""absmiddle"" /></A>
					";

			span.Controls.Add(lit3);

			span.Controls.Add(javascriptStuff);
			span.Controls.Add(calendarDivText);


			if (row.ShowTimeWithDate)
			{
				Literal timeLit = new Literal();
				timeLit.Text = "&nbsp;&nbsp;<b>Time:</b> <font size=\"1\">(h:mm) </font>";
				span.Controls.Add(timeLit);
				TextBox timeTB = new TextBox();
				timeTB.Text = (initialValue == null || TextUtils.IsMidnight(initialValue)) ? "" : initialValue.Value.ToString("h:mm");
				timeTB.ID = col.Name + "_shundeTime";
				timeTB.TabIndex = 1;
				timeTB.Width = new Unit("40px");
				span.Controls.Add(timeTB);

				DropDownList ampmDDL = new DropDownList();
				ampmDDL.ID = col.Name + "_ampmDDL";
				ampmDDL.Items.Add(new ListItem("am", "0"));
				ampmDDL.Items.Add(new ListItem("pm", "12"));
				ampmDDL.TabIndex = 1;
				ampmDDL.SelectedIndex = (initialValue != null && initialValue.Value.Hour > 12) ? 1 : 0;
				span.Controls.Add(ampmDDL);

			}

			CreateStandardRow(row, headerControls, tableRow, span);

		}




		static RequiredFieldValidator CreateRequiredFieldValidator(string controlToValidateId, string errorMessage, string validationGroup)
		{
			RequiredFieldValidator rfv = new RequiredFieldValidator();
			rfv.ControlToValidate = controlToValidateId;
			rfv.ErrorMessage = errorMessage;
			rfv.Display = ValidatorDisplay.Static;
			rfv.Text = "*";
			rfv.SetFocusOnError = true;
			rfv.ValidationGroup = validationGroup;
			return rfv;
		}

		static RegularExpressionValidator CreateRegularExpressionValidator(string controlToValidateId, string regularExpression, string errorMessage, string validationGroup)
		{
			RegularExpressionValidator rev = new RegularExpressionValidator();
			rev.ControlToValidate = controlToValidateId;
			string msg = errorMessage;
			rev.ErrorMessage = msg;
			rev.Display = ValidatorDisplay.Dynamic;
			rev.Text = "*";
			rev.ValidationExpression = regularExpression;
			rev.SetFocusOnError = true;
			rev.ValidationGroup = validationGroup;
			return rev;
		}


		static RangeValidator CreateRangeValidator(string controlToValidateId, string header, ValidationDataType dataType, IComparable minAllowed, IComparable maxAllowed, string validationGroup)
		{

			RangeValidator rv = new RangeValidator();
			rv.Type = dataType;

			rv.ControlToValidate = controlToValidateId;
			rv.Display = ValidatorDisplay.Dynamic;
			rv.Text = "*";
			rv.SetFocusOnError = true;
			rv.ValidationGroup = validationGroup;

			if (minAllowed != null && maxAllowed != null)
			{
				rv.MinimumValue = minAllowed.ToString();
				rv.MaximumValue = maxAllowed.ToString();
				rv.ErrorMessage = "The value of " + header + " must be between " + minAllowed + " and " + maxAllowed + " (inclusive).";
			}
			else if (minAllowed != null)
			{
				rv.MinimumValue = minAllowed.ToString();
				if (dataType == ValidationDataType.Double)
				{
					rv.MaximumValue = double.MaxValue.ToString();
				}
				else if (dataType == ValidationDataType.Integer)
				{
					rv.MaximumValue = int.MaxValue.ToString();
				}
				else if (dataType == ValidationDataType.Date)
				{
					rv.MaximumValue = new DateTime(2999, 1, 1).ToString();
				}
				rv.ErrorMessage = "The minimum value allowed for " + header + " is " + minAllowed;
			}
			else
			{
				rv.MaximumValue = maxAllowed.ToString();
				if (dataType == ValidationDataType.Double)
				{
					rv.MaximumValue = double.MinValue.ToString();
				}
				else if (dataType == ValidationDataType.Integer)
				{
					rv.MaximumValue = int.MinValue.ToString();
				}
				else if (dataType == ValidationDataType.Date)
				{
					rv.MaximumValue = new DateTime(1000, 1, 1).ToString();
				}
				rv.ErrorMessage = "The maximum value allowed for " + header + " is " + maxAllowed;
			}

			return rv;
		}



		private ControlCreator() { }

	}
}
