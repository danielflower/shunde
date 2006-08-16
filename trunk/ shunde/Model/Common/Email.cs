using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using Shunde.Utilities;

namespace Shunde.Common
{
	/// <summary>An email message that can be sent to people</summary>
	public class Email : MailMessage
	{

		/// <summary>CSS Style used fixed-width characters</summary>
		public const string FixedSizeFont = "body { font-family: Courier New; font-size: 14px; }";

		/// <summary>Arial CSS</summary>
		public const string NiceFont = "body { font-family: Arial; font-size: 14px; }";

		/// <summary>Constructor</summary>
		public Email()
		{
			BodyEncoding = System.Text.Encoding.UTF8;
		}

		/// <summary>Sets the message body as HTML</summary>
		/// <param Name="message">The html message, which this method will wrap in &lt;body&gt; tags etc</param>
		/// <param Name="cssStyles">CSS Styles for use in the message. If null, then default styles are used.</param>
		public void SetHtmlMessage(string message, string cssStyles)
		{

			this.IsBodyHtml = true;

			Body = @"<html>
<head>
	<style type=""text/css"">
	<!--
" + cssStyles + @"
	// -->
	</style>
</head>
<body>" + message + "</body></html>";
		}

		/// <summary>Sends this email message</summary>
		/// <remarks>Uses SMTP server localhost on port 25</remarks>
		public void Send()
		{
			Send("localhost");
		}
		
		/// <summary>Sends this email message</summary>
		/// <param Name="server">The IP address or Host Name of the SMTP Server</param>
		/// <remarks>Uses Port 25</remarks>
		public void Send(string server)
		{
			Send(server, 25);
		}

		/// <summary>Sends this email message</summary>
		/// <param Name="server">The IP address or Host Name of the SMTP Server</param>
		/// <param Name="port">The port number of the SMTP server</param>
		public void Send(string server, int port)
		{
			Validate();

			SmtpClient client = new SmtpClient(server, port);
			client.Send(this);
			
		}

		/// <summary>
		/// Sends this email to bulk recipients
		/// </summary>
		/// <param Name="recipients">A list of email addresses</param>
		/// <remarks>Places the email addresses in the BCC fields, breaking it up into multiple sends if necessary</remarks>
		public void BulkSend(string smtpServer, int port, List<string> recipients)
		{

			Validate();

			const int EmailsAddressesPerSend = 100;


			int numEmailsToSend = recipients.Count / EmailsAddressesPerSend;
			if (recipients.Count % EmailsAddressesPerSend > 0)
			{
				numEmailsToSend++;
			}

			for (int i = 0; i < numEmailsToSend; i++) {
				this.Bcc.Clear();
				int startIndex = i * EmailsAddressesPerSend;
				int endIndex = Math.Min(i * EmailsAddressesPerSend + EmailsAddressesPerSend, recipients.Count);
				for (int j = startIndex; j < endIndex; j++) {
					this.Bcc.Add( new MailAddress( recipients[j] ) );
				}
				Send(smtpServer, port);
			}
		
		}

		private void Validate()
		{
			if (this.From == null) {
				throw new ValidationException( "You must specify a From address" );
			}
			if (!TextUtils.IsValidEmailAddress(this.From.Address))
			{
				throw new ValidationException( "The From address (" + this.From.Address + ") is not a valid email address." );
			}
			if (this.To.Count == 0) {
				throw new ValidationException( "You must specify at least one recipient in the To field" );
			}
			if (!TextUtils.IsValidEmailAddress(this.To[0].Address))
			{
				throw new ValidationException("The To address (" + this.To[0].Address + ") is not a valid email address.");
			}
		}

	}

}
