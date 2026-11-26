using System.Windows.Controls;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Pure (no GTK widget) tests for the Stretch→ContentFit mapping.
/// Covers every <see cref="Stretch"/> enum value.
/// </summary>
public class ImageStretchMapTests
{
	[Fact]
	public void ToContentFit_None_maps_to_ScaleDown()
	{
		Assert.Equal(Gtk.ContentFit.ScaleDown, ImageStretchMap.ToContentFit(Stretch.None));
	}

	[Fact]
	public void ToContentFit_Fill_maps_to_Fill()
	{
		Assert.Equal(Gtk.ContentFit.Fill, ImageStretchMap.ToContentFit(Stretch.Fill));
	}

	[Fact]
	public void ToContentFit_Uniform_maps_to_Contain()
	{
		Assert.Equal(Gtk.ContentFit.Contain, ImageStretchMap.ToContentFit(Stretch.Uniform));
	}

	[Fact]
	public void ToContentFit_UniformToFill_maps_to_Cover()
	{
		Assert.Equal(Gtk.ContentFit.Cover, ImageStretchMap.ToContentFit(Stretch.UniformToFill));
	}
}
