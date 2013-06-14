using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using Gdk;
using NTwain;
using NTwain.Data;
using NTwain.Values;

namespace earchive
{
	public class ScanWorks
	{
		private bool WorkWithTwain;
		TwainSession twain;
		List<Pixbuf> Images;
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
			TWIdentity appId = TWIdentity.Create(DataGroups.Image, new Version(1, 0), "My Company", "Test Family", "Tester", null);
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
			};

			twain.SourceDisabled += delegate
			{
				//step = "Close DS";
				var rc2 = twain.CloseSource();
				rc2 = twain.CloseManager();

/*				if (InvokeRequired)
				{
					Invoke(new Action(delegate
					                  {
						MessageBox.Show("Success!");
					}));
				}
				else
				{
					MessageBox.Show("Success!");
				} */
			};
			twain.TransferReady += (s, te) =>
			{
				//te.CancelAll = true;
			};
			//Application.AddMessageFilter(twain);
		}

		private Pixbuf BitmapToPixbuf( Bitmap img)
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
			TWIdentity dsId;
			TWStatus status = null;

			string step = "Open DSM";

			WindowsPlatform.GtkWin32Proxy WinProxy = new WindowsPlatform.GtkWin32Proxy(_parent);

			var hand = new HandleRef(_parent, WinProxy.Handle);

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
						rc = twain.EnableSource(SourceEnableMode.NoUI, false, hand);
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

			//MessageBox.Show(string.Format("Failed at {0}: RC={1}, CC={2}", step, rc, status.ConditionCode));
		}
	}
}

