using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Documents;

public class FlowDocumentDpTests
{
	[Fact]
	public void FlowDocument_defaults_match_WPF()
	{
		var d = new FlowDocument();
		Assert.True(double.IsNaN(d.ColumnWidth));
		Assert.True(double.IsNaN(d.ColumnGap));
		Assert.Equal(0.0, d.ColumnRuleWidth);
		Assert.Null(d.ColumnRuleBrush);
		Assert.True(d.IsColumnWidthFlexible);
		Assert.True(double.IsNaN(d.PageWidth));
		Assert.True(double.IsNaN(d.PageHeight));
		Assert.Equal(0.0, d.MinPageWidth);
		Assert.Equal(0.0, d.MinPageHeight);
		Assert.Equal(double.PositiveInfinity, d.MaxPageWidth);
		Assert.Equal(double.PositiveInfinity, d.MaxPageHeight);
		Assert.False(d.IsHyphenationEnabled);
		Assert.False(d.IsOptimalParagraphEnabled);
		Assert.True(double.IsNaN(d.LineHeight));
		Assert.Equal(LineStackingStrategy.MaxHeight, d.LineStackingStrategy);
		Assert.Null(d.Background);
		Assert.Equal(FontStyles.Normal, d.FontStyle);
		Assert.Equal(FontWeights.Normal, d.FontWeight);
		Assert.Equal(FontStretches.Normal, d.FontStretch);
		Assert.Null(d.TextEffects);
	}

	[Fact]
	public void FlowDocument_values_round_trip()
	{
		var d = new FlowDocument
		{
			ColumnWidth = 300, ColumnGap = 20, ColumnRuleWidth = 15, ColumnRuleBrush = Brushes.LightGray,
			IsColumnWidthFlexible = false, PageWidth = 700, PageHeight = 800, MinPageWidth = 400, MaxPageWidth = 1000,
			IsHyphenationEnabled = true, IsOptimalParagraphEnabled = true, LineHeight = 12,
			Background = Brushes.Azure, FontStyle = FontStyles.Italic, FontWeight = FontWeights.Bold,
			TextEffects = new TextEffectCollection { new TextEffect { PositionCount = 3 } },
		};
		Assert.Equal(300, d.ColumnWidth);
		Assert.Equal(20, d.ColumnGap);
		Assert.False(d.IsColumnWidthFlexible);
		Assert.True(d.IsHyphenationEnabled);
		Assert.True(Block.GetIsHyphenationEnabled(d));
		Assert.Equal(1000, d.MaxPageWidth);
		Assert.Equal(FontWeights.Bold, d.FontWeight);
		Assert.Equal(3, d.TextEffects![0].PositionCount);
	}

	[Theory]
	[InlineData(nameof(FlowDocument.ColumnWidth), -1.0)]
	[InlineData(nameof(FlowDocument.ColumnWidth), double.PositiveInfinity)]
	[InlineData(nameof(FlowDocument.ColumnGap), -0.5)]
	[InlineData(nameof(FlowDocument.ColumnRuleWidth), -1.0)]
	[InlineData(nameof(FlowDocument.ColumnRuleWidth), double.NaN)]
	[InlineData(nameof(FlowDocument.PageWidth), -2.0)]
	[InlineData(nameof(FlowDocument.PageHeight), double.NegativeInfinity)]
	[InlineData(nameof(FlowDocument.MinPageWidth), double.NaN)]
	[InlineData(nameof(FlowDocument.MinPageHeight), -1.0)]
	[InlineData(nameof(FlowDocument.MaxPageWidth), -1.0)]
	[InlineData(nameof(FlowDocument.MaxPageHeight), double.NaN)]
	[InlineData(nameof(FlowDocument.LineHeight), 0.0)]
	[InlineData(nameof(FlowDocument.LineHeight), 200000.0)]
	public void FlowDocument_rejects_invalid_values(string property, double value)
	{
		var d = new FlowDocument();
		var dp = DependencyProperty.FindByName(property, typeof(FlowDocument))!;
		Assert.Throws<ArgumentException>(() => d.SetValue(dp, value));
	}

	[Fact]
	public void FlowDocument_accepts_NaN_for_automatic_sizes()
	{
		var d = new FlowDocument { ColumnWidth = 5, PageWidth = 5 };
		d.ColumnWidth = double.NaN;
		d.PageWidth = double.NaN;
		Assert.True(double.IsNaN(d.ColumnWidth));
	}

	[Fact]
	public void Block_defaults_match_WPF()
	{
		var p = new Paragraph();
		Assert.False(p.BreakPageBefore);
		Assert.False(p.BreakColumnBefore);
		Assert.Null(p.BorderBrush);
		Assert.Equal(new Thickness(0), p.BorderThickness);
		Assert.Equal(WrapDirection.None, p.ClearFloaters);
		Assert.False(p.IsHyphenationEnabled);
		Assert.Equal(0.0, p.TextIndent);
		Assert.False(p.KeepTogether);
		Assert.False(p.KeepWithNext);
		Assert.Equal(0, p.MinOrphanLines);
		Assert.Equal(0, p.MinWidowLines);
		Assert.True(new Section().HasTrailingParagraphBreakOnPaste);
	}

	[Fact]
	public void Block_properties_store_and_validate()
	{
		var s = new Section { BreakPageBefore = true, BreakColumnBefore = true, ClearFloaters = WrapDirection.Both };
		Assert.True(s.BreakPageBefore);
		Assert.Equal(WrapDirection.Both, s.ClearFloaters);
		s.HasTrailingParagraphBreakOnPaste = false;
		Assert.False(s.HasTrailingParagraphBreakOnPaste);
		Assert.Throws<ArgumentException>(() => s.BorderThickness = new Thickness(-1));
		Assert.Throws<ArgumentException>(() => s.ClearFloaters = (WrapDirection)9);
		var p = new Paragraph { TextIndent = 25, KeepTogether = true, KeepWithNext = true, MinOrphanLines = 2 };
		Assert.Equal(25.0, p.TextIndent);
		Assert.Throws<ArgumentException>(() => p.MinOrphanLines = -1);
		Assert.Throws<ArgumentException>(() => p.MinWidowLines = -1);
		Assert.Throws<ArgumentException>(() => p.TextIndent = double.NaN);
	}

	[Fact]
	public void Hyphenation_inherits_from_the_document()
	{
		var d = new FlowDocument { IsHyphenationEnabled = true };
		var p = new Paragraph();
		d.Blocks.Add(p);
		Assert.True(p.IsHyphenationEnabled);
	}

	[Fact]
	public void View_markup_wraps_local_font_style_and_weight_only()
	{
		var d = new FlowDocument();
		d.Blocks.Add(new Paragraph(new Run("x")));
		Assert.Equal("x", d.BuildViewMarkup());
		d.FontWeight = FontWeights.Bold;
		d.FontStyle = FontStyles.Italic;
		Assert.Equal("<span style=\"italic\" weight=\"700\">x</span>", d.BuildViewMarkup());
		Assert.Equal("x", d.BuildMarkup());
	}

	[Fact]
	public void Existing_ClR_members_keep_their_semantics()
	{
		var d = new FlowDocument { FontFamily = new FontFamily("Serif"), FontSize = 14 };
		Assert.Equal("Serif", d.FontFamily.Source);
		Assert.Equal(14.0, d.FontSize);
		Assert.Equal(12.0, new FlowDocument().FontSize);
	}
}
