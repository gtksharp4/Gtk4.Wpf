using System.Windows.Controls;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>GTK smoke tests: the overlay is wired when the widget tree is realized.</summary>
public class AdornerGtkSmokeTests : WpfGtkTestBase
{
	/// <summary>
	/// AdornerDecorator.Widget is a Gtk.Overlay; adding a child sets the overlay's
	/// main child widget; the AdornerLayer has the overlay reference.
	/// </summary>
	[Fact]
	public void AdornerDecorator_widget_is_GtkOverlay() => Run(() =>
	{
		var dec = new AdornerDecorator();
		var inner = new TextBlock { Text = "hi" };
		dec.Child = inner;

		Assert.IsType<Gtk.Overlay>(dec.Widget);
	});

	[Fact]
	public void AdornerDecorator_AdornerLayer_has_overlay_after_realize() => Run(() =>
	{
		var dec = new AdornerDecorator();
		_ = dec.Widget; // realize
		Assert.NotNull(dec.AdornerLayer.GtkOverlay);
	});

	[Fact]
	public void AdornerDecorator_child_is_overlay_main_content() => Run(() =>
	{
		var dec = new AdornerDecorator();
		var inner = new TextBlock { Text = "content" };
		dec.Child = inner;

		var overlay = (Gtk.Overlay)dec.Widget;
		Assert.Same(inner.Widget, overlay.Child);
	});
}
