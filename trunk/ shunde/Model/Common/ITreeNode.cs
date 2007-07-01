using System;
using System.Collections.Generic;
using System.Text;
using Shunde.Framework;

namespace Shunde.Common
{


	/// <summary>
	/// An object which implements this interface is considered a node in a tree
	/// </summary>
	public interface ITreeNode<T>
		where T : DBObject
	{

		/// <summary>
		/// The parent of this node, or null if it is the root of the tree
		/// </summary>
		T Parent
		{
			get;
			set;
		}

		/// <summary>
		/// The children of this node, or an empty list if this is a leaf
		/// </summary>
		IList<T> Children
		{
			get;
			set;
		}

		/// <summary>
		/// The name of this node
		/// </summary>
		string Name
		{
			get;
			set;
		}

	}

}
