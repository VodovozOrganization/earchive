using System;
using System.Xml.Serialization;
using Gdk;

namespace earchive
{
	public class RecognazeRule
	{
		public int FieldId;

		public bool NextAfterTextMarker;
		public int ShiftWordsCount;

		public RelationalRectangle Box;
	}

	public class TextMarker
	{
		public string Text;
		public bool BeginOfLine;
		public double PatternPosX;
		public double PatternPosY;
		public RelationalRectangle Zone;
	}

	public class RelationalRectangle
	{
		public double RelativePosX;
		public double RelativePosY;
		public double RelativeWidth;
		public double RelativeHeigth;

		[XmlIgnore]
		public int TargetHeigth;
		[XmlIgnore]
		public int TargetWidth;

		[XmlIgnore]
		public int ShiftX;
		[XmlIgnore]
		public int ShiftY;

		public RelationalRectangle()
		{
			RelativePosX = 0;
			RelativePosY = 0;
			RelativeWidth = 0;
			RelativeHeigth = 0;

			ShiftX = 0;
			ShiftY = 0;

			TargetWidth = 0;
			TargetHeigth = 0;
		}

		public RelationalRectangle(double RPosX, double RPosY, double RWidth, double RHeigth)
		{
			RelativePosX = RPosX;
			RelativePosY = RPosY;
			RelativeWidth = RWidth;
			RelativeHeigth = RHeigth;

			ShiftX = 0;
			ShiftY = 0;

			TargetWidth = 0;
			TargetHeigth = 0;
		}

		public void SetTarget(int width, int heigth)
		{
			TargetWidth = width;
			TargetHeigth = heigth;
		}

		public void SetShift(int shiftx, int shifty)
		{
			ShiftX = shiftx;
			ShiftY = shifty;
		}

		public void SetShiftByPosition(int X, int Y)
		{
			ShiftX = X - (int)(RelativePosX * TargetWidth);
			ShiftY = Y - (int)(RelativePosY * TargetHeigth);
		}

		public int PosX{
			get{ return (int)(RelativePosX * TargetWidth) + ShiftX;}
		}

		public int PosY{
			get{ return (int)(RelativePosY * TargetHeigth) + ShiftY;}
		}

		public int Width{
			get{ return (int)(RelativeWidth * TargetWidth);}
		}

		public int Heigth{
			get{ return (int)(RelativeHeigth * TargetHeigth);}
		}

	}

}

