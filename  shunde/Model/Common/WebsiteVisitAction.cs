using System;
using System.Collections.Generic;
using System.Text;
using Shunde.Common;
using Shunde.Framework;
using Shunde.Utilities;

namespace Shunde.Common
{
	/// <summary>An action during a visit</summary>
	public class WebsiteVisitAction : DBObject
	{

		private WebsiteVisit visit;

		/// <summary>The visit that this view occured in</summary>
		public WebsiteVisit Visit
		{
			get { return visit; }
			set { visit = value; }
		}

		private DateTime timeStamp;

		/// <summary>The time of this view</summary>
		public DateTime TimeStamp
		{
			get { return timeStamp; }
			set { timeStamp = value; }
		}

		private DBObject viewedObject;

		/// <summary>The object that was viewed</summary>
		public DBObject ViewedObject
		{
			get { return viewedObject; }
			set { viewedObject = value; }
		}

		private string url;

		/// <summary>The URL, if possible, that the view took place at</summary>
		public string Url
		{
			get { return url; }
			set { url = value; }
		}

		private string description;

		/// <summary>A description of the action</summary>
		public string Description
		{
			get { return description; }
			set { description = value; }
		}

		/// <summary>Sets up the <see cref="Shunde.Framework.ObjectInfo" /> for this class</summary>
		static WebsiteVisitAction()
		{

			DBTable tbl = new DBTable("WebsiteVisitAction", new DBColumn[] {
				new DBColumn( "visit", typeof(WebsiteVisit), false ),
				new DBColumn( "timeStamp", typeof(DateTime), false ),
				new DBColumn( "viewedObject", typeof(DBObject), true ),
				new DBColumn( "url", typeof(string), 0, 1000 ),
				new DBColumn( "description", typeof(string), 0, 200 )
			});

			ObjectInfo.RegisterObjectInfo(typeof(WebsiteVisitAction), tbl);

		}


		/// <summary>Gets an array of WebsiteVisitAction objects for the given WebsiteVisit</summary>
		public static WebsiteVisitAction[] GetUserVisits(WebsiteVisit visit)
		{

			Type t = typeof(WebsiteVisitAction);

			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);

			String sql = oi.GetSelectStatement() + " WHERE DBObject.isDeleted = 0 AND visitId = " + visit.Id;

			return (WebsiteVisitAction[])DBObject.GetObjects(sql, t);

		}

	}
}
