using System;
using System.Collections.Generic;
using System.Text;
using Shunde.Framework;

namespace Shunde.Framework.Columns
{

	/// <summary>
	/// A column which is (optionally) validated against a minimum and maximum value
	/// </summary>
	public interface IRangeValidatedColumn
	{

		/// <summary>
		/// The minimum value allowed for the column, which may be null
		/// </summary>
		IComparable MinimumAllowed
		{
			get;
			set;
		}

		/// <summary>
		/// The maximum value allowed for the column, which may be null
		/// </summary>
		IComparable MaximumAllowed
		{
			get;
			set;
		}

	}
}
