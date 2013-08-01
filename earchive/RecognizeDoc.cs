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

				//Вычисляем сдвиг
				Pixbuf WorkImage = Images[0];
				Marker.SetTarget(WorkImage.Width, WorkImage.Height);

				Pixbuf PixBox = new Pixbuf(WorkImage, Marker.Zone.PosX, Marker.Zone.PosY, Marker.Zone.Width, Marker.Zone.Heigth);
				using (var img = PixbufToPix(PixBox)) {
					using (var page = engine.Process(img, PageSegMode.Count)) {

						int MarkerPosX, MarkerPosY;
						double MarkerSkew;
						int Distance = GetTextPosition(Marker.Text, page, out MarkerPosX, out MarkerPosY, out MarkerSkew);
						AddToLog(String.Format("TextMarker <{0}> Distance: {1}", Marker.Text, Distance));
						if(Distance < 5)
						{
							Marker.ActualPosX = MarkerPosX + Marker.Zone.PosX;
							Marker.ActualPosY = MarkerPosY + Marker.Zone.PosY;
							Marker.ActualSkew = MarkerSkew;
							AddToLog(String.Format("Image Shift X: {0}", Marker.ShiftX));
							AddToLog(String.Format("Image Shift Y: {0}", Marker.ShiftY));
						}
						else
						{
							Marker.ActualPosX = (int)(Marker.PatternPosX * Marker.TargetWidth);
							Marker.ActualPosY = (int)(Marker.PatternPosY * Marker.TargetHeigth);
							Marker.ActualSkew = 0;
							ShowImage(PixBox, "Маркер не найден. Зона маркера.");
						}
					}
				}

				RecognazeRule CurRule;
				//Распознаем номер
				if(Doc.Template.NumberRule != null)
				{
					CurRule = Doc.Template.NumberRule;

					CurRule.Box.SetShiftByMarker(Marker);
					AddToLog(String.Format("Number Shift X: {0}", CurRule.Box.ShiftX));
					AddToLog(String.Format("Number Shift Y: {0}", CurRule.Box.ShiftY));

					PixBox = new Pixbuf(WorkImage, CurRule.Box.PosX, CurRule.Box.PosY, CurRule.Box.Width, CurRule.Box.Heigth);
					using (var img = PixbufToPix(PixBox)) {
						using (var page = engine.Process(img, PageSegMode.SingleLine)) {

							string FieldText = page.GetText().Trim();
							Doc.DocNumber = FieldText;
							Doc.DocNumberConfidence = page.GetMeanConfidence();
							AddToLog(String.Format("Found Field Value: {0}", FieldText));
							AddToLog(String.Format("Recognize confidence: {0}", page.GetMeanConfidence()));
							if(FieldText == "")
								ShowImage(PixBox, "Номер пустой. Зона номера документа.");
							else
								ShowImage(PixBox, "Зона номера документа.");
						}
					}
				}

				//Распознаем Дату
				if(Doc.Template.DateRule != null)
				{
					CurRule = Doc.Template.DateRule;

					CurRule.Box.SetShiftByMarker(Marker);

					PixBox = new Pixbuf(WorkImage, CurRule.Box.PosX, CurRule.Box.PosY, CurRule.Box.Width, CurRule.Box.Heigth);
					using (var img = PixbufToPix(PixBox)) {
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

		private Pix PixbufToPix(Pixbuf image)
		{
			MemoryStream  stream = new MemoryStream(image.SaveToBuffer("png"));
			stream.Position = 0;

			Bitmap sysimage = new Bitmap(stream);
			stream.Close();

			return PixConverter.ToPix(sysimage);
		}

	}

	public static class FuzzyStringComparer
	{
    	/// <summary>
	   	/// Calculates distance between <paramref name="source"/> and <paramref name="target"/>
	    /// required for convertion from the first to the second using Needleman-Wunch algorithm.
		/// </summary>
		/// <param name="source">Source string</param>
		/// <param name="target">Target string</param>
		/// <param name="gap">Weight factor for insertion and deletion operations.</param>
        /// <param name="comparisonType">Comparison type</param>
		/// <returns></returns>
		public static int GetDistance(string source, string target, int gap, StringComparison comparisonType = StringComparison.CurrentCulture)
	    {
        	if (source == null)
            	throw new ArgumentNullException("source");
            if (target == null)
				throw new ArgumentNullException("target");
			if (source == target)
				return 0;

	        if (source == string.Empty)
				return target.Length * gap;
			if (target == string.Empty)
				return source.Length * gap;

	        var matrix = new int[source.Length + 1, target.Length + 1];
	        int sourceUpperBound = matrix.GetUpperBound(0);
			int targetUpperBound = matrix.GetUpperBound(1);

	        for (int i = 0; i <= sourceUpperBound; i++)
				matrix[i, 0] = i;

			for (int j = 0; j <= targetUpperBound; j++)
				matrix[0, j] = j;

	        for (int i = 1; i <= sourceUpperBound; i++)
		        for (int j = 1; j <= targetUpperBound; j++)
				{
					int cost = string.Equals(source[i - 1].ToString(), target[j - 1].ToString(), comparisonType) ? 0 : 1;

		            matrix[i, j] = Math.Min(
						   Math.Min(
						            matrix[i - 1, j] + gap, // deletion
					                matrix[i, j - 1] + gap), // insertion
					       matrix[i - 1, j - 1] + cost); // substitution
				}
            return matrix[sourceUpperBound, targetUpperBound];
		}

        /// <summary>
        /// Calculates distance between <paramref name="source"/> and <paramref name="target"/>
        /// required for convertion from the first to the second using Levenshtein algorithm.
		/// </summary>
		/// <param name="source">Source string</param>
		/// <param name="target">Target string</param>
		/// <param name="comparisonType">Comparison type</param>
		/// <returns></returns>
        public static int GetDistanceLevenshtein(string source, string target, StringComparison comparisonType = StringComparison.CurrentCulture)
		{
			return GetDistance(source, target, 1, comparisonType);
		}

     }
}

