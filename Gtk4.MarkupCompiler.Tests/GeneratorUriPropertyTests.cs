using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>A string attribute on a Uri-typed property converts like WPF's UriTypeConverter.</summary>
public class GeneratorUriPropertyTests : GeneratorV2TestBase
{
	[Fact]
	public void NavigationWindowSource_UsesTheOwnedDependencyProperty()
	{
		string code = Gen("<NavigationWindow " + Head + " x:Class=\"A.W\" Source=\"Homepage.xaml\"/>");

		Assert.Contains("SetValue(System.Windows.Navigation.NavigationWindow.SourceProperty", code);
		Assert.DoesNotContain("this.Source = \"Homepage.xaml\"", code);
		AssertCompiles(code, "namespace A { public partial class W : System.Windows.Navigation.NavigationWindow "
			+ "{ public W() { InitializeComponent(); } } }");
	}

	[Fact]
	public void ClrUriProperty_BecomesAUri()
	{
		string code = Gen("<Window " + Head + " x:Class=\"A.W\"><Hyperlink NavigateUri=\"P.xaml\"/></Window>");

		Assert.Contains("new System.Uri(\"P.xaml\", System.UriKind.RelativeOrAbsolute)", code);
	}
}
