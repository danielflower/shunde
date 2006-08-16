using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;

namespace Shunde.Utilities
{


	/// <summary>A Panel with a display order so that you can order the panels after creating them</summary>
	public class ComparableRow : TableRow, IComparable
	{

		private int displayOrder;

		/// <summary>
		/// The relative number to order this panel to another with
		/// </summary>
		public int DisplayOrder
		{
			get { return displayOrder; }
			set { displayOrder = value; }
		}


		/// <summary></summary>
		public int CompareTo(Object another)
		{
			if (another is ComparableRow)
			{
				return displayOrder.CompareTo(((ComparableRow)another).displayOrder);
			}
			else
			{
				return 0;
			}
		}

	}

}
