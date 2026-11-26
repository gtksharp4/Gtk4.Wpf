using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Appearance;

public class AppearanceDpTests
{
	[Fact]
	public void Control_and_TextBlock_share_TextElement_dp_instances()
	{
		Assert.Same(TextElement.FontSizeProperty, Control.FontSizeProperty);
		Assert.Same(TextElement.FontSizeProperty, TextBlock.FontSizeProperty);
		Assert.Same(TextElement.ForegroundProperty, Control.ForegroundProperty);
		Assert.Same(TextElement.FontFamilyProperty, TextBlock.FontFamilyProperty);
		Assert.Same(TextElement.FontWeightProperty, Control.FontWeightProperty);
		Assert.Same(TextElement.FontStyleProperty, TextBlock.FontStyleProperty);
		Assert.Same(TextElement.FontStretchProperty, Control.FontStretchProperty);
	}

	[Fact]
	public void Background_and_border_dps_are_shared_with_panel_and_border()
	{
		Assert.Same(Panel.BackgroundProperty, Control.BackgroundProperty);
		Assert.Same(Panel.BackgroundProperty, TextBlock.BackgroundProperty);
		Assert.Same(Panel.BackgroundProperty, Border.BackgroundProperty);
		Assert.Same(Border.BorderBrushProperty, Control.BorderBrushProperty);
		Assert.Same(Border.BorderThicknessProperty, Control.BorderThicknessProperty);
	}

	[Fact]
	public void Defaults_match_wpf()
	{
		var b = new Button();
		Assert.Null(b.Background);
		Assert.Null(b.BorderBrush);
		Assert.Equal(new Thickness(), b.BorderThickness);
		Assert.Equal(12.0, b.FontSize);
		Assert.Equal("Segoe UI", b.FontFamily.Source);
		Assert.Equal(FontStyles.Normal, b.FontStyle);
		Assert.Equal(FontWeights.Normal, b.FontWeight);
		Assert.Equal(FontStretches.Normal, b.FontStretch);
		Assert.Same(SystemColors.ControlTextBrush, b.Foreground);
	}

	[Fact]
	public void TextBlock_Panel_and_Border_defaults()
	{
		var tb = new TextBlock();
		Assert.Null(tb.Background);
		Assert.Equal(12.0, tb.FontSize);
		Assert.Null(new StackPanel().Background);
		var border = new Border();
		Assert.Null(border.Background);
		Assert.Null(border.BorderBrush);
		Assert.Equal(new CornerRadius(), border.CornerRadius);
	}

	[Fact]
	public void Clr_wrappers_round_trip_through_dps()
	{
		var red = Brushes.Red;
		var b = new Button
		{
			Background = red, FontSize = 20, FontWeight = FontWeights.Bold,
			FontFamily = new FontFamily("Arial"), FontStyle = FontStyles.Italic,
			FontStretch = FontStretches.Condensed, BorderThickness = new Thickness(2),
		};
		Assert.Same(red, b.GetValue(Control.BackgroundProperty));
		Assert.Equal(20.0, b.GetValue(TextElement.FontSizeProperty));
		Assert.Equal(FontWeights.Bold, b.GetValue(Control.FontWeightProperty));
		Assert.Equal("Arial", b.FontFamily.Source);
		Assert.Equal(FontStyles.Italic, b.FontStyle);
		Assert.Equal(FontStretches.Condensed, b.FontStretch);
		Assert.Equal(new Thickness(2), b.BorderThickness);
	}

	[Fact]
	public void Name_lookup_resolves_per_owner()
	{
		Assert.Same(TextElement.FontSizeProperty, DependencyProperty.FindByName("FontSize", typeof(Button)));
		Assert.Same(TextElement.FontSizeProperty, DependencyProperty.FindByName("FontSize", typeof(TextBlock)));
		Assert.Same(Panel.BackgroundProperty, DependencyProperty.FindByName("Background", typeof(Grid)));
		Assert.Same(Panel.BackgroundProperty, DependencyProperty.FindByName("Background", typeof(Border)));
		Assert.Same(Border.BorderBrushProperty, DependencyProperty.FindByName("BorderBrush", typeof(Button)));
		Assert.Null(DependencyProperty.FindByName("FontSize", typeof(Grid)));
	}

	[Fact]
	public void TextElement_attached_accessors()
	{
		var b = new Button();
		TextElement.SetFontSize(b, 30);
		TextElement.SetFontWeight(b, FontWeights.Black);
		var blue = Brushes.Blue;
		TextElement.SetForeground(b, blue);
		Assert.Equal(30.0, b.FontSize);
		Assert.Equal(FontWeights.Black, TextElement.GetFontWeight(b));
		Assert.Same(blue, b.Foreground);
		Assert.Equal(12.0, TextElement.GetFontSize(new TextBlock()));
	}

	[Fact]
	public void Border_attached_inheriting_flags()
	{
		var meta = (FrameworkPropertyMetadata)TextElement.FontSizeProperty.GetMetadata(typeof(Button));
		Assert.True(meta.Inherits);
		Assert.False(((FrameworkPropertyMetadata)Panel.BackgroundProperty.GetMetadata(typeof(Button))).Inherits);
	}
}
