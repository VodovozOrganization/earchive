using System;
using ZXing.QrCode;
using Gdk;
using ZXing;
using ZXing.Common;
using System.ComponentModel;
using System.Drawing;
using NLog;

namespace earchive
{
	public static class QRScanner
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public static Result ReadQRCode(Pixbuf pb)
		{
			Result result = null;
			using (Bitmap bm = GetBitmap(pb)) {
				if(bm == null) {
					return result;
				}

				result = DecodeQRCode(bm);
				if(result != null) {
					return result;
				}
				using (Bitmap scaledBitmap = ScaleImage (bm, 0.99f)) {
					result = DecodeQRCode (scaledBitmap);
					if (result != null) {
						return result;
					}
				}
				result = TransformAndDecode(bm, 1, 0.99f);
				if (result != null) {
					return result;
				}
				result = TransformAndDecode (bm, 2, 0.99f);
				if (result != null) {
					return result;
				}
				result = TransformAndDecode (bm, -1, 0.99f);
				if (result != null) {
					return result;
				}
				result = TransformAndDecode (bm, -2, 0.99f);
			}
			return result;
		}

		private static Result TransformAndDecode(Bitmap bm, int rotateAngle, float scaleValue)
		{
			Result result = null;
			using(Bitmap rotatedBitmap = RotateImage(bm, rotateAngle)) 
			using (Bitmap scaledBitmap = ScaleImage(rotatedBitmap, scaleValue)){
				result = DecodeQRCode (rotatedBitmap);
				if (result != null) {
					return result;
				}
				result = DecodeQRCode(scaledBitmap);
				if(result != null) {
					return result;
				}
			}
			return result;
		}

		private static Result DecodeQRCode (Bitmap bm)
		{
			Result decodeResult = null;
			try {
				QRCodeReader reader = new QRCodeReader();
				BitmapLuminanceSource source = new BitmapLuminanceSource (bm);
				BinaryBitmap bb = new BinaryBitmap (new HybridBinarizer (source));
				decodeResult = reader.decode(bb);
			} catch (Exception ex) {
				logger.Error(ex.Message);
				return null;
			}
			return decodeResult;
		}

		private static Bitmap GetBitmap(Pixbuf pb)
		{
			var imgBuffer = pb.SaveToBuffer ("jpeg");
			return TypeDescriptor.GetConverter (typeof (Bitmap)).ConvertFrom (imgBuffer) as Bitmap;
		}

		private static Bitmap RotateImage (Bitmap b, float angle)
		{
			Bitmap returnBitmap = new Bitmap (b.Width, b.Height);
			using (Graphics g = Graphics.FromImage (returnBitmap)) {
				g.TranslateTransform ((float)b.Width / 2, (float)b.Height / 2);
				g.RotateTransform (angle);
				g.TranslateTransform (-(float)b.Width / 2, -(float)b.Height / 2);
				g.DrawImage (b, new System.Drawing.Point (0, 0));
			}
			return returnBitmap;
		}

		private static Bitmap ScaleImage (Bitmap b, float k)
		{
			Bitmap returnBitmap = new Bitmap (b.Width, b.Height);
			using (Graphics g = Graphics.FromImage (returnBitmap)) {
				g.TranslateTransform ((float)b.Width / 2, (float)b.Height / 2);
				g.ScaleTransform (k, k);
				g.TranslateTransform (-(float)b.Width / 2, -(float)b.Height / 2);
				g.DrawImage (b, new System.Drawing.Point (0, 0));
			}
			return returnBitmap;
		}
	}
}
