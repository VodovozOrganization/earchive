using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
//using System.Runtime.InteropServices;
//using System.Windows.Forms;
using Gdk;
//using NTwain;
//using NTwain.Data;
//using NTwain.Values;
using Saraff.Twain;

namespace earchive
{
	public class ScanWorks
	{
		private bool WorkWithTwain;
		//TwainSession twain;
		Twain32 _twain32;
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
		/*	TWIdentity appId = TWIdentity.Create(DataGroups.Image, new Version(1, 0), "Quality Solution", "Open Source", "EArchive", null);
			twain = new TwainSession(appId);
			twain.DataTransferred += (s, e) =>
			{
				if (e.Data != IntPtr.Zero)
				{
					//_ptrTest = e.Data;
					var img = e.Data.GetDrawingBitmap();
					if (img != null)
						Images.Add(BitmapToPixbuf(img));
				}
				else if (!string.IsNullOrEmpty(e.File))
				{
					var img = new Pixbuf(e.File);
					Images.Add(img);
				}
				Console.WriteLine("DataTransferred");
			};

			twain.SourceDisabled += delegate
			{
				//step = "Close DS";
				var rc2 = twain.CloseSource();
				rc2 = twain.CloseManager();

				MainClass.WaitRedraw ();
				Console.WriteLine("SourceDisabled");
			};
			twain.TransferReady += (s, te) =>
			{
				//te.CancelAll = true;
				Console.WriteLine("TransferReady");
			};
			Application.AddMessageFilter(twain);
*/

			_twain32 = new Twain32 ();
			_twain32.OpenDSM();

			_twain32.AcquireCompleted+=(object sender,EventArgs e) => {
				for(int i = 0; i < _twain32.ImageCount; i++)
				{
					Images.Add(WinImageToPixbuf(_twain32.GetImage(i)));
				}
				Console.WriteLine("DataTransferred");
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
		/*	TWIdentity dsId;
			TWStatus status = null;

			string step = "Open DSM";

			var WinHandle = WindowsPlatform.GdkWin32.HgdiobjGet (_parent.RootWindow);

			var hand = new HandleRef(_parent, WinHandle);

			var rc = twain.OpenManager(hand);
			if (rc == ReturnCode.Success)
			{
				step = "User Select";
				rc = twain.DGControl.Identity.UserSelect(out dsId);
				//rc = DGControl.Status.Get(ref stat);

				//TwEntryPoint entry;
				//rc = DGControl.EntryPoint.Get(dsId, out entry);
				if (rc == ReturnCode.Success)
				{
					step = "Open DS";
					rc = twain.OpenSource(dsId);
					//rc = DGControl.Status.Get(dsId, ref stat);
					if (rc == ReturnCode.Success)
					{
						step = "Enable DS";
						rc = twain.EnableSource(SourceEnableMode.ShowUI, true, hand);
						return;
					}
					else
					{
						twain.DGControl.Status.GetSource(out status);
					}
				}
				else
				{
					twain.DGControl.Status.GetManager(out status);
				}
				twain.CloseManager();
			}
			else
			{
				twain.DGControl.Status.GetManager(out status);
			}

			Console.WriteLine(string.Format("Failed at {0}: RC={1}, CC={2}", step, rc, status.ConditionCode));
			MainClass.StatusMessage ("Ошибка запуска сканирования");
			*/

			_twain32.OpenDataSource();
			_twain32.Acquire();
		}

		public void Close()
		{
			_twain32.CloseDataSource ();
			_twain32.CloseDSM ();
		}
	}
}

