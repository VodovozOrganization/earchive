using Gtk;
using QS.Print;
using System;
using System.Collections.Generic;
using System.Linq;

namespace earchive.Print
{
	public class PrintableImagesPrinter
	{
		private bool _cancelPrinting = false;

		public delegate void ImagePrintedEventHandler(IPrintableImage sender, EventArgs eventArgs);

		public event ImagePrintedEventHandler PrintableImagePrinted;
		public event EventHandler PrintingCanceled;

		private List<IPrintableImage> _imagesToPrint;

		public PrintableImagesPrinter(IList<IPrintableImage> imagesToPrint = null)
		{
			_imagesToPrint = imagesToPrint != null ? imagesToPrint.ToList() : new List<IPrintableImage>(); ;

			ImagePrinter.PrintingCanceled += OnImagePrinterPrintingCanceled;
		}

		public List<IPrintableImage> ImagesToPrint => _imagesToPrint;

		public PrintSettings PrinterSettings { get; set; }

		private void OnImagePrinterPrintingCanceled(object sender, EventArgs e)
		{
			_cancelPrinting = true;

			PrintingCanceled?.Invoke(sender, e);
		}

		public void SetImagesPrintList(IList<IPrintableImage> imagesToPrint)
		{
			_imagesToPrint.Clear();
			_imagesToPrint.AddRange(imagesToPrint);
		}

		public void AddImagesToPrintList(IList<IPrintableImage> imagesToPrint)
		{
			_imagesToPrint.AddRange(imagesToPrint);
		}

		public void AddImageToPrintList(IPrintableImage imageToPrint)
		{
			_imagesToPrint.Add(imageToPrint);
		}

		public void Print(bool clearPrintSettings = true)
		{
			if (!_cancelPrinting)
			{
				if(clearPrintSettings)
				{
					ImagePrinter.PrintSettings = null;
					PrinterSettings = null;

				}

				foreach (var document in _imagesToPrint)
				{
					DocumentPrinters.ImagePrinter?.Print(new IPrintableImage[] { document }, PrinterSettings);
					PrintableImagePrinted?.Invoke(document, EventArgs.Empty);

					PrinterSettings = ImagePrinter.PrintSettings;
				}
			}
			else
			{
				PrintingCanceled?.Invoke(this, new EventArgs());
			}
		}
	}
}
