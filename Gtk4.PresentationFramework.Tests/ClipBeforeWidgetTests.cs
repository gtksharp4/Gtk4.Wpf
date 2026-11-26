using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>A Clip set before the widget exists must still reach the widget.</summary>
public class ClipBeforeWidgetTests : WpfGtkTestBase
{
	[Fact]
	public void Clip_set_before_the_widget_exists_clips_the_panel() => Run(() =>
	{
		var p = new StackPanel { Clip = new RectangleGeometry(new Rect(0, 0, 10, 10)) };
		Assert.Equal(Gtk.Overflow.Hidden, p.Widget.Overflow);
	});

	[Fact]
	public void Clip_set_before_the_widget_exists_clips_the_image() => Run(() =>
	{
		var image = new Image { Clip = new RectangleGeometry(new Rect(0, 0, 10, 10)) };
		Assert.Equal(Gtk.Overflow.Hidden, image.Widget.Overflow);
	});
}
