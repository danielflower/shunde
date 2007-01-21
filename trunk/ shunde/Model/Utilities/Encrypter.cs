using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Shunde.Utilities
{

	/// <summary>
	/// Some methods for encrypting text
	/// </summary>
	public class Encrypter
	{






		/// <summary>Makes a one-way encryption on a string</summary>
		/// <param name="input">The string to encrypt</param>
		/// <returns>Returns an encrypted string which should be nearly impossible to return to the original string</returns>
		public static string EncryptOneWay(string input)
		{

			byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);

			MD5 md5 = new MD5CryptoServiceProvider();
			bytes = md5.ComputeHash(bytes);
			return ToHexString(bytes);

		}

		/// <summary>The hex digits in use</summary>
		public static char[] HexDigits = {
        '0', '1', '2', '3', '4', '5', '6', '7',
        '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

		/// <summary>Converts an array of bytes into hexadecimal digits</summary> 
		public static string ToHexString(byte[] bytes)
		{
			char[] chars = new char[bytes.Length * 2];
			for (int i = 0; i < bytes.Length; i++)
			{
				int b = bytes[i];
				chars[i * 2] = HexDigits[b >> 4];
				chars[i * 2 + 1] = HexDigits[b & 0xF];
			}
			return new string(chars);
		}





	}
}
