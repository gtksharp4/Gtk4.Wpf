using System.Windows.Controls;
using System.Windows.Media;
using Xunit;
using WpfImage = System.Windows.Controls.Image;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// GTK smoke tests for Image.Stretch and Image.StretchDirection.
/// Bodies run on the GTK thread; skipped when native GTK is unavailable.
/// </summary>
public class ImageGtkSmokeTests : WpfGtkTestBase
{
	[Fact]
	public void Image_Widget_is_GtkPicture() => Run(() =>
	{
		var img = new WpfImage();
		Assert.IsAssignableFrom<Gtk.Picture>(img.Widget);
	});

	[Fact]
	public void Image_Stretch_Fill_sets_ContentFit_Fill() => Run(() =>
	{
		var img = new WpfImage { Stretch = Stretch.Fill };
		var pic = (Gtk.Picture)img.Widget;
		Assert.Equal(Gtk.ContentFit.Fill, pic.ContentFit);
	});

	[Fact]
	public void Image_Stretch_Uniform_sets_ContentFit_Contain() => Run(() =>
	{
		var img = new WpfImage { Stretch = Stretch.Uniform };
		var pic = (Gtk.Picture)img.Widget;
		Assert.Equal(Gtk.ContentFit.Contain, pic.ContentFit);
	});

	[Fact]
	public void Image_Stretch_UniformToFill_sets_ContentFit_Cover() => Run(() =>
	{
		var img = new WpfImage { Stretch = Stretch.UniformToFill };
		var pic = (Gtk.Picture)img.Widget;
		Assert.Equal(Gtk.ContentFit.Cover, pic.ContentFit);
	});

	[Fact]
	public void Image_Stretch_None_sets_ContentFit_ScaleDown() => Run(() =>
	{
		var img = new WpfImage { Stretch = Stretch.None };
		var pic = (Gtk.Picture)img.Widget;
		Assert.Equal(Gtk.ContentFit.ScaleDown, pic.ContentFit);
	});

	[Fact]
	public void Image_Stretch_change_after_realize_updates_ContentFit() => Run(() =>
	{
		var img = new WpfImage { Stretch = Stretch.Uniform };
		var pic = (Gtk.Picture)img.Widget;
		img.Stretch = Stretch.Fill;
		Assert.Equal(Gtk.ContentFit.Fill, pic.ContentFit);
	});

	[Fact]
	public void Image_StretchDirection_set_persists_after_realize() => Run(() =>
	{
		var img = new WpfImage { StretchDirection = StretchDirection.UpOnly };
		_ = img.Widget;
		Assert.Equal(StretchDirection.UpOnly, img.StretchDirection);
	});

	[Fact]
	public void Image_Source_null_does_not_throw() => Run(() =>
	{
		var img = new WpfImage { Source = null };
		var ex = Record.Exception(() => _ = img.Widget);
		Assert.Null(ex);
	});
}
