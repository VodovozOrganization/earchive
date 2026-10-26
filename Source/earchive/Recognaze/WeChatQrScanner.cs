using System;
using System.Collections.Generic;
using System.IO;
using Gdk;
using NLog;
using OpenCvSharp;

namespace earchive
{
	/// <summary>
	/// Локальный распознаватель QR-кодов на движке WeChat QRCode (OpenCV wechat_qrcode).
	/// Загружает модель в процесс приложения (x86) и не требует gRPC-сервиса.
	/// Перед распознаванием пробует несколько вариантов изображения: исходное,
	/// увеличенную область QR, повышение контраста (CLAHE), резкость,
	/// бинаризацию и небольшие повороты.
	/// </summary>
	public static class WeChatQrScanner
	{
		private const long MaxImagePixels = 60_000_000;

		// WeChat-детектор плохо работает на очень больших страницах (например,
		// 600 dpi, 4960x7014): QR после внутреннего ресайза становится слишком
		// мелким. Ограничиваем длинную сторону.
		private const int MaxDetectionDimension = 2500;

		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private static readonly object sync = new object();
		private static readonly double[] SkewAngles = { -3.0, -2.0, -1.0, 1.0, 2.0, 3.0 };

		private static WeChatQRCode detector;
		private static bool initialized;
		private static volatile bool unavailable;
		private static string lastError;

		/// <summary>Текст последней ошибки инициализации/работы локального распознавателя.</summary>
		public static string LastError
		{
			get
			{
				lock (sync)
					return lastError;
			}
		}

		/// <summary>
		/// Распознаёт QR-коды на изображении.
		/// </summary>
		/// <returns>
		/// null — локальный распознаватель недоступен (нет модели/OpenCV), нужно использовать ZXing;
		/// пустой список — QR-код не найден.
		/// </returns>
		public static IReadOnlyList<string> Read(Pixbuf image, bool tryAllOrientations = true)
		{
			if (image is null)
				return Array.Empty<string>();

			// PNG без потерь: не портим и без того слабый QR повторным JPEG-сжатием.
			return ReadImage(image.SaveToBuffer("png"), tryAllOrientations);
		}

		/// <summary>
		/// Распознаёт QR-коды по содержимому изображения (PNG/JPEG/BMP).
		/// </summary>
		public static IReadOnlyList<string> ReadImage(byte[] imageBytes, bool tryAllOrientations = true)
		{
			if (imageBytes is null || imageBytes.Length == 0)
				return Array.Empty<string>();

			if (unavailable)
				return null;

			try
			{
				EnsureInitialized();
				if (detector is null)
					return null;

				using (var source = Cv2.ImDecode(imageBytes, ImreadModes.Color))
				{
					if (source.Empty())
					{
						logger.Warn("Не удалось декодировать изображение страницы ({0} байт).", imageBytes.Length);
						return Array.Empty<string>();
					}

					if ((long)source.Width * source.Height > MaxImagePixels)
					{
						logger.Warn("Изображение слишком большое ({0}x{1}), QR не распознаётся.", source.Width, source.Height);
						return Array.Empty<string>();
					}

					Mat resized = null;

					try
					{
						Mat working = source;

						int maxDimension = Math.Max(source.Width, source.Height);

						if (maxDimension > MaxDetectionDimension)
						{
							double scale = (double)MaxDetectionDimension / maxDimension;

							resized = new Mat();
							Cv2.Resize(
								source,
								resized,
								new OpenCvSharp.Size(
									(int)Math.Round(source.Width * scale),
									(int)Math.Round(source.Height * scale)),
								0,
								0,
								InterpolationFlags.Area);

							working = resized;

							logger.Debug(
								"Страница уменьшена для распознавания: {0}x{1} -> {2}x{3}.",
								source.Width,
								source.Height,
								resized.Width,
								resized.Height);
						}

						lock (sync)
						{
							var fullResults = TryFullImageScales(working);
							if (fullResults.Count > 0)
								return fullResults;

							return RecognizePage(working, tryAllOrientations);
						}
					}
					finally
					{
						if (resized != null)
							resized.Dispose();
					}
				}
			}
			catch (Exception ex)
			{
				MarkUnavailable(ex);
				return null;
			}
		}

		/// <summary>
		/// Совместимый псевдоним <see cref="ReadImage"/>.
		/// </summary>
		public static IReadOnlyList<string> ReadJpeg(byte[] jpeg)
		{
			return ReadImage(jpeg);
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
							throw new FileNotFoundException("Не найден файл модели WeChat QR: " + modelFile, modelFile);
					}

					logger.Info(
						"Локальный WeChat QR: процесс {0}-бит, модели из {1}.",
						IntPtr.Size * 8,
						modelsDir);

					if (IntPtr.Size != 4)
					{
						logger.Warn(
							"Нативная библиотека OpenCV собрана под x86. В 64-битном процессе " +
							"нужна x64-сборка OpenCvSharpExtern.dll.");
					}

					detector = WeChatQRCode.Create(detectProto, detectModel, srProto, srModel);
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
					logger.Error(ex, "Локальное распознавание WeChat QR недоступно. Для QR будет использован ZXing.");

				unavailable = true;
				lastError = ex.Message;
			}
		}

		private static IReadOnlyList<string> TryFullImageScales(Mat source)
		{
			var results = new List<string>();
			int maxDimension = Math.Max(source.Width, source.Height);

			TryDetectWhole(source, results);
			if (results.Count > 0)
				return results;

			foreach (int size in new[] { 2500, 2000, 1600, 1200, 900, 700, 500, 400, 300 })
			{
				if (size >= maxDimension)
					continue;

				using (var resized = ResizeToMax(source, size))
				{
					TryDetectWhole(resized, results);
					if (results.Count > 0)
						return results;
				}
			}

			return results;
		}

		private static void TryDetectWhole(Mat image, List<string> results)
		{
			try
			{
				Mat[] boxes;
				string[] texts;
				detector.DetectAndDecode(image, out boxes, out texts);

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

				logger.Debug(
					"WeChat QR, кадр целиком {0}x{1}: найдено кодов {2}.",
					image.Width,
					image.Height,
					texts.Length);
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Ошибка распознавания кадра целиком {0}x{1}.", image.Width, image.Height);
			}
		}

		private static Mat ResizeToMax(Mat source, int maxDimension)
		{
			double scale = (double)maxDimension / Math.Max(source.Width, source.Height);

			var resized = new Mat();
			Cv2.Resize(
				source,
				resized,
				new OpenCvSharp.Size(
					(int)Math.Round(source.Width * scale),
					(int)Math.Round(source.Height * scale)),
				0,
				0,
				InterpolationFlags.Area);

			return resized;
		}

		private static IReadOnlyList<string> RecognizePage(Mat source, bool tryAllOrientations)
		{
			var results = Recognize(source, true);
			if (results.Count > 0 || !tryAllOrientations)
				return results;

			// Скан мог быть повёрнут на 90/180/270 градусов. Пробуем вернуть
			// страницу в нормальную ориентацию: тогда QR снова окажется
			// в правом верхнем углу.
			foreach (var flags in new[]
			{
				RotateFlags.Rotate90Clockwise,
				RotateFlags.Rotate180,
				RotateFlags.Rotate90Counterclockwise
			})
			{
				using (var rotated = RotateQuarter(source, flags))
				{
					var wholeResults = TryFullImageScales(rotated);
					if (wholeResults.Count > 0)
						return wholeResults;

					results = Recognize(rotated, false);
					if (results.Count > 0)
						return results;
				}
			}

			logger.Debug("WeChat QR не нашёл кодов ни в одной ориентации страницы.");
			return results;
		}

		private static IReadOnlyList<string> Recognize(Mat source, bool logMiss)
		{
			var results = new List<string>();
			var variants = new List<Mat>();
			var failures = 0;
			Exception firstFailure = null;

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

						logger.Debug("WeChat QR, вариант {0}: найдено кодов {1}.", i, texts.Length);

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
						failures++;

						if (firstFailure is null)
							firstFailure = ex;

						if (IsFatal(ex))
							throw;

						logger.Warn(ex, "Ошибка распознавания варианта изображения {0}.", i);
					}
				}

				if (results.Count == 0 && failures == variants.Count && firstFailure != null)
					throw new InvalidOperationException("Все варианты изображения завершились ошибкой распознавания.", firstFailure);

				if (results.Count == 0 && logMiss)
					logger.Debug("WeChat QR не нашёл кодов на странице (проверено вариантов: {0}).", variants.Count);
			}
			finally
			{
				foreach (var variant in variants)
					variant.Dispose();
			}

			return results;
		}

		private static bool IsFatal(Exception ex)
		{
			return ex is DllNotFoundException
				|| ex is BadImageFormatException
				|| ex is EntryPointNotFoundException
				|| ex is TypeInitializationException;
		}

		private static void CreateVariants(Mat source, List<Mat> variants)
		{
			variants.Add(source.Clone());

			foreach (int k in new[] { 3, 5, 7, 9 })
				variants.Add(Blur(source, k));

			foreach (int k in new[] { 3, 4, 5 })
				variants.Add(MorphOpen(source, k));

			using (var roi = CropQrRegion(source))
			using (var padded = Pad(roi, 48))
			using (var gray = padded.CvtColor(ColorConversionCodes.BGR2GRAY))
			using (var enhancedGray = new Mat())
			using (var clahe = Cv2.CreateCLAHE(2.0, new OpenCvSharp.Size(8, 8)))
			using (var enhanced = new Mat())
			{
				variants.Add(padded.Clone());

				variants.Add(RotateQuarter(padded, RotateFlags.Rotate90Clockwise));
				variants.Add(RotateQuarter(padded, RotateFlags.Rotate180));
				variants.Add(RotateQuarter(padded, RotateFlags.Rotate90Counterclockwise));

				clahe.Apply(gray, enhancedGray);
				Cv2.CvtColor(enhancedGray, enhanced, ColorConversionCodes.GRAY2BGR);
				variants.Add(enhanced.Clone());

				foreach (double angle in SkewAngles)
					variants.Add(Rotate(enhanced, angle));

				variants.Add(Unsharp(enhanced));

				variants.Add(ThresholdAndScale(gray, 2.0));
			}
		}

		private static Mat CropQrRegion(Mat source)
		{
			if (source.Width < 1000 || source.Height < 700)
				return source.Clone();

			int x = (int)(source.Width * 0.55);
			int width = source.Width - x;
			int height = (int)(source.Height * 0.40);

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

		private static Mat RotateQuarter(Mat source, RotateFlags flags)
		{
			var rotated = new Mat();
			Cv2.Rotate(source, rotated, flags);
			return rotated;
		}

		private static Mat Blur(Mat source, int kernelSize)
		{
			var blurred = new Mat();
			Cv2.GaussianBlur(
				source,
				blurred,
				new OpenCvSharp.Size(kernelSize, kernelSize),
				0);

			return blurred;
		}

		private static Mat MorphOpen(Mat source, int kernelSize)
		{
			var opened = new Mat();

			using (var kernel = Cv2.GetStructuringElement(
				MorphShapes.Rect,
				new OpenCvSharp.Size(kernelSize, kernelSize)))
			{
				Cv2.MorphologyEx(source, opened, MorphTypes.Open, kernel);
			}

			return opened;
		}

		private static Mat Unsharp(Mat source)
		{			var sharpened = new Mat();

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
