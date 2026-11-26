using System.Windows;
using Xunit;
using WpfImage = System.Windows.Controls.Image;

namespace Gtk4.PresentationFramework.Tests.Imaging;

/// <summary>WPF arranges a Stretch-aligned Stretch.None Image centred in a larger slot (alignment offset).</summary>
public class ImageAlignmentDisplayTests : WpfGtkTestBase
{
	// Rows from firstRow down: the titlebar above holds the default window icon, which has red pixels.
	static int FirstRedColumn(WidgetPixels p, int firstRow)
	{
		for (int x = 0; x < p.Width; x++)
			for (int y = firstRow; y < p.Height; y++)
			{
				var (b, g, r, a) = p.At(x, y);
				if (a == 255 && r > 200 && g < 60 && b < 60)
					return x;
			}
		return -1;
	}

	[Fact]
	public void Stretch_None_is_centred_in_a_wider_slot() => Run(() =>
	{
		using var probe = new ImageDisplayProbe(ImageDisplayFixtures.Quadrant(), scroll: false);
		Assert.Equal(ImageDisplayFixtures.Size, probe.Image.ActualWidth);
		// A symmetric Margin(20) leaves the centre unchanged (20 + (560-64)/2 == (600-64)/2) and
		// Margin is not applied to GTK widgets (docs/samples-coverage.md): this proves centring only.
		int titlebar = ((Gtk.Window)probe.Window.Widget).Titlebar!.Height;
		// The slot is the window widget (600 wide minus any solid client-side border GTK draws without a
		// compositor), so derive the centre from the widget and check the ink lands at the picture's left edge.
		var window = probe.Window.Widget;
		Assert.True(probe.Image.Widget.ComputeBounds(window, out var bounds));
		Assert.Equal((window.Width - ImageDisplayFixtures.Size) / 2.0, bounds.X, 1.0);
		Assert.Equal(bounds.X, FirstRedColumn(probe.WindowPixels(), titlebar), 1.0);
	});

	[Fact]
	public void Stretch_None_picture_is_centred_when_both_alignments_stretch() => Run(() =>
	{
		using var probe = new ImageDisplayProbe(ImageDisplayFixtures.Quadrant(), scroll: false);
		var pic = (Gtk.Picture)probe.Image.Widget;
		Assert.Equal(Gtk.Align.Center, pic.Halign);
		Assert.Equal(Gtk.Align.Center, pic.Valign);
	});

	[Theory]
	[InlineData(HorizontalAlignment.Left, Gtk.Align.Start)]
	[InlineData(HorizontalAlignment.Center, Gtk.Align.Center)]
	[InlineData(HorizontalAlignment.Right, Gtk.Align.End)]
	public void Explicit_horizontal_alignment_is_kept(HorizontalAlignment alignment, Gtk.Align expected) => Run(() =>
	{
		using var probe = new ImageDisplayProbe(ImageDisplayFixtures.Quadrant(), scroll: false,
			configure: img => img.HorizontalAlignment = alignment);
		Assert.Equal(expected, ((Gtk.Picture)probe.Image.Widget).Halign);
	});

	[Fact]
	public void A_Stretch_Fill_image_keeps_Fill_alignment() => Run(() =>
	{
		using var probe = new ImageDisplayProbe(ImageDisplayFixtures.Quadrant(), System.Windows.Media.Stretch.Fill,
			scroll: false);
		Assert.Equal(Gtk.Align.Fill, ((Gtk.Picture)probe.Image.Widget).Halign);
	});
}
