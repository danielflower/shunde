using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Shunde.Framework;
using Shunde.Utilities;

namespace Shunde.Common
{
	/// <summary>A node of a tree, to allow for heirachical structures</summary>
	public abstract class TreeNode : DBObject, IComparable
	{

		private string name;

		/// <summary>The Name of the TreeNode</summary>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		private string notes;

		/// <summary>Notes pertaining to this TreeNode</summary>
		public string Notes
		{
			get { return notes; }
			set { notes = value; }
		}

		private TreeNode parent;

		/// <summary>The Parent of this TreeNode</summary>
		public TreeNode Parent
		{
			get { return parent; }
			set { parent = value; }
		}

		private bool isHidden;

		/// <summary>Specifies that this TreeNode is hidden</summary>
		public bool IsHidden
		{
			get { return isHidden; }
			set { isHidden = value; }
		}

		private TreeNode[] children;

		/// <summary>The Children of this TreeNode</summary>
		public TreeNode[] Children
		{
			get { return children; }
			set { children = value; }
		}

		private bool isShaded;

		/// <summary>Specifies whether a TreeNode is visible, or shaded</summary>
		/// <remarks>A TreeNode is 'shaded' if either it is hidden, or one of its ancestors is hidden - this is not set from the database; rather it is a 'cached' variable, only valid if set, to save time working it out every time it is needed.</remarks>
		public bool IsShaded
		{
			get { return isShaded; }
		}

		/// <summary>Compares the Name of this string to another TreeNode</summary>
		public virtual int CompareTo(Object another)
		{
			TreeNode cat = another as TreeNode;
			if (cat != null)
			{
				return FullName.CompareTo(cat.FullName);
			}
			else
			{
				return 0;
			}
		}

		/// <summary>Returns the Name of this TreeNode, with all ancestors names</summary>
		public string FullName
		{
			get
			{
				string fullName = "";

				TreeNode tempParent = Parent;
				while (tempParent != null)
				{
					fullName = tempParent.Name + " > " + fullName;
					tempParent = tempParent.Parent;
				}

				return fullName + Name;
			}
		}


		/// <summary>
		/// Gets the friendly name of the object
		/// </summary>
		public override string FriendlyName
		{
			get { return FullName; }
		}

		/// <summary>Sets up the <see cref="ObjectInfo" /> for this class</summary>
		static TreeNode()
		{

			DBTable tbl = new DBTable("TreeNode", new DBColumn[] {
				new DBColumn( "name", typeof(string), 1, 200 ),
				new DBColumn( "notes", typeof(string), true ),
				new DBColumn( "parent", typeof(TreeNode), true),
				new DBColumn( "isHidden", typeof(bool), false)
			});

			ObjectInfo.RegisterObjectInfo(typeof(TreeNode), tbl);

		}

		/// <summary>Populates the ancestors of this tree node</summary>
		public void PopulateAncestors()
		{
			TreeNode cat = this.Parent;
			while (cat != null)
			{
				cat.Populate();
				cat = cat.Parent;
			}
		}


		/// <summary>Saves this object to the database</summary>
		/// <remarks>Before saving, TreeNode loops are checked for</remarks>
		public override void Save()
		{


			// check that there isn't a heirachy loop!
			TreeNode tempParent = Parent;
			while (tempParent != null)
			{
				if (tempParent.Id == Id)
				{
					throw new ValidationException("You have selected as the parent category one of the descendents of the current category. This means you have made a category loop, which is not allowed. Please select a different parent for this category.");
				}
				tempParent = tempParent.Parent;
			}


			base.Save();
		}

		/// <summary>Returns true if this TreeNode is a leaf node (ie. has no Children)</summary>
		public bool IsLeaf
		{
			get
			{
				return Children.Length == 0;
			}
		}

		/// <summary>Returns the root node of the tree that this TreeNode belongs in</summary>
		public TreeNode RootNode
		{
			get
			{
				TreeNode cur = this;
				while (cur.Parent != null)
				{
					cur = cur.Parent;
				}
				return cur;
			}
		}

		/// <summary>Gets all the tree nodes in the database</summary>
		/// <remarks>Even gets deleted tree nodes</remarks>
		public static TreeNode[] GetTreeNodesAsForest(Type baseType, Type[] catTypes)
		{
			ObjectInfo oi = ObjectInfo.GetObjectInfo(baseType);
			return GetTreeNodesAsForest(oi.GetSelectStatement() + " ORDER BY displayOrder ASC, name ASC", baseType, catTypes);
		}


		/// <summary>Gets all the tree nodes in the database</summary>
		public static TreeNode[] GetTreeNodesAsForest(string where, Type baseType, Type[] catTypes)
		{
			TreeNode[] nodes = (TreeNode[])DBObject.GetObjects(baseType, catTypes, where);
			return ConvertToForest(nodes, baseType);
		}


		/// <summary>Converts an array of TreeNode objects into a forest of trees</summary>
		protected static TreeNode[] ConvertToForest(TreeNode[] allCats, Type baseType)
		{
			// first, put every TreeNode into a dictionary
			Dictionary<int,int> dictionary = new Dictionary<int,int>();

			for (int i = 0; i < allCats.Length; i++)
			{
				TreeNode ac = allCats[i];
				ac.Children = (TreeNode[])Array.CreateInstance(baseType, 0);
				dictionary.Add(ac.Id, i);
			}


			// now, go through each TreeNode. If Parent is null, then it is a Parent, so add it to the 'parents' ArrayList, which is what will be returned from this method. If Parent is not null, then find the index of the Parent in the allCats array, and set that to be it's Parent
			List<TreeNode> parents = new List<TreeNode>();

			foreach (TreeNode cat in allCats)
			{

				if (cat.IsDeleted)
				{
					continue;
				}

				if (cat.Parent == null)
				{
					parents.Add(cat);
				}
				else
				{
					if (!dictionary.ContainsKey(cat.Parent.Id))
					{
						continue;
					}
					int parentIndex = dictionary[cat.Parent.Id];
					cat.Parent = allCats[parentIndex];



					if (!cat.Parent.IsDeleted)
					{

						TreeNode[] children = (TreeNode[])Array.CreateInstance(baseType, cat.Parent.Children.Length + 1);
						cat.Parent.Children.CopyTo(children, 0);
						children[cat.Parent.Children.Length] = cat;
						cat.Parent.Children = children;

					}


				}
			}


			TreeNode[] cats = (TreeNode[])Array.CreateInstance(baseType, parents.Count);
			parents.CopyTo(cats);

			//TreeNode[] cats = (TreeNode[])parents.ToArray();

			// now go through each TreeNode checking to see if it is 'shaded' - and setting the cache variable
			foreach (TreeNode cat in cats)
			{
				SetShadedProperty(cat, false);
			}


			return cats;
		}

		/// <summary>Sets the cached variable for a TreeNode and it's descendants</summary>
		private static void SetShadedProperty(TreeNode cat, bool shadeValue)
		{

			if (cat.IsHidden)
			{
				shadeValue = true;
				cat.isShaded = true;
			}


			for (int i = 0; i < cat.Children.Length; i++)
			{
				TreeNode child = (TreeNode)cat.Children[i];
				child.isShaded = shadeValue;
				SetShadedProperty(child, shadeValue);
			}

		}

		/// <summary>Finds the TreeNode from the Children, or descendants of this TreeNode</summary>
		public TreeNode FindDescendant(int catId)
		{
			if (this.Id == catId)
			{
				return this;
			}
			for (int i = 0; i < Children.Length; i++)
			{
				TreeNode child = (TreeNode)Children[i];
				TreeNode search = child.FindDescendant(catId);
				if (search != null)
				{
					return search;
				}
			}
			return null;
		}

		/// <summary>Finds the TreeNode with the TreeNode id catId from the tree of categories</summary>
		public static TreeNode FindTreeNode(TreeNode[] cats, int catId)
		{

			foreach (TreeNode cat in cats)
			{
				if (cat.Id == catId)
				{
					return cat;
				}
				TreeNode descendant = cat.FindDescendant(catId);
				if (descendant != null)
				{
					return descendant;
				}
			}

			return null;
		}

		/// <summary>Returns true if this TreeNode, and none of its parents, are hidden</summary>
		public bool IsVisible
		{
			get
			{
				if (Parent == null)
				{
					return !IsHidden;
				}
				else
				{
					return !IsHidden && Parent.IsVisible;
				}
			}
		}

		/// <summary>Converts a heirachacle Array of Categories into a flat array of Categories</summary>
		/// <remarks>The relationships between Parent/Children are retained. It orders the categories alphabetically.</remarks>
		public static TreeNode[] ConvertToFlatArray(TreeNode[] cats)
		{
			return ConvertToFlatArray(cats, typeof(TreeNode));
		}

		/// <summary>Converts a heirachacle Array of Categories into a flat array of Categories</summary>
		/// <remarks>The relationships between Parent/Children are retained. It orders the categories alphabetically.</remarks>
		public static TreeNode[] ConvertToFlatArray(TreeNode[] cats, Type baseType)
		{
			List<TreeNode> al = new List<TreeNode>();
			foreach (TreeNode cat in cats)
			{
				AddCats(al, cat);
			}

			al.Sort();

			if (baseType.Equals(typeof(TreeNode)))
			{
				return al.ToArray();
			}
			else
			{
				TreeNode[] array = (TreeNode[])Array.CreateInstance(baseType, al.Count);
				al.CopyTo(array);
				return array;
			}

			
		}

		private static void AddCats(List<TreeNode> list, TreeNode cat)
		{
			list.Add(cat);
			foreach (TreeNode child in cat.Children)
			{
				AddCats(list, child);
			}
		}


		/// <summary>Searches for a TreeNode</summary>
		/// <param Name="query">The search query</param>
		/// <param Name="cats">A heirachical array of Categories. Rather than creating new Categories, it will just return an array of pointers to the categories within this parameter</param>
		public static TreeNode[] Search(String query, TreeNode[] cats)
		{

			query = query.Trim();
			if (query.Length == 0)
			{
				return new TreeNode[0];
			}

			String sql = "SELECT id FROM TreeNode WHERE FREETEXT( *, '" + DBUtils.ParseSql(query) + "' )";

			List<TreeNode> al = new List<TreeNode>();
			SqlDataReader sdr = DBUtils.ExecuteSqlQuery(sql);
			while (sdr.Read())
			{
				TreeNode curCat = FindTreeNode(cats, System.Convert.ToInt32(sdr["id"]));
				if (curCat != null)
				{
					al.Add(curCat);
				}
			}
			sdr.Close();

			return al.ToArray();

		}


	}

}
