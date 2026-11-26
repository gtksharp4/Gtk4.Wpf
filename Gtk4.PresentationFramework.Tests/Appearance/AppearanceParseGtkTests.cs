using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Appearance;

/// <summary>GTK ignores invalid CSS silently, so assert the provider reports no parse errors and that it takes effect.</summary>
public class AppearanceParseGtkTests : WpfGtkTestBase
{
	static readonly string[] Kinds = { "Button", "TextBlock", "Border", "StackPanel" };

	static void Border2(DependencyObject d)
	{
		d.SetValue(Border.BorderBrushProperty, Brushes.Red);
		d.SetValue(Border.BorderThicknessProperty, new Thickness(2));
	}

	static List<(string Name, Action<DependencyObject> Apply)> Cases()
	{
		var linear = new LinearGradientBrush(Colors.Red, Colors.Blue, 45);
		var radial = new RadialGradientBrush(Colors.Red, Colors.Blue);
		var cases = new List<(string, Action<DependencyObject>)>
		{
			("solid", d => d.SetValue(Panel.BackgroundProperty, Brushes.Red)),
			("linear", d => d.SetValue(Panel.BackgroundProperty, linear)),
			("radial", d => d.SetValue(Panel.BackgroundProperty, radial)),
			("weight", d => d.SetValue(Control.FontWeightProperty, FontWeight.FromOpenTypeWeight(650))),
			("italic", d => d.SetValue(Control.FontStyleProperty, FontStyles.Italic)),
			("oblique", d => d.SetValue(Control.FontStyleProperty, FontStyles.Oblique)),
			("families", d => d.SetValue(Control.FontFamilyProperty, new FontFamily("Arial, ./Fonts/#Roboto"))),
			("border", Border2),
			("noBrush", d => d.SetValue(Border.BorderThicknessProperty, new Thickness(2))),
			("radius", d => d.SetValue(Border.CornerRadiusProperty, new CornerRadius(4))),
		};
		for (int i = 1; i <= 9; i++)
		{
			int n = i;
			cases.Add(("stretch" + n, d => d.SetValue(Control.FontStretchProperty, FontStretch.FromOpenTypeStretch(n))));
		}
		return cases;
	}

	public static TheoryData<int, string> Data()
	{
		var data = new TheoryData<int, string>();
		for (int i = 0; i < Cases().Count; i++)
			foreach (var kind in Kinds)
				data.Add(i, kind);
		return data;
	}

	static UIElement Make(string kind) => kind switch
	{
		"Button" => new Button(),
		"TextBlock" => new TextBlock { Text = "x" },
		"Border" => new Border(),
		_ => new StackPanel(),
	};

	[Theory]
	[MemberData(nameof(Data))]
	public void Appearance_css_parses_without_errors(int index, string kind) => Run(() =>
	{
		var (name, apply) = Cases()[index];
		var el = Make(kind);
		_ = el.Widget;
		apply(el);
		Assert.True(el.AppearanceParseErrors == 0, $"{name} on {kind}: {el.AppearanceCss}");
	});

	// GTK applies CSS on the frame clock, so pump the main loop until the measurement moves (or times out).
	static int PumpUntilChanged(Func<int> measure, int before)
	{
		for (int i = 0; i < 200; i++)
		{
			Gtk.Application.RunIteration(false);
			if (measure() != before) break;
			System.Threading.Thread.Sleep(10);
		}
		return measure();
	}

	[Fact]
	public void Font_size_changes_the_measured_height() => Run(() =>
	{
		var tb = new TextBlock { Text = "Hg" };
		var win = new Window { Content = tb };
		win.Show();
		int Height()
		{
			tb.Widget.Measure(Gtk.Orientation.Vertical, -1, out _, out var nat, out _, out _);
			return nat;
		}
		int nat0 = PumpUntilChanged(Height, -1);
		tb.FontSize = 40;
		int nat1 = PumpUntilChanged(Height, nat0);
		win.Close();
		Assert.True(nat1 > nat0 + 10, $"{nat0} -> {nat1}");
	});

	[Fact]
	public void Border_thickness_grows_the_minimum_width() => Run(() =>
	{
		var border = new Border { BorderThickness = new Thickness(0) };
		var win = new Window { Content = border };
		win.Show();
		int MinWidth()
		{
			border.Widget.Measure(Gtk.Orientation.Horizontal, -1, out var min, out _, out _, out _);
			return min;
		}
		int min0 = PumpUntilChanged(MinWidth, -1);
		border.BorderThickness = new Thickness(10);
		border.BorderBrush = Brushes.Red;
		int min1 = PumpUntilChanged(MinWidth, min0);
		win.Close();
		Assert.True(min1 >= min0 + 20, $"{min0} -> {min1}");
	});
}
