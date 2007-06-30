using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Shunde.Common;
using Shunde.Utilities;
using System.Web.UI.HtmlControls;
using Shunde.Framework;

[assembly: TagPrefix("Shunde.Web", "Shunde")]

namespace Shunde.Web
{

	/// <summary>
	/// Displays a forest of trees, using HTML unordered lists
	/// </summary>
	[ToolboxData("<{0}:TreeDisplayer runat=server />")]
	public class TreeDisplayer<T> : WebControl
		where T : DBObject, ITreeNode<T>
	{

		private IEnumerable<T> forest = null;

		/// <summary>
		/// The forest of treenodes to display
		/// </summary>
		public IEnumerable<T> Forest
		{
			get { return forest; }
			set { forest = value; }
		}


		private NodeToHtmlDelegate nodeToHtml = null;

		/// <summary>
		/// A delegate that converts a node to HTML. Each node that gets displayed will use this delegate to create 
		/// the text to show. If no delegate is specified, then the <see cref="Shunde.Common.ITreeNode{T}.Name" /> field 
		/// will be used.
		/// </summary>
		public NodeToHtmlDelegate NodeToHtml
		{
			get { return nodeToHtml; }
			set { nodeToHtml = value; }
		}
	


		/// <summary>
		/// Renders the control
		/// </summary>
		protected override void RenderContents(HtmlTextWriter output)
		{
			if (forest == null)
			{
				throw new Exception("The TreeDisplayer has not been given any treenodes");
			}

			HtmlGenericControl ul = new HtmlGenericControl("ul");
			foreach (T node in forest)
			{
				AddNode(ul, node);
			}
			ul.RenderControl(output);

		}


		private void AddNode(HtmlGenericControl ul, T node)
		{
			HtmlGenericControl li = new HtmlGenericControl("li");

			string nodeHtml = (NodeToHtml == null) ? node.Name : NodeToHtml(node);

			li.Controls.Add(new LiteralControl(nodeHtml));

			if (node.Children.Count > 0)
			{
				HtmlGenericControl subUl = new HtmlGenericControl("ul");
				foreach (T child in node.Children)
				{
					AddNode(subUl, child);
				}
				li.Controls.Add(subUl);
			}

			ul.Controls.Add(li);


		}


		/// <summary>
		/// A delegate that gets an HTML representation for the treenode
		/// </summary>
		public delegate string NodeToHtmlDelegate(T node);


	}


}
