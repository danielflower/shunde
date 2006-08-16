using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Shunde.Framework;
using System.Web;

namespace Shunde.Utilities
{
	/// <summary>A Utility class for text</summary>
	public class TextUtils
	{

		/// <summary>Converts a number of bytes into a friendly Name, such as 603 bytes, 14 KB, 1,048 MB etc</summary>
		public static string GetFriendlyFileSize(long sizeInBytes)
		{
			if (sizeInBytes < 1000)
			{
				return sizeInBytes + " bytes";
			}
			if (sizeInBytes < 1000000)
			{
				return (sizeInBytes / 1000) + " KB";
			}

			long temp = sizeInBytes / 10000;
			double megs = temp / 100.0;

			return megs.ToString("n") + " MB";

		}

		/// <summary>
		/// Converts a string's line breaks into Html line breaks
		/// </summary>
		public static string ToHtml(string text)
		{
			return text.Replace("\n", "<br/>\n");
		}

		/// <summary>Gets the textual representation of a date in a "friendly" format, using the specified month and year formats. If the time is not midnight, then the time is included</summary>
		public static string GetFriendlyDate(DateTime date, String monthFormat, String yearFormat)
		{
			string format = "d " + monthFormat + " " + yearFormat;
			if (!IsMidnight(date))
			{
				format = "h:mmt\\m, " + format;
			}
			return date.ToString(format);
		}

		/// <summary>Gets, as text, the start and end date and time for a start date and end date using the format strings for the month and year</summary>
		public static String GetFriendlyDateRange(DateTime start, DateTime end, String monthFormat, String yearFormat)
		{

			if (start == DBColumn.DateTimeNullValue)
			{
				return "";
			}

			if (AreSameDay(start, end) || end == DateTime.MinValue)
			{

				if (AreSameTime(start, end) || end == DateTime.MinValue)
				{
					if (IsMidnight(start))
					{
						return start.ToString("d " + monthFormat + " " + yearFormat);
					}
					else
					{
						return start.ToString("h:mmt\\m, d " + monthFormat + " " + yearFormat);
					}
				}
				else
				{
					return start.ToString("h:mmt\\m") + " - " + end.ToString("h:mmt\\m") + ", " + start.ToString("d " + monthFormat + " " + yearFormat);
				}

			}


			string t;

			if (IsMidnight(start))
			{
				t = start.Day.ToString();
				// if the start date is midnight, then we may be able to make the date more compact
				// for example, 10 Dec 03 - 14 Dec 03 could be 10 - 14 Dec 03
				if (start.Year == end.Year)
				{
					if (start.Month == end.Month)
					{
						// month and year are the same, so don't print anything
					}
					else
					{
						// just print the month as the years of the two dates are the same
						t += start.ToString(" " + monthFormat);
					}
				}
				else
				{
					// the years are different
					t += start.ToString(" " + monthFormat + " " + yearFormat);
				}
			}
			else
			{
				t = start.ToString("h:mmt\\m, d " + monthFormat + " " + yearFormat);
			}

			t += " - ";

			if (!IsMidnight(end))
			{
				t += end.ToString("h:mmt\\m") + ", ";
			}

			t += end.ToString("d " + monthFormat + " " + yearFormat);



			return t;

		}



		/// <summary>Returns true if the time is midnight</summary>
		/// <remarks>Ignores the seconds and milliseconds</remarks>
		public static bool IsMidnight(DateTime d)
		{
			return (d.Hour == 0 && d.Minute == 0);
		}

		/// <summary>Returns true if 2 date times are on the same day</summary>
		public static bool AreSameDay(DateTime d1, DateTime d2)
		{
			return (d1.Year == d2.Year && d1.Month == d2.Month && d1.Day == d2.Day);
		}

		/// <summary>Returns true if 2 date times have the same Hour and Minute values</summary>
		public static bool AreSameTime(DateTime d1, DateTime d2)
		{
			return (d1.Hour == d2.Hour && d1.Minute == d2.Minute);
		}




		/// <summary>Converts a string so that it can be used as a javascript string</summary>
		/// <remarks>Uses escape codes for new lines, apostrophes, speech marks</remarks>
		public static String JavascriptStringEncode(string s)
		{
			return s.Replace("'", "\\'").Replace("\n", "\\n").Replace("\"", "' + String.fromCharCode(34) + '").Replace("\r", "");
		}




		/// <summary>Checks to see if a string is a valid email address. A valid email address is at least 5 characters, and has an '@' and a '.' in it</summary>
		/// <param Name="email">String containing email address to check</param>
		/// <returns>Returns boolean indicating whether this is a valid email address or not</returns>
		public static bool IsValidEmailAddress(string email)
		{
			email += "";
			if (email.Length < 5 || email.IndexOf("@") == -1 || email.IndexOf(".") == -1)
				return false;
			else
				return true;
		}



		/// <summary>Strips a string of all but the alpha-numeric characters</summary>
		/// <param Name="input">The string you wish to parse</param>
		/// <returns>Returns just the alpha-numeric characters of the input string</returns>
		public static string RemoveNonAlphaNumeric(string input)
		{
			char[] chars = input.ToCharArray();
			string output = "";
			for (int i = 0; i < chars.Length; i++)
			{
				char c = chars[i];
				if (Char.IsLetterOrDigit(c))
				{
					output += c.ToString();
				}
			}
			return output;
		}

		/// <summary>Strips a string of all but the numeric characters</summary>
		/// <param Name="input">The string you wish to parse</param>
		/// <returns>Returns just the numeric characters of the input string</returns>
		public static string RemoveNonNumeric(string input)
		{
			char[] chars = input.ToCharArray();
			string output = "";
			for (int i = 0; i < chars.Length; i++)
			{
				char c = chars[i];
				if (Char.IsDigit(c))
				{
					output += c.ToString();
				}
			}
			return output;
		}


		/// <summary>
		/// Removes illegal characters, such as ':' and '\' from filenames
		/// </summary>
		/// <param Name="filename">The original filename, excluding the path (can include file extension)</param>
		/// <returns>The filename where any illegal characters are replaced with underscores</returns>
		public static string RemoveIllegalCharactersFromFilename(string filename)
		{
			char[] invalidChars = new char[Path.GetInvalidFileNameChars().Length + 4];
			invalidChars[0] = Path.PathSeparator;
			invalidChars[1] = Path.VolumeSeparatorChar;
			invalidChars[2] = Path.AltDirectorySeparatorChar;
			invalidChars[3] = Path.DirectorySeparatorChar;
			Array.Copy(Path.GetInvalidFileNameChars(), 0, invalidChars, 4, Path.GetInvalidFileNameChars().Length);

			foreach (char c in invalidChars)
			{
				filename = filename.Replace(c, '_');
			}

			return filename;

		}


		/// <summary>Appends a Name and Value pair to a URL</summary>
		/// <remarks>Automatically adds the '?' or '&amp;' as necessary, and URLEncodes the Value</remarks>
		public static String AppendNameValueToUrl(string url, string name, string value)
		{
			if (url.IndexOf("?") > 0)
			{
				url += "&";
			}
			else
			{
				url += "?";
			}

			return url + name + "=" + HttpUtility.UrlEncode(value);
		}



		/// <summary>Cuts a string to end at the end of a word. It appends "..." to the end if the text is cut (that is, if <i>length</i> is greater than the length of the string.</summary>
		/// <param Name="str">The string to be cut</param>
		/// <param Name="length">The number of characters that the string should be cut at</param>
		/// <returns>A string cut at the specified point</returns>
		public static string CutText(string str, int length)
		{
			if (str == null || length < 0)
			{
				return "";
			}
			if (length < str.Length)
			{
				int cutPoint = str.IndexOf(" ", length);
				if (cutPoint > -1)
				{
					str = str.Substring(0, cutPoint) + "...";
				}
			}
			return str;

		}


		/// <summary>Gets a number as an ordered number, if you know what I mean. For example, converts 1 to "1st" or 2 to "2nd"</summary>
		public static String GetOrdinalSuffix(int number)
		{
			if ((number % 100) > 3 && (number % 100) < 21)
			{
				return "th";
			}
			switch (number % 10)
			{
				case 1:
					return "st";
				case 2:
					return "nd";
				case 3:
					return "rd";
				default:
					return "th";
			}
		}


		/// <summary>Parses a string so that it is suitable for a CONTAINSTABLE query</summary>
		/// <param Name="searchQuery">The query to parse</param>
		/// <param Name="noiseWords">The noisewords for this database - all lowercase</param>
		/// <param Name="searchType">The type of search to do; either <i>AND</i> or <i>OR</i></param>
		/// <returns>The string ready to be queried</returns>
		public static string ParseStringForFullTextSearch(string searchQuery, string[] noiseWords, string searchType)
		{
			string query = (" " + searchQuery + " ").Replace("'", "''").ToLower();
			query = query.Replace("\"", "");
			for (int i = 0; i < noiseWords.Length; i++)
			{
				while (query.IndexOf(" " + noiseWords[i] + " ") != -1)
					query = query.Replace(" " + noiseWords[i] + " ", " ");
			}
			while (query.IndexOf("  ") != -1)
			{
				query = query.Replace("  ", " ");
			}
			query = query.Trim();
			query = "\"" + query.Replace(" ", "\" " + searchType + " \"") + "\"";
			return query;
		}

		/// <summary>
		/// Creates a dictionary containing all the words in the noise file
		/// </summary>
		/// <param Name="filePath">The full path of the noise file</param>
		/// <returns>Returns a hash Table</returns>
		public static Dictionary<string,bool> GetNoiseWords(string filePath)
		{

			StreamReader noiseFile = File.OpenText(filePath);
			string noiseWords = noiseFile.ReadToEnd();
			noiseFile.Close();
			noiseWords = noiseWords.Replace("\r\n", " ");
			while (noiseWords.IndexOf("  ") != -1)
			{
				noiseWords = noiseWords.Replace("  ", " ");
			}
			noiseWords = noiseWords.Trim();
			noiseWords = noiseWords.Replace(" ", "|").ToLower();

			string[] noiseWordsArray = noiseWords.Split(new Char[] { '|' });

			Dictionary<string, bool> dict = new Dictionary<string, bool>();
			foreach (string word in noiseWordsArray)
			{
				dict.Add(word, true);
			}

			return dict;

		}

		/// <summary>Parses a string so that it is suitable for a CONTAINSTABLE query</summary>
		/// <param Name="searchQuery">The query to parse</param>
		/// <param Name="noiseWords">The noisewords for this database - all lowercase</param>
		/// <param Name="searchType">The type of search to do; either <i>AND</i> or <i>OR</i></param>
		/// <returns>The string ready to be queried</returns>
		public static string ParseStringForFullTextSearch(string searchQuery, Dictionary<string,bool> noiseWords, string searchType)
		{
			string query = (" " + searchQuery + " ").Replace("'", "''").ToLower();
			query = query.Replace("\"", "");
			string[] words = query.Split(new char[] { ' ' });
			foreach (string word in words)
			{
				if (noiseWords.ContainsKey(word))
				{
					query = query.Replace(" " + word + " ", " ");
				}
			}
			while (query.IndexOf("  ") != -1)
			{
				query = query.Replace("  ", " ");
			}
			query = query.Trim();
			query = "\"" + query.Replace(" ", "\" " + searchType + " \"") + "\"";
			return query;
		}

		/// <summary>Gets a diagnostic report of an exception</summary>
		public static string GetExceptionReportAsHtml(Exception ex, HttpRequest request, string extraInformation)
		{

			// don't report ThreadAbortException exceptions as these are thrown by Response.Redirect
			if (ex is System.Threading.ThreadAbortException)
			{
				throw new ShundeException("Thread abort exception - not important");
			}

			string url = "";
			string referrer = "";
			string userAgent = "";
			if (request != null)
			{
				url = request.Url.AbsoluteUri;
				referrer = (request.UrlReferrer != null) ? request.UrlReferrer.AbsoluteUri : "null";
				userAgent = request.UserAgent;
			}


			string summary = "<b>" + ex.GetType().FullName + "</b> details follow:<br><br>" +
				"<b>URL:</b> <a href=\"" + url + "\">" + url + "</a><br><br>" +
				"<b>Referrer:</b> <a href=\"" + referrer + "\">" + referrer + "</a><br><br>" +
				"<i>" + extraInformation + "</i><br><br>" + ex.Message + "<br><br>";

			// if it is an SqlException, send details, including SQL statement
			if (ex is Shunde.ShundeSqlException)
			{
				summary += ((ShundeSqlException)ex).ToHtml();
			}

			summary += "<b>Stack Trace :</b><br><br>" + ex.StackTrace.Replace("\n", "<br>") + "<br><br><br><B>User Agent:</B><br>" + userAgent;

			return summary;

		}


		/// <summary>Converts a string in camel format suchAsThis into a string Such As This</summary>
		public static string MakeFriendly(string s)
		{
			char[] chars = s.ToCharArray();
			string output = chars[0].ToString().ToUpper();
			for (int i = 1; i < chars.Length; i++)
			{
				char c = chars[i];
				if (c >= 'A' && c <= 'Z')
				{
					output += " " + c.ToString();
				}
				else
				{
					output += c.ToString();
				}
			}
			return output;

		}

		private TextUtils() { }

	}
}
