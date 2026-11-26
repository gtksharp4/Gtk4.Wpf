using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class TextElementTypographyTests
{
	static (FlowDocument doc, Run run) Doc()
	{
		var doc = new FlowDocument { FontFamily = new FontFamily("Serif"), FontSize = 20 };
		var run = new Run("x");
		doc.Blocks.Add(new Paragraph(run));
		return (doc, run);
	}

	[Fact]
	public void An_inherited_value_reads_through_on_a_run()
	{
		var (doc, run) = Doc();
		Assert.Equal("Serif", run.FontFamily.Source);
		Assert.Equal(20.0, run.FontSize);
		Assert.Same(DependencyProperty.UnsetValue, run.ReadLocalValue(TextElement.FontFamilyProperty));
		doc.FontWeight = FontWeights.Bold;
		Assert.Equal(FontWeights.Bold, run.FontWeight);
	}

	[Fact]
	public void ClearValue_restores_inheritance()
	{
		var (_, run) = Doc();
		run.FontFamily = new FontFamily("Mono");
		run.FontSize = 8;
		Assert.Equal("Mono", run.FontFamily.Source);
		run.ClearValue(TextElement.FontFamilyProperty);
		run.ClearValue(TextElement.FontSizeProperty);
		Assert.Equal("Serif", run.FontFamily.Source);
		Assert.Equal(20.0, run.FontSize);
	}

	[Fact]
	public void Assigning_null_to_FontFamily_throws_as_in_WPF()
	{
		Assert.Throws<ArgumentException>(() => new Run("x").FontFamily = null!);
		Assert.Throws<ArgumentException>(() => new FlowDocument().FontFamily = null!);
	}

	[Fact]
	public void Typography_members_are_effective_values_and_never_null_or_NaN()
	{
		var run = new Run("x");
		Assert.Equal(FontWeights.Normal, run.FontWeight);
		Assert.Equal(FontStyles.Normal, run.FontStyle);
		Assert.Equal(FontStretches.Normal, run.FontStretch);
		Assert.Equal(12.0, run.FontSize);
		Assert.NotNull(run.Foreground);
		run.FontStretch = FontStretches.Condensed;
		Assert.Equal(FontStretches.Condensed, run.GetValue(TextElement.FontStretchProperty));
	}

	[Fact]
	public void The_xaml_round_trip_writes_no_inherited_attributes()
	{
		var (doc, run) = Doc();
		run.FontWeight = FontWeights.Bold;
		var ms = new MemoryStream();
		new TextRange(doc.ContentStart, doc.ContentEnd).Save(ms, DataFormats.Xaml);
		string xaml = Encoding.UTF8.GetString(ms.ToArray());
		Assert.Contains("FontWeight=\"Bold\"", xaml);
		Assert.DoesNotContain("FontFamily", xaml);
		Assert.DoesNotContain("FontSize", xaml);
		Assert.DoesNotContain("Foreground", xaml);
	}

	[Fact]
	public void Property_values_distinguish_local_from_inherited_on_a_range()
	{
		var (doc, run) = Doc();
		var range = new TextRange(doc.ContentStart, doc.ContentEnd);
		Assert.Equal("Serif", ((FontFamily)range.GetPropertyValue(TextElement.FontFamilyProperty)).Source);
		range.ApplyPropertyValue(TextElement.FontFamilyProperty, new FontFamily("Mono"));
		Assert.Equal("Mono", run.FontFamily.Source);
	}

	[Fact]
	public void Pango_markup_emits_only_local_formatting()
	{
		var (doc, run) = Doc();
		Assert.Equal("x", doc.BuildMarkup());
		run.Foreground = Brushes.Red;
		Assert.Equal("<span foreground=\"#FF0000\">x</span>", doc.BuildMarkup());
	}
}
