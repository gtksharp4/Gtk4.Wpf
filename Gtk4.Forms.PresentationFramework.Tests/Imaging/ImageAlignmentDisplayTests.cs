using System.Windows;
using Xunit;
using WpfImage = System.Windows.Controls.Image;

namespace Gtk4.Forms.PresentationFramework.Tests.Imaging;

/// <summary>WPF arranges a Stretch-aligned Stretch.None Image centred in a larger slot (alignment offset).</summary>
public class ImageAlignmentDisplayTests : WpfGtkTestBase
{
	static int FirstRedColumn(WidgetPixels p)
	{
		for (int x = 0; x < p.Width; x++)
			for (int y = 0; y < p.Height; y++)
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
		Assert.Equal(20 + (560 - ImageDisplayFixtures.Size) / 2, FirstRedColumn(probe.WindowPixels()));
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
