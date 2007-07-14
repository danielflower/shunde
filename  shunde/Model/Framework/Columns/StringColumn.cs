using System;
using System.Collections.Generic;
using System.Text;
using Shunde.Utilities;
using System.Text.RegularExpressions;

namespace Shunde.Framework.Columns
{

	/// <summary>
	/// A column which holds either a <see cref="SingleLineString"/> or <see cref="MultiLineString"/>
	/// </summary>
	public abstract class StringColumn : DBColumn
	{

		private string regularExpression = null;

		/// <summary>
		/// A regular expression to validate string data against (ignored if the value is string.Empty and 0-length strings are allowed)
		/// </summary>
		public string RegularExpression
		{
			get { return regularExpression; }
			set { regularExpression = value; }
		}

		private string regularExpressionErrorMessage = null;

		/// <summary>
		/// An error message to display if the string does not match the regular expression
		/// </summary>
		public string RegularExpressionErrorMessage
		{
			get { return regularExpressionErrorMessage; }
			set { regularExpressionErrorMessage = value; }
		}


		private int minLength = 0;

		/// <summary>This is the minimum number of characters allowed for a string in this column</summary>
		/// <remarks>
		/// 	<para>For a string Value, this should always be at least 0. For other types, this Value is ignored.</para>
		/// 	<para>If a string column has a Value shorter than the set Value, then a <see cref="ValidationException" /> will be thrown.</para>
		/// </remarks>
		public int MinLength
		{
			get { return minLength; }
			set { minLength = value; }
		}

		private int maxLength = -1;

		/// <summary>This is the maximum number of characters allowed for a string in this column</summary>
		/// <remarks>
		/// 	<para>If this is set to a positive Value then the type of column is assumed to be nvarchar. A Value of -1 means that the column is assumed to be an ntext column.</para>
		/// 	<para>If a string column has a Value longer than the set Value and the set Value is positive, then a <see cref="ValidationException" /> will be thrown.</para>
		/// </remarks>
		public int MaxLength
		{
			get { return maxLength; }
			set { maxLength = value; }
		}


		/// <summary>
		/// Creates a new string column
		/// </summary>
		protected StringColumn(string colName, bool allowNulls)
			: base(colName, typeof(string), allowNulls)
		{
		}

		/// <summary>
		/// Returns true if the string is null or empty
		/// </summary>
		public override bool IsNull(object value)
		{
			return string.IsNullOrEmpty((string)value);
		}


		/// <summary>Checks that the given Value is within the constraints placed upon it by this column.</summary>
		/// <remarks>This does not check the specific constraints specified in the <see cref="DBColumn.Constraints" /> field. A Value violating those constraints will be found when attempting to save the object.</remarks>
		/// <exception cref="ValidationException">Thrown if the Value violates the constraints of this column. The Message property contains a friendly error message, suitable to show to end users, on why the validation failed.</exception>
		public override void Validate(DBObject obj, object value)
		{
			base.Validate(obj, value);
			if (value == null)
			{
				return;
			}

			string friendlyName = TextUtils.MakeFriendly(this.Name);

			if (this.MaxLength > 0)
			{
				int len = value.ToString().Length;
				if (len > maxLength)
				{
					throw new ValidationException("The maximum length allowed for \"" + friendlyName + "\" is " + maxLength + " characters. You have written " + len + " characters.");
				}
			}

			if (minLength > 0)
			{
				int len = value.ToString().Length;
				if (len < minLength)
				{
					throw new ValidationException("The minimum length allowed for \"" + friendlyName + "\" is " + minLength + " character" + ((minLength == 1) ? "" : "s") + ". You have written " + len + " characters.");
				}
			}

			if (regularExpression != null)
			{
				if (!Regex.IsMatch((string)value, regularExpression))
				{

					string msg = "The value \"" + value + "\" entered for \"" + friendlyName + "\" is invalid.";
					if (regularExpressionErrorMessage != null)
					{
						msg += " " + regularExpressionErrorMessage;
					}
					throw new ValidationException(msg);
				}
			}

		}


	}


	/// <summary>
	/// Regular expressions, with error messages
	/// </summary>
	public static class RegularExpressionConstants
	{

		/// <summary>
		/// Checks that the string is an email address
		/// </summary>
		public const string Email = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";

		/// <summary>
		/// The error message for when an email address is not entered correctly
		/// </summary>
		public const string EmailErrorMessage = "Please enter a valid email address.";



		/// <summary>
		/// Checks that the string has no HTML in it (actually, checks for the &lt; and &gt; symbols)
		/// </summary>
		public const string NoHtml = @"^[^<>]*$";

		/// <summary>
		/// The error message for when a string contains HTML
		/// </summary>
		public const string NoHtmlErrorMessage = "Sorry, HTML, or the \"<\" and \">\" signs are not allowed.";



		/// <summary>
		/// Checks that the string contains only digits
		/// </summary>
		public const string Numeric = @"^[0-9]*$";

		/// <summary>
		/// The error message for when a string does not contain only digits
		/// </summary>
		public const string NumericErrorMessage = "Only digits from 0-9 are allowed.";



		/// <summary>
		/// Checks that the string contains only letters from the English alphabet
		/// </summary>
		public const string Alphabetical = @"^[a-zA-Z]*$";

		/// <summary>
		/// The error message for when a string Checks that the string does not contain only letters from the English alphabet
		/// </summary>
		public const string AlphabeticalErrorMessage = "Only alphabetical characters are allowed.";





		/// <summary>
		/// Checks that the string contains only letters from the English alphabet and/or digits from 0-9
		/// </summary>
		public const string Alphanumerical = @"^[a-zA-Z0-9]*$";

		/// <summary>
		/// The error message for when a string Checks that the string does not contain only letters from the English alphabet and/or digits
		/// </summary>
		public const string AlphanumericalErrorMessage = "Only alphabetical characters and digits are allowed.";





	}


}
