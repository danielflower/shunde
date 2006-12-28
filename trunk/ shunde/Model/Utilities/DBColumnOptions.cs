using System;
using System.Collections.Generic;
using System.Text;
using Shunde.Framework;
using System.Web.UI.WebControls;

namespace Shunde.Utilities
{
	/// <summary>A Utility class for specifying values of a <see cref="DBColumn" /> for programmatically creating ASP.NET controls.</summary>
	public class ColumnInfo
	{

		/// <summary>The regular expression that matches an email address in SQL Server</summary>
		public static string SqlEmailRegex = "_%@_%._%";

		/// <summary>The regular expression that matches an email address in standard Regular Expression syntax</summary>
		public static string EmailRegex = "..*@..*\\...*";



		private FindObjectDelegate findObjectDelegate = null;

		/// <summary>
		/// If a TextBox mode is being used, then this delegate will be called to either find or create an object
		/// </summary>
		public FindObjectDelegate FindObjectDelegate
		{
			get { return findObjectDelegate; }
			set { findObjectDelegate = value; }
		}


		private RequiredField requiredField = RequiredField.UseColumnDefault;

		/// <summary>
		/// Specifies whether to make this field required; by default the value specified in the DBTable for this column is used.
		/// </summary>
		public RequiredField RequiredField
		{
			get { return requiredField; }
			set { requiredField = value; }
		}
	


		private SelectionMode selectionMode = SelectionMode.Default;

		/// <summary>
		/// If this is a selections column, then this specifies the mode to use to select objects.
		/// </summary>
		public SelectionMode SelectionMode
		{
			get { return selectionMode; }
			set { selectionMode = value; }
		}


		private string friendlyName = "";

		/// <summary>The Name to show end users, rather than the database Name</summary>
		/// <remarks>If left blank, then this gets populated automatically using <see cref="TextUtils.MakeFriendly(string)" /> on the column Name</remarks>
		public string FriendlyName
		{
			get { return friendlyName; }
			set { friendlyName = value; }
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

		private bool showThisColumn = true;

		/// <summary>Specifies whether to display this column to the end user</summary>
		public bool ShowThisColumn
		{
			get { return showThisColumn; }
			set { showThisColumn = value; }
		}

		private bool isInvisible = false;

		/// <summary>Specifies that this column should be made into a text box in the form, however it should be invisible to the user (by using an Html hidden input field).</summary>
		public bool IsInvisible
		{
			get { return isInvisible; }
			set { isInvisible = value; }
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



		private int displayOrder = 1000000;

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

		private String defaultValue = "";

		/// <summary>The default Value of the column</summary>
		public String DefaultValue
		{
			get { return defaultValue; }
			set { defaultValue = value; }
		}

		private bool autoPopulate = false;

		/// <summary>If true, then for a selection, the Object Editor will automatically populate the selections</summary>
		public bool AutoPopulate
		{
			get { return autoPopulate; }
			set { autoPopulate = value; }
		}

		private IList<DBObject> selections = null;

		/// <summary>The available selections for a drop-down list</summary>
		public IList<DBObject> Selections
		{
			get { return selections; }
			set { selections = value; }
		}

		private bool useRichTextEditor = false;

		/// <summary>If this is an unbounded string, then a RTE can be used if this is set to true</summary>
		public bool UseRichTextEditor
		{
			get { return useRichTextEditor; }
			set { useRichTextEditor = value; }
		}

		private int maxAllowedInDropDown = int.MaxValue;

		/// <summary>The maximum number of options in a drop down allowed before a popup window is used to select an object. To force the use of a pop up window, set this to -1</summary>
		public int MaxAllowedInDropDown
		{
			get { return maxAllowedInDropDown; }
			set { maxAllowedInDropDown = value; }
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


		/// <summary>Gets the html to create a link which invokes a javascript alert containing the <see cref="MoreInfo" /> details</summary>
		public String GetMoreInfoAsJavascriptPopupHtml()
		{
			if (moreInfo.Length == 0)
			{
				return "";
			}
			return " <nobr><a href=\"javascript:void(null);\" onClick=\"javascript:alert('" + TextUtils.JavascriptStringEncode(moreInfo) + "');\" class=\"moreInfoLink\">[more info]</a></nobr>";
		}


	}

	/// <summary>
	/// These are the different modes available for selecting an object in the ObjectEditor
	/// </summary>
	public enum SelectionMode
	{
		
		/// <summary>
		/// Uses a dropdown list to select the object.
		/// </summary>
		DropDownList = 1,

		/// <summary>
		/// Uses a radiobuttonlist. This is best when there are only a few options to choose from.
		/// </summary>
		RadioButtonList = 2,

		/// <summary>
		/// Uses a textbox to allow the user to enter in a name to find the object. If a url for a popup window is given, then it uses a popup window. This is best when there are hundreds or more objects, or some advanced search is required to find the object. If an XML search url is specified, then as they write, matching objects are displayed, allowing them to select an object.
		/// </summary>
		TextBox = 3,

		/// <summary>
		/// The default action is to use a dropdown list, unless a specified threshold is passed.
		/// </summary>
		Default = 4

	}

	/// <summary>
	/// Specifies whether to make a field required or not
	/// </summary>
	public enum RequiredField
	{

		/// <summary>
		/// Use the default for the column, as specified in the DBTable for the object
		/// </summary>
		UseColumnDefault,

		/// <summary>
		/// Make the field required
		/// </summary>
		Yes,

		/// <summary>
		/// Make the field not required
		/// </summary>
		No

	}


	/// <summary>
	/// A delegate to handle the creation of a new object, given only the name.
	/// </summary>
	/// <param name="name">The name that the user has typed in</param>
	/// <param name="createIfNotExists">If true, then the object will be attempted to be created, if possible, based on just the name</param>
	public delegate DBObject FindObjectDelegate(string name, bool createIfNotExists);


}
