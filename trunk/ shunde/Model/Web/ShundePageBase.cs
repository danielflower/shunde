using System;
using System.Collections;
using System.Net.Mail;
using System.Text;
using System.Web;
using Shunde.Utilities;
using Shunde.Common;

namespace Shunde.Web
{
	/// <summary>The base of each webpage.</summary>
	/// <remarks>This is inherited by each ASP.NET page. It exposes certain properties and methods needed throughout a website</remarks>
	public abstract class ShundePageBase : System.Web.UI.Page
	{



		/// <summary>Gets the database connection string</summary>
		public abstract string ConnectionString
		{
			get;
		}

		/// <summary>Gets the errors email address for this website</summary>
		public abstract string ErrorsEmail
		{
			get;
		}

		/// <summary>Gets the website Name</summary>
		public abstract string WebsiteName
		{
			get;
		}

		/// <summary>
		/// The IP address or host Name of the SMTP server
		/// </summary>
		public abstract string SmtpServer
		{
			get;
		}

		/// <summary>
		/// The port number of the SMTP server
		/// </summary>
		public abstract int SmtpPortNumber
		{
			get;
		}

		/// <summary>Called when the page loads</summary>
		public virtual void Page_Init(object Sender, EventArgs e)
		{
			DBUtils.SetSqlConnection(ConnectionString);
		}

		/// <summary>Called when the page loads</summary>
		public virtual void Page_Load(object Sender, EventArgs e)
		{
			
		}

		/// <summary>Called when the page un-loads</summary>
		public virtual void Page_Unload(object Sender, EventArgs e)
		{
			DBUtils.CloseSqlConnection();
		}


		/// <summary>Redirets to the given URL (absolute or relative)</summary>
		public void Redirect(string url)
		{
			// TODO: check to see if Redirect calls Page_Unload


			if (url.StartsWith("~"))
			{
				string appPath = Request.ApplicationPath;

				if (appPath.Length > 1)
				{
					url = appPath + url.Substring(1);
				}
			}
			

			Response.Redirect(url);
		}

		/// <summary>Gets a short from the querystring. Returns -1 if the param does not exist or is in an incorrect format</summary>
		/// <param Name="paramName">The Name of the querystring parameter</param>
		/// <returns>Returns retrieved parameter as a short</returns>
		public short GetShortParam(string paramName)
		{
			try
			{
				return System.Convert.ToInt16(Request.Params[paramName]);
			}
			catch
			{
				return (short)-1;
			}
		}

		/// <summary>Gets an int from the querystring. Returns -1 if the param does not exist or is in an incorrect format</summary>
		/// <param Name="paramName">The Name of the querystring parameter</param>
		/// <returns>Returns retrieved parameter as an int</returns>
		public int GetIntParam(string paramName)
		{
			try
			{
				return System.Convert.ToInt32(Request.Params[paramName]);
			}
			catch
			{
				return -1;
			}
		}

		/// <summary>Gets a long from the querystring. Returns -1 if the param does not exist or is in an incorrect format</summary>
		/// <param Name="paramName">The Name of the querystring parameter</param>
		/// <returns>Returns retrieved parameter as a long</returns>
		public long GetLongParam(string paramName)
		{
			try
			{
				return System.Convert.ToInt64(Request.Params[paramName]);
			}
			catch
			{
				return (long)-1;
			}
		}


		/// <summary>Gets a float from the querystring. Returns -1 if the param does not exist or is in an incorrect format</summary>
		/// <param Name="paramName">The Name of the querystring parameter</param>
		/// <returns>Returns retrieved parameter as a float</returns>
		public float GetFloatParam(string paramName)
		{
			try
			{
				return System.Convert.ToSingle(Request.Params[paramName]);
			}
			catch
			{
				return -1.0f;
			}
		}

		/// <summary>Gets a double from the querystring. Returns -1 if the param does not exist or is in an incorrect format</summary>
		/// <param Name="paramName">The Name of the querystring parameter</param>
		/// <returns>Returns retrieved parameter as a double</returns>
		public double GetDoubleParam(string paramName)
		{
			try
			{
				return System.Convert.ToDouble(Request.Params[paramName]);
			}
			catch
			{
				return -1.0;
			}
		}

		/// <summary>Gets a string from the querystring. Returns an empty string if the param does not exist</summary>
		/// <param Name="paramName">The Name of the querystring parameter</param>
		/// <returns>Returns retrieved parameter as a string</returns>
		public string GetStringParam(string paramName)
		{
			string paramValue;
			paramValue = Request.Params[paramName];
			if (paramValue == null)
			{
				paramValue = "";
			}
			return paramValue.Trim();
		}


		/// <summary>Gets called whenever there is a page error</summary>
		public void Page_Error(object Sender, EventArgs e)
		{
			try
			{
				Exception ex = Server.GetLastError();
				HandleException(ex, Request, "Unhandled exception");
			}
			catch (Exception ex2)
			{
				try
				{
					HandleException(ex2, null, "Exception occured while trying to process an exception in the Page_Error event");
				}
				catch
				{
					// do nothing
				}
			}
		}

		/// <summary>Handles any exceptions that occur on the site</summary>
		/// <param Name="ex">The Exception that was thrown</param>
		/// <param Name="request">The <i>HttpRequest</i> object that requested the page</param>
		/// <param Name="extraInformation">Any extra information that may be useful for trouble-shooting</param>
		public string HandleException(Exception ex, HttpRequest request, string extraInformation)
		{

			string summary = "";
			try
			{
				summary = TextUtils.GetExceptionReportAsHtml(ex, request, extraInformation);
			}
			catch (ShundeException)
			{
				// A ShundeException here means that the Exception was not worth thinking about (ThreadAbortException)
				return "";
			}

			Email email = new Email();
			email.To.Add(new MailAddress(ErrorsEmail));
			email.From = new MailAddress(ErrorsEmail);
			email.Subject = "Site Error at " + WebsiteName;
			email.SetHtmlMessage(summary, Email.FixedSizeFont);
			email.Send(SmtpServer, SmtpPortNumber);
			return "The system administrator has been emailed regarding this error.";

		}


		/// <summary>Removes all the cached objects</summary>
		public void RemoveCache()
		{
			RemoveCache("{REMOVEALL}");
		}


		/// <summary>Removes the cache from the site</summary>
		/// <param Name="cacheName">The Name of the cache to be removed</param>
		/// <remarks>Giving cacheToRemove a Value of <i>{REMOVEALL}</i> removes all the cached items</remarks>
		public void RemoveCache(string cacheName)
		{

			if (cacheName == "{REMOVEALL}")
			{

				foreach (DictionaryEntry objItem in Cache)
				{
					string strName = objItem.Key.ToString();
					if (strName.IndexOf("System.") != 0 && strName.IndexOf("ISAPI") != 0 && Cache[strName] != null)
					{
						Cache.Remove(strName);
					}
				}

			}
			else if (Cache[cacheName] != null)
			{
				Cache.Remove(cacheName);
			}

		}




		/// <summary>Gets some text with the given Code</summary>
		/// <remarks>Uses data in the cache if available. If not available, it is retrieved from the database and added to the cache. Using a Code that doesn't exist is how you create new sections.</remarks>
		/// <param name="nameForAdministrators">The name to give for administrators</param>
		/// <param name="code">The Code of the text</param>
		/// <returns>Returns a TextSection object</returns>
		public virtual TextSection GetTextSection(string code, string nameForAdministrators)
		{

			object fromCache = Cache["textSection_" + code];
			if (fromCache == null)
			{
				TextSection t = null;
				try
				{
					t = new TextSection(code);
				}
				catch (Exception ex)
				{
					HandleException(ex, Request, "Error while populating text " + code);
					HttpContext.Current.Trace.Write("getTextSection", "Error on code " + code + ": " + ex.Message);
					t = new TextSection();
					t.Code = code;
				}
				t.NameForAdministrators = nameForAdministrators;
				Cache["textSection_" + code] = t;
				return t;
			}
			else
			{
				return (TextSection)fromCache;
			}
		}



	}
}
