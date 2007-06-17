using System;
using System.Collections.Generic;
using System.Text;
using Shunde.Framework;
using Shunde.Utilities;
using Shunde.Framework.Columns;

namespace Shunde.Common
{
	/// <summary>An Type of an <see cref="Attrib" /></summary>
	public class AttribType : DBObject
	{

		private DBObject owner;

		/// <summary>The object that this Attribute Type belongs to</summary>
		public DBObject Owner
		{
			get { return owner; }
			set { owner = value; }
		}

		private string name;

		/// <summary>The Name of this attribute</summary>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		private string instructions;

		/// <summary>Instructions on how this attribute should be filled in, for site administrators</summary>
		public string Instructions
		{
			get { return instructions; }
			set { instructions = value; }
		}

		private AttribDataType dataType;

		/// <summary>The type of the attribute</summary>
		public AttribDataType DataType
		{
			get { return dataType; }
			set { dataType = value; }
		}

		private bool isShared;

		/// <summary>Specifies that this is a shared attribute. This means more than one object can access this attribute.</summary>
		public bool IsShared
		{
			get { return isShared; }
			set { isShared = value; }
		}

		private bool allowMultiSelections = false;

		/// <summary>Allow multiple selections to be made. Only valid if this is a shared attribute type.</summary>
		public bool AllowMultiSelections
		{
			get { return allowMultiSelections; }
			set { allowMultiSelections = value; }
		}

		private string suffix;

		/// <summary>The text which should follow the Value of the attribute</summary>
		/// <remarks>This is useful for those attributes which are measures. For example, the Suffix may be "meters", in which case "meters" will follow the Value of the attribute</remarks>
		public string Suffix
		{
			get { return suffix; }
			set { suffix = value; }
		}

		private int? decimalPlaces = null;

		/// <summary>The number of decimal places to round to, for float numbers</summary>
		public int? DecimalPlaces
		{
			get { return decimalPlaces; }
			set { decimalPlaces = value; }
		}


		private bool isRequired = true;

		/// <summary>
		/// Specifies that this attribute is required to have a value
		/// </summary>
		public bool IsRequired
		{
			get { return isRequired; }
			set { isRequired = value; }
		}


		private IEnumerable<IAttribRelation> currentRelations;

		/// <summary>
		/// Currently selected relations for this attribute type
		/// </summary>
		public IEnumerable<IAttribRelation> CurrentRelations
		{
			get { return currentRelations; }
			set { currentRelations = value; }
		}

		private bool isImage = false;

		/// <summary>
		/// Applicable to Binary data; specifies that it is an image
		/// </summary>
		public bool IsImage
		{
			get { return isImage; }
			set { isImage = value; }
		}


		private bool isMultiline = false;

		/// <summary>
		/// Applicable to strings only, specifies that it is a multi-line string
		/// </summary>
		public bool IsMultiLine
		{
			get { return isMultiline; }
			set { isMultiline = value; }
		}

		private bool useRichTextEditor = false;

		/// <summary>
		/// Applicable to Multiline strings only, specifies that, if available, a rich-text editor should be used for editing
		/// </summary>
		public bool UseRichTextEditor
		{
			get { return useRichTextEditor; }
			set { useRichTextEditor = value; }
		}

		/// <summary>
		/// Gets the friendly name of this object
		/// </summary>
		public override string FriendlyName
		{
			get
			{
				return name;
			}
		}


		/// <summary>Sets up the <see cref="Shunde.Framework.ObjectInfo" /> for this class</summary>
		static AttribType()
		{

			DBTable tbl = new DBTable("AttribType", new DBColumn[] {
				new DBObjectColumn( "owner", typeof(DBObject), false ),
				new SingleLineString( "name", 1, 200 ),
				new MultiLineString( "instructions", true ),
				new DBObjectColumn( "dataType", typeof(AttribDataType), false ),
				new BoolColumn( "isShared"),
				new BoolColumn( "allowMultiSelections"),
				new SingleLineString( "suffix", 0, 20 ),
				new NumberColumn( "decimalPlaces", typeof(int?), 0, 100 ),
				new BoolColumn( "isRequired" ),
				new BoolColumn( "isImage"),
				new BoolColumn( "isMultiline"),
				new BoolColumn( "useRichTextEditor" )
			});

			ObjectInfo.RegisterObjectInfo(typeof(AttribType), tbl);

		}

		/// <summary>Gets the Type of the attribute</summary>
		public Type GetDataType()
		{
			return DataType.GetDataType();
		}


		/// <summary>Gets and populates the AttribTypes that exist in the given object</summary>
		public static AttribType[] GetAttribTypes(DBObject owner)
		{
			Type t = typeof(AttribType);
			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);
			return (AttribType[])DBObject.GetObjects(oi.GetSelectStatement() + " WHERE DBObject.isDeleted = 0 AND AttribType.ownerId = " + owner.Id + " ORDER BY DBObject.displayOrder ASC, AttribType.name ASC", t);
		}

		/// <summary>Gets an AttribType with the given Name</summary>
		public static AttribType GetAttribType(string name)
		{
			return (AttribType)DBObject.GetObject("SELECT id FROM AttribType WHERE name = '" + DBUtils.ParseSql(name) + "'");
		}

	}
}
