using System;
using ZXing.QrCode;
using Gdk;
using ZXing;
using ZXing.Common;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace earchive
{
    public static class QRCodeRecognizer
	{

        public static bool TryParse(Pixbuf[] images, ref Document doc)
        {
            QRCodeReader qrCodeReader = new QRCodeReader ();
			foreach(var image in images) {
				var qrResult = QRScanner.ReadQRCode(image);
				if (qrResult == null) {
					return false;
				}
				string result = qrResult.Text;
                var parameters = result.Split(';');
                if(parameters.Count() != 3) {
                    continue;
                }
				string documentType = parameters[0];
                string orderId = parameters[1];
                DateTime orderDate;
                var validDate = DateTime.TryParseExact(parameters[2], 
			                                           "ddMMyyyy", 
			                                           CultureInfo.InvariantCulture, 
			                                           DateTimeStyles.None, 
			                                           out orderDate);
                if(String.IsNullOrEmpty(documentType) || String.IsNullOrEmpty (documentType) || !validDate) {
                    continue;
                }
                Document docum = new Document(documentType);
                if(docum == null) {
                    continue;
                }
                if (doc == null) {
                    doc = docum;
                }
				doc.DocNumber = orderId;
                doc.DocNumberConfidence = 1;
                doc.DocDate = orderDate;
                doc.DocDateConfidence = 1;
                return true;
			}

            return false;
		}
	}
}
