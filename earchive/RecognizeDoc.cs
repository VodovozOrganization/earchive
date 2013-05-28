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

		public RecognizeDoc (Document doc, Pixbuf[] images)
		{
			Doc = doc;
			Images = images;

		}

		public void Recognize()
		{

			using (var engine = new TesseractEngine(@"./tessdata", "rus", EngineMode.Default)) 
			{
				using (var img = PixbufToPix(Images[0])) {
					using (var page = engine.Process(img)) {
						RecognazeRule testrule = new RecognazeRule();
						testrule.TextMarker = "ТОВАРНАЯ НАКЛАДНАЯ";
						testrule.ShiftWordsCount = 1;

						ResultIterator iter;
						int Word;
						int Distance = LookingTextMarker(testrule, page, out iter, out Word);
						AddToLog(String.Format("TextMarker <{0}> Distance: {1}", testrule.TextMarker, Distance));
						if(Distance < 5)
						{
							int TextMarkerWordsCount = (testrule.TextMarker.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries)).Length;
							for(int num = 1; num < Word + TextMarkerWordsCount - 1 + testrule.ShiftWordsCount; num++)
							{
								iter.Next(PageIteratorLevel.Word);
							}
							AddToLog(String.Format("Found Field Value: {0}", iter.GetText(PageIteratorLevel.Word)));
							AddToLog(String.Format("Recognize confidence: {0}", iter.GetConfidence(PageIteratorLevel.Word)));
						}
						iter.Dispose();
					}
				}
			}
		}

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
					word = CurrentWordNumber;
					if(BestLineIter != null)
						BestLineIter.Dispose();
					BestLineIter = LineIter.Clone();
					BestDistance = CurrentBestDistance;
				}
			} while( LineIter.Next(PageIteratorLevel.TextLine));
			LineIter.Dispose();
			return BestDistance;
		}

		void AddToLog(string str)
		{
			log += str + "\n";
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

