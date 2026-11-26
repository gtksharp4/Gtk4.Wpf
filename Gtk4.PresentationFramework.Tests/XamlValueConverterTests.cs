using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests (no GTK) for <see cref="XamlValueConverter.Convert"/>.
/// Each test fails without the converter implementation.
/// </summary>
public class XamlValueConverterTests
{
	// ── Minimal element with DPs of each type ────────────────────────────────

	sealed class TestElement : FrameworkElement
	{
		public static readonly DependencyProperty ThicknessProp =
			DependencyProperty.Register("ThicknessVal", typeof(Thickness), typeof(TestElement));

		public static readonly DependencyProperty CornerRadiusProp =
			DependencyProperty.Register("CornerRadiusVal", typeof(CornerRadius), typeof(TestElement));

		public static readonly DependencyProperty GridLengthProp =
			DependencyProperty.Register("GridLengthVal", typeof(GridLength), typeof(TestElement));

		public static readonly DependencyProperty DoubleProp =
			DependencyProperty.Register("DoubleVal", typeof(double), typeof(TestElement));

		public static readonly DependencyProperty IntProp =
			DependencyProperty.Register("IntVal", typeof(int), typeof(TestElement));

		public static readonly DependencyProperty BoolProp =
			DependencyProperty.Register("BoolVal", typeof(bool), typeof(TestElement));

		public static readonly DependencyProperty ColorProp =
			DependencyProperty.Register("ColorVal", typeof(Color), typeof(TestElement));

		public static readonly DependencyProperty BrushProp =
			DependencyProperty.Register("BrushVal", typeof(Brush), typeof(TestElement));

		public static readonly DependencyProperty PointProp =
			DependencyProperty.Register("PointVal", typeof(Point), typeof(TestElement));

		public static readonly DependencyProperty SizeProp =
			DependencyProperty.Register("SizeVal", typeof(Size), typeof(TestElement));

		public static readonly DependencyProperty RectProp =
			DependencyProperty.Register("RectVal", typeof(Rect), typeof(TestElement));

		public static readonly DependencyProperty VisibilityProp =
			DependencyProperty.Register("VisibilityVal", typeof(Visibility), typeof(TestElement));

		public static readonly DependencyProperty StringProp =
			DependencyProperty.Register("StringVal", typeof(string), typeof(TestElement));

		protected override Gtk.Widget CreateWidget() => throw new NotSupportedException("pure test");
	}

	static T Conv<T>(DependencyProperty dp, string raw) =>
		(T)XamlValueConverter.Convert(typeof(TestElement), dp, raw)!;

	static object? ConvObj(DependencyProperty dp, string raw) =>
		XamlValueConverter.Convert(typeof(TestElement), dp, raw);

	// ── Thickness ────────────────────────────────────────────────────────────

	[Fact]
	public void Thickness_Uniform()
	{
		Assert.Equal(new Thickness(5), Conv<Thickness>(TestElement.ThicknessProp, "5"));
	}

	[Fact]
	public void Thickness_TwoPart()
	{
		Assert.Equal(new Thickness(5, 10, 5, 10), Conv<Thickness>(TestElement.ThicknessProp, "5,10"));
	}

	[Fact]
	public void Thickness_FourPart()
	{
		Assert.Equal(new Thickness(1, 2, 3, 4), Conv<Thickness>(TestElement.ThicknessProp, "1,2,3,4"));
	}

	// ── CornerRadius ─────────────────────────────────────────────────────────

	[Fact]
	public void CornerRadius_Uniform()
	{
		Assert.Equal(new CornerRadius(8), Conv<CornerRadius>(TestElement.CornerRadiusProp, "8"));
	}

	[Fact]
	public void CornerRadius_FourPart()
	{
		Assert.Equal(new CornerRadius(1, 2, 3, 4), Conv<CornerRadius>(TestElement.CornerRadiusProp, "1,2,3,4"));
	}

	// ── GridLength ───────────────────────────────────────────────────────────

	[Fact]
	public void GridLength_Auto()
	{
		Assert.Equal(GridLength.Auto, Conv<GridLength>(TestElement.GridLengthProp, "Auto"));
	}

	[Fact]
	public void GridLength_Star()
	{
		Assert.Equal(new GridLength(1.0, GridUnitType.Star), Conv<GridLength>(TestElement.GridLengthProp, "*"));
	}

	[Fact]
	public void GridLength_TwoStar()
	{
		Assert.Equal(new GridLength(2.0, GridUnitType.Star), Conv<GridLength>(TestElement.GridLengthProp, "2*"));
	}

	[Fact]
	public void GridLength_Pixels()
	{
		Assert.Equal(new GridLength(100.0, GridUnitType.Pixel), Conv<GridLength>(TestElement.GridLengthProp, "100"));
	}

	// ── Scalars ──────────────────────────────────────────────────────────────

	[Fact]
	public void Double_Parses()
	{
		Assert.Equal(3.14, Conv<double>(TestElement.DoubleProp, "3.14"), precision: 10);
	}

	[Fact]
	public void Int_Parses()
	{
		Assert.Equal(42, Conv<int>(TestElement.IntProp, "42"));
	}

	[Fact]
	public void Bool_True()
	{
		Assert.True(Conv<bool>(TestElement.BoolProp, "True"));
	}

	[Fact]
	public void Bool_False()
	{
		Assert.False(Conv<bool>(TestElement.BoolProp, "false"));
	}

	// ── Color ────────────────────────────────────────────────────────────────

	[Fact]
	public void Color_Named()
	{
		Assert.Equal(Colors.Red, Conv<Color>(TestElement.ColorProp, "Red"));
	}

	[Fact]
	public void Color_HexRRGGBB()
	{
		Assert.Equal(Colors.Red, Conv<Color>(TestElement.ColorProp, "#FF0000"));
	}

	[Fact]
	public void Color_HexAARRGGBB()
	{
		Assert.Equal(Color.FromArgb(0x80, 0xFF, 0, 0), Conv<Color>(TestElement.ColorProp, "#80FF0000"));
	}

	[Fact]
	public void Color_HexRGB_Short()
	{
		Assert.Equal(Colors.Red, Conv<Color>(TestElement.ColorProp, "#F00"));
	}

	// ── Brush ────────────────────────────────────────────────────────────────

	[Fact]
	public void Brush_Named_ReturnsCorrectColor()
	{
		var result = Conv<SolidColorBrush>(TestElement.BrushProp, "Red");
		Assert.Equal(Colors.Red, result.Color);
	}

	[Fact]
	public void Brush_Hex_ReturnsCorrectColor()
	{
		var result = Conv<SolidColorBrush>(TestElement.BrushProp, "#0000FF");
		Assert.Equal(Colors.Blue, result.Color);
	}

	// ── Point / Size / Rect ──────────────────────────────────────────────────

	[Fact]
	public void Point_Parses()
	{
		Assert.Equal(new Point(10, 20), Conv<Point>(TestElement.PointProp, "10,20"));
	}

	[Fact]
	public void Size_Parses()
	{
		Assert.Equal(new Size(100, 200), Conv<Size>(TestElement.SizeProp, "100,200"));
	}

	[Fact]
	public void Rect_Parses()
	{
		Assert.Equal(new Rect(1, 2, 3, 4), Conv<Rect>(TestElement.RectProp, "1,2,3,4"));
	}

	// ── Visibility (enum, case-insensitive) ──────────────────────────────────

	[Fact]
	public void Visibility_CaseInsensitive()
	{
		Assert.Equal(Visibility.Hidden, Conv<Visibility>(TestElement.VisibilityProp, "hidden"));
	}

	[Fact]
	public void Visibility_Collapsed()
	{
		Assert.Equal(Visibility.Collapsed, Conv<Visibility>(TestElement.VisibilityProp, "Collapsed"));
	}

	// ── String passthrough ───────────────────────────────────────────────────

	[Fact]
	public void String_Passthrough()
	{
		Assert.Equal("hello", ConvObj(TestElement.StringProp, "hello"));
	}

	// ── FormatException on bad input names the property ──────────────────────

	[Fact]
	public void BadThickness_ThrowsFormatException_NamesProperty()
	{
		var ex = Assert.Throws<FormatException>(
			() => Conv<Thickness>(TestElement.ThicknessProp, "not-a-number"));
		Assert.Contains("ThicknessVal", ex.Message);
	}

	[Fact]
	public void BadColor_ThrowsFormatException_NamesProperty()
	{
		var ex = Assert.Throws<FormatException>(
			() => Conv<Color>(TestElement.ColorProp, "NotAColor"));
		Assert.Contains("ColorVal", ex.Message);
	}
}

/// <summary>
/// Tests that a string <see cref="Setter.Value"/> is converted to the target DP type on apply.
/// </summary>
public class SetterValueConverterTests
{
	sealed class ThemedElement : FrameworkElement
	{
		public static readonly DependencyProperty Margin2Property =
			DependencyProperty.Register("Margin2", typeof(Thickness), typeof(ThemedElement),
				new PropertyMetadata(default(Thickness)));

		public Thickness Margin2
		{
			get => (Thickness)GetValue(Margin2Property)!;
			set => SetValue(Margin2Property, value);
		}

		protected override Gtk.Widget CreateWidget() => throw new NotSupportedException("pure test");
	}

	[Fact]
	public void Setter_StringValue_UniformThickness_Applied()
	{
		var el = new ThemedElement();
		var style = new Style(typeof(ThemedElement));
		style.Setters.Add(new Setter { PropertyName = "Margin2", Value = "5" });
		style.Apply(el);

		Assert.Equal(new Thickness(5), el.Margin2);
	}

	[Fact]
	public void Setter_StringValue_FourPartThickness_Applied()
	{
		var el = new ThemedElement();
		var style = new Style(typeof(ThemedElement));
		// Use Property directly (bypasses FindByName) to isolate CoerceValue.
		style.Setters.Add(new Setter { Property = ThemedElement.Margin2Property, Value = "1,2,3,4" });
		style.Apply(el);

		Assert.Equal(new Thickness(1, 2, 3, 4), el.Margin2);
	}

}