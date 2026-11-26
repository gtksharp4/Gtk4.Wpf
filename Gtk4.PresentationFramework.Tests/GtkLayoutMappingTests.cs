using Xunit;
using WpfButton = System.Windows.Controls.Button;
using WpfLabel = System.Windows.Controls.Label;
using WpfTextBlock = System.Windows.Controls.TextBlock;
using WpfWindow = System.Windows.Window;
using HAlign = System.Windows.HorizontalAlignment;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Maps WPF layout properties (Width/Height, HorizontalAlignment/VerticalAlignment, and a
/// Window's size) onto the backing GTK widget so they are not ignored on screen.
/// </summary>
public class GtkLayoutMappingTests : WpfGtkTestBase
{
	[Fact]
	public void Explicit_Width_sets_WidthRequest() => Run(() =>
	{
		var btn = new WpfButton { Content = "Go", Width = 120 };
		Assert.Equal(120, btn.Widget.WidthRequest);
	});

	[Fact]
	public void Explicit_Width_on_Stretch_pins_to_Center() => Run(() =>
	{
		// Default HorizontalAlignment is Stretch; an explicit Width pins it centered (WPF rule).
		var btn = new WpfButton { Content = "Go", Width = 120 };
		Assert.Equal(Gtk.Align.Center, btn.Widget.Halign);
	});

	[Fact]
	public void HorizontalAlignment_Center_maps_to_GtkCenter() => Run(() =>
	{
		var lbl = new WpfLabel { Content = "Label", HorizontalAlignment = HAlign.Center };
		Assert.Equal(Gtk.Align.Center, lbl.Widget.Halign);
	});

	[Fact]
	public void HorizontalAlignment_Left_maps_to_GtkStart() => Run(() =>
	{
		var lbl = new WpfLabel { Content = "L", HorizontalAlignment = HAlign.Left };
		Assert.Equal(Gtk.Align.Start, lbl.Widget.Halign);
	});

	[Fact]
	public void Default_Stretch_maps_to_GtkFill() => Run(() =>
	{
		var tb = new WpfTextBlock { Text = "x" };
		Assert.Equal(Gtk.Align.Fill, tb.Widget.Halign);
	});

	[Fact]
	public void Window_Width_Height_set_default_size() => Run(() =>
	{
		var win = new WpfWindow { Title = "Sized", Width = 800, Height = 450 };
		var gtkWin = (Gtk.Window)win.Widget;
		Assert.Equal(800, gtkWin.DefaultWidth);
		Assert.Equal(450, gtkWin.DefaultHeight);
	});

	[Fact]
	public void Window_Width_Height_also_pin_size_request() => Run(() =>
	{
		// The GTK4 Windows backend ignores default size for a resizable top-level, so the
		// window is pinned to its Width/Height via a size request too.
		var win = new WpfWindow { Title = "Pinned", Width = 800, Height = 450 };
		var gtkWin = (Gtk.Window)win.Widget;
		Assert.Equal(800, gtkWin.WidthRequest);
		Assert.Equal(450, gtkWin.HeightRequest);
	});
}