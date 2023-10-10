using System;
using System.Xml.Serialization;
using Gdk;

namespace earchive
{

	public class TextMarker
	{
		public string Text;
		public bool BeginOfLine;
		public double PatternPosX;
		public double PatternPosY;
		public RelationalRectangle Zone;

		[XmlIgnore]
		public int TargetHeigth;
		[XmlIgnore]
		public int TargetWidth;

		[XmlIgnore]
		public int ActualPosX;
		[XmlIgnore]
		public int ActualPosY;
		[XmlIgnore]
		public double ActualSkew = 0;

		[XmlIgnore]
		public float Confidence;

		public int ShiftX{
			get{ return ActualPosX - (int)(PatternPosX * TargetWidth);}
		}

		public int ShiftY{
			get{ return ActualPosY - (int)(PatternPosY * TargetHeigth);}
		}

		public void SetTarget(int width, int heigth)
		{
			TargetWidth = width;
			TargetHeigth = heigth;

			Zone.SetTarget(width, heigth);
		}

	}

}
