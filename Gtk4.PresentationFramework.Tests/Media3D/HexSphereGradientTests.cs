using System.Windows.Controls;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media3D;

/// <summary>The sc# radial gradient behind HexSphereDemo's Viewport3D reaches GTK as the WPF colours.</summary>
public class HexSphereGradientTests : WpfGtkTestBase
{
	const string Xaml = @"
<Grid xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
      xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' Background='{StaticResource GridBackground}'>
  <Grid.Resources>
    <RadialGradientBrush x:Key='GridBackground'>
      <GradientBrush.GradientStops>
        <GradientStopCollection>
          <GradientStop Color='sc#1, 0.1, 0, 0' Offset='1' />
          <GradientStop Color='sc#1, 1, 0.1, 0' Offset='0.5' />
          <GradientStop Color='sc#1, 1, 1, 0.1' Offset='0' />
        </GradientStopCollection>
      </GradientBrush.GradientStops>
    </RadialGradientBrush>
  </Grid.Resources>
</Grid>";

	[Fact]
	public void The_grid_background_css_is_a_radial_gradient_from_yellow_to_dark_red() => Run(() =>
	{
		var grid = new Grid { Background = (System.Windows.Media.Brush)((Grid)XamlReader.Parse(
			Xaml.Replace("Background='{StaticResource GridBackground}'", "")))
			.Resources["GridBackground"]! };
		string css = grid.BuildAppearanceCss();
		Assert.Contains("radial-gradient(", css);
		Assert.Contains("rgba(255,255,89,1) 0%", css);
		Assert.Contains("rgba(255,89,0,1) 50%", css);
		Assert.Contains("rgba(89,0,0,1) 100%", css);
	});
}
