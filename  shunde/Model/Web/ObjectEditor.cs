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


		/// <summary>
		/// Gets or sets the row with the specified column name
		/// </summary>
		/// <param name="columnName">The name of column that this row refers to</param>
		public ObjectEditorRow this[string columnName]
		{
			get
			{
				return this.GetRow(columnName);
			}
			set
			{
				this.AddRow(columnName);
			}
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
		public Unit FileUploaderWidth
		{
			get { return fileUploaderWidth; }
			set { fileUploaderWidth = value; }
		}		

		/// <summary>
		/// 
		/// </summary>
		private List<ObjectEditorRow> rows = new List<ObjectEditorRow>();
		

		private ButtonPanel buttonPanel;

		/// <summary>
		/// The button panel
		/// </summary>
		public ButtonPanel ButtonPanel
		{
			get { return buttonPanel; }
			set { buttonPanel = value; }
		}

		private string validationGroup = null;

		/// <summary>
		/// The validation group for the validation controls
		/// </summary>
		public string ValidationGroup
		{
			get { return validationGroup; }
			set { validationGroup = value; }
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
			this.Width = new Unit("95%");
		}



		/// <summary>
		/// The OnInit event
		/// </summary>
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			this.Page.ClientScript.RegisterClientScriptInclude(typeof(DBObject), "ShundeScripts", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "Shunde.Resources.ShundeScripts.js"));
			this.Page.ClientScript.RegisterClientScriptInclude(typeof(DBObject), "ObjectEditorScripts", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "Shunde.Resources.ObjectEditorScripts.js"));


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
				infoMessage.Text = ((ShundePageBase)Page).HandleException(ex, Page.Request, "Error while deleting " + DBObject);
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
			//catch (Exception ex)
			{
			//	string exceptionHandled = ((ShundePageBase)Page).HandleException(ex, Page.Request, "Error while saving " + DBObject);
			//	infoMessage.Text = "There was an error while saving that. " + exceptionHandled;
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
		/// Makes the table
		/// </summary>
		public void PopulateTable()
		{


			// write the javascript stuff we need for the calendar dropdowns first
			this.Controls.Add(new LiteralControl(@"
<SCRIPT LANGUAGE=""JavaScript"">

	document.write(getCalendarStyles());
 
</SCRIPT>

"
			));

			Table editorTable = new Table();
			editorTable.Width = new Unit(95, UnitType.Percentage);
			editorTable.CssClass = "ObjectEditorTable";
			editorTable.Style["table-layout"] = "fixed";

			{
				TableRow tr = new TableRow();
				editorTable.Rows.Add(tr);
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



			this.Controls.Add(editorTable);


			this.rows.Sort();
			foreach (ObjectEditorRow row in this.rows)
			{

				if (!row.Visible)
				{
					continue;
				}


				object value = row.DBColumn.FieldInfo.GetValue(DBObject);


				TableRow tableRow = new TableRow();
				editorTable.Rows.Add(tableRow);
				ControlCreator.CreateObjectEditorTableRow(row, tableRow, value);
			}





			if (extraControls.Count > 0)
			{
				TableRow extraControlsRow = new TableRow();
				editorTable.Rows.Add(extraControlsRow);
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

			editorTable.Rows.Add(buttonRow);









			buttonPanel.SaveButton.Click += new EventHandler(saveButton_Click);
			buttonPanel.CancelButton.Click += new EventHandler(cancelButton_Click);
			buttonPanel.DeleteButton.Click += new EventHandler(deleteButton_Click);

		}








		/// <summary>
		/// Writes the user-inputted values to the object
		/// </summary>
		public void SaveUserInputToObject()
		{

			this.rows.Sort();
			foreach (ObjectEditorRow row in this.rows)
			{

				if (!row.Visible)
				{
					continue;
				}


				object valueFromControl = ControlCreator.GetValueFromControl(row);
				if (valueFromControl == null)
				{
					row.DBColumn.FieldInfo.SetValue(DBObject, null);
				}
				else
				{
					object value = FrameworkUtils.ChangeType(valueFromControl, row.DBColumn.Type);
					row.DBColumn.FieldInfo.SetValue(DBObject, value);
				}
			}


			/*

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
			*/
		}



		ObjectEditorRow FindRow(string fieldName)
		{
			foreach (ObjectEditorRow row in this.rows)
			{
				if (row.DBColumn.Name.Equals(row))
				{
					return row;
				}
			}
			return null;
		}


		/// <summary>Pass an array of strings and column infos for each string will be made with display order in ascending order</summary>
		public void SetRows(IEnumerable<string> fieldNames)
		{
			int disOrd = (this.rows.Count == 0) ? 0 : this.rows[this.rows.Count - 1].DisplayOrder;
			foreach(string fieldName in fieldNames)
			{
				ObjectEditorRow row = GetRow(fieldName);
				row.DisplayOrder = disOrd;
				disOrd += 10;
			}
		}

		/// <summary>
		/// Adds the field with the given name to the rows
		/// </summary>
		/// <param name="fieldName"></param>
		public ObjectEditorRow AddRow(string fieldName)
		{
			if (FindRow(fieldName) != null)
			{
				throw new ShundeException(fieldName + " has already been added.");
			}
			ObjectEditorRow row = new ObjectEditorRow(this, fieldName);
			row.Header = TextUtils.MakeFriendly(fieldName);
			if (this.rows.Count > 0)
			{
				row.DisplayOrder = this.rows[this.rows.Count - 1].DisplayOrder + 100;
			}
			rows.Add(row);
			return row;
		}

		/// <summary>
		/// Gets the column info of a column
		/// </summary>
		public ObjectEditorRow GetRow(string fieldName)
		{
			ObjectEditorRow row = FindRow(fieldName);
			if (row == null)
			{
				row = AddRow(fieldName);
			}
			return row;
		}


		/// <summary>
		/// Sets the header for a column
		/// </summary>
		public void SetHeader(string fieldName, string header)
		{
			GetRow(fieldName).Header = header;
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
			<th>Date Imported</th><td><b>Date:</b> (dd/mm/yyyy) <input name=""ObjectEditor$dateImported"" type=""text"" value=""16/02/2006""  style=""width:70px"" id=""ObjectEditor_dateImported"" tabindex=""1"" />
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
