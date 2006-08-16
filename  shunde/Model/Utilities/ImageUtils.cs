using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using Shunde.Framework;


namespace Shunde.Utilities
{
	/// <summary>An image with methods to load, resize, and save images</summary>
	public class ImageUtils
	{


		/// <summary>Instantiates a new ShundeImage from the given URI</summary>
		public static Bitmap CreateFromUri(Uri uri)
		{
			WebClient wc = new WebClient();
			Bitmap img = new Bitmap(wc.OpenRead(uri.AbsoluteUri));
			wc.Dispose();
			return img;
		}


		/// <summary>Instantiates a new ShundeImage from the given URI</summary>
		public static Bitmap CreateFromBinaryData(BinaryData bd)
		{
			if (bd.Data.Length == 0)
			{
				throw new Exception("The binary data has no bytes");
			}

			MemoryStream ms = new MemoryStream(bd.Data);

			Bitmap bitmap = new Bitmap(ms);
			ms.Close();
			return bitmap;

		}


		/// <summary>Gets this image as an array of bytes</summary>
		public static byte[] GetBytes(Bitmap bitmap, long compression)
		{

			ImageCodecInfo codecInfo = GetJpgCodec();
			Encoder encoder = Encoder.Quality;
			EncoderParameters encoderParams = new EncoderParameters(1);
			EncoderParameter encoderParam = new EncoderParameter(encoder, compression);
			encoderParams.Param[0] = encoderParam;

			MemoryStream ms = new MemoryStream();
			bitmap.Save(ms, codecInfo, encoderParams);

			return ms.GetBuffer();
		}




		/// <summary>Creates a thumbnail of the current image, keeping width/height dimensions</summary>
		/// <param Name="image">The image to be copied and resized</param>
		/// <param Name="newSize">The new Size to convert to</param>
		/// <param Name="addWhiteSpace">If true, white space will be added to the sides to keep the ratio. If false, the ratio will be kept, however either the width or height will not be the requested size.</param>
		public static Bitmap CreatedResizedCopy(Image image, Size newSize, bool addWhiteSpace)
		{
			return CreatedResizedCopy(image, newSize.Width, newSize.Height, addWhiteSpace);
		}

		/// <summary>Creates a thumbnail of the current image, keeping width/height dimensions</summary>
		/// <param Name="image">The image to be copied and resized</param>
		/// <param Name="newWidth">The new width to convert to</param>
		/// <param Name="newHeight">The new height to convert to</param>
		/// <param Name="addWhiteSpace">If true, white space will be added to the sides to keep the ratio. If false, the ratio will be kept, however either the width or height will not be the requested size.</param>
		public static Bitmap CreatedResizedCopy(Image image, int newWidth, int newHeight, bool addWhiteSpace)
		{

			return CreatedResizedCopy(image, newWidth, newHeight, addWhiteSpace, Color.White);

		}

		/// <summary>Creates a thumbnail of the current image, keeping width/height dimensions</summary>
		/// <param Name="image">The image to be copied and resized</param>
		/// <param Name="newWidth">The new width to convert to</param>
		/// <param Name="newHeight">The new height to convert to</param>
		/// <param Name="addWhiteSpace">If true, white space will be added to the sides to keep the ratio. If false, the ratio will be kept, however either the width or height will not be the requested size.</param>
		/// <param Name="backgroundColor">If addWhiteSpace is true and white space is added, the colour of the "white" space is this color</param>
		public static Bitmap CreatedResizedCopy(Image image, int newWidth, int newHeight, bool addWhiteSpace, Color backgroundColor)
		{
			return CreatedResizedCopy(image, newWidth, newHeight, addWhiteSpace, backgroundColor, System.Web.UI.WebControls.VerticalAlign.Middle, System.Web.UI.WebControls.HorizontalAlign.Center);
		}



		/// <summary>Creates a thumbnail of the current image, keeping width/height dimensions</summary>
		/// <param Name="image">The image to be copied and resized</param>
		/// <param Name="newWidth">The new width to convert to</param>
		/// <param Name="newHeight">The new height to convert to</param>
		/// <param Name="addWhiteSpace">If true, white space will be added to the sides to keep the ratio. If false, the ratio will be kept, however either the width or height will not be the requested size.</param>
		/// <param Name="backgroundColor">If addWhiteSpace is true and white space is added, the colour of the "white" space is this color</param>
		/// <param Name="verticalAlign">If addWhiteSpace is true, specifies how original image will be vertically aligned in the new sized image</param>
		/// <param Name="horizontalAlign">If addWhiteSpace is true, specifies how original image will be horizontally aligned in the new sized image</param>
		public static Bitmap CreatedResizedCopy(Image image, int newWidth, int newHeight, bool addWhiteSpace, Color backgroundColor, System.Web.UI.WebControls.VerticalAlign verticalAlign, System.Web.UI.WebControls.HorizontalAlign horizontalAlign)
		{

			double resizeMultiplier = GetResizeMultiplier(image, newWidth, newHeight);

			int changeWidth = (int)(image.Width * resizeMultiplier);
			int changeHeight = (int)(image.Height * resizeMultiplier);

			/*
						if (changeWidth > img.Width || changeHeight > img.Height) {
							changeWidth = img.Width;
							changeHeight = img.Height;
						}
			*/






			Bitmap newImg = new Bitmap(changeWidth, changeHeight);



			Graphics gr = Graphics.FromImage(newImg);

			// High Quality Smoothing of Images
			gr.SmoothingMode = SmoothingMode.HighQuality;

			// High Quality Pixel Interpolation for shrinking
			gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

			// High Quality, low speed rendering
			gr.PixelOffsetMode = PixelOffsetMode.HighQuality;

			// When a color is rendered, it overwrites the bg color
			gr.CompositingMode = CompositingMode.SourceCopy;

			// During shrinking, this says how many surrounding pixels to take
			// into account when making new composite pixels
			gr.CompositingQuality = CompositingQuality.HighQuality;

			gr.DrawImage(image, 0, 0, changeWidth, changeHeight);





			int yOffset;
			if (verticalAlign == System.Web.UI.WebControls.VerticalAlign.Top)
			{
				yOffset = 0;
			}
			else if (verticalAlign == System.Web.UI.WebControls.VerticalAlign.Bottom)
			{
				yOffset = newHeight - changeHeight;
			}
			else
			{
				yOffset = (int)((newHeight - changeHeight) / 2);
			}

			int xOffset;
			if (horizontalAlign == System.Web.UI.WebControls.HorizontalAlign.Left)
			{
				xOffset = 0;
			}
			else if (horizontalAlign == System.Web.UI.WebControls.HorizontalAlign.Right)
			{
				xOffset = newWidth - changeWidth;
			}
			else
			{
				xOffset = (int)((newWidth - changeWidth) / 2);
			}

			if (!addWhiteSpace)
			{
				newHeight -= newHeight - changeHeight;
				newWidth -= newWidth - changeWidth;
				yOffset = 0;
				xOffset = 0;
			}


			Bitmap secondBitmap = new Bitmap(newWidth, newHeight);
			Graphics g = Graphics.FromImage(secondBitmap);

			g.Clear(backgroundColor);

			g.DrawImage(newImg, xOffset, yOffset);

			return secondBitmap;

		}



		/// <summary>Calculates amount to strectch image so the width and height are at a maximum while retaining aspect ratio</summary>
		/// <param Name="image">The image that the multiplier is for</param>
		/// <param Name="newWidth">The new width to convert to</param>
		/// <param Name="newHeight">The new height to convert to</param>
		private static double GetResizeMultiplier(Image image, double newWidth, double newHeight)
		{

			int newHeightDif = (int)(newHeight - image.Height);
			int newWidthDif = (int)(newWidth - image.Width);

			double m;
			if (newHeightDif < newWidthDif)
				m = newHeight / image.Height;
			else
				m = newWidth / image.Width;

			return m;

		}

		/// <summary>Gets the JPG codec</summary>
		public static ImageCodecInfo GetJpgCodec()
		{
			return GetEncoderInfo("image/jpeg");
		}


		/// <summary>Gets a codec info object from a mime-type</summary>
		private static ImageCodecInfo GetEncoderInfo(string mimeType)
		{

			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
			foreach (ImageCodecInfo codec in codecs)
			{
				if (codec.MimeType == mimeType)
				{
					return codec;
				}
			}
			return null;
		}

	}
}
