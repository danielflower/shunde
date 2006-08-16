using System;
using System.IO;
using System.Net;
using System.Web.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Shunde.Utilities
{

	/// <summary>A utility class for web-related tasks, eg making web requests or sending emails</summary>
	public class WebUtils
	{

		/// <summary>The number of email addresses put into the Blind Carbon Copy field that can be sent at once, for bulk email sending</summary> 
		public const int EmailsPerNewsletter = 100;



		/// <summary>
		/// Renders a control to a string. Useful for things such as using a control to create HTML for an email
		/// </summary>
		/// <param Name="control">The control, with any properties already set</param>
		/// <returns>The HTML as a string of the control</returns>
		public static string RenderControlToString(Control control)
		{
			StringWriter sw = new StringWriter();
			HtmlTextWriter htw = new HtmlTextWriter(sw);
			control.RenderControl(htw);
			htw.Close();
			string output = sw.ToString();
			sw.Close();
			return output;
		}

		/// <summary>
		/// Renders a control to a string. Useful for things such as using a control to create HTML for an email
		/// </summary>
		/// <param Name="page">The page it's displayed on</param>
		/// <param Name="control">The control, with any properties already set</param>
		/// <returns>The HTML as a string of the control</returns>
		public static string RenderWebUserControlToString(Page page, Control control)
		{
			PlaceHolder ph = new PlaceHolder();
			ph.Controls.Add(control);
			page.Controls.Add(ph);
			ph.Visible = false;
			return RenderControlToString(control);
		}


		/// <summary>Gets the host Name from an IP Address</summary>
		public static string GetHostName(String ipAddress)
		{
			string hostName;
			try
			{
				hostName = Dns.GetHostEntry(ipAddress).HostName;
			}
			catch
			{
				hostName = "";
			}
			return hostName;
		}

		/// <summary>Makes a request to a Uri, and returns the output</summary>
		/// <param Name="target">The target destinations</param>
		/// <param Name="postValue">A string of Name/Value pairs, in the form of a querystring</param>
		/// <returns>The output of the requested Uri</returns>
		public static string MakeWebRequest(Uri target, string postValue)
		{
			return MakeWebRequest(target, postValue, null);
		}


		/// <summary>Makes a request to a Uri, and returns the output</summary>
		/// <param Name="target">The target destinations</param>
		/// <param Name="postValue">A string of Name/Value pairs, in the form of a querystring</param>
		/// <param Name="userName">The user Name to authenticate for this request</param>
		/// <param Name="password">The password for the login</param>
		/// <param Name="domain">The domain of the login</param>
		/// <returns>The output of the requested Uri</returns>
		public static string MakeWebRequest(Uri target, string postValue, string userName, string password, string domain)
		{

			NetworkCredential nc = new NetworkCredential(userName, password, domain);
			return MakeWebRequest(target, postValue, nc);

		}

		/// <summary>Makes a request to a Uri, and returns the output</summary>
		/// <param Name="target">The target destination</param>
		/// <param Name="postValue">A string of Name/Value pairs, in the form of a querystring</param>
		/// <param Name="networkCredentials">The network credentials for this request. Null for anonymous access</param>
		/// <returns>The output of the requested Uri</returns>
		public static string MakeWebRequest(Uri target, string postValue, NetworkCredential networkCredentials)
		{

			string result = "";
			StreamWriter myWriter = null;

			HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(target);
			webRequest.Method = "POST";
			webRequest.ContentLength = postValue.Length;
			webRequest.ContentType = "application/x-www-form-urlencoded";
			webRequest.AllowAutoRedirect = true;
			webRequest.Credentials = networkCredentials;

			myWriter = new StreamWriter(webRequest.GetRequestStream());
			myWriter.Write(postValue);
			myWriter.Close();


			HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
			using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
			{
				result = sr.ReadToEnd();
				// Close and clean up the StreamReader
				sr.Close();
			}
			return result;

		}


		/// <summary>Makes a request to a Uri, and returns the Response</summary>
		/// <param Name="target">The target destination</param>
		/// <param Name="postValue">A string of Name/Value pairs, in the form of a querystring</param>
		/// <param Name="networkCredentials">The network credentials for this request. Null for anonymous access</param>
		/// <returns>The output of the requested Uri</returns>
		public static HttpWebResponse GetResponse(Uri target, string postValue, NetworkCredential networkCredentials)
		{

			StreamWriter myWriter = null;

			HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(target);
			webRequest.Method = "POST";
			webRequest.ContentLength = postValue.Length;
			webRequest.ContentType = "application/x-www-form-urlencoded";
			webRequest.AllowAutoRedirect = true;
			webRequest.Credentials = networkCredentials;

			myWriter = new StreamWriter(webRequest.GetRequestStream());
			myWriter.Write(postValue);
			myWriter.Close();

			return (HttpWebResponse)webRequest.GetResponse();

		}




		/// <summary>Sends a bulk email</summary>
		public static void sendBulkEmail(string from, String[] to, string subject, string body, string styles)
		{
			/*
			MailMessage mail = new MailMessage();
			mail.From = from;
			mail.To = from;
			mail.Subject = subject;
			mail.Body = body;
			mail.BodyFormat = MailFormat.Html;



			int numEmailsToSend = to.Length / EMAILS_PER_NEWSLETTER;
			if (to.Length % EMAILS_PER_NEWSLETTER > 0)
			{
				numEmailsToSend++;
			}

			for (int i = 0; i < numEmailsToSend; i++)
			{
				String bccList = "";
				int startIndex = i * EMAILS_PER_NEWSLETTER;
				int endIndex = Math.Min(i * EMAILS_PER_NEWSLETTER + EMAILS_PER_NEWSLETTER, to.Length);
				for (int j = startIndex; j < endIndex; j++)
				{
					bccList += to[j] + ";";
				}
				mail.Bcc = bccList;
				SmtpMail.Send(mail);
			}*/
		}



	}

}
