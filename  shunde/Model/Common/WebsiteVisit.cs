using System;
using System.Collections.Generic;
using Shunde.Framework;
using Shunde.Utilities;
using System.Web;

namespace Shunde.Common
{
	/// <summary>Information on a website visit</summary>
	public class WebsiteVisit : DBObject
	{


		private DBObject visitor;

		/// <summary>
		/// The visitor, or null if unknown
		/// </summary>
		public DBObject Visitor
		{
			get { return visitor; }
			set { visitor = value; }
		}



		private DateTime startTime;

		/// <summary>The start time of this visit</summary>
		public DateTime StartTime
		{
			get { return startTime; }
			set { startTime = value; }
		}

		private DateTime endTime = DBColumn.DateTimeNullValue;

		/// <summary>The end time of this Visit</summary>
		public DateTime EndTime
		{
			get { return endTime; }
			set { endTime = value; }
		}

		private string userAgent;

		/// <summary>The user agent string</summary>
		public string UserAgent
		{
			get { return userAgent; }
			set { userAgent = value; }
		}


		private string siteReferrer;

		/// <summary>The place that this visit came from</summary>
		public string SiteReferrer
		{
			get { return siteReferrer; }
			set { siteReferrer = value; }
		}

		private string searchString;

		/// <summary>The string that the person searched for to find the website</summary>
		public string SearchString
		{
			get { return searchString; }
			set { searchString = value; }
		}

		private string ipAddress;

		/// <summary>The IP address of the client</summary>
		public string IPAddress
		{
			get { return ipAddress; }
			set { ipAddress = value; }
		}

		private string userHostName;

		/// <summary>The User-host Name</summary>
		public string UserHostName
		{
			get { return userHostName; }
			set { userHostName = value; }
		}


		private int numberOfPagesViewed;

		/// <summary>
		/// The number of pages which were viewed
		/// </summary>
		public int NumberOfPagesViewed
		{
			get { return numberOfPagesViewed; }
			set { numberOfPagesViewed = value; }
		}




		/// <summary>Sets up the <see cref="Shunde.Framework.ObjectInfo" /> for this class</summary>
		static WebsiteVisit()
		{

			DBTable tbl = new DBTable("WebsiteVisit", new DBColumn[] {
				new DBColumn( "visitor", typeof(DBObject), true ),
				new DBColumn( "startTime", typeof(DateTime), false ),
				new DBColumn( "endTime", typeof(DateTime), true ),
				new DBColumn( "userAgent", typeof(string), 0, 400 ),
				new DBColumn( "siteReferrer", typeof(string), 0, 1000 ),
				new DBColumn( "searchString", typeof(string), 0, 200 ),
				new DBColumn( "ipAddress", typeof(string), 0, 50 ),
				new DBColumn( "userHostName", typeof(string), 0, 200 ),
				new DBColumn( "numberOfPagesViewed", typeof(int), false )
			});

			ObjectInfo.RegisterObjectInfo(typeof(WebsiteVisit), tbl);

		}

		/// <summary>Creates a new instance of a UserVisit</summary>
		public WebsiteVisit() { }

		/// <summary>Creates a new instance of a UserVisit populating the variables with values from an HttpRequest object</summary>
		public WebsiteVisit(HttpRequest request)
		{
			this.startTime = DateTime.Now;

			this.userAgent = request.UserAgent;
			this.siteReferrer = (request.UrlReferrer == null) ? "" : request.UrlReferrer.ToString();
			this.ipAddress = request.UserHostAddress;

			if (this.siteReferrer.Length > 0)
			{
				Uri uri = new Uri(this.siteReferrer);
				if (uri.Query.Length > 0)
				{
					string[] pairs = uri.Query.Substring(1).Split(new char[] { '&' });
					foreach (string nameValue in pairs)
					{
						if (nameValue.StartsWith("q=") || nameValue.StartsWith("p="))
						{
							this.searchString = HttpUtility.UrlDecode(nameValue.Substring(2).Replace("+", " "));
							if (this.searchString.Length > 199)
							{
								this.searchString = this.searchString.Substring(0, 199);
							}
						}
					}
				}
			}

		}


		/// <summary>Attempts to update the host Name of site visits that don't have host names, returning the number of successes</summary>
		public static int UpdateHostNames()
		{
			int successes = 0;
			WebsiteVisit[] visits = GetWebsiteVisitsWithoutHostName();

			for (int i = 0; i < visits.Length; i++)
			{
				String host = WebUtils.GetHostName(visits[i].ipAddress);
				visits[i].userHostName = host;
				visits[i].Save();
				if (host.Length > 0)
				{
					successes++;
				}
			}
			return successes;
		}

		/// <summary>Gets the number of site visits that don't have host names</summary>
		public static int GetNumberWithoutHostNames()
		{
			return DBUtils.GetIntFromSqlSelect("SELECT COUNT(1) AS intValue FROM UserVisit WHERE userHostName = ''");
		}

		/// <summary>Gets 20 random UserVisit objects with no UserHostName</summary>
		public static WebsiteVisit[] GetWebsiteVisitsWithoutHostName()
		{
			return GetWebsiteVisitsWhere(" WHERE userHostName = '' ORDER BY updateId ASC", 20);
		}

		/// <summary>Gets an array of UserVisit objects with the given SQL WHERE clause</summary>
		public static WebsiteVisit[] GetWebsiteVisitsWhere(string whereClause, int numToGet)
		{

			Type t = typeof(WebsiteVisit);

			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);

			string sql = oi.GetSelectStatement(numToGet) + " " + whereClause;

			return (WebsiteVisit[])DBObject.GetObjects(sql, t);

		}

		/// <summary>Sets the end time for those occasions where the session has timed out</summary>
		public void SetEndTime(System.Web.SessionState.HttpSessionState session)
		{
			endTime = DateTime.Now.AddMinutes(-session.Timeout);
		}

		/// <summary>Sets the user host Name</summary>
		public void SetUserHostName()
		{
			userHostName = WebUtils.GetHostName(ipAddress);
		}

	}
}
