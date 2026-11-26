using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Page.Background, UIElement.Clip, TextBlock accessors, SystemColors and path markup.</summary>
public class GraphicsTailTests : WpfGtkTestBase
{
	[Fact]
	public void PageBackgroundDefaultsToNullAndRendersAsCss() => Run(() =>
	{
		var page = new Page();
		_ = page.Widget;
		Assert.Null(page.Background);
		Assert.Same(Panel.BackgroundProperty, Page.BackgroundProperty);
		page.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));
		Assert.Contains("background", page.AppearanceCss);
		page.ClearValue(Page.BackgroundProperty);
		Assert.DoesNotContain("background", page.AppearanceCss);
	});

	[Fact]
	public void PageStoreOnlyProperties() => Run(() =>
	{
		var page = new Page();
		Assert.True(page.ShowsNavigationUI);
		Assert.False(page.KeepAlive);
		Assert.Null(page.Template);
		page.KeepAlive = true;
		page.FontSize = 20;
		Assert.True(page.KeepAlive);
		Assert.Equal(20, page.FontSize);
	});

	[Fact]
	public void ClipDefaultsToNullAndRoundTrips() => Run(() =>
	{
		var b = new Border();
		Assert.Null(b.Clip);
		var g = new RectangleGeometry(new Rect(0, 0, 5, 5));
		b.Clip = g;
		Assert.Same(g, b.Clip);
	});

	[Fact]
	public void ClipOnNativeWidgetClipsToBoundsAndRestores() => Run(() =>
	{
		var panel = new StackPanel();
		var original = panel.Widget.Overflow;
		panel.Clip = new EllipseGeometry(new Point(5, 5), 5, 5);
		Assert.Equal(Gtk.Overflow.Hidden, panel.Widget.Overflow);
		panel.ClipToBounds = true;
		panel.ClearValue(UIElement.ClipProperty);
		Assert.Equal(Gtk.Overflow.Hidden, panel.Widget.Overflow);
		panel.ClipToBounds = false;
		Assert.Equal(original, panel.Widget.Overflow);
	});

	[Fact]
	public void ClipOnShapeKeepsWidgetOverflowAndRedraws() => Run(() =>
	{
		var r = new Rectangle();
		var original = r.Widget.Overflow;
		r.Clip = new RectangleGeometry(new Rect(0, 0, 3, 3));
		Assert.Equal(original, r.Widget.Overflow);
	});

	[Fact]
	public void TextBlockAttachedAccessorsRoundTripOnAnyElement() => Run(() =>
	{
		var d = new Border();
		var brush = new SolidColorBrush(Color.FromRgb(1, 2, 3));
		TextBlock.SetForeground(d, brush);
		TextBlock.SetFontSize(d, 33);
		TextBlock.SetFontFamily(d, new FontFamily("Mono"));
		TextBlock.SetFontWeight(d, FontWeights.Bold);
		TextBlock.SetFontStyle(d, FontStyles.Italic);
		TextBlock.SetFontStretch(d, FontStretches.Expanded);
		TextBlock.SetBaselineOffset(d, 4);
		TextBlock.SetLineHeight(d, 18);
		TextBlock.SetLineStackingStrategy(d, LineStackingStrategy.BlockLineHeight);
		TextBlock.SetTextAlignment(d, TextAlignment.Center);
		Assert.Same(brush, TextBlock.GetForeground(d));
		Assert.Equal(33, TextBlock.GetFontSize(d));
		Assert.Equal(FontWeights.Bold, TextBlock.GetFontWeight(d));
		Assert.Equal(FontStyles.Italic, TextBlock.GetFontStyle(d));
		Assert.Equal(FontStretches.Expanded, TextBlock.GetFontStretch(d));
		Assert.Equal(4, TextBlock.GetBaselineOffset(d));
		Assert.Equal(18, TextBlock.GetLineHeight(d));
		Assert.Equal(LineStackingStrategy.BlockLineHeight, TextBlock.GetLineStackingStrategy(d));
		Assert.Equal(TextAlignment.Center, TextBlock.GetTextAlignment(d));
		Assert.Equal("Mono", TextBlock.GetFontFamily(d).Source);
	});

	[Fact]
	public void TextBlockAccessorsRejectNull()
	{
		Assert.Throws<ArgumentNullException>(() => TextBlock.SetFontSize(null!, 1));
		Assert.Throws<ArgumentNullException>(() => TextBlock.GetForeground(null!));
		Assert.Throws<ArgumentNullException>(() => TextBlock.SetTextAlignment(null!, TextAlignment.Left));
	}

	[Fact]
	public void SystemColorsHaveUniqueKeysAndCachedBrushes() => Run(() =>
	{
		var keys = new System.Collections.Generic.List<ResourceKey>
		{
			SystemColors.WindowColorKey, SystemColors.WindowBrushKey,
			SystemColors.InactiveSelectionHighlightBrushKey, SystemColors.InactiveSelectionHighlightTextColorKey,
			SystemColors.ControlColorKey, SystemColors.HighlightBrushKey,
		};
		Assert.Equal(keys.Count, keys.Distinct().Count());
		Assert.Same(SystemColors.WindowBrush, SystemColors.WindowBrush);
		Assert.Equal(SystemColors.WindowColor, SystemColors.WindowBrush.Color);
		Assert.Equal(Color.FromRgb(0, 120, 215), SystemColors.HighlightColor);
		Assert.Equal(Color.FromRgb(240, 240, 240), SystemColors.MenuBarColor);
	});

	[Fact]
	public void SystemColorKeysResolveThroughFindResource() => Run(() =>
	{
		var b = new Border();
		Assert.Same(SystemColors.WindowBrush, b.FindResource(SystemColors.WindowBrushKey));
		Assert.Equal(SystemColors.HighlightColor, b.FindResource(SystemColors.HighlightColorKey));
		Assert.Same(SystemColors.ControlBrush, b.TryFindResource(SystemColors.ControlBrushKey));
	});

	[Fact]
	public void SystemColorKeyUserResourceWins() => Run(() =>
	{
		var b = new Border();
		var mine = new SolidColorBrush(Color.FromRgb(9, 9, 9));
		b.Resources[SystemColors.WindowBrushKey] = mine;
		Assert.Same(mine, b.FindResource(SystemColors.WindowBrushKey));
	});

	[Fact]
	public void PathMarkupConvertsToGeometryForClipAndData() => Run(() =>
	{
		var g = System.Windows.Markup.XamlValueConverter.Convert(
			typeof(Image), UIElement.ClipProperty, "M0,150 L100,0 200,150Z");
		Assert.IsType<PathGeometry>(g);
		var data = System.Windows.Markup.XamlValueConverter.Convert(
			typeof(Path), Path.DataProperty, "M10,100 C 100,0 200,200 300,100 z");
		Assert.IsType<PathGeometry>(data);
	});
}
