using System;
using System.Xml.Serialization;
using System.IO;

namespace earchive
{
	public class RecognizeTemplate
	{
		public int DocTypeId;
		public string Name;
		public TextMarker[] Markers;
		public RecognazeRule NumberRule;
		public RecognazeRule DateRule;
		public RecognazeRule[] FieldRules;

		public RecognizeTemplate ()
		{
		}

		public string SaveToString()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(RecognizeTemplate));
			TextWriter Writer = new StringWriter();
			serializer.Serialize(Writer, this);
			return Writer.ToString();
		}

		public void SaveToStream(Stream xml)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(RecognizeTemplate));
			TextWriter Writer = new StreamWriter(xml);
			serializer.Serialize(Writer, this);
			Writer.Close();
		}

		public static RecognizeTemplate Load(string xml)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(RecognizeTemplate));
			TextReader Reader = new StringReader(xml);
			return (RecognizeTemplate)serializer.Deserialize(Reader);
		}

		public static RecognizeTemplate Load(Stream xml)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(RecognizeTemplate));
			TextReader Reader = new StreamReader(xml);
			return (RecognizeTemplate)serializer.Deserialize(Reader);
		}
	
	}
}

