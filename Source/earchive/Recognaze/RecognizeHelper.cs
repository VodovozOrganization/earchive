using System.Drawing;
using System.IO;
using Tesseract;
using Gdk;

namespace earchive
{
	public class RecognizeHelper
	{
		public static Pix PixbufToPix(Pixbuf image)
		{
			using (var stream = new MemoryStream(image.SaveToBuffer("png")))
			using (var rawBitmap = new Bitmap(stream))
			using (var sysimage = new Bitmap(rawBitmap))
			{
				return PixConverter.ToPix(sysimage);
			}
		}

		public static Bitmap PixbufToBitmap(Pixbuf image)
		{
			using (var stream = new MemoryStream(image.SaveToBuffer("png")))
			using (var rawBitmap = new Bitmap(stream))
			{
				return new Bitmap(rawBitmap);
			}
		}

	}
}