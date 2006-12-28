using System;
using System.Collections.Generic;
using System.Text;
using Shunde.Framework;
using Shunde.Utilities;

namespace Shunde.Common
{
	/// <summary>An Attrib, short for Attribute, is just an attribute for an object</summary>
	/// <remarks>This class exists so that you don't need to create a database table for every single kind of attribute. An example of an attribute is Screen Type (LCD or CRT etc) of a Monitor. The monitor would have screenType variable of type Attrib, which would be one of the Attribs in the database with the "ScreenType" <see cref="AttribType" />.</remarks>
	public class Attrib : TreeNode
	{


		private AttribType attribType;

		/// <summary>The type of this Attribute</summary>
		/// <remarks>For example, "ScreenType".</remarks>
		public AttribType AttribType
		{
			get { return attribType; }
			set { attribType = value; }
		}


		private BinaryData dataValue;

		/// <summary>The Value of this attribute</summary>
		public object Value
		{
			get {
				Type t = attribType.GetDataType();
				if (t.Equals(typeof(BinaryData)))
				{
					return this.dataValue;
				}
				else if (attribType.IsMultiLine)
				{
					return this.Notes;
				}
				else
				{
					return Convert.ChangeType(this.Name, t);
				}
			}
			set {
				Type t = attribType.GetDataType();
				if (t.Equals(typeof(BinaryData)))
				{
					this.dataValue = (BinaryData)value;
				}
				else if (attribType.IsMultiLine)
				{
					this.Notes = value.ToString();
					this.Name = TextUtils.CutText(this.Notes.Replace('\n', ' '), 50);
				}
				else
				{
					this.Name = value.ToString();
				}
			}
		}

		/// <summary>
		/// Returns the textual value of this attribute
		/// </summary>
		public string TextualValue
		{
			get
			{
				return this.Name;
			}
		}


		/// <summary>Sets up the <see cref="Shunde.Framework.ObjectInfo" /> for this class</summary>
		static Attrib()
		{

			DBTable tbl = new DBTable("Attrib", new DBColumn[] {
				new DBColumn( "attribType", typeof(AttribType), false ),
				new DBColumn( "dataValue", typeof(BinaryData), true )
			});

			ObjectInfo.RegisterObjectInfo(typeof(Attrib), tbl);

		}





		/// <summary>
		/// Saves this object
		/// </summary>
		public override void Save()
		{
			Type t = attribType.GetDataType();
			if (t.Equals(typeof(BinaryData)))
			{
				if (attribType.IsRequired && !dataValue.Exists)
				{
					throw new ValidationException("Please enter a value for " + attribType.Name);
				}
			} else {
				if (attribType.IsRequired && Name.Length == 0) {
					throw new ValidationException("Please enter a value for " + attribType.Name);
				}
			}
			base.Save();
		}







		/// <summary>Gets and populates all the Attribs for a given <see cref="AttribType" /></summary>
		/// <param name="attribType">The <see cref="AttribType" /> that the attribs are for</param>
		/// <returns>Returns a forest of 0 or more Attribs for the given AttribType</returns>
		public static Attrib[] GetAttribsAsForest(AttribType attribType)
		{

			Type t = typeof(Attrib);
			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);
			string sql = oi.GetSelectStatement() + " WHERE DBObject.isDeleted = 0 AND Attrib.attribTypeId = " + attribType.Id + " ORDER BY DBObject.displayOrder ASC, TreeNode.name ASC";

			Attrib[] objs = (Attrib[])DBObject.GetObjects(sql, t);
			foreach (Attrib attrib in objs)
			{
				attrib.AttribType = attribType;
			}
			return (Attrib[])ConvertToForest(objs, typeof(Attrib));

		}





		/// <summary>Creates a new Attrib with the given Value and Parent and returns the newly created object</summary>
		/// <remarks>If the object already existed and the AttribType is a shared attribute, then nothing is created and that object is returned.</remarks>
		public static Attrib CreateFromValue(AttribType attribType, string value, Attrib parent)
		{


			if (attribType.IsShared)
			{
				try
				{
					try
					{

						string parentValue = (parent == null) ? "null" : parent.Id.ToString();

						return (Attrib)DBObject.GetObject("SELECT a.[id] FROM Attrib a INNER JOIN DBObject obj ON obj.id = a.id INNER JOIN TreeNode c ON c.id = a.id WHERE obj.isDeleted = 0 AND c.name = '" + DBUtils.ParseSql(value) + "' AND a.attribTypeId = " + attribType.Id + " AND c.parentId = " + parentValue);
					}
					catch (ValidationException vex)
					{
						// a validation exception is thrown if more than one row is returned
						// in this case, we change to a ShundeException because this shouldn't
						// occur on the website
						throw new ShundeException(vex.Message);
					}
				}
				catch (ObjectDoesNotExistException)
				{
					// do nothing
				}
			}

			Attrib att = new Attrib();
			att.Parent = parent;
			att.AttribType = attribType;
			att.Name = value;
			att.LastUpdatedBy = "Attrib.createFromValue";
			att.Save();
			return att;

		}



	}
}
