using System;
using System.Xml.Serialization;
using ZXing;

namespace earchive
{

	[XmlInclude(typeof(BarCodeDateNumberRule))]
	public abstract class BarCodeRule
	{
		public RelationalRectangle Box{ get; set;}

		public abstract void ParseCode(Document doc, BarcodeFormat format, string codeText);
	}


	public class BarCodeDateNumberRule : BarCodeRule
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public override void ParseCode(Document doc, BarcodeFormat format, string codeText)
		{
			switch (format) {
			case BarcodeFormat.CODE_39:
				ParseCode39 (doc, codeText);
				break;
			case BarcodeFormat.EAN_13:
				ParseCode13 (doc, codeText);
				break;
			default:
				logger.Error ("Нет правил разбора для штрих-кода {0}.", format);
				break;
			}
		}

		private void ParseCode39(Document doc, string codeText)
		{
			if (String.IsNullOrWhiteSpace (codeText))
			{
				logger.Error ("Штрих код пустой.");
				return;
			}
				
			if(!codeText.Contains ("-"))
			{
				logger.Error ("Разделитель даты и номера не найден. Штрих код должен быть в формате {ddmmyy-n}");
				return;
			}
				
			var split = codeText.Split (new char[] {'-'}, 2);
			var dateText = split [0];
			var numText = split [1];

			if(!String.IsNullOrWhiteSpace (numText))
			{
				logger.Debug ("Номер документа: {0}", numText);
				doc.DocNumber = numText;
				doc.DocNumberConfidence = 1;
			}

			if(dateText.Length != 6)
			{
				logger.Error ("Дата должна быть в формате {ddmmyy}");
				return;
			}

			DateTime date;
			var formated = dateText.Insert (4, ".").Insert (2, ".");

			if(DateTime.TryParse (formated, out date))
			{
				logger.Debug ("Дата документа: {0:D}", date);
				doc.DocDate = date;
				doc.DocDateConfidence = 1;
			}
			else
			{
				logger.Error ("Строку {0}(до преобразования {1}) не удалось разобрать как дату.", formated, dateText);
			}
		}

		private void ParseCode13(Document doc, string codeText)
		{
			if (String.IsNullOrWhiteSpace (codeText))
			{
				logger.Error ("Штрих код пустой.");
				return;
			}

			if(codeText.Length != 13)
			{
				logger.Error ("Код имеет неправильную длинну.");
				return;
			}
				
			var dateText = codeText.Substring (0, 6);
			var numText = codeText.Substring (6, 6);

			if(!String.IsNullOrWhiteSpace (numText))
			{
				logger.Debug ("Номер документа: {0}", numText);
				doc.DocNumber = numText;
				doc.DocNumberConfidence = 1;
			}

			if(dateText.Length != 6)
			{
				logger.Error ("Дата должна быть в формате {yymmdd}");
				return;
			}
				
			int day = int.Parse (dateText.Substring (4, 2));
			int month = int.Parse (dateText.Substring (2, 2));
			int year = int.Parse (dateText.Substring (0, 2)) + 2000;

			DateTime date = new DateTime(year, month, day);

			logger.Debug ("Дата документа: {0:D}", date);
			doc.DocDate = date;
			doc.DocDateConfidence = 1;
		}
	}

}
