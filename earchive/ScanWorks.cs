using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using Gdk;
using Saraff.Twain;
using NLog;

namespace earchive
{
	public class ScanWorks
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private bool WorkWithTwain;
		Twain32 _twain32;

		public event EventHandler<ImageTransferEventArgs> ImageTransfer;
		int TotalImages = -1;

		public List<Pixbuf> Images;
		Gtk.Window _parent;

		public ScanWorks (Gtk.Window parent)
		{
			Images = new List<Pixbuf>();
			_parent = parent;

			if(Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				WorkWithTwain = true;
				SetupTwain();
			}
			else
			{
				WorkWithTwain = false;
				//FIXME Setup Linux scanner
			}
		}

		private void SetupTwain()
		{
			_twain32 = new Twain32 ();
			_twain32.OpenDSM();

			_twain32.AcquireCompleted+=(object sender,EventArgs e) => {
				TotalImages = _twain32.ImageCount;
				for(int i = 0; i < _twain32.ImageCount; i++)
				{
					Pixbuf CurImg = WinImageToPixbuf(_twain32.GetImage(i));
					if(ImageTransfer == null)
					{// Записываем во внутренний массив
						Images.Add(CurImg);
					}
					else
					{// Передаем через событие
						ImageTransferEventArgs arg = new ImageTransferEventArgs();
						arg.AllImages = TotalImages;
						arg.Image = CurImg;
						ImageTransfer(this, arg);
					}
				}
				logger.Debug("DataTransferred");
			};

		}

		private Pixbuf WinImageToPixbuf( System.Drawing.Image img)
		{
			MemoryStream  stream = new MemoryStream();
			img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
			stream.Position = 0;

			Pixbuf PixImage = new Pixbuf(stream);
			stream.Close();

			return PixImage;
		}

		public void GetImages()
		{
			if(WorkWithTwain)
			{
				RunTwain();

			}


		}

		private void RunTwain()
		{
			_twain32.OpenDataSource();
			_twain32.Acquire();
		}

		public void Close()
		{
			_twain32.CloseDataSource ();
			_twain32.CloseDSM ();
		}

		public class ImageTransferEventArgs : EventArgs
		{
			public int AllImages { get; set; }
			public Pixbuf Image { get; set; }
		}

	}
}

