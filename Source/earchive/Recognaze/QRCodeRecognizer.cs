using System;
using System.Collections.Generic;
using System.Globalization;
using Gdk;
using NLog;

namespace earchive
{
	public static class QRCodeRecognizer
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Пытается распознать QR-код с данными документа на страницах изображения.
		/// Сначала используется локальный движок WeChat, при его недоступности — ZXing.
		/// </summary>
		/// <param name="images">Страницы документа.</param>
		/// <param name="doc">Документ, в который записываются номер и дата.</param>
		public static bool TryParse(Pixbuf[] images, ref Document doc)
		{
			if (images == null)
				return false;

			for (int i = 0; i < images.Length; i++)
			{
				var image = images[i];

				if (image == null)
					continue;

				// Перебор ориентаций страницы имеет смысл в основном для первой
				// страницы: на остальных это заметно замедляет пакетное распознавание.
				foreach (var text in ReadCodes(image, i == 0))
				{
					if (TryApplyCode(text, ref doc))
						return true;
				}
			}

			return false;
		}

		private static IEnumerable<string> ReadCodes(Pixbuf image, bool tryAllOrientations)
		{
			// Локальный WeChat QR — основной способ, без обращения к хосту.
			var localCodes = WeChatQrScanner.Read(image, tryAllOrientations);

			if (localCodes != null)
			{
				if (localCodes.Count == 0)
					logger.Debug("Локальный WeChat QR не нашёл кодов на странице.");

				foreach (var localCode in localCodes)
					yield return localCode;

				yield break;
			}

			logger.Warn("Локальный WeChat QR недоступен: {0}. Пробуем ZXing.", WeChatQrScanner.LastError);

			var localResult = QRScanner.ReadQRCode(image);
			if (localResult != null && !string.IsNullOrWhiteSpace(localResult.Text))
				yield return localResult.Text;
		}

		private static bool TryApplyCode(string text, ref Document doc)
		{
			if (string.IsNullOrWhiteSpace(text))
				return false;

			var parameters = text.Trim().Split(';');
			if (parameters.Length != 3)
			{
				logger.Debug("QR-код пропущен: ожидалось 3 параметра, получено {0} ({1}).", parameters.Length, text);
				return false;
			}

			string documentType = parameters[0].Trim();
			string orderId = parameters[1].Trim();

			if (string.IsNullOrWhiteSpace(documentType) || string.IsNullOrWhiteSpace(orderId))
			{
				logger.Debug("QR-код пропущен: пустой тип документа или номер ({0}).", text);
				return false;
			}

			DateTime orderDate;
			bool validDate = DateTime.TryParseExact(
				parameters[2].Trim(),
				"ddMMyyyy",
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out orderDate);

			if (!validDate)
			{
				logger.Debug("QR-код пропущен: не удалось разобрать дату '{0}'.", parameters[2]);
				return false;
			}

			Document parsedDocument = new Document(documentType);
			if (parsedDocument == null)
				return false;

			if (doc == null)
				doc = parsedDocument;

			doc.DocNumber = orderId;
			doc.DocNumberConfidence = 1;
			doc.DocDate = orderDate;
			doc.DocDateConfidence = 1;

			logger.Info("QR-код распознан: тип '{0}', номер '{1}', дата {2:dd.MM.yyyy}.",
				documentType, orderId, orderDate);

			return true;
		}
	}
}
