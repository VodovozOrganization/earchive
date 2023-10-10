using System;
using Gtk;
using Gdk;

namespace earchive
{
	public class ImageListCellRenderer : Gtk.CellRenderer
	{
				
		public ImageListCellRenderer (Pango.FontDescription font, IconSize IconsSize) : base ()
		{
			this.Font = font;
			this.IconsSize = IconsSize;
		}
		
		private Pango.FontDescription _font;
		public Pango.FontDescription Font
		{
			get {
				return _font;
			}
			set {
				if (value == null)
					throw (new System.ArgumentNullException ());
				
				_font = value;
			}
		}
		
		private IconSize _IconsSize;
		public IconSize IconsSize
		{
			get {
				return _IconsSize;
			}
			set {
				if (value <= 0)
					throw (new System.ArgumentOutOfRangeException ());
				
				_IconsSize = value;
			}
		}

		bool _ImageRow ;
		[GLib.Property ("IsImageRow", "Get/Set ImageRow", "This is the description")]
		public bool ImageRow
		{
			get {
				return _ImageRow;
			}
			set {
				_ImageRow = value;
			}
		}

		string  _IconName ;
		[GLib.Property ("IconName", "Get/Set IconName", "This is the description")]
		public string IconName
		{
			get {
				return _IconName;
			}
			set {
				_IconName = value;
			}
		}

		string  _Text ;
		[GLib.Property ("text", "Get/Set text", "This is the description")]
		public string Text
		{
			get {
				return _Text;
			}
			set {
				_Text = value;
			}
		}

		Pixbuf _pixbuf ;
		[GLib.Property ("pixbuf", "Get/Set pixbuf", "This is the description")]
		public Pixbuf pixbuf
		{
			get {
				return _pixbuf;
			}
			set {
				_pixbuf = value;
			}
		}

		public void RenderDoc (Cairo.Context CairoContext, Gtk.Widget widget, int height)
		{
			Pixbuf icon = widget.RenderIcon (_IconName, _IconsSize, "");

			Pango.Layout Layout = Pango.CairoHelper.CreateLayout (CairoContext);
			Layout.FontDescription = _font;
			Layout.SetMarkup (_Text);

			int layout_x = icon.Width + 5;
			int layoutWidth, layoutHeight;
			Layout.GetPixelSize (out layoutWidth, out layoutHeight);
			
			int layout_y = (height - layoutHeight) / 2;
			CairoContext.MoveTo ((double) layout_x, (double) layout_y);
			Pango.CairoHelper.ShowLayout (CairoContext, Layout);

			Gdk.CairoHelper.SetSourcePixbuf (CairoContext, icon, 0, 0);
			CairoContext.Paint ();
			Layout.FontDescription.Dispose ();
			Layout.Dispose ();
		}

		public void RenderImage (Cairo.Context CairoContext, int height)
		{
			Gdk.CairoHelper.SetSourcePixbuf (CairoContext, _pixbuf, 0, 0);
			CairoContext.Paint ();
		}

		protected override void Render (Gdk.Drawable drawable, Gtk.Widget widget, Gdk.Rectangle background_area, Gdk.Rectangle cell_area, Gdk.Rectangle expose_area, Gtk.CellRendererState flags)
		{
			if (_ImageRow && _pixbuf == null)
				return;
			
			Gdk.Rectangle pix_rect = Gdk.Rectangle.Zero;
			
			this.GetSize (widget, ref cell_area, out pix_rect.X, out pix_rect.Y, out pix_rect.Width, out pix_rect.Height);
			
			// Take care of padding
			pix_rect.X += cell_area.X + (int) this.Xpad;
			pix_rect.Y += cell_area.Y + (int) this.Ypad;
			// Remove left, right, top and buttom borders which were added to the returned width
			pix_rect.Width  -= (int) this.Xpad * 2;
			pix_rect.Height -= (int) this.Ypad * 2;
			
			Cairo.Context context = Gdk.CairoHelper.Create (drawable);
			context.Translate (pix_rect.X, pix_rect.Y);

			if(_ImageRow)
				RenderImage (context, pix_rect.Height);
			else 
				RenderDoc (context, widget, pix_rect.Height);
			
			(context as System.IDisposable).Dispose ();
		}
		
		public override void GetSize (Gtk.Widget widget, ref Gdk.Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			if(_ImageRow)
			{
				width = (int) this.Xpad * 2 + _pixbuf.Width;
				height = (int) this.Ypad * 2 + _pixbuf.Height;
			}
			else
			{
				int IconW, IconH;
				Icon.SizeLookup (_IconsSize, out IconW, out IconH);
				Pango.Layout Layout = new Pango.Layout (widget.PangoContext);
				Layout.FontDescription = _font;
				Layout.SetMarkup (_Text);
				int layoutWidth, layoutHeight;
				Layout.GetPixelSize (out layoutWidth, out layoutHeight);
				Layout.Dispose ();

				width = (int) this.Xpad * 2 + IconW + 5 + layoutWidth;
				height = (int) this.Ypad * 2 + Math.Max ( IconH, layoutHeight);
			}

			if (cell_area != Gdk.Rectangle.Zero) {
				if (widget.Direction == Gtk.TextDirection.Rtl)
					x_offset =  (int) ((1.0 - this.Xalign) * (cell_area.Width - width));
				else
					x_offset = (int) (this.Xalign * (cell_area.Width - width));
				x_offset = System.Math.Max (x_offset, 0);
				
				y_offset = (int) (this.Yalign * (cell_area.Height - height));
				y_offset = System.Math.Max (y_offset, 0);
			} else {
				x_offset = 0;
				y_offset = 0;
			}
		}

	} 
}

