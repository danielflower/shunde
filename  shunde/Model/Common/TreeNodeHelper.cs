using System;
using System.Collections.Generic;
using System.Text;
using Shunde.Framework;

namespace Shunde.Common
{

	/// <summary>
	/// A helper class for objects which implement <see cref="ITreeNode{T}"/>
	/// </summary>
	public static class TreeNodeHelper<T>
		where T : DBObject, ITreeNode<T>
	{

		/// <summary>
		/// Checks whether there is a loop from the current node up the tree back to the current node
		/// </summary>
		public static bool HasHeirachyLoop(T node)
		{

			T tempParent = node.Parent;
			while (tempParent != null)
			{
				if (tempParent.Id == node.Id)
				{
					return true;
				}
				tempParent = tempParent.Parent;
			}
			return false;

		}

		/// <summary>Returns the root node of the tree that the given TreeNode belongs in</summary>
		public static T GetRootNode(T node)
		{
			T cur = node;
			while (cur.Parent != null)
			{
				cur = cur.Parent;
			}
			return cur;
		}


		/// <summary>
		/// Gets the name of the ancestors of the given node, and this node, with each node separated by the given separator string
		/// </summary>
		/// <example>
		///		If the node is "Guangzhou", with a parent "China", which has a parent "Asia", and separator is " / ", then
		///		the following call would be used to produce "Asia / China / Guangzhou":
		///		<code>string fullName = GetFullName(guangzhou, " / ");</code>
		/// </example>
		/// <param name="node">The node to get the full name of</param>
		/// <param name="separator">A separator to place between each name</param>
		public static string GetFullName(T node, string separator)
		{
			string fullName = "";

			T tempParent = node.Parent;
			while (tempParent != null)
			{
				fullName = tempParent.Name + " > " + fullName;
				tempParent = tempParent.Parent;
			}

			return fullName + node.Name;
		}


		/// <summary>Gets all the tree nodes in the database as a forest of trees</summary>
		/// <remarks>Even gets deleted tree nodes</remarks>
		/// <param name="extendingTypes">An array of all the types which extend the base type which may be returned, or null if there are no extended types</param>
		public static IList<T> GetTreeNodesAsForest(Type[] extendingTypes)
		{
			Type treeNodeType = typeof(T);
			ObjectInfo oi = ObjectInfo.GetObjectInfo(treeNodeType);
			return GetTreeNodesAsForest(oi.GetSelectStatement(), extendingTypes);
		}


		/// <summary>Gets all the tree nodes in the database as a forest of trees with match the optionally specified SQL WHERE clause</summary>
		/// <param name="where">A string containing an SQL WHERE clause, such as "WHERE isDeleted = 0", or an empty string for no WHERE clause</param>
		/// <param name="extendingTypes">An array of all the types which extend the base type which may be returned, or null if there are no extended types</param>
		public static IList<T> GetTreeNodesAsForest(string where, Type[] extendingTypes)
		{
			Type treeNodeType = typeof(T);
			T[] nodes = (T[])DBObject.GetObjects(treeNodeType, extendingTypes ?? Type.EmptyTypes, where);
			return ConvertToForest(nodes);
		}


		/// <summary>Converts an array of ITreeNode objects into a forest of trees</summary>
		private static IList<T> ConvertToForest(T[] allCats)
		{
			Type treeNodeType = typeof(T);

			// first, put every TreeNode into a dictionary
			Dictionary<int, int> dictionary = new Dictionary<int, int>();

			int index = 0;
			foreach(T node in allCats)
			{
				node.Children = new List<T>();
				dictionary.Add(node.Id, index);
				index++;
			}


			// now, go through each TreeNode. If Parent is null, then it is a Parent, so add it to the 'parents' ArrayList, which is what will be returned from this method. If Parent is not null, then find the index of the Parent in the allCats array, and set that to be it's Parent
			List<T> parents = new List<T>();

			foreach (T cat in allCats)
			{

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
					cat.Parent = (T)allCats[parentIndex];

					cat.Parent.Children.Add(cat);

				}
			}


			return parents;
		}

		/// <summary>Finds the TreeNode from the Children, or descendants of this TreeNode</summary>
		public static T FindDescendant(T searchRoot, int dbObjectId)
		{
			if (searchRoot.Id == dbObjectId)
			{
				return searchRoot;
			}
			foreach(T child in searchRoot.Children)
			{
				T search = FindDescendant(child, dbObjectId);
				if (search != null)
				{
					return search;
				}
			}
			return null;
		}

		/// <summary>Finds the TreeNode with the TreeNode id catId from the tree of categories</summary>
		public static T FindTreeNode(IEnumerable<T> cats, int catId)
		{

			foreach (T cat in cats)
			{
				if (cat.Id == catId)
				{
					return cat;
				}
				T descendant = FindDescendant(cat, catId);
				if (descendant != null)
				{
					return descendant;
				}
			}

			return null;
		}



		/// <summary>Converts a heirachacle Array of Tree Nodes into a flat array of nodes</summary>
		/// <remarks>The relationships between Parent/Children are retained</remarks>
		public static List<T> ConvertToFlatArray(IEnumerable<T> cats)
		{
			List<T> al = new List<T>();
			foreach (T cat in cats)
			{
				AddCats(al, cat);
			}

			if (al.Count > 0)
			{
				if (al[0] is IComparable)
				{
					al.Sort();
				}
				else
				{
					al.Sort(FullNameComparer);
				}
			}

			return al;

		}

		/// <summary>
		/// Gets a Comparer to alphanumerically compare the <see cref="GetFullName" />-generated name of two nodes
		/// </summary>
		public static readonly Comparison<T> FullNameComparer = new Comparison<T>(
			delegate(T node1, T node2)
			{
				string node1FullName = GetFullName(node1, ":");
				string node2FullName = GetFullName(node2, ":");
				return node1FullName.CompareTo(node2FullName);
			}
		);

		private static void AddCats(List<T> list, T cat)
		{
			list.Add(cat);
			foreach (T child in cat.Children)
			{
				AddCats(list, child);
			}
		}


		#region Useful tree properties

		/// <summary>
		/// Gets the depth of a node, e.g. a root node has depth 0, its children have depth 1, etc.
		/// </summary>
		public static int GetDepth(T node)
		{
			if (node == null)
			{
				throw new ArgumentException("The specified node was null", "node");
			}
			int depth = 0;
			while (node.Parent != null)
			{
				++depth;
				node = node.Parent;
			}
			return depth;
		}


		/// <summary>
		/// Gets the siblings of the given node. Note that the given node is included in the
		/// returned list.  Throws an <see cref="Exception" /> if this is a root node.
		/// </summary>
		public static IList<T> GetSiblings(T node)
		{
			if (GetNodeType(node) == NodeType.Root)
			{
				throw new Exception("GetSiblings called on a root node.");
			}

			return node.Parent.Children;

		}

		/// <summary>
		/// Gets the type of node that the specified node is.
		/// </summary>
		public static NodeType GetNodeType(T node)
		{
			if (node.Parent == null)
			{
				return NodeType.Root;
			}
			else if (node.Children.Count == 0)
			{
				return NodeType.Leaf;
			}
			return NodeType.Internal;
		}

		#endregion


	}

	/// <summary>
	/// A type of tree node.
	/// </summary>
	public enum NodeType
	{
		/// <summary>
		/// A node which is at the root of the tree, i.e. it has no parents.
		/// </summary>
		Root,

		/// <summary>
		/// A node which has parent and children.
		/// </summary>
		Internal,

		/// <summary>
		/// A node with no children.
		/// </summary>
		Leaf
	}

}
