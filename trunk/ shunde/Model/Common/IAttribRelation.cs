using System;
using System.Collections.Generic;
using System.Text;
using Shunde.Framework;

namespace Shunde.Common
{
	/// <summary>
	/// A relation between some object and an attribute
	/// </summary>
	public interface IAttribRelation
	{

		/// <summary>
		/// The attribute in the relation
		/// </summary>
		Attrib Attrib
		{
			get;
			set;
		}

		/// <summary>
		/// The object that is related to the attrib
		/// </summary>
		DBObject DBObject
		{
			get;
			set;
		}

		/// <summary>
		/// Specifies whether this relation is deleted or not
		/// </summary>
		bool IsDeleted
		{
			get;
			set;
		}

		/// <summary>
		/// Saves any changes to this relation;
		/// </summary>
		void Save();


	}
}
