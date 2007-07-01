using System;
using Shunde.Framework;
using Shunde.Framework.Columns;
using Shunde.Common;
using System.Drawing;
using System.Collections.Generic;

namespace TestingGround
{



	/// <summary>
	/// A Category object
	/// </summary>
	public class Category : DBObject, ITreeNode<Category>
	{

		#region Fields
		private Category parent;
		private string name;
		private Color color;
		private IList<Category> children;
		private DateTime? dateAdded = DateTimeColumn.NotSetValue;

		#endregion


		#region Properties


		/// <summary></summary>
		public string Name
		{
			get { return this.name; }
			set { this.name = value; }
		}

		public override string FriendlyName
		{
			get
			{
				return this.name;
			}
		}

		/// <summary></summary>
		public Color Color
		{
			get { return this.color; }
			set { this.color = value; }
		}
		public DateTime? DateAdded
		{
			get { return dateAdded; }
			set { dateAdded = value; }
		}


		#endregion


		#region Static Constructor

		/// <summary>Sets up the <see cref="ObjectInfo" /> for this class</summary>
		static Category()
		{

			DBTable tbl = new DBTable("Category", new DBColumn[] {
				new DBObjectColumn("parent", typeof(Category), true),
				new SingleLineString("name", 1, 100),
				new ColorColumn("color", false),
				new DateTimeColumn("dateAdded", false, DateTimePart.DateAndOptionallyTime)
			});

			ObjectInfo.RegisterObjectInfo(typeof(Category), tbl);

		}

		#endregion


		public override void Save()
		{
			if (TreeNodeHelper<Category>.HasHeirachyLoop(this))
			{
				throw new Shunde.ValidationException("There is a loop!");
			}
			base.Save();
		}

		#region Static Methods

		/// <summary>Gets all the Category objects in the database</summary>
		public static Category[] GetCategories()
		{

			Type t = typeof(Category);
			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);
			string sql = oi.GetSelectStatement() + " WHERE [DBObject].[isDeleted] = 0 ORDER BY [DBObject].[displayOrder] ASC";
			return (Category[])DBObject.GetObjects(sql, t);

		}

		/// <summary>Gets all the Category objects in the database filtered by the given <see cref="Category">parent</see></summary>
		/// <param name="parent">The Category to filter by</param>
		public static Category[] GetCategories(Category parent)
		{

			Type t = typeof(Category);
			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);
			string sql = oi.GetSelectStatement() + " WHERE [DBObject].[isDeleted] = 0 AND [Category].[parentId] = " + parent.Id + " ORDER BY [DBObject].[displayOrder] ASC";
			return (Category[])DBObject.GetObjects(sql, t);

		}

		#endregion



		#region ITreeNode<Category> Members


		public Category Parent
		{
			get
			{
				return this.parent;
			}
			set
			{
				this.parent = value;
			}
		}

		public IList<Category> Children
		{
			get
			{
				return this.children;
			}
			set
			{
				this.children = value;
			}
		}

		#endregion


	}



}

