using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Appearance;

/// <summary>
/// The CSS depends on the value source: a local/style value equal to the inherited or default one
/// must still render, or GTK CSS inheritance would draw the ancestor value instead.
/// </summary>
public class AppearanceSourceGtkTests : WpfGtkTestBase
{
	static (Window win, Button btn) Tree()
	{
		var win = new Window();
		var btn = new Button();
		win.Content = new StackPanel { Children = { btn } };
		_ = win.Widget;
		_ = btn.Widget;
		return (win, btn);
	}

	[Fact]
	public void Local_value_equal_to_the_inherited_one_keeps_its_css_when_the_ancestor_changes() => Run(() =>
	{
		var (win, btn) = Tree();
		win.FontSize = 16;
		btn.FontSize = 16;
		Assert.Contains("font-size: 16px", btn.AppearanceCss);
		win.FontSize = 20;
		Assert.Equal(16.0, btn.FontSize);
		Assert.Contains("font-size: 16px", btn.AppearanceCss);
	});

	[Fact]
	public void Local_value_equal_to_the_default_keeps_its_css_when_the_ancestor_changes() => Run(() =>
	{
		var (win, btn) = Tree();
		btn.FontSize = 12;
		win.FontSize = 20;
		Assert.Equal(12.0, btn.FontSize);
		Assert.Contains("font-size: 12px", btn.AppearanceCss);
	});

	[Fact]
	public void ClearValue_removes_the_css_of_a_local_value_equal_to_the_inherited_one() => Run(() =>
	{
		var (win, btn) = Tree();
		win.FontSize = 16;
		btn.FontSize = 16;
		btn.ClearValue(Control.FontSizeProperty);
		Assert.DoesNotContain("font-size", btn.AppearanceCss);
	});

	[Fact]
	public void Window_foreground_does_not_reach_a_button_and_default_buttons_stay_provider_free() => Run(() =>
	{
		System.Windows.Themes.Generic.Reset();
		var (win, btn) = Tree();
		Assert.False(btn.HasAppearanceProvider);
		win.Foreground = Brushes.Red;
		Assert.Contains("color: rgba(0,0,0,1);", btn.AppearanceCss);
		win.ClearValue(Control.ForegroundProperty);
		Assert.DoesNotContain("color:", btn.AppearanceCss);
	});

	[Fact]
	public void Border_without_a_brush_keeps_the_width_but_draws_transparent() => Run(() =>
	{
		var b = new Border { BorderThickness = new Thickness(3) };
		_ = b.Widget;
		Assert.Contains("border-color: transparent", b.AppearanceCss);
		var btn = new Button { BorderThickness = new Thickness(3) };
		_ = btn.Widget;
		Assert.Contains("border-color: transparent", btn.AppearanceCss);
	});
}
