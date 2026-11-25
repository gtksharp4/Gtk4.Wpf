using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Appearance;

/// <summary>
/// Pixel tests on real, shown widgets. Measured cause of the report (Button Background=Blue showed the theme's light
/// face, white caption invisible): the theme paints a button's face with <c>background-image</c> (and
/// <c>box-shadow</c>), which covers a plain <c>background-color</c>. A rule on the widget reaches only its own CSS
/// node in GTK 4, so check/radio indicators and the dropdown's inner button take a provider on the child widget.
/// </summary>
public partial class ControlBrushPixelGtkTests : WpfGtkTestBase
{
	// A widget that cannot be targeted never takes the :hover face, wherever the real pointer rests.
	static (Window Win, WidgetPixels Px) Show(FrameworkElement el)
	{
		el.IsHitTestVisible = false;
		var win = new Window { Content = el, Width = 300, Height = 200 };
		win.Show();
		GtkPump.Until(() => el.Widget.IsMapped && el.Widget.Width > 0);
		GtkPump.For(250);
		return (win, WidgetPixels.Capture(el.Widget));
	}

	static (int R, int G, int B) Settle(FrameworkElement el, Window win)
	{
		GtkPump.For(250);
		var face = WidgetPixels.Capture(el.Widget).At(6, 20);
		win.Close();
		return face;
	}

	static Window Host(FrameworkElement el)
	{
		el.IsHitTestVisible = false;
		var win = new Window { Content = el, Width = 300, Height = 200 };
		win.Show();
		GtkPump.Until(() => el.Widget.IsMapped && el.Widget.Width > 0);
		return win;
	}

	static bool IsBlue((int R, int G, int B) c) => c.R < 20 && c.G < 20 && c.B > 235;

	static bool Near((int R, int G, int B) c, int r, int g, int b) =>
		Math.Abs(c.R - r) <= 3 && Math.Abs(c.G - g) <= 3 && Math.Abs(c.B - b) <= 3;

	[Fact]
	public void Blue_button_face_is_blue() => Run(() =>
	{
		var (win, px) = Show(new Button { Content = "Caption", Background = Brushes.Blue, Foreground = Brushes.White });
		var face = px.At(6, px.Height / 2);
		win.Close();
		Assert.True(IsBlue(face), face.ToString());
	});

	[Fact]
	public void White_caption_pixels_show_on_the_blue_face() => Run(() =>
	{
		var (win, px) = Show(new Button { Content = "Caption", Background = Brushes.Blue, Foreground = Brushes.White });
		int white = 0;
		for (int y = 0; y < px.Height; y++)
			for (int x = 0; x < px.Width; x++)
			{
				var c = px.At(x, y);
				if (c.R > 200 && c.G > 200 && c.B > 200) white++;
			}
		win.Close();
		Assert.True(white > 20, $"white pixels: {white}");
	});

	[Fact]
	public void Gradient_button_paints_its_first_stop_at_the_top() => Run(() =>
	{
		var brush = new LinearGradientBrush(Colors.Red, Colors.Blue, 90);
		var (win, px) = Show(new Button { Content = "x", Background = brush });
		var top = px.At(6, 3);
		var bottom = px.At(6, px.Height - 4);
		win.Close();
		Assert.True(top.R > 200 && top.B < 60, top.ToString());
		Assert.True(bottom.B > 200 && bottom.R < 60, bottom.ToString());
	});

	[Fact]
	public void Border_brush_replaces_the_theme_frame() => Run(() =>
	{
		var b = new Button
		{
			Content = "x", Background = Brushes.Blue, BorderBrush = Brushes.Red, BorderThickness = new Thickness(3)
		};
		var (win, px) = Show(b);
		var frame = px.At(1, px.Height / 2);
		var face = px.At(8, px.Height / 2);
		win.Close();
		Assert.True(frame.R > 200 && frame.G < 40 && frame.B < 40, frame.ToString());
		Assert.True(IsBlue(face), face.ToString());
	});

	[Fact]
	public void Button_without_a_brush_keeps_the_pure_theme_and_has_no_provider() => Run(() =>
	{
		var b = new Button { Content = "Caption" };
		var (win, px) = Show(b);
		var theme = px.At(6, px.Height / 2);
		win.Close();
		Assert.False(b.HasAppearanceProvider);
		Assert.Equal(string.Empty, b.AppearanceRules);
		Assert.Equal(string.Empty, b.AppearanceChildRules);
		Assert.True(theme.R > 200 && theme.G > 200 && theme.B > 200, theme.ToString());
	});

	[Fact]
	public void ClearValue_restores_the_theme_face() => Run(() =>
	{
		var b = new Button { Content = "x", Background = Brushes.Blue };
		var win = Host(b);
		b.ClearValue(Control.BackgroundProperty);
		var face = Settle(b, win);
		Assert.False(IsBlue(face), face.ToString());
		Assert.Equal(string.Empty, b.AppearanceRules);
	});

	[Fact]
	public void CheckBox_background_paints_the_indicator_not_the_root() => Run(() =>
	{
		var c = new CheckBox { Content = "Caption", Background = Brushes.Blue };
		var (win, px) = Show(c);
		// The caption label sits beside the indicator, so the widget's centre is no longer the box.
		int blue = CountBlue(px);
		var root = px.At(px.Width - 2, px.Height / 2);
		win.Close();
		Assert.True(blue > 20, $"blue pixels: {blue}");
		Assert.False(IsBlue(root), root.ToString());
		Assert.DoesNotContain("background", c.AppearanceCss);
		Assert.Contains("background-color", c.AppearanceChildRules);
	});

	[Fact]
	public void RadioButton_background_paints_the_indicator() => Run(() =>
	{
		var r = new RadioButton { Content = "Caption", Background = Brushes.Blue };
		var (win, px) = Show(r);
		int blue = CountBlue(px);
		win.Close();
		Assert.True(blue > 20, $"blue pixels: {blue}");
	});

	[Fact]
	public void TextBox_background_paints_the_text_area() => Run(() =>
	{
		var (win, px) = Show(new TextBox { Text = "Caption", Background = Brushes.Blue });
		var area = px.At(px.Width / 2, px.Height / 2);
		win.Close();
		Assert.True(IsBlue(area), area.ToString());
	});

	[Fact]
	public void ComboBox_background_paints_the_dropdown_button() => Run(() =>
	{
		var cb = new ComboBox { Background = Brushes.Blue };
		cb.Items.Add("x");
		cb.SelectedIndex = 0;
		var (win, px) = Show(cb);
		var face = px.At(px.Width / 2, px.Height / 2);
		win.Close();
		Assert.True(IsBlue(face), face.ToString());
	});

	[Fact]
	public void ListBox_background_paints_the_list() => Run(() =>
	{
		var lb = new ListBox { Background = Brushes.Blue, Width = 100, Height = 80 };
		lb.Items.Add("x");
		var (win, px) = Show(lb);
		var area = px.At(px.Width - 10, px.Height / 2);
		win.Close();
		Assert.True(IsBlue(area), area.ToString());
	});

	[Fact]
	public void Disabled_button_shows_the_aero2_face() => Run(() =>
	{
		var (win, px) = Show(new Button { Content = "x", Background = Brushes.Blue, IsEnabled = false });
		var face = px.At(6, px.Height / 2);
		win.Close();
		Assert.True(Near(face, 244, 244, 244), face.ToString());
	});

	[Fact]
	public void Hovered_button_shows_the_aero2_mouse_over_face() => Run(() =>
	{
		var b = new Button { Content = "x", Background = Brushes.Blue };
		var win = Host(b);
		b.Widget.SetStateFlags(Gtk.StateFlags.Prelight, false);
		var face = Settle(b, win);
		Assert.True(Near(face, 0xBE, 0xE6, 0xFD), face.ToString());
	});

	[Fact]
	public void Pressed_button_shows_the_aero2_pressed_face() => Run(() =>
	{
		var b = new Button { Content = "x", Background = Brushes.Blue };
		var win = Host(b);
		b.Widget.SetStateFlags(Gtk.StateFlags.Active, false);
		var face = Settle(b, win);
		Assert.True(Near(face, 0xC4, 0xE5, 0xF6), face.ToString());
	});

	[Fact]
	public void Appearance_css_has_no_parse_errors() => Run(() =>
	{
		var cb = new ComboBox { Background = Brushes.Blue };
		var items = new Control[]
		{
			new Button { Background = Brushes.Blue, BorderBrush = Brushes.Red, BorderThickness = new Thickness(2) },
			new CheckBox { Background = Brushes.Blue, BorderBrush = Brushes.Red, BorderThickness = new Thickness(2) },
			cb, new TextBox { Background = new LinearGradientBrush(Colors.Red, Colors.Blue, 0) },
		};
		foreach (var item in items)
		{
			_ = item.Widget;
			Assert.Equal(0, item.AppearanceParseErrors);
			Assert.Equal(0, item.AppearanceChildParseErrors);
		}
	});
}
