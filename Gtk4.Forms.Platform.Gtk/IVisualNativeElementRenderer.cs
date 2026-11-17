using Control = Gtk.Widget;

namespace Gtk4.Forms.Platform.GTK;

public interface IVisualNativeElementRenderer : IVisualElementRenderer
{
	Control Control { get; }
}