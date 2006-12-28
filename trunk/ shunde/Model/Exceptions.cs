using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace Shunde
{

    /// <summary>A miscellaneous exception</summary>
    /// <exception cref="System.Exception">General exception to be thrown</exception>
    public class ShundeException : Exception
    {
        /// <summary>Misc exception</summary>
        public ShundeException() : base() { }
        /// <summary>Misc exception, with details</summary>
        public ShundeException(string message) : base(message) { }
    }

    /// <summary>A miscellaneous exception</summary>
    /// <exception cref="System.Exception">General exception to be thrown</exception>
    public class ObjectDoesNotExistException : ShundeException
    {
        /// <summary>Misc exception</summary>
        public ObjectDoesNotExistException() : base() { }
        /// <summary>Misc exception, with details</summary>
        public ObjectDoesNotExistException(string message) : base(message) { }
    }

    /// <summary>Exceptions that are to do with validation. The error message should be a friendly message to deliver to the client.</summary>
    public class ValidationException : ShundeException
    {

        /// <summary>Misc exception</summary>
        public ValidationException() : base() { }
        /// <summary>Misc exception, with details</summary>
        public ValidationException(string message) : base(message) { }
    }

    /// <summary>A warning only. Different from a ValidationException in that it may be valid, but there is a suspicion that something may not be right anyway</summary>
    public class WarningException : ShundeException
    {

		private string warningId;

		/// <summary>
		/// Used to uniquely identify a warning message.
		/// </summary>
		public string WarningId
		{
			get { return warningId; }
			set { warningId = value; }
		}
	

        /// <summary>Misc exception</summary>
		public WarningException(string warningId) : base() { this.WarningId = warningId; }
        /// <summary>Misc exception, with details</summary>
		public WarningException(string warningId, string message) : base(message) { this.warningId = warningId; }
    }

    /// <summary>A concurrency exception is thrown when the same data is updated at once. The first to complete will go through, however the 2nd to complete will throw a concurrency exception.</summary>
    public class ConcurrencyException : ValidationException
    {
        /// <summary>Misc exception</summary>
        public ConcurrencyException() : base("The data you are updating has been simulataneously updated by another person or application. Saving your data now would mean the data inputted by the other person or application would be lost, therefore your data cannot be saved. You should make a copy of any changes you have made, exit, then re-edit this object.") { }
        /// <summary>Misc exception, with details</summary>
        public ConcurrencyException(string message) : base(message) { }
    }

    /// <summary>An Sql Exception</summary>
    /// <exception cref="System.Exception">Thrown when an SqlException is created. This is an exception who's <i>InnerException</i> property is an SqlException, with the extra property <i>sqlStatement</i> holding the SQL statement that created the exception.</exception>
    public class ShundeSqlException : Exception
    {

        /// <summary>The SQL statement that caused the error</summary>
        public string sqlStatement = "";

        /// <summary>Creates a new exception</summary>
        /// <param Name="sqlStatement">The SQL statement that created the exception</param>
        /// <param Name="ex">The Exception that was caught (generally an <i>SqlException</i> exception)</param>
        public ShundeSqlException(string sqlStatement, Exception ex)
            : base(ex.Message, ex)
        {
            this.sqlStatement = sqlStatement;
        }

        /// <summary>The reason for the problem</summary>
        public override string Message
        {
            get
            {
                return base.Message + "\n\n\n" + sqlStatement + "\n\n\n";
            }
        }

        /// <summary>Prints out details of this exception</summary>
        public string ToHtml()
        {
            string summary = "<b>SQL Statement that caused the error:</b><br><br>" + System.Web.HttpUtility.HtmlEncode(sqlStatement).Replace("\n", "<br>") + "<br><br>";

            if (InnerException is SqlException)
            {
                SqlException se = (SqlException)InnerException;
                summary +=
                    "Source: " + se.Source + "<br>" +
                    "Number: " + se.Number.ToString() + "<br>" +
                    "State: " + se.State.ToString() + "<br>" +
                    "Class: " + se.Class.ToString() + "<br>" +
                    "Server: " + se.Server + "<br>" +
                    "Procedure: " + se.Procedure + "<br>" +
                    "Line: " + se.LineNumber.ToString() + "<br><br>";
            }
            else
            {
                summary += "Exception was not an SqlException:<br>" + InnerException + "<br><br>";
            }
            return summary;
        }

    }
    

}
