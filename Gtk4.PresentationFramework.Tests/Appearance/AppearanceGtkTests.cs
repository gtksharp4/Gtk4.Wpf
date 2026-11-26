using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Appearance;

/// <summary>
/// Real-widget tests. GtkSharp4 exposes no gtk_widget_get_color, so the assertions target the CSS the
/// per-widget provider was loaded with (<c>AppearanceCss</c>) and the unique widget name it keys on.
/// </summary>
public class AppearanceGtkTests : WpfGtkTestBase
{
	[Fact]
	public void Button_background_attaches_provider_with_red_background_color() => Run(() =>
	{
		var b = new Button { Background = Brushes.Red };
		var w = b.Widget;
		Assert.IsType<Gtk.Button>(w);
		Assert.True(b.HasAppearanceProvider);
		Assert.Contains("background-color: rgba(255,0,0,1);", b.AppearanceCss);
		Assert.False(string.IsNullOrEmpty(w.Name));
	});

	[Fact]
	public void TextBlock_font_css_lands_on_label() => Run(() =>
	{
		var tb = new TextBlock { Text = "hi", FontSize = 20, FontWeight = FontWeights.Bold };
		Assert.IsType<Gtk.Label>(tb.Widget);
		Assert.Contains("font-size: 20px;", tb.AppearanceCss);
		Assert.Contains("font-weight: 700;", tb.AppearanceCss);
	});

	[Fact]
	public void Border_brush_thickness_and_radius_land_on_frame() => Run(() =>
	{
		var b = new Border
		{
			BorderBrush = Brushes.Blue, BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(5),
			Background = Brushes.Lime,
		};
		Assert.IsAssignableFrom<Gtk.Frame>(b.Widget);
		Assert.Contains("border-width: 2px 2px 2px 2px;", b.AppearanceCss);
		Assert.Contains("border-style: solid;", b.AppearanceCss);
		Assert.Contains("border-color: rgba(0,0,255,1);", b.AppearanceCss);
		Assert.Contains("border-radius: 5px 5px 5px 5px;", b.AppearanceCss);
		Assert.Contains("background-color: rgba(0,255,0,1);", b.AppearanceCss);
	});

	[Fact]
	public void Change_after_realize_reloads_css() => Run(() =>
	{
		var b = new Button { Background = Brushes.Red };
		_ = b.Widget;
		b.Background = Brushes.Blue;
		Assert.Contains("rgba(0,0,255,1)", b.AppearanceCss);
		Assert.DoesNotContain("rgba(255,0,0,1)", b.AppearanceCss);
		b.ClearValue(Control.BackgroundProperty);
		Assert.Equal(string.Empty, b.AppearanceCss);
	});

	[Fact]
	public void Value_set_after_realize_attaches_provider_late() => Run(() =>
	{
		var b = new Button();
		_ = b.Widget;
		Assert.False(b.HasAppearanceProvider);
		b.FontSize = 15;
		Assert.True(b.HasAppearanceProvider);
		Assert.Contains("font-size: 15px;", b.AppearanceCss);
	});

	[Fact]
	public void Inherited_only_value_emits_no_per_widget_css() => Run(() =>
	{
		var win = new Window { FontSize = 18 };
		var btn = new Button();
		var tb = new TextBlock();
		win.Content = new StackPanel { Children = { btn, tb } };
		_ = btn.Widget;
		_ = tb.Widget;
		Assert.False(btn.HasAppearanceProvider);
		Assert.Equal(string.Empty, btn.AppearanceCss);
		Assert.Equal(string.Empty, tb.AppearanceCss);
		Assert.Equal(18.0, btn.FontSize);
	});

	[Fact]
	public void Default_controls_have_no_provider() => Run(() =>
	{
		Assert.False(Realized(new Button()).HasAppearanceProvider);
		Assert.False(Realized(new TextBlock()).HasAppearanceProvider);
		Assert.False(Realized(new Border()).HasAppearanceProvider);
		Assert.False(Realized(new StackPanel()).HasAppearanceProvider);
	});

	[Fact]
	public void Style_setter_value_is_emitted_but_inherited_is_not() => Run(() =>
	{
		var style = new Style(typeof(Button));
		style.Setters.Add(new Setter { Property = Control.ForegroundProperty, Value = Brushes.Green });
		var b = new Button { Style = style };
		_ = b.Widget;
		Assert.Contains("color: rgba(0,128,0,1);", b.AppearanceCss);
	});

	[Fact]
	public void Panel_background_applies_to_widget() => Run(() =>
	{
		var p = new StackPanel { Background = Brushes.Red };
		_ = p.Widget;
		Assert.Contains("background-color: rgba(255,0,0,1);", p.AppearanceCss);
	});

	static UIElement Realized(UIElement e)
	{
		_ = e.Widget;
		return e;
	}
}
