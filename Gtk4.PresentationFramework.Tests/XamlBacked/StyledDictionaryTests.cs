using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.XamlBacked;

public class StyledDictionaryTests : WpfGtkTestBase
{
	[Fact]
	public void StaticResource_inside_a_dictionary_root_resolves_earlier_keys() => Run(() =>
	{
		var dictionary = new StyledDictionary();

		Assert.True(dictionary.Contains("tagConverter"));
		var button = new Button { Width = 5, Style = (Style)dictionary["Tagged"]! };
		Assert.Equal("tag:5", button.Content);
	});
}
