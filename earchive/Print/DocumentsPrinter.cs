using Gtk;
using QS.Print;
using System;
using System.Collections.Generic;
using System.Linq;

namespace earchive.Print
{
	public class DocumentsPrinter
	{
		private bool _cancelPrinting = false;

		public delegate void DocumentPrintedHandler(IPrintableImage sender, EventArgs eventArgs);

		public event DocumentPrintedHandler DocumentsPrinted;
		public event EventHandler PrintingCanceled;

		private List<IPrintableImage> _documentsToPrint;

		public DocumentsPrinter(IList<IPrintableImage> documentsToPrint = null)
		{
			_documentsToPrint = documentsToPrint != null ? documentsToPrint.ToList() : new List<IPrintableImage>(); ;

			ImagePrinter.PrintingCanceled += OnImagePrinterPrintingCanceled;
		}

		public List<IPrintableImage> DocumentsToPrint => _documentsToPrint;

		public PrintSettings PrinterSettings { get; set; }

		private void OnImagePrinterPrintingCanceled(object sender, EventArgs e)
		{
			_cancelPrinting = true;

			PrintingCanceled?.Invoke(sender, e);
		}

		public void SetDocumentsToPrint(IList<IPrintableImage> documentsToPrint)
		{
			_documentsToPrint.Clear();
			_documentsToPrint.AddRange(documentsToPrint);
		}

		public void AddDocumentsToPrint(IList<IPrintableImage> documentsToPrint)
		{
			_documentsToPrint.AddRange(documentsToPrint);
		}

		public void AddDocumentToPrint(IPrintableImage documentToPrint)
		{
			_documentsToPrint.Add(documentToPrint);
		}

		public void Print()
		{
			if (!_cancelPrinting)
			{
				foreach (var document in _documentsToPrint)
				{
					DocumentPrinters.ImagePrinter?.Print(new IPrintableImage[] { document }, PrinterSettings);
					DocumentsPrinted?.Invoke(document, EventArgs.Empty);

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
