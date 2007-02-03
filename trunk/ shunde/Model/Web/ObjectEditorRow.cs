using System;
using System.Collections.Generic;
using System.Text;
using Shunde.Framework;
using System.Web.UI.WebControls;
using System.Reflection;
using Shunde.Utilities;

namespace Shunde.Web
{
	/// <summary>A Utility class for specifying values of a <see cref="DBColumn" /> for programmatically creating ASP.NET controls.</summary>
	public class ObjectEditorRow : IComparable
	{
		private DBColumn dbColumn;

		/// <summary>
		/// The DBColumn this row is for
		/// </summary>
		public DBColumn DBColumn
		{
			get { return dbColumn; }
		}

		
		/// <summary>
		/// Gets a unique ID for this row
		/// </summary>
		public string Id
		{
			get { return dbColumn.Name; }
		}



		private FindObjectDelegate findObjectDelegate = null;

		/// <summary>
		/// If a TextBox mode is being used, then this delegate will be called to either find or create an object
		/// </summary>
		public FindObjectDelegate FindObjectDelegate
		{
			get { return findObjectDelegate; }
			set { findObjectDelegate = value; }
		}


		private bool requiredField = false;

		/// <summary>
		/// Specifies whether to make this field required; by default the value specified in the DBTable for this column is used.
		/// </summary>
		public bool RequiredField
		{
			get { return requiredField; }
			set { requiredField = value; }
		}
	


		private InputMode inputMode = InputMode.Unspecified;

		/// <summary>
		/// If this is a selections column, then this specifies the mode to use to select objects.
		/// </summary>
		public InputMode InputMode
		{
			get { return inputMode; }
			set { inputMode = value; }
		}


		private string header = "";

		/// <summary>The Name to show end users, rather than the database Name</summary>
		/// <remarks>If left blank, then this gets populated automatically using <see cref="TextUtils.MakeFriendly(string)" /> on the column Name</remarks>
		public string Header
		{
			get { return header; }
			set { header = value; }
		}

		private string moreInfo = "";

		/// <summary>A description of the column to show end users</summary>
		public string MoreInfo
		{
			get { return moreInfo; }
			set { moreInfo = value; }
		}

		private string validationRegex = "";

		/// <summary>An optional regular expression to match against user input. This is in addition to any regular expression defined on the DBColumn itself.</summary>
		public string ValidationRegex
		{
			get { return validationRegex; }
			set { validationRegex = value; }
		}

		private string regexErrorMessage = "";

		/// <summary>The error message to give when the user entered data does not match against <see cref="ValidationRegex" /></summary>
		public string RegexErrorMessage
		{
			get { return regexErrorMessage; }
			set { regexErrorMessage = value; }
		}

		private bool visible = true;

		/// <summary>Specifies whether to display this column to the end user</summary>
		public bool Visible
		{
			get { return visible; }
			set { visible = value; }
		}


		private bool showTimeWithDate = true;

		/// <summary>Specifies that on a date field, a box for entering the time should be shown also. True, by default.</summary>
		public bool ShowTimeWithDate
		{
			get { return showTimeWithDate; }
			set { showTimeWithDate = value; }
		}

		private Unit texboxWidth;

		/// <summary>The width of the textbox. If null, then the default for the datatype is used.</summary>
		public Unit TextboxWidth
		{
			get { return texboxWidth; }
			set { texboxWidth = value; }
		}

		private Unit texboxHeight;

		/// <summary>The height of the textbox. If null, then the default for the datatype is used.</summary>
		public Unit TexboxHeight
		{
			get { return texboxHeight; }
			set { texboxHeight = value; }
		}



		private int displayOrder = 0;

		/// <summary>The order to display this column in, relative to others</summary>
		public int DisplayOrder
		{
			get { return displayOrder; }
			set { displayOrder = value; }
		}

		private string noSelectionName = "<none>";

		/// <summary>The option Name in a dropdown list to signify that no selection has been made</summary>
		public string NoSelectionName
		{
			get { return noSelectionName; }
			set { noSelectionName = value; }
		}

		private bool addOnTheFly = false;

		/// <summary>Specifies that, for a selection, new values can be typed in by the user</summary>
		public bool AddOnTheFly
		{
			get { return addOnTheFly; }
			set { addOnTheFly = value; }
		}


		private bool autoPopulate = false;

		/// <summary>If true, then for a selection, the Object Editor will automatically populate the selections</summary>
		public bool AutoPopulate
		{
			get { return autoPopulate; }
			set { autoPopulate = value; }
		}

		private List<ListItem> listItems = null;

		/// <summary>The available selections for a drop-down list</summary>
		public List<ListItem> ListItems
		{
			get { return listItems; }
			set { listItems = value; }
		}

		private bool useRichTextEditor = false;

		/// <summary>If this is an unbounded string, then a RTE can be used if this is set to true</summary>
		public bool UseRichTextEditor
		{
			get { return useRichTextEditor; }
			set { useRichTextEditor = value; }
		}


		private string selectionsPopupUrl = "";

		/// <summary>The Url of the popup window to select the object from</summary>
		public string SelectionsPopupUrl
		{
			get { return selectionsPopupUrl; }
			set { selectionsPopupUrl = value; }
		}

		private string searchBoxUrl = "";

		/// <summary>
		/// The url of the page which will return the search results of that which is being typed in, if the selection mode for this column is set to SearchBox
		/// </summary>
		public string SearchBoxUrl
		{
			get { return searchBoxUrl; }
			set { searchBoxUrl = value; }
		}

		private string viewBinaryDataUrl = "";

		/// <summary>The address to preview binary data</summary>
		public string ViewBinaryDataUrl
		{
			get { return viewBinaryDataUrl; }
			set { viewBinaryDataUrl = value; }
		}

		/// <summary>
		/// Gets the object editor that this row is part of
		/// </summary>
		public ObjectEditor ObjectEditor
		{
			get { return this.objectEditor; }
		}


		private ObjectEditor objectEditor;


		/// <summary>
		/// Creates a new ObjectEditor row, using the specified name to get the DBColumn reference
		/// </summary>
		public ObjectEditorRow(ObjectEditor objectEditor, string name)
		{
			this.objectEditor = objectEditor;
			ObjectInfo oi = ObjectInfo.GetObjectInfo(this.objectEditor.DBObject.GetType());
			this.dbColumn = oi.FindDBColumn(name);
			this.RequiredField = !dbColumn.AllowNulls;
			this.AutoDetectAndSetInputMode();
		}

		/// <summary>
		/// Creates a new ObjectEditor row for the given column
		/// </summary>
		public ObjectEditorRow(ObjectEditor objectEditor, DBColumn dbColumn)
		{
			this.objectEditor = objectEditor;
			this.dbColumn = dbColumn;
			this.RequiredField = !dbColumn.AllowNulls;
			this.AutoDetectAndSetInputMode();
		}

		/// <summary>
		/// Sets the ListItems for this row, basing it on DBObjects
		/// </summary>
		public void SetListItems(IEnumerable<DBObject> dbObjects, DBObject selectedObject)
		{
			SetListItems(dbObjects, selectedObject, "FriendlyName");
		}

		/// <summary>
		/// Sets the ListItems for this row, basing it on DBObjects
		/// </summary>
		public void SetListItems(IEnumerable<DBObject> dbObjects, DBObject selectedObject, string textProperty)
		{
			if (this.ListItems == null)
			{
				this.ListItems = new List<ListItem>();
			}
			foreach (DBObject obj in dbObjects)
			{
				ListItem li = new ListItem();
				li.Value = obj.Id.ToString();
				li.Selected = (selectedObject != null && selectedObject.Equals(obj));
				if (textProperty == "FriendlyName")
				{
					li.Text = obj.FriendlyName; // why do this? it's faster than reflection, and will normally (?) be used
				}
				else
				{
					PropertyInfo propertyInfo = obj.GetType().GetProperty(textProperty, BindingFlags.Public | BindingFlags.Instance);
					if (propertyInfo == null)
					{
						throw new Exception("The property " + textProperty + " could not be found on the type " + obj.GetType().FullName);
					}
					li.Text = (string)propertyInfo.GetValue(obj, null);
				}
				this.ListItems.Add(li);
			}
		}

		/// <summary>
		/// Sets the ListItems to be the enumeration values in an enumeration
		/// </summary>
		/// <param name="type">The type of the enumeration</param>
		/// <param name="currentValue">The current value</param>
		public void SetEnumListItems(Type type, Enum currentValue)
		{
			if (!FrameworkUtils.IsEnumOrNullableEnum(type))
			{
				throw new ArgumentException("The type " + type.FullName + " is not an enumeration", "type");
			}

			if (this.ListItems == null)
			{
				this.ListItems = new List<ListItem>();
			}
			Array allValues;
			if (type.IsEnum)
			{
				allValues = Enum.GetValues(type);
			}
			else
			{
				allValues = Enum.GetValues(Nullable.GetUnderlyingType(type));
			}
			foreach (Enum val in allValues)
			{
				ListItem li = new ListItem();
				li.Text = val.ToString();
				li.Value = Convert.ToInt32(val).ToString();
				li.Selected = val.Equals(currentValue);
				this.ListItems.Add(li);
			}

		}

		/// <summary>
		/// Guesses which input mode should be given, based on the DBColumn's object type, and sets the <see cref="ObjectEditorRow.InputMode" />.
		/// </summary>
		public void AutoDetectAndSetInputMode()
		{
			Type t = this.DBColumn.Type;
			if (t == typeof(bool))
			{
				this.inputMode = InputMode.Checkbox;
			}
			else if (t == typeof(short) || t == typeof(int) || t == typeof(long) || t == typeof(float) || t == typeof(double))
			{
				this.inputMode = InputMode.NumberTextBox;
			}
			else if (t == typeof(short?) || t == typeof(int?) || t == typeof(long?) || t == typeof(float?) || t == typeof(double?))
			{
				this.inputMode = InputMode.NumberTextBox;
			}
			else if (t == typeof(string))
			{
				if (this.DBColumn.MaxLength < 0)
				{
					this.inputMode = InputMode.MultilineTextBox;
				}
				else if (this.dbColumn.Name.ToLower().Contains("password"))
				{
					this.inputMode = InputMode.Password;
					if (!this.DBColumn.AllowNulls && this.ObjectEditor.DBObject.Exists())
					{
						this.RequiredField = false;
					}
				}
				else
				{
					this.inputMode = InputMode.TextBox;
				}
			}
			else if (t == typeof(BinaryData))
			{
				this.inputMode = InputMode.FileUpload;
			}
			else if (t == typeof(DBObject) || t.IsSubclassOf(typeof(DBObject)))
			{
				this.inputMode = InputMode.DropDownList;
			}
			else if (t == typeof(DateTime) || t == typeof(DateTime?))
			{
				this.inputMode = InputMode.DateTimePicker;
			}
			else if (FrameworkUtils.IsEnumOrNullableEnum(t))
			{
				this.inputMode = InputMode.DropDownList;
			}
			else
			{
				throw new ShundeException("No input mode could be set for column " + this.DBColumn.Name);
			}
		}


		/// <summary>Gets the html to create a link which invokes a javascript alert containing the <see cref="MoreInfo" /> details</summary>
		public string GetMoreInfoAsJavascriptPopupHtml()
		{
			if (moreInfo.Length == 0)
			{
				return "";
			}
			return " <nobr><a href=\"javascript:void(null);\" onClick=\"javascript:alert('" + TextUtils.JavascriptStringEncode(moreInfo) + "');\" class=\"moreInfoLink\">[more info]</a></nobr>";
		}



		#region IComparable Members

		/// <summary>
		/// Compares this column options object to another based on the display order
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int CompareTo(object obj)
		{
			ObjectEditorRow co = obj as ObjectEditorRow;
			if (co == null) {
				return 0;
			}
			return this.DisplayOrder.CompareTo(co.DisplayOrder);
		}

		#endregion


	}

	/// <summary>
	/// These are the different modes available for inputting an object in the ObjectEditor
	/// </summary>
	public enum InputMode
	{
		
		/// <summary>
		/// The input mode has not been specified
		/// </summary>
		Unspecified,

		/// <summary>
		/// Auto-detect the best input mode based on the column's data type
		/// </summary>
		Checkbox,

		/// <summary>
		/// Uses a dropdown list to select the object.
		/// </summary>
		DropDownList,

		/// <summary>
		/// Uses a radiobuttonlist. This is best when there are only a few options to choose from.
		/// </summary>
		RadioButtonList,

		/// <summary>
		/// Uses a ComboBox
		/// </summary>
		ComboBox,

		/// <summary>
		/// Uses a textbox to allow the user to enter in a name to find the object. If a url for a popup window is given, then it uses a popup window. This is best when there are hundreds or more objects, or some advanced search is required to find the object. If an XML search url is specified, then as they write, matching objects are displayed, allowing them to select an object.
		/// </summary>
		TextBox,

		/// <summary>
		/// Users a multi-line textbox
		/// </summary>
		MultilineTextBox,

		/// <summary>
		/// A textbox for entering numbers
		/// </summary>
		NumberTextBox,

		/// <summary>
		/// Binary file upload
		/// </summary>
		FileUpload,

		/// <summary>
		/// Date and/or time picker
		/// </summary>
		DateTimePicker,

		/// <summary>
		/// Rendered as an HTML hidden input field
		/// </summary>
		HiddenField,

		/// <summary>
		/// Rendered as password control, with a confirmation password input
		/// </summary>
		Password

	}


	/// <summary>
	/// A delegate to handle the creation of a new object, given only the name.
	/// </summary>
	/// <param name="name">The name that the user has typed in</param>
	/// <param name="createIfNotExists">If true, then the object will be attempted to be created, if possible, based on just the name</param>
	public delegate DBObject FindObjectDelegate(string name, bool createIfNotExists);


}
