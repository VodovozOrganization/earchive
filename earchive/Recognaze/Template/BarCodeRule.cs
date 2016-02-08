using System;
using System.Xml.Serialization;

namespace earchive
{

	[XmlInclude(typeof(BarCodeDateNumberRule))]
	public abstract class BarCodeRule
	{
		public RelationalRectangle Box{ get; set;}

		public abstract void ParseCode(Document doc, string codeText);
	}


	public class BarCodeDateNumberRule : BarCodeRule
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public override void ParseCode(Document doc, string codeText)
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
	}

}
