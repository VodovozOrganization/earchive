using System;
using System.Drawing;
using System.Linq;
using Gdk;
using NLog;
using Tesseract;
using ZXing;

namespace earchive
{
	class RecognizeDoc
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		Document Doc;
		Pixbuf[] Images;
		public bool DiagnosticMode = false;
		public Gtk.Window parent;

		public RecognizeDoc (Document doc, Pixbuf[] images)
		{
			Doc = doc;
			Images = images;

		}

		public void Recognize()
		{
			if (Doc.Template.UseBarCode)
			{
				foreach (var rule in Doc.Template.BarCodeRules)
					RecognizeBarCode (rule);
			}

			if (Doc.Template.UseTess)
				RecognizeTess ();
		}

		private void RecognizeTess()
		{
			using (var engine = new TesseractEngine(@"./tessdata", "rus", EngineMode.Default))
			{
				TextMarker Marker = Doc.Template.Markers[0];
				Pixbuf PixBox;

				logger.Info("Вычисляем сдвиг");
				Pixbuf WorkImage = Images[0];
				Marker.SetTarget(WorkImage.Width, WorkImage.Height);
				
				RelationalRectangle WorkZone = Marker.Zone.Clone();
				for (int i = 1; i <= 7; i++)
				{
					logger.Debug("Попытка {0}, box: x={1},y={2},w={3},h={4}", i, WorkZone.PosX, WorkZone.PosY, WorkZone.Width, WorkZone.Heigth);
					PixBox = new Pixbuf(WorkImage, WorkZone.PosX, WorkZone.PosY, WorkZone.Width, WorkZone.Heigth);
					using (var img = RecognizeHelper.PixbufToPix(PixBox))
					{
						using (var page = engine.Process(img, PageSegMode.Auto))
						{
							int Distance, MarkerPosX, MarkerPosY;
							double MarkerSkew;
							RecognazeRule[] rules = new RecognazeRule[]{ Doc.Template.NumberRule, Doc.Template.DateRule};
							Distance = GetTextPosition(Marker.Text, page, out MarkerPosX, out MarkerPosY, out MarkerSkew, rules);
							if (Distance < 5)
							{
								logger.Debug("TextMarker <{0}> Distance: {1} Try:{2}", Marker.Text, Distance, i);
								Marker.ActualPosX = MarkerPosX + WorkZone.PosX;
								Marker.ActualPosY = MarkerPosY + WorkZone.PosY;
								Marker.ActualSkew = MarkerSkew;
								Marker.Confidence = page.GetMeanConfidence ();
								logger.Debug("Image Shift X: {0}", Marker.ShiftX);
								logger.Debug("Image Shift Y: {0}", Marker.ShiftY);
								break;
							}
							else if (i == 7 || 
								(WorkZone.RelativePosX == 0 && WorkZone.RelativePosY == 0 && WorkZone.RelativeHeigth == 1 && WorkZone.RelativeWidth == 1) )
							{
								Marker.ActualPosX = (int)(Marker.PatternPosX * Marker.TargetWidth);
								Marker.ActualPosY = (int)(Marker.PatternPosY * Marker.TargetHeigth);
								Marker.ActualSkew = 0;
								ShowImage(PixBox, "Маркер не найден. Зона маркера.");
							}
							else
							{ //Увеличиваем размер зоны поиска.
								ExpandRelativeZone(WorkZone);
							}
						}
					}
				}
				RecognazeRule CurRule;

				logger.Info("Распознаем номер");
				if (Doc.Template.NumberRule != null)
				{
					CurRule = Doc.Template.NumberRule;

					if(CurRule.Box != null)
					{
						CurRule.Box.SetShiftByMarker(Marker);
						logger.Debug("Number Shift X: {0}", CurRule.Box.ShiftX);
						logger.Debug("Number Shift Y: {0}", CurRule.Box.ShiftY);
						WorkZone = CurRule.Box.Clone();
						//FIXME С этим пока работает лучше
						WorkZone.RelativePosX += WorkZone.RelativeWidth * 0.2;
						WorkZone.RelativePosY += WorkZone.RelativeHeigth * 0.2;
						WorkZone.RelativeWidth *= 0.6;
						WorkZone.RelativeHeigth *= 0.6;
						
						Doc.DocNumberConfidence = 0;
						for (int i = 1; i <= 5; i++) {
							PixBox = new Pixbuf (WorkImage, WorkZone.PosX, WorkZone.PosY, WorkZone.Width, WorkZone.Heigth);
							using (var img = RecognizeHelper.PixbufToPix (PixBox)) {
								using (var page = engine.Process (img, PageSegMode.SingleLine)) {

									string FieldText = page.GetText ().Trim ();
									if (page.GetMeanConfidence () > Doc.DocNumberConfidence) {
										Doc.DocNumber = FieldText;
										Doc.DocNumberConfidence = page.GetMeanConfidence ();
										logger.Debug ("Try: {0}", i);
										logger.Debug ("Found Field Value: {0}", FieldText);
										logger.Debug ("Recognize confidence: {0}", page.GetMeanConfidence ());
										if (FieldText == "")
											ShowImage (PixBox, "Номер пустой. Зона номера документа.");
										else
											ShowImage (PixBox, "Зона номера документа.");
									}
									if (CurRule.Validate == ValidationTypes.IsNumber) {
										int tempInt;
										bool BoxIsNumber = int.TryParse (Doc.DocNumber, out tempInt);
										if (BoxIsNumber) {
											break;
										} else {
											Doc.DocNumberConfidence = 0;
										}
									}
									if (Doc.DocNumberConfidence > 0.8) {
										break;
									} else {//Увеличиваем размер зоны поиска.
										ExpandRelativeZone (WorkZone);
									}
								}
							}
						}
					}
					else if (CurRule.NextAfterTextMarker)
					{
						logger.Debug ("By after market detection: {0}", CurRule.AfterTextMarkerValue);
						Doc.DocNumber = CurRule.AfterTextMarkerValue;
						Doc.DocNumberConfidence = CurRule.AfterTextMarkerConfidence;
					}
				}

				logger.Info("Распознаем Дату");
				if(Doc.Template.DateRule != null)
				{
					CurRule = Doc.Template.DateRule;
					if (CurRule.Box != null) {
						CurRule.Box.SetShiftByMarker (Marker);

						PixBox = new Pixbuf (WorkImage, CurRule.Box.PosX, CurRule.Box.PosY, CurRule.Box.Width, CurRule.Box.Heigth);
						using (var img = RecognizeHelper.PixbufToPix (PixBox)) {
							using (var page = engine.Process (img, PageSegMode.SingleLine)) {

								string FieldText = page.GetText ().Trim ();
								DateTime TempDate;
								string [] words = FieldText.Split (new char [] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
								string Date = "";
								foreach (string word in words) {
									if (DateTime.TryParse (word, out TempDate))
										Date = word;
								}

								if (DateTime.TryParse (Date, out TempDate)) {
									Doc.DocDate = TempDate;
									Doc.DocDateConfidence = page.GetMeanConfidence ();
								} else {
									Doc.DocDateConfidence = -2;
								}
								logger.Debug ("Found Field Value: {0}", FieldText);
								logger.Debug ("Split date Value: {0}", Date);
								logger.Debug ("Recognize confidence: {0}", page.GetMeanConfidence ());
								if (FieldText == "")
									ShowImage (PixBox, "Дата пустая. Зона даты документа.");
								else
									ShowImage (PixBox, "Зона даты документа.");
							}
						}
					}
					else
					{
						logger.Debug ("By after market detection: {0}", CurRule.AfterTextMarkerValue);
						if (DateTime.TryParse (CurRule.AfterTextMarkerValue, out Doc.DocDate)) {
							logger.Debug ("Date parsed:{0}", Doc.DocDate);
							Doc.DocDateConfidence = CurRule.AfterTextMarkerConfidence;
						} else
							Doc.DocDateConfidence = -2;
					}
				}

				//FIXME Добавить распознование дополнительных полей.
			}
		}

		private void RecognizeBarCode (BarCodeRule rule)
		{
			Pixbuf WorkImage = Images[0];

			RelationalRectangle WorkZone =  rule.Box.Clone();

			WorkZone.SetTarget(WorkImage.Width, WorkImage.Height);

			logger.Debug("Зона штрихкода, box: x={0},y={1},w={2},h={3}", WorkZone.PosX, WorkZone.PosY, WorkZone.Width, WorkZone.Heigth);
			var cutedPixbuf = new Pixbuf(WorkImage, WorkZone.PosX, WorkZone.PosY, WorkZone.Width, WorkZone.Heigth);

			ShowImage (cutedPixbuf, "Зона штрихкода");

			// create a barcode reader instance
			IBarcodeReader reader = new BarcodeReader();
			// load a bitmap
			var barcodeBitmap = RecognizeHelper.PixbufToBitmap (cutedPixbuf);
			// detect and decode the barcode inside the bitmap
			var result = reader.Decode(barcodeBitmap);
			// do something with the result
			if (result != null) {
				logger.Debug ("Формат штрих кода: {0}", result.BarcodeFormat.ToString ());
				logger.Debug ("Текст штрих кода разпознан как: {0}", result.Text);
				rule.ParseCode (Doc, result.BarcodeFormat, result.Text);
			} else
				logger.Warn ("Штрих код не распознан.");
		}

		internal void ExpandRelativeZone(RelationalRectangle zone)
		{
			zone.RelativePosX -= zone.RelativeWidth * 0.1;
			if (zone.RelativePosX < 0)
				zone.RelativePosX = 0;
			zone.RelativePosY -= zone.RelativeHeigth * 0.1;
			if (zone.RelativePosY < 0)
				zone.RelativePosY = 0;
			zone.RelativeWidth *= 1.2;
			if (zone.RelativeWidth > 1)
				zone.RelativeWidth = 1;
			zone.RelativeHeigth *= 1.2;
			if (zone.RelativeHeigth > 1)
				zone.RelativeHeigth = 1;
		}

		/*
		int LookingTextMarker(RecognazeRule Rule, Page page, out ResultIterator BestLineIter, out int word)
		{
			word = -1;
			BestLineIter = null;
			int BestDistance = 10000;

			ResultIterator LineIter = page.GetIterator();
			string[] Words = Rule.TextMarker.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
			int NumberOfWords = Words.Length;
			LineIter.Begin();
			do
			{
				int CurrentWordNumber = -1;
				int CurrentBestDistance = 10000;
				string Line = LineIter.GetText(PageIteratorLevel.TextLine);
				if(Line == null)
					continue;
				string[] WordsOfLine = Line.Split(new char[] {' '}, StringSplitOptions.None);
				if(WordsOfLine.Length < NumberOfWords)
					continue;

				for(int shift = 0; shift <= WordsOfLine.Length - NumberOfWords; shift++)
				{
					int PassDistance = 0;
					for(int i = 0; i < NumberOfWords; i++)
					{
						PassDistance += FuzzyStringComparer.GetDistanceLevenshtein(WordsOfLine[shift + i],
						                                                              Words[i],
						                                                              StringComparison.CurrentCultureIgnoreCase);
					}
					if(PassDistance < CurrentBestDistance)
					{
						CurrentBestDistance = PassDistance;
						CurrentWordNumber = shift + 1;
					}
				}
				if(CurrentBestDistance < BestDistance)
				{
					AddToLog ("new best");
					AddToLog (LineIter.GetText(PageIteratorLevel.Word));
					word = CurrentWordNumber;
					if(BestLineIter != null)
						BestLineIter.Dispose();
					BestLineIter = LineIter.Clone();
					AddToLog (BestLineIter.GetText(PageIteratorLevel.TextLine));
					BestDistance = CurrentBestDistance;
				}
			} while( LineIter.Next(PageIteratorLevel.TextLine));
			LineIter.Dispose();
			return BestDistance;
		} */

		int GetTextPosition(string Text, Page page, out int PosX, out int PosY, out double AngleRad, RecognazeRule[] AfterMarkerRules)
		{
			int BestDistance = 10000;
			PosX = -1;
			PosY = -1;
			AngleRad = 0;
			logger.Debug("Marker zone text:{0}", page.GetText());
			ResultIterator LineIter = page.GetIterator();
			string[] Words = Text.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
			int NumberOfWords = Words.Length;
			LineIter.Begin();
			do
			{
				int CurrentWordNumber = -1;
				int CurrentAfterWord = 0;
				int CurrentBestDistance = 10000;
				string Line = LineIter.GetText(PageIteratorLevel.TextLine);

				if(Line == null || Line == "")
					continue;
				Line = Line.Trim();
				string[] WordsOfLine = Line.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);

				if(WordsOfLine.Length == 0)
					continue;
				for(int shift = 0; shift < WordsOfLine.Length; shift++)
				{
					for(int i = 1; i <= NumberOfWords && i <= WordsOfLine.Length - shift; i++)
					{
						string passString = String.Join(" ", WordsOfLine, shift, i);

						int PassDistance = FuzzyStringComparer.GetDistanceLevenshtein(passString,
							Text,
						    StringComparison.CurrentCultureIgnoreCase);
						if(PassDistance < CurrentBestDistance)
						{
							CurrentBestDistance = PassDistance;
							CurrentWordNumber = shift;
							CurrentAfterWord = shift + i;
						}
					}
				}
				if(CurrentBestDistance < BestDistance)
				{
					logger.Debug("new best");
					logger.Debug(LineIter.GetText(PageIteratorLevel.TextLine).Trim());
					//Заполняем поля данными после маркера.
					foreach(RecognazeRule rule in AfterMarkerRules)
					{
						if(rule.NextAfterTextMarker && WordsOfLine.Length > CurrentAfterWord + rule.ShiftWordsCount)
						{
							rule.AfterTextMarkerValue = WordsOfLine[CurrentAfterWord + rule.ShiftWordsCount];
						}
					}

					BestDistance = CurrentBestDistance;
					for(int i = 0; i < CurrentWordNumber; i++)
					{
						LineIter.Next(PageIteratorLevel.Word);
					}
					Rect Box;
					LineIter.TryGetBoundingBox(PageIteratorLevel.Word, out Box);
					PosX = Box.X1;
					PosY = Box.Y1;
					logger.Debug("Position X1:{0} Y1:{1} X2:{2} Y2:{3}", Box.X1, Box.Y1, Box.X2, Box.Y2);
					LineIter.TryGetBaseline(PageIteratorLevel.Word, out Box);
					logger.Debug("BaseLine X1:{0} Y1:{1} X2:{2} Y2:{3}", Box.X1, Box.Y1, Box.X2, Box.Y2);
					AngleRad = Math.Atan2(Box.Y2 - Box.Y1, Box.X2 - Box.X1); //угл наклона базовой линии.
					double AngleGrad = AngleRad * (180/Math.PI);
					logger.Debug("Angle rad:{0} grad:{1}", AngleRad, AngleGrad);

					//Получаем уровень распознования полей в маркере.
					int iterAlreadyShifted = CurrentWordNumber - CurrentAfterWord;
					bool stopIteration = false;
					foreach (RecognazeRule rule in AfterMarkerRules.Where(x => x.NextAfterTextMarker).OrderBy(x => x.ShiftWordsCount)) 
					{
						while(iterAlreadyShifted < rule.ShiftWordsCount)
						{
							if(LineIter.IsAtFinalOf (PageIteratorLevel.TextLine, PageIteratorLevel.Word))
							{
								stopIteration = true;
								break;
							}
							LineIter.Next (PageIteratorLevel.Word);
							iterAlreadyShifted++;
						}
						if (stopIteration)
							break;
						rule.AfterTextMarkerConfidence = LineIter.GetConfidence (PageIteratorLevel.Word);
						logger.Debug ("Cлово {0} со сдвигом {1} имеет точность {2}.", LineIter.GetText (PageIteratorLevel.Word), rule.ShiftWordsCount, rule.AfterTextMarkerConfidence);
					}
				}
			} 
			while( LineIter.Next(PageIteratorLevel.TextLine));
			LineIter.Dispose();
			return BestDistance;
		}

		void ShowImage(Pixbuf img, string title)
		{
			if(DiagnosticMode)
			{
				Gtk.Dialog Win = new Gtk.Dialog(title, parent, Gtk.DialogFlags.Modal, Gtk.ButtonsType.Ok);
				Gtk.Image Image = new Gtk.Image(img);
				Win.VBox.Add(Image);
				Win.ShowAll();
				Win.Run();
				Image.Pixbuf = null;
				Win.Destroy();
			}
			logger.Info(title);
		}
	}
}

