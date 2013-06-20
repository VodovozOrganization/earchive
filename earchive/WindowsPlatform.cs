using System;
using System.Runtime.InteropServices;

namespace earchive
{
	public class WindowsPlatform
	{
		public WindowsPlatform ()
		{
		}

		public static class GdkWin32
		{
			[System.Runtime.InteropServices.DllImport ("libgdk-win32-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr gdk_win32_drawable_get_handle (IntPtr drawable);

			[System.Runtime.InteropServices.DllImport ("libgdk-win32-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr gdk_win32_hdc_get (IntPtr drawable, IntPtr gc, int usage);

			[System.Runtime.InteropServices.DllImport ("libgdk-win32-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
			static extern void gdk_win32_hdc_release (IntPtr drawable, IntPtr gc, int usage);

			public static IntPtr HgdiobjGet (Gdk.Drawable drawable)
			{
				return gdk_win32_drawable_get_handle (drawable.Handle);
			}

			public static IntPtr HdcGet (Gdk.Drawable drawable, Gdk.GC gc, Gdk.GCValuesMask usage)
			{
				return gdk_win32_hdc_get (drawable.Handle, gc.Handle, (int) usage);
			}

			public static void HdcRelease (Gdk.Drawable drawable, Gdk.GC gc, Gdk.GCValuesMask usage)
			{
				gdk_win32_hdc_release (drawable.Handle, gc.Handle, (int) usage);
			}
		}

	}
}

