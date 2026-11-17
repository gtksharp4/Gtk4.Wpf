namespace Gtk4.Forms.Platform.GTK;

public static class RendererFactory
{
	public static IVisualElementRenderer CreateRenderer(VisualElement element)
	{
		return Platform.CreateRenderer(element);
	}
}