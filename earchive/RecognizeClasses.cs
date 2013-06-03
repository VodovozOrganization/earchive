using System;
using Gdk;

namespace earchive
{
	class RecognazeRule
	{
		public int FieldId;

		public bool NextAfterTextMarker;
		public int ShiftWordsCount;

		public RelationalRectangle Box;
	}

	class TextMarker
	{
		public string Text;
		public bool BeginOfLine;
		public double PatternPosX;
		public double PatternPosY;
		public RelationalRectangle Zone;
	}

	class RelationalRectangle
	{
		public double RelativePosX;
		public double RelativePosY;
		public double RelativeWidth;
		public double RelativeHeigth;

		public int TargetHeigth;
		public int TargetWidth;

		public int ShiftX;
		public int ShiftY;

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

