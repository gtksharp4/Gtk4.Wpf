using Xunit;

namespace Gtk4.PresentationFramework.Tests.XamlBacked;

/// <summary>An uninspectable same-assembly element takes {DynamicResource} through the hooked emission.</summary>
public class HookedDynWindowTests : WpfGtkTestBase
{
	[Fact]
	public void Hooked_element_resolves_ancestor_keys_once_parented() => Run(() =>
	{
		var window = new HookedDynWindow();

		Assert.Equal("alpha", window.Pane.A);
		Assert.Equal("beta", window.Pane.B);
	});
}
