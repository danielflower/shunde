using System;
using System.Collections.Generic;
using System.Text;

namespace Shunde.Framework
{
	/// <summary>A class for holding binary data</summary>
	public class BinaryData
	{

		/// <summary>The cached size of the data</summary>
		/// <remarks>Used when the data isn't loaded</remarks>
		private int size;

		private byte[] data;

		private string mimeType;

		private string filename;

		/// <summary>The data</summary>
		public byte[] Data
		{
			get { return data; }
			set { data = value; }
		}

		/// <summary>The MIME Type, if it is a file</summary>
		public string MimeType
		{
			get { return mimeType; }
			set { mimeType = value; }
		}

		/// <summary>The filename</summary>
		public string Filename
		{
			get { return filename; }
			set { filename = value; }
		}
		

		/// <summary>Creates a new binary data with the specified data and mimetype</summary>
		public BinaryData(byte[] data, string mimeType, string filename)
		{
			this.data = data;
			this.mimeType = mimeType;
			this.filename = filename;
			this.size = 0;
		}

		/// <summary>Creates a new binary data with the specified size and mimetype</summary>
		public BinaryData(int size, string mimeType, string filename)
		{
			this.size = size;
			this.mimeType = mimeType;
			this.filename = filename;
			this.data = null;
		}



		/// <summary>Gets the size, in bytes, of this data</summary>
		public int Size
		{
			get
			{
				if (data == null)
				{
					return size;
				}
				return data.Length;
			}
		}


	}
}
