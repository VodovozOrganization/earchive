using System;
using System.Drawing;
using System.IO;
using Tesseract;
using Gdk;

namespace earchive
{
	public class RecognizeHelper
	{
		public RecognizeHelper ()
		{
		}

		public static Pix PixbufToPix(Pixbuf image)
		{
			MemoryStream  stream = new MemoryStream(image.SaveToBuffer("png"));
			stream.Position = 0;

			Bitmap sysimage = new Bitmap(stream);
			stream.Close();

			return PixConverter.ToPix(sysimage);
		}

		public static Bitmap PixbufToBitmap(Pixbuf image)
		{
			MemoryStream  stream = new MemoryStream(image.SaveToBuffer("png"));
			stream.Position = 0;

			Bitmap sysimage = new Bitmap(stream);
			stream.Close();

			return sysimage;
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

