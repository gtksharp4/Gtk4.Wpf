using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>XAML <c>Command="Slider.DecreaseSmall"</c> resolves a control-owned command.</summary>
public class CommandConverterOwnersTests
{
	[Fact]
	public void A_Slider_command_is_found_by_its_qualified_name()
	{
		Assert.Same(Slider.DecreaseSmall, XamlValueConverter.ConvertByType(typeof(ICommand), "Slider.DecreaseSmall"));
		Assert.Same(Slider.IncreaseLarge, XamlValueConverter.ConvertByType(typeof(ICommand), "Slider.IncreaseLarge"));
	}

	[Fact]
	public void A_Slider_command_needs_its_owner_name()
	{
		Assert.Throws<System.NotSupportedException>(() =>
			XamlValueConverter.ConvertByType(typeof(ICommand), "DecreaseSmall"));
	}
}
