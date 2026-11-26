using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Appearance;

public class BorderCssTests
{
	[Fact]
	public void Thickness_without_a_brush_draws_transparent()
	{
		string css = ElementCssStyle.Border(new Border { BorderThickness = new Thickness(3) });
		Assert.Contains("border-width: 3px 3px 3px 3px;", css);
		Assert.Contains("border-style: solid;", css);
		Assert.Contains("border-color: transparent", css);
		Assert.Contains("border-color: transparent", ElementCssStyle.Border(new Button { BorderThickness = new Thickness(3) }));
	}
}
