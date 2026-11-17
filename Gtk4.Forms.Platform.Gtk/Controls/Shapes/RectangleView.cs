using System;
using Cairo;
using Gtk4.Forms.Platform.GTK.Extensions;

namespace Gtk4.Forms.Platform.GTK.Controls;

public class RectangleView : ShapeView
{
	protected override void Draw(Gdk.Rectangle area, Context cr)
	{
		cr.Rectangle(0, 0, _width, _height);

		base.Draw(area, cr);
	}
}