using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>RenderOptions attached properties: stored with WPF defaults, no drawing effect on GTK.</summary>
public class RenderOptionsTests : WpfGtkTestBase
{
	[Fact]
	public void Defaults_match_WPF() => Run(() =>
	{
		var image = new Image();

		Assert.Equal(BitmapScalingMode.Unspecified, RenderOptions.GetBitmapScalingMode(image));
		Assert.Equal(EdgeMode.Unspecified, RenderOptions.GetEdgeMode(image));
		Assert.Equal(CachingHint.Unspecified, RenderOptions.GetCachingHint(image));
		Assert.Equal(ClearTypeHint.Auto, RenderOptions.GetClearTypeHint(image));
		Assert.Equal(0.707, RenderOptions.GetCacheInvalidationThresholdMinimum(image));
		Assert.Equal(1.414, RenderOptions.GetCacheInvalidationThresholdMaximum(image));
	});

	[Fact]
	public void Values_round_trip_and_are_not_inherited() => Run(() =>
	{
		var panel = new StackPanel();
		var child = new Image();
		panel.Children.Add(child);

		RenderOptions.SetBitmapScalingMode(panel, BitmapScalingMode.NearestNeighbor);
		RenderOptions.SetEdgeMode(panel, EdgeMode.Aliased);

		Assert.Equal(BitmapScalingMode.NearestNeighbor, RenderOptions.GetBitmapScalingMode(panel));
		Assert.Equal(EdgeMode.Aliased, RenderOptions.GetEdgeMode(panel));
		Assert.Equal(BitmapScalingMode.Unspecified, RenderOptions.GetBitmapScalingMode(child));
	});

	[Fact]
	public void Enum_aliases_share_values_and_invalid_values_are_rejected() => Run(() =>
	{
		Assert.Equal(BitmapScalingMode.Linear, BitmapScalingMode.LowQuality);
		Assert.Equal(BitmapScalingMode.Fant, BitmapScalingMode.HighQuality);
		Assert.Throws<ArgumentException>(() => RenderOptions.SetEdgeMode(new Image(), (EdgeMode)7));
		Assert.Throws<ArgumentNullException>(() => RenderOptions.SetEdgeMode(null!, EdgeMode.Aliased));
	});
}
