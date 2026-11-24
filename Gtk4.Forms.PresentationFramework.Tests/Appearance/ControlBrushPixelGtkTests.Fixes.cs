using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Appearance;

public partial class ControlBrushPixelGtkTests
{
	static int CountBlue(WidgetPixels px, int x0 = 0, int y0 = 0, int x1 = int.MaxValue, int y1 = int.MaxValue)
	{
		int n = 0;
		for (int y = y0; y < Math.Min(y1, px.Height); y++)
			for (int x = x0; x < Math.Min(x1, px.Width); x++)
				if (IsBlue(px.At(x, y))) n++;
		return n;
	}

	[Fact]
	public void Grouped_RadioButton_background_paints_the_indicator() => Run(() =>
	{
		var first = new RadioButton { Content = "one", GroupName = "g" };
		var second = new RadioButton { Content = "two", GroupName = "g", Background = Brushes.Blue };
		var panel = new StackPanel();
		panel.Children.Add(first);
		panel.Children.Add(second);
		var win = new Window { Content = panel, Width = 300, Height = 200 };
		win.Show();
		GtkPump.Until(() => second.Widget.IsMapped && second.Widget.Width > 0);
		GtkPump.For(250);
		int blue = CountBlue(WidgetPixels.Capture(second.Widget));
		win.Close();
		Assert.True(blue > 20, $"blue pixels: {blue}");
	});

	[Fact]
	public void Disabled_Button_with_a_background_still_gets_the_aero2_disabled_face() => Run(() =>
	{
		var b = new Button { Content = "x", Background = Brushes.Red, IsEnabled = false };
		_ = b.Widget;
		Assert.Contains("#F4F4F4", b.AppearanceRules);
	});

	[Fact]
	public void Disabled_MenuItem_keeps_its_background_and_gets_no_button_state_faces() => Run(() =>
	{
		var item = new MenuItem { Header = "x", Background = Brushes.Red, IsEnabled = false };
		_ = item.Widget;
		Assert.DoesNotContain(":disabled", item.AppearanceRules);
		Assert.DoesNotContain("#F4F4F4", item.AppearanceRules);
		var (win, px) = Show(item);
		var face = px.At(2, px.Height / 2);
		win.Close();
		Assert.True(face.R > 200 && face.G < 60 && face.B < 60, face.ToString());
	});

	[Fact]
	public void Disabled_DataGridColumnHeader_gets_no_button_state_faces() => Run(() =>
	{
		var header = new DataGridColumnHeader { Content = "x", Background = Brushes.Red, IsEnabled = false };
		_ = header.Widget;
		Assert.DoesNotContain(":disabled", header.AppearanceRules);
		Assert.DoesNotContain("#F4F4F4", header.AppearanceRules);
	});

	[Fact]
	public void Multiline_TextBox_background_paints_the_text_area() => Run(() =>
	{
		var tb = new TextBox { AcceptsReturn = true, Text = "x", Background = Brushes.Blue, Height = 100 };
		var (win, px) = Show(tb);
		int blue = CountBlue(px, 20, 20, px.Width - 20, px.Height - 20);
		win.Close();
		Assert.True(blue > 100, $"blue pixels: {blue}");
		Assert.Equal(0, tb.AppearanceChildParseErrors);
	});

	[Fact]
	public void Editable_ComboBox_background_paints_the_entry() => Run(() =>
	{
		var cb = new ComboBox { IsEditable = true, Text = "x", Background = Brushes.Blue };
		var (win, px) = Show(cb);
		var centre = px.At(px.Width / 3, px.Height / 2);
		win.Close();
		Assert.True(IsBlue(centre), centre.ToString());
	});
}
