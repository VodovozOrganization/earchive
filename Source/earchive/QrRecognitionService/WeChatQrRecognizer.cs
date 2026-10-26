using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using OpenCvSharp;

namespace Earchive.QrService
{

    /// <summary>
    /// Распознаватель QR-кодов на движке WeChat QRCode (OpenCV wechat_qrcode).
    /// Перед распознаванием пробует несколько вариантов изображения: исходное,
    /// увеличенную область QR, повышение контраста (CLAHE), резкость,
    /// бинаризацию и небольшие повороты.
    /// </summary>
    public sealed class WeChatQrRecognizer : IDisposable
    {
        private const long MaxImagePixels = 60_000_000;

        private static readonly double[] SkewAngles = { -3.0, -2.0, -1.0, 1.0, 2.0, 3.0 };

        private readonly WeChatQRCode _detector;
        private readonly CLAHE _clahe;
        private readonly ILogger<WeChatQrRecognizer> _logger;
        private readonly object _sync = new();

        public WeChatQrRecognizer(string modelsPath, ILogger<WeChatQrRecognizer> logger)
        {
            _logger = logger;

            string detectProto = Path.Combine(modelsPath, "detect.prototxt");
            string detectModel = Path.Combine(modelsPath, "detect.caffemodel");
            string srProto = Path.Combine(modelsPath, "sr.prototxt");
            string srModel = Path.Combine(modelsPath, "sr.caffemodel");

            foreach (var modelFile in new[] { detectProto, detectModel, srProto, srModel })
            {
                if (!File.Exists(modelFile))
                    throw new FileNotFoundException($"Не найден файл модели WeChat QR: {modelFile}", modelFile);
            }

            _detector = new WeChatQRCode(detectProto, detectModel, srProto, srModel);
            _clahe = Cv2.CreateCLAHE(2.0, new Size(8, 8));

            _logger.LogInformation("Модели WeChat QR загружены из {ModelsPath}.", modelsPath);
        }

        /// <summary>
        /// Распознаёт QR-коды на изображении.
        /// </summary>
        /// <returns>Тексты найденных QR-кодов; пустой список, если кодов нет.</returns>
        public IReadOnlyList<string> Recognize(byte[] imageBytes, CancellationToken cancellationToken)
        {
            var results = new List<string>();

            if (imageBytes == null || imageBytes.Length == 0)
                return results;

            using var source = Cv2.ImDecode(imageBytes, ImreadModes.Color);

            if (source.Empty())
            {
                _logger.LogWarning("Не удалось декодировать изображение ({Size} байт).", imageBytes.Length);
                return results;
            }

            if ((long)source.Width * source.Height > MaxImagePixels)
            {
                _logger.LogWarning(
                    "Изображение слишком большое ({Width}x{Height}), распознавание пропущено.",
                    source.Width,
                    source.Height);
                return results;
            }

            var variants = new List<Mat>();

            try
            {
                CreateVariants(source, variants);

                lock (_sync)
                {
                    for (int i = 0; i < variants.Count; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            string[] texts = _detector.DetectAndDecode(variants[i], out _);

                            foreach (var text in texts)
                            {
                                if (!string.IsNullOrWhiteSpace(text) && !results.Contains(text))
                                    results.Add(text);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Ошибка распознавания варианта изображения {Index}.", i);
                        }

                        if (results.Count > 0)
                            break;
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

        private void CreateVariants(Mat source, List<Mat> variants)
        {
            variants.Add(source.Clone());

            using var roi = CropQrRegion(source);
            using var padded = Pad(roi, 32);

            variants.Add(padded.Clone());

            using var gray = padded.CvtColor(ColorConversionCodes.BGR2GRAY);

            using var enhancedGray = new Mat();
            _clahe.Apply(gray, enhancedGray);
            using var enhanced = enhancedGray.CvtColor(ColorConversionCodes.GRAY2BGR);

            variants.Add(enhanced.Clone());

            foreach (double angle in SkewAngles)
                variants.Add(Rotate(enhanced, angle));

            variants.Add(Unsharp(enhanced));

            variants.Add(ThresholdAndScale(gray, 2.0));
        }

        /// <summary>
        /// Область в правом верхнем углу страницы, где обычно расположен QR-код УПД.
        /// Для небольших изображений возвращает копию целиком.
        /// </summary>
        private static Mat CropQrRegion(Mat source)
        {
            if (source.Width < 1000 || source.Height < 700)
                return source.Clone();

            int x = (int)(source.Width * 0.62);
            int width = source.Width - x;
            int height = (int)(source.Height * 0.36);

            if (width < 120 || height < 120)
                return source.Clone();

            using var region = new Mat(source, new Rect(x, 0, width, height));
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

            using var rotationMatrix = Cv2.GetRotationMatrix2D(center, angle, 1.0);
            var rotated = new Mat();

            Cv2.WarpAffine(
                source,
                rotated,
                rotationMatrix,
                source.Size(),
                InterpolationFlags.Cubic,
                BorderTypes.Constant,
                new Scalar(255, 255, 255));

            return rotated;
        }

        private static Mat Unsharp(Mat source)
        {
            using var blurred = new Mat();
            Cv2.GaussianBlur(source, blurred, new Size(0, 0), 2.0);

            var sharpened = new Mat();
            Cv2.AddWeighted(source, 2.0, blurred, -1.0, 0.0, sharpened);

            return sharpened;
        }

        private static Mat ThresholdAndScale(Mat gray, double scale)
        {
            using var binary = new Mat();
            Cv2.Threshold(gray, binary, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

            var scaled = new Mat();
            Cv2.Resize(
                binary,
                scaled,
                new Size((int)(binary.Width * scale), (int)(binary.Height * scale)),
                0,
                0,
                InterpolationFlags.Cubic);

            var bgr = new Mat();
            Cv2.CvtColor(scaled, bgr, ColorConversionCodes.GRAY2BGR);

            scaled.Dispose();
            return bgr;
        }

        public void Dispose()
        {
            _clahe.Dispose();
            _detector.Dispose();
        }
    }

}
