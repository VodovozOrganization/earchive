using Gtk;
using QS.Print;
using QSReport;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;

namespace earchive.Print
{
	public class DocumentsPrinter
	{
		private bool _cancelPrinting = false;

		public event EventHandler DocumentsPrinted;
		public event EventHandler PrintingCanceled;

		public DocumentsPrinter()
		{
			DocPrinterInit();
		}

		public void SetDocumentsToPrint(IList<SelectablePrintDocument> documentsToPrint)
		{
			MultiDocPrinter.PrintableDocuments = new GenericObservableList<SelectablePrintDocument>(documentsToPrint);
        }

        public void AddDocumentToPrint(SelectablePrintDocument documentsToPrint)
        {
            MultiDocPrinter.PrintableDocuments.Add(documentsToPrint);
        }

        private MultipleDocumentPrinter MultiDocPrinter { get; set; }
		public static PrintSettings PrinterSettings { get; set; }
		public IList<SelectablePrintDocument> MultiDocPrinterPrintableDocuments => MultiDocPrinter.PrintableDocuments;

		private void DocPrinterInit()
		{
			MultiDocPrinter = new MultipleDocumentPrinter();
			MultiDocPrinter.PrintableDocuments = new GenericObservableList<SelectablePrintDocument>();

            MultiDocPrinter.DocumentsPrinted += (o, args) => DocumentsPrinted?.Invoke(o, args);
		}

		public void Print()
		{
			if (!_cancelPrinting)
			{
				MultiDocPrinter.PrinterSettings = PrinterSettings;

				foreach(var document in MultiDocPrinter.PrintableDocuments)
				{
					MultiDocPrinter.PrintDocument(document);

					PrinterSettings = MultiDocPrinter.PrinterSettings;
				}
			}
			else
			{
				PrintingCanceled?.Invoke(this, new EventArgs());
			}
        }

        public void Print(bool manyImages)
        {
            if (!_cancelPrinting)
            {
				MultiDocPrinter.PrintSelectedDocuments();
            }
            else
            {
                PrintingCanceled?.Invoke(this, new EventArgs());
            }
        }
    }
}
