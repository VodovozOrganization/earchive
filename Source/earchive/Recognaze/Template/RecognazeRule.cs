using System;
using System.Xml.Serialization;
using Gdk;

namespace earchive
{
	public enum ValidationTypes{
		None,
		IsNumber,
		IsDate
	}

	public class RecognazeRule
	{
		public int FieldId;
		public ValidationTypes Validate;

#region Распознование в зоне маркера

		public bool NextAfterTextMarker;
		public int ShiftWordsCount;

		[XmlIgnore]
		public string AfterTextMarkerValue;

		[XmlIgnore]
		public float AfterTextMarkerConfidence;
#endregion

		public RelationalRectangle Box;
	}

}
