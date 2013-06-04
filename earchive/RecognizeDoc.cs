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

				int ShiftX = 0;
				int ShiftY = 0;
				//Вычисляем сдвиг
				Pixbuf WorkImage = Images[0];
				Marker.Zone.SetTarget(WorkImage.Width, WorkImage.Height);

				Pixbuf PixBox = new Pixbuf(WorkImage, Marker.Zone.PosX, Marker.Zone.PosY, Marker.Zone.Width, Marker.Zone.Heigth);
				using (var img = PixbufToPix(PixBox)) {
					using (var page = engine.Process(img)) {

						int MarkerPosX, MarkerPosY;
						int Distance = GetTextPosition(Marker.Text, page, out MarkerPosX, out MarkerPosY);
						AddToLog(String.Format("TextMarker <{0}> Distance: {1}", Marker.Text, Distance));
						if(Distance < 5)
						{
							MarkerPosX = MarkerPosX + Marker.Zone.PosX;
							MarkerPosY = MarkerPosY + Marker.Zone.PosY;
							ShiftX = MarkerPosX - (int)(Marker.PatternPosX * WorkImage.Width);
							ShiftY = MarkerPosY - (int)(Marker.PatternPosY * WorkImage.Height);
						}
						else
						{
							ShowImage(PixBox, "Маркер не найден. Зона маркера.");
						}
					}
				}

				RecognazeRule CurRule;
				//Распознаем номер
				if(Doc.Template.NumberRule != null)
				{
					CurRule = Doc.Template.NumberRule;

					CurRule.Box.SetTarget(WorkImage.Width, WorkImage.Height);
					CurRule.Box.SetShift(ShiftX, ShiftY);

					PixBox = new Pixbuf(WorkImage, CurRule.Box.PosX, CurRule.Box.PosY, CurRule.Box.Width, CurRule.Box.Heigth);
					using (var img = PixbufToPix(PixBox)) {
						using (var page = engine.Process(img)) {

							string FieldText = page.GetText();
							Doc.DocNumber = FieldText;
							Doc.DocNumberConfidence = page.GetMeanConfidence();
							AddToLog(String.Format("Found Field Value: {0}", FieldText));
							AddToLog(String.Format("Recognize confidence: {0}", page.GetMeanConfidence()));
						}
					}
				}

				//Распознаем Дату
				if(Doc.Template.DateRule != null)
				{
					CurRule = Doc.Template.DateRule;

					CurRule.Box.SetTarget(WorkImage.Width, WorkImage.Height);
					CurRule.Box.SetShift(ShiftX, ShiftY);

					PixBox = new Pixbuf(WorkImage, CurRule.Box.PosX, CurRule.Box.PosY, CurRule.Box.Width, CurRule.Box.Heigth);
					using (var img = PixbufToPix(PixBox)) {
						using (var page = engine.Process(img)) {

							string FieldText = page.GetText();
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

		int GetTextPosition(string Text, Page page, out int PosX, out int PosY)
		{
			int BestDistance = 10000;
			PosX = -1;
			PosY = -1;
			ResultIterator LineIter = page.GetIterator();
			string[] Words = Text.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
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
						CurrentWordNumber = shift;
					}
				}
				if(CurrentBestDistance < BestDistance)
				{
					AddToLog ("new best");
					AddToLog (LineIter.GetText(PageIteratorLevel.TextLine));
					BestDistance = CurrentBestDistance;
					for(int i = 0; i < CurrentWordNumber; i++)
					{
						LineIter.Next(PageIteratorLevel.Word);
					}
					Rect Box;
					LineIter.TryGetBoundingBox(PageIteratorLevel.Word, out Box);
					PosX = Box.X1;
					PosY = Box.Y1;
				}
			} while( LineIter.Next(PageIteratorLevel.TextLine));
			LineIter.Dispose();
			return BestDistance;
		}

		void ShowImage(Pixbuf img, string title)
		{
			Gtk.Dialog Win = new Gtk.Dialog(title, parent, Gtk.DialogFlags.Modal, Gtk.ButtonsType.Ok);
			Gtk.Image Image = new Gtk.Image(img);
			Win.VBox.Add(Image);
			Win.ShowAll();
			Win.Run();
			Image.Pixbuf = null;
			Win.Destroy();
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

