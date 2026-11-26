using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>An x:Name on a Freezable is registered in the root name scope and still gets its field.</summary>
public class GeneratorFreezableNameTests : GeneratorV2TestBase
{
	const string Xaml = @"<Window x:Class=""A.W"" " + Head + @">
<Canvas>
	<Canvas.Background><VisualBrush x:Name=""LogoBrush"" TileMode=""Tile""><VisualBrush.Visual><Border>
		<TextBlock x:Name=""geo"" Text=""g""/></Border></VisualBrush.Visual></VisualBrush></Canvas.Background>
	<Rectangle><Rectangle.Fill><SolidColorBrush x:Name=""Fill"" Color=""Red""/></Rectangle.Fill>
		<Rectangle.RenderTransform><RotateTransform x:Name=""Rot""/></Rectangle.RenderTransform></Rectangle>
	<Canvas.Triggers><EventTrigger RoutedEvent=""Canvas.Loaded""><BeginStoryboard><Storyboard>
		<DoubleAnimation Storyboard.TargetName=""LogoBrush"" Storyboard.TargetProperty=""Opacity"" To=""0""/>
	</Storyboard></BeginStoryboard></EventTrigger></Canvas.Triggers>
</Canvas></Window>";

	[Fact]
	public void NamedFreezables_AreRegisteredAndHaveFields()
	{
		string code = Gen(Xaml);

		Assert.True(code.Contains("RegisterName(\"LogoBrush\""), code);
		Assert.Contains("RegisterName(\"Fill\"", code);
		Assert.Contains("RegisterName(\"Rot\"", code);
		Assert.Contains("this.LogoBrush = ", code);
		Assert.Contains("RegisterName(\"geo\"", code);
		Assert.Contains(".Visual = ", code);
		AssertCompiles(code,
			"namespace A { public partial class W : System.Windows.Window { public W() { InitializeComponent(); } } }");
	}
}
