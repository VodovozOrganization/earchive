using System;
using System.Collections.Generic;
using System.IO;
using Gdk;
using NLog;
using OpenCvSharp;

namespace earchive
{
	/// <summary>
	/// Р›РѕРєР°Р»СЊРЅС‹Р№ СЂР°СЃРїРѕР·РЅР°РІР°С‚РµР»СЊ QR-РєРѕРґРѕРІ РЅР° РґРІРёР¶РєРµ WeChat QRCode (OpenCV wechat_qrcode).
	/// Р—Р°РіСЂСѓР¶Р°РµС‚ РјРѕРґРµР»СЊ РІ РїСЂРѕС†РµСЃСЃ РїСЂРёР»РѕР¶РµРЅРёСЏ (x86) Рё РЅРµ С‚СЂРµР±СѓРµС‚ gRPC-СЃРµСЂРІРёСЃР°.
	/// РџРµСЂРµРґ СЂР°СЃРїРѕР·РЅР°РІР°РЅРёРµРј РїСЂРѕР±СѓРµС‚ РЅРµСЃРєРѕР»СЊРєРѕ РІР°СЂРёР°РЅС‚РѕРІ РёР·РѕР±СЂР°Р¶РµРЅРёСЏ: РёСЃС…РѕРґРЅРѕРµ,
	/// СѓРІРµР»РёС‡РµРЅРЅСѓСЋ РѕР±Р»Р°СЃС‚СЊ QR, РїРѕРІС‹С€РµРЅРёРµ РєРѕРЅС‚СЂР°СЃС‚Р° (CLAHE), СЂРµР·РєРѕСЃС‚СЊ,
	/// Р±РёРЅР°СЂРёР·Р°С†РёСЋ Рё РЅРµР±РѕР»СЊС€РёРµ РїРѕРІРѕСЂРѕС‚С‹.
	/// </summary>
	public static class WeChatQrScanner
	{
		private const long MaxImagePixels = 60_000_000;

		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private static readonly object sync = new object();
		private static readonly double[] SkewAngles = { -3.0, -2.0, -1.0, 1.0, 2.0, 3.0 };

		private static WeChatQRCode detector;
		private static bool initialized;
		private static volatile bool unavailable;
		private static string lastError;

		/// <summary>РўРµРєСЃС‚ РїРѕСЃР»РµРґРЅРµР№ РѕС€РёР±РєРё РёРЅРёС†РёР°Р»РёР·Р°С†РёРё/СЂР°Р±РѕС‚С‹ Р»РѕРєР°Р»СЊРЅРѕРіРѕ СЂР°СЃРїРѕР·РЅР°РІР°С‚РµР»СЏ.</summary>
		public static string LastError
		{
			get
			{
				lock (sync)
					return lastError;
			}
		}

		/// <summary>
		/// Р Р°СЃРїРѕР·РЅР°С‘С‚ QR-РєРѕРґС‹ РЅР° РёР·РѕР±СЂР°Р¶РµРЅРёРё.
		/// </summary>
		/// <returns>
		/// null вЂ” Р»РѕРєР°Р»СЊРЅС‹Р№ СЂР°СЃРїРѕР·РЅР°РІР°С‚РµР»СЊ РЅРµРґРѕСЃС‚СѓРїРµРЅ (РЅРµС‚ РјРѕРґРµР»Рё/OpenCV), РЅСѓР¶РЅРѕ РёСЃРїРѕР»СЊР·РѕРІР°С‚СЊ ZXing;
		/// РїСѓСЃС‚РѕР№ СЃРїРёСЃРѕРє вЂ” QR-РєРѕРґ РЅРµ РЅР°Р№РґРµРЅ.
		/// </returns>
		public static IReadOnlyList<string> Read(Pixbuf image)
		{
			if (image is null)
				return Array.Empty<string>();

			return ReadJpeg(image.SaveToBuffer("jpeg"));
		}

		/// <summary>
		/// Р Р°СЃРїРѕР·РЅР°С‘С‚ QR-РєРѕРґС‹ РїРѕ JPEG-СЃРѕРґРµСЂР¶РёРјРѕРјСѓ СЃС‚СЂР°РЅРёС†С‹.
		/// </summary>
		public static IReadOnlyList<string> ReadJpeg(byte[] jpeg)
		{
			if (jpeg == null || jpeg.Length == 0)
				return Array.Empty<string>();

			if (unavailable)
				return null;

			try
			{
				EnsureInitialized();
				if (detector is null)
					return null;

				using (var source = Cv2.ImDecode(jpeg, ImreadModes.Color))
				{
					if (source.Empty())
						return Array.Empty<string>();

					if ((long)source.Width * source.Height > MaxImagePixels)
					{
						logger.Warn("РР·РѕР±СЂР°Р¶РµРЅРёРµ СЃР»РёС€РєРѕРј Р±РѕР»СЊС€РѕРµ ({0}x{1}), QR РЅРµ СЂР°СЃРїРѕР·РЅР°С‘С‚СЃСЏ.", source.Width, source.Height);
						return Array.Empty<string>();
					}

					lock (sync)
					{
						return Recognize(source);
					}
				}
			}
			catch (Exception ex)
			{
				MarkUnavailable(ex);
				return null;
			}
		}

		private static void EnsureInitialized()
		{
			if (initialized)
				return;

			lock (sync)
			{
				if (initialized)
					return;

				initialized = true;

				try
				{
					string modelsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "QrModels");

					string detectProto = Path.Combine(modelsDir, "detect.prototxt");
					string detectModel = Path.Combine(modelsDir, "detect.caffemodel");
					string srProto = Path.Combine(modelsDir, "sr.prototxt");
					string srModel = Path.Combine(modelsDir, "sr.caffemodel");

					foreach (var modelFile in new[] { detectProto, detectModel, srProto, srModel })
					{
						if (!File.Exists(modelFile))
							throw new FileNotFoundException("РќРµ РЅР°Р№РґРµРЅ С„Р°Р№Р» РјРѕРґРµР»Рё WeChat QR: " + modelFile, modelFile);
					}

					detector = WeChatQRCode.Create(detectProto, detectModel, srProto, srModel);

					logger.Info("Р›РѕРєР°Р»СЊРЅС‹Рµ РјРѕРґРµР»Рё WeChat QR Р·Р°РіСЂСѓР¶РµРЅС‹ РёР· {0}.", modelsDir);
				}
				catch (Exception ex)
				{
					MarkUnavailable(ex);
				}
			}
		}

		private static void MarkUnavailable(Exception ex)
		{
			lock (sync)
			{
				if (!unavailable)
					logger.Error(ex, "Р›РѕРєР°Р»СЊРЅРѕРµ СЂР°СЃРїРѕР·РЅР°РІР°РЅРёРµ WeChat QR РЅРµРґРѕСЃС‚СѓРїРЅРѕ. Р”Р»СЏ QR Р±СѓРґРµС‚ РёСЃРїРѕР»СЊР·РѕРІР°РЅ ZXing.");

				unavailable = true;
				lastError = ex.Message;
			}
		}

		private static IReadOnlyList<string> Recognize(Mat source)
		{
			var results = new List<string>();
			var variants = new List<Mat>();

			try
			{
				CreateVariants(source, variants);

				for (int i = 0; i < variants.Count; i++)
				{
					try
					{
                        detector.DetectAndDecode(variants[i], out Mat[] boxes, out string[] texts);

                        if (boxes != null)
						{
							foreach (var box in boxes)
								box.Dispose();
						}

						foreach (var text in texts)
						{
							if (!string.IsNullOrWhiteSpace(text) && !results.Contains(text))
								results.Add(text);
						}

						if (results.Count > 0)
							break;
					}
					catch (Exception ex)
					{
						logger.Warn(ex, "РћС€РёР±РєР° СЂР°СЃРїРѕР·РЅР°РІР°РЅРёСЏ РІР°СЂРёР°РЅС‚Р° РёР·РѕР±СЂР°Р¶РµРЅРёСЏ {0}.", i);
					}
				}
			}
			finally
			{
				foreach (var variant in variants)
					variant.Dispose();
			}

			return results;
		}

		private static void CreateVariants(Mat source, List<Mat> variants)
		{
			// 1. РСЃС…РѕРґРЅР°СЏ СЃС‚СЂР°РЅРёС†Р° С†РµР»РёРєРѕРј.
			variants.Add(source.Clone());

			using (var roi = CropQrRegion(source))
			using (var padded = Pad(roi, 32))
			using (var gray = padded.CvtColor(ColorConversionCodes.BGR2GRAY))
			using (var enhancedGray = new Mat())
			using (var clahe = Cv2.CreateCLAHE(2.0, new OpenCvSharp.Size(8, 8)))
			using (var enhanced = new Mat())
			{
				// 2. РћР±Р»Р°СЃС‚СЊ QR Р±РµР· РёР·РјРµРЅРµРЅРёР№.
				variants.Add(padded.Clone());

				// 3. РџРѕРІС‹С€РµРЅРёРµ РєРѕРЅС‚СЂР°СЃС‚Р°.
				clahe.Apply(gray, enhancedGray);
				Cv2.CvtColor(enhancedGray, enhanced, ColorConversionCodes.GRAY2BGR);
				variants.Add(enhanced.Clone());

				// 4. РќРµР±РѕР»СЊС€РёРµ РїРѕРІРѕСЂРѕС‚С‹ РЅР° СЃР»СѓС‡Р°Р№ РїРµСЂРµРєРѕСЃР° СЃРєР°РЅР°.
				foreach (double angle in SkewAngles)
					variants.Add(Rotate(enhanced, angle));

				// 5. Р РµР·РєРѕСЃС‚СЊ.
				variants.Add(Unsharp(enhanced));

				// 6. Р‘РёРЅР°СЂРёР·Р°С†РёСЏ Рё СѓРІРµР»РёС‡РµРЅРёРµ вЂ” РїРѕРјРѕРіР°РµС‚ РЅР° Р±Р»РµРґРЅРѕР№ РїРµС‡Р°С‚Рё.
				variants.Add(ThresholdAndScale(gray, 2.0));
			}
		}

		private static Mat CropQrRegion(Mat source)
		{
			if (source.Width < 1000 || source.Height < 700)
				return source.Clone();

			int x = (int)(source.Width * 0.62);
			int width = source.Width - x;
			int height = (int)(source.Height * 0.36);

			if (width < 120 || height < 120)
				return source.Clone();

			using (var region = new Mat(source, new Rect(x, 0, width, height)))
				return region.Clone();
		}

		private static Mat Pad(Mat source, int padding)
		{
			var padded = new Mat();
			Cv2.CopyMakeBorder(
				source,
				padded,
				padding,
				padding,
				padding,
				padding,
				BorderTypes.Constant,
				new Scalar(255, 255, 255));

			return padded;
		}

		private static Mat Rotate(Mat source, double angle)
		{
			var center = new Point2f(source.Width / 2f, source.Height / 2f);
			var rotated = new Mat();

			using (var rotationMatrix = Cv2.GetRotationMatrix2D(center, angle, 1.0))
			{
				Cv2.WarpAffine(
					source,
					rotated,
					rotationMatrix,
					source.Size(),
					InterpolationFlags.Cubic,
					BorderTypes.Constant,
					new Scalar(255, 255, 255));
			}

			return rotated;
		}

		private static Mat Unsharp(Mat source)
		{
			var sharpened = new Mat();

			using (var blurred = new Mat())
			{
				Cv2.GaussianBlur(source, blurred, new OpenCvSharp.Size(0, 0), 2.0);
				Cv2.AddWeighted(source, 2.0, blurred, -1.0, 0.0, sharpened);
			}

			return sharpened;
		}

		private static Mat ThresholdAndScale(Mat gray, double scale)
		{
			var bgr = new Mat();

			using (var binary = new Mat())
			using (var scaled = new Mat())
			{
				Cv2.Threshold(gray, binary, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
				Cv2.Resize(
					binary,
					scaled,
					new OpenCvSharp.Size((int)(binary.Width * scale), (int)(binary.Height * scale)),
					0,
					0,
					InterpolationFlags.Cubic);

				Cv2.CvtColor(scaled, bgr, ColorConversionCodes.GRAY2BGR);
			}

			return bgr;
		}

		/// <summary>РћСЃРІРѕР±РѕР¶РґР°РµС‚ РЅР°С‚РёРІРЅС‹Рµ СЂРµСЃСѓСЂСЃС‹ OpenCV (РІС‹Р·С‹РІР°РµС‚СЃСЏ РїСЂРё Р·Р°РєСЂС‹С‚РёРё РїСЂРёР»РѕР¶РµРЅРёСЏ).</summary>
		public static void Shutdown()
		{
			lock (sync)
			{
                detector?.Dispose();
                detector = null;

                initialized = false;
			}
		}
	}
}

