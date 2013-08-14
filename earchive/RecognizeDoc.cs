using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using Gdk;
using Tesseract;

namespace earchive
{
	class RecognizeDoc
	{
		Document Doc;
		Pixbuf[] Images;
		public string log;
		public bool DiagnosticMode = false;
		public Gtk.Window parent;

		public RecognizeDoc (Document doc, Pixbuf[] images)
		{
			Doc = doc;
			Images = images;

		}

		public void Recognize()
		{
			using (var engine = new TesseractEngine(@"./tessdata", "rus", EngineMode.Default))
			{
				TextMarker Marker = Doc.Template.Markers[0];
				Pixbuf PixBox;

				//Вычисляем сдвиг
				Pixbuf WorkImage = Images[0];
				Marker.SetTarget(WorkImage.Width, WorkImage.Height);
				
				RelationalRectangle WorkZone = Marker.Zone.Clone();
				for (int i = 1; i <= 10; i++)
				{
					PixBox = new Pixbuf(WorkImage, WorkZone.PosX, WorkZone.PosY, WorkZone.Width, WorkZone.Heigth);
					using (var img = RecognizeHelper.PixbufToPix(PixBox))
					{
						using (var page = engine.Process(img, PageSegMode.Auto))
						{
							int Distance, MarkerPosX, MarkerPosY;
							double MarkerSkew;
							Distance = GetTextPosition(Marker.Text, page, out MarkerPosX, out MarkerPosY, out MarkerSkew);
							if (Distance < 5)
							{
								AddToLog(String.Format("TextMarker <{0}> Distance: {1} Try:{2}", Marker.Text, Distance, i));
								Marker.ActualPosX = MarkerPosX + WorkZone.PosX;
								Marker.ActualPosY = MarkerPosY + WorkZone.PosY;
								Marker.ActualSkew = MarkerSkew;
								AddToLog(String.Format("Image Shift X: {0}", Marker.ShiftX));
								AddToLog(String.Format("Image Shift Y: {0}", Marker.ShiftY));
								break;
							}
							else if (i == 10)
							{
								Marker.ActualPosX = (int)(Marker.PatternPosX * Marker.TargetWidth);
								Marker.ActualPosY = (int)(Marker.PatternPosY * Marker.TargetHeigth);
								Marker.ActualSkew = 0;
								ShowImage(PixBox, "Маркер не найден. Зона маркера.");
							}
							else
							{ //Увеличиваем размер зоны поиска.
								WorkZone.RelativePosX -= WorkZone.RelativeWidth * 0.1;
								if (WorkZone.RelativePosX < 0)
									WorkZone.RelativePosX = 0;
								WorkZone.RelativePosY -= WorkZone.RelativeHeigth * 0.1;
								if (WorkZone.RelativePosY < 0)
									WorkZone.RelativePosY = 0;
								WorkZone.RelativeWidth *= 1.2;
								WorkZone.RelativeHeigth *= 1.2;

							}
						}
					}
				}
				RecognazeRule CurRule;
				//Распознаем номер
				if (Doc.Template.NumberRule != null)
				{
					CurRule = Doc.Template.NumberRule;

					CurRule.Box.SetShiftByMarker(Marker);
					AddToLog(String.Format("Number Shift X: {0}", CurRule.Box.ShiftX));
					AddToLog(String.Format("Number Shift Y: {0}", CurRule.Box.ShiftY));
					WorkZone = CurRule.Box.Clone();
					//FIXME Для теста
					WorkZone.RelativePosX += WorkZone.RelativeWidth * 0.2;
					WorkZone.RelativePosY += WorkZone.RelativeHeigth * 0.2;
					WorkZone.RelativeWidth *= 0.6;
					WorkZone.RelativeHeigth *= 0.6;
					
					Doc.DocNumberConfidence = 0;
					for (int i = 1; i <= 5; i++)
					{
						PixBox = new Pixbuf(WorkImage, WorkZone.PosX, WorkZone.PosY, WorkZone.Width, WorkZone.Heigth);
						using (var img = RecognizeHelper.PixbufToPix(PixBox))
						{
							using (var page = engine.Process(img, PageSegMode.SingleLine))
							{
	
								string FieldText = page.GetText().Trim();
								if (page.GetMeanConfidence() > Doc.DocNumberConfidence)
								{
									Doc.DocNumber = FieldText;
									Doc.DocNumberConfidence = page.GetMeanConfidence();
									AddToLog(String.Format("Try: {0}", i));
									AddToLog(String.Format("Found Field Value: {0}", FieldText));
									AddToLog(String.Format("Recognize confidence: {0}", page.GetMeanConfidence()));
									if (FieldText == "")
										ShowImage(PixBox, "Номер пустой. Зона номера документа.");
									else
										ShowImage(PixBox, "Зона номера документа.");
								}
								if (Doc.DocNumberConfidence > 0.8)
								{
									break;
								}
								else
								{//Увеличиваем размер зоны поиска.
									WorkZone.RelativePosX -= WorkZone.RelativeWidth * 0.1;
									if (WorkZone.RelativePosX < 0)
										WorkZone.RelativePosX = 0;
									WorkZone.RelativePosY -= WorkZone.RelativeHeigth * 0.1;
									if (WorkZone.RelativePosY < 0)
										WorkZone.RelativePosY = 0;
									WorkZone.RelativeWidth *= 1.2;
									WorkZone.RelativeHeigth *= 1.2;
								}
							}
						}
					}
				}

				//Распознаем Дату
				if(Doc.Template.DateRule != null)
				{
					CurRule = Doc.Template.DateRule;

					CurRule.Box.SetShiftByMarker(Marker);

					PixBox = new Pixbuf(WorkImage, CurRule.Box.PosX, CurRule.Box.PosY, CurRule.Box.Width, CurRule.Box.Heigth);
					using (var img = RecognizeHelper.PixbufToPix(PixBox)) {
						using (var page = engine.Process(img, PageSegMode.SingleLine)) {

							string FieldText = page.GetText().Trim();
							DateTime TempDate;
							string[] words = FieldText.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
							string Date = "";
							foreach(string word in words)
							{
								if(DateTime.TryParse(word, out TempDate))
								   Date = word;
							}

							if(DateTime.TryParse(Date, out TempDate))
							{
								Doc.DocDate = TempDate;
								Doc.DocDateConfidence = page.GetMeanConfidence();
							}
							else
							{
								Doc.DocDateConfidence = -2;
							}
							AddToLog(String.Format("Found Field Value: {0}", FieldText));
							AddToLog(String.Format("Split date Value: {0}", Date));
							AddToLog(String.Format("Recognize confidence: {0}", page.GetMeanConfidence()));
							if(FieldText == "")
								ShowImage(PixBox, "Дата пустая. Зона даты документа.");
							else
								ShowImage(PixBox, "Зона даты документа.");
						}
					}
				}

				//FIXME Добавить распознование дополнительных полей.
			}
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

		int GetTextPosition(string Text, Page page, out int PosX, out int PosY, out double AngleRad)
		{
			int BestDistance = 10000;
			PosX = -1;
			PosY = -1;
			AngleRad = 0;
			AddToLog ("Marker zone text:");
			AddToLog (page.GetText());
			ResultIterator LineIter = page.GetIterator();
			string[] Words = Text.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
			int NumberOfWords = Words.Length;
			LineIter.Begin();
			do
			{
				int CurrentWordNumber = -1;
				int CurrentBestDistance = 10000;
				string Line = LineIter.GetText(PageIteratorLevel.TextLine);

				if(Line == null || Line == "")
					continue;
				Line = Line.Trim();
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
						CurrentWordNumber = shift;
					}
				}
				if(CurrentBestDistance < BestDistance)
				{
					AddToLog ("new best");
					AddToLog (LineIter.GetText(PageIteratorLevel.TextLine).Trim());
					BestDistance = CurrentBestDistance;
					for(int i = 0; i < CurrentWordNumber; i++)
					{
						LineIter.Next(PageIteratorLevel.Word);
					}
					Rect Box;
					LineIter.TryGetBoundingBox(PageIteratorLevel.Word, out Box);
					PosX = Box.X1;
					PosY = Box.Y1;
					AddToLog (String.Format("Position X1:{0} Y1:{1} X2:{2} Y2:{3}", Box.X1, Box.Y1, Box.X2, Box.Y2));
					LineIter.TryGetBaseline(PageIteratorLevel.Word, out Box);
					AddToLog (String.Format("BaseLine X1:{0} Y1:{1} X2:{2} Y2:{3}", Box.X1, Box.Y1, Box.X2, Box.Y2));
					AngleRad = Math.Atan2(Box.Y2 - Box.Y1, Box.X2 - Box.X1); //угл наклона базовой линии.
					double AngleGrad = AngleRad * (180/Math.PI);
					AddToLog (String.Format("Angle rad:{0} grad:{1}", AngleRad, AngleGrad));
				}

			} while( LineIter.Next(PageIteratorLevel.TextLine));
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
			AddToLog(title);
		}

		void AddToLog(string str)
		{
			log += str + "\n";
			Console.WriteLine (str);
		}

	}
}

