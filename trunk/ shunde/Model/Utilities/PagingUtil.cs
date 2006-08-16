using System;
using System.Collections.Generic;
using System.Text;

namespace Shunde.Utilities
{
	/// <summary>A class that helps create paging for an array of objects</summary>
	public class PagingUtil
	{

		/// <summary>Holds Value of property numberPerPage</summary>
		public int NumberPerPage
		{
			get { return numberPerPage; }
			set { numberPerPage = value; }
		}

		private int numberPerPage;

		/// <summary>Holds Value of property currentPage</summary>
		public int CurrentPage
		{
			get { return currentPage; }
			set { currentPage = Math.Max( 1, value ); }
		}

		private int currentPage = 1;

		/// <summary>Holds Value of property size</summary>
		public int NumberOfItems
		{
			get { return numberOfItems; }
			set { numberOfItems = value; }
		}

		private int numberOfItems;

		/// <summary>
		/// The total number of pages of items
		/// </summary>
		public int TotalPages
		{
			get
			{
				int totalPages = numberOfItems / numberPerPage;
				if (numberOfItems % numberPerPage != 0)
				{
					totalPages++;
				}
				return totalPages;
			}
		}

		/// <summary>
		/// Specifies whether there is another page or not
		/// </summary>
		public bool HasNextPage
		{
			get	{ return currentPage < TotalPages; }
		}

		/// <summary>
		/// Specifies whether there is a previous page or not
		/// </summary>
		public bool HasPreviousPage
		{
			get { return currentPage > 1; }
		}


		/// <summary>
		/// Creates a new Paging Util object
		/// </summary>
		public PagingUtil() { }

		/// <summary>
		/// Creates a new Paging Util object
		/// </summary>
		/// <param Name="currentPage">The current page that is being viewed</param>
		/// <param Name="numberOfItems">The total number of items to page through</param>
		/// <param Name="numberPerPage">The number per page to display</param>
		public PagingUtil(int currentPage, int numberOfItems, int numberPerPage)
		{
			this.currentPage = Math.Max(1, currentPage);
			this.numberOfItems = numberOfItems;
			this.numberPerPage = Math.Max( 1, numberPerPage );
		}

		/// <summary>Gets the list of pages, with next, page nums, and previous buttons</summary>
		public string GetPageLinksAsHtml(string linkUrl, string separator)
		{
			int totalPages = TotalPages;

			if (totalPages == 1)
			{
				return "";
			}

			StringBuilder pc = new StringBuilder();

			for (int i = 1; i <= totalPages; i++)
			{

				string url = linkUrl + "&page=" + i;

				if (i == 1)
				{
					if (currentPage == 1)
					{
						pc.Append( "Previous &lt;&lt; " );
					}
					else
					{
						pc.Append( "<a href=\"" + linkUrl + "&page=" + (currentPage - 1) + "\">Previous &lt;&lt;</a> " );
					}
				}

				if (i == currentPage)
				{
					pc.Append("<b>" + i + "</b>");
				}
				else
				{
					pc.Append("<a href=\"" + url + "\">" + i + "</a>");
				}

				if (i < totalPages)
				{
					pc.Append( separator );
				}
				else
				{
					if (currentPage == totalPages)
					{
						pc.Append( " &gt;&gt; Next " );
					}
					else
					{
						pc.Append( " <a href=\"" + linkUrl + "&page=" + (currentPage + 1) + "\">&gt;&gt; Next</a> " );
					}
				}
			}

			return pc.ToString();

		}

		/// <summary>Gets text saying Page X of Y</summary>
		public string getPageXOfY()
		{
			return "Page " + currentPage + " of " + TotalPages;
		}


		/// <summary>Gets the start index for this page</summary>
		public int GetStart()
		{
			int start = 0;
			int end = numberOfItems;
			start = (currentPage - 1) * numberPerPage;
			end = start + numberPerPage;
			end = Math.Min(end, numberOfItems);
			return start;
		}

		/// <summary>Gets the end index for this page</summary>
		public int GetEnd()
		{
			int start = 0;
			int end = numberOfItems;
			start = (currentPage - 1) * numberPerPage;
			end = start + numberPerPage;
			end = Math.Min(end, numberOfItems);
			return end;
		}


	}

}
