using Gdk;
using QS.Print;
using System;

namespace earchive.Print
{
	public class PrintableImage : IPrintableImage
	{
		public PrintableImage(Pixbuf pixbuf)
		{
			if (pixbuf is null)
			{
				throw new ArgumentNullException(nameof(pixbuf));
			}

			PixBuf = pixbuf;
		}

		public PrinterType PrintType => PrinterType.Image;

		public DocumentOrientation Orientation => PixBuf?.Height < PixBuf?.Width ? DocumentOrientation.Landscape : DocumentOrientation.Portrait;

		public int CopiesToPrint { get; set; } = 1;

		public string Name { get; set; } = string.Empty;

		public Pixbuf PixBuf { get; private set; }

		public Pixbuf GetPixbuf() => PixBuf;
	}
}
