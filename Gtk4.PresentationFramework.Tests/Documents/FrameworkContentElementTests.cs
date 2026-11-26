using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Documents;

public class FrameworkContentElementTests
{
	[Fact]
	public void Flow_content_is_framework_content_elements()
	{
		Assert.IsAssignableFrom<DependencyObject>(new Run());
		Assert.IsAssignableFrom<FrameworkContentElement>(new Paragraph());
		Assert.IsAssignableFrom<FrameworkContentElement>(new FlowDocument());
		Assert.IsAssignableFrom<ContentElement>(new Section());
	}

	[Fact]
	public void Name_tag_and_runtime_name_property()
	{
		var p = new Paragraph { Name = "para", Tag = 5 };
		Assert.Equal("para", p.Name);
		Assert.Equal(5, p.Tag);
		var attr = (RuntimeNamePropertyAttribute?)Attribute.GetCustomAttribute(
			typeof(FrameworkContentElement), typeof(RuntimeNamePropertyAttribute));
		Assert.Equal("Name", attr?.Name);
	}

	[Fact]
	public void DataContext_inherits_from_the_logical_parent()
	{
		var doc = new FlowDocument { DataContext = "ctx" };
		var p = new Paragraph();
		var run = new Run("x");
		p.Inlines.Add(run);
		Assert.Null(run.DataContext);
		doc.Blocks.Add(p);
		Assert.Equal("ctx", p.DataContext);
		Assert.Equal("ctx", run.DataContext);
		doc.DataContext = "other";
		Assert.Equal("other", run.DataContext);
		run.DataContext = "own";
		Assert.Equal("own", run.DataContext);
		Assert.Same(p, run.Parent);
		Assert.Same(doc, p.Parent);
	}

	[Fact]
	public void Removing_a_child_clears_its_parent_and_inherited_value()
	{
		var doc = new FlowDocument { DataContext = "ctx" };
		var p = new Paragraph();
		doc.Blocks.Add(p);
		doc.Blocks.Remove(p);
		Assert.Null(p.Parent);
		Assert.Null(p.DataContext);
	}

	[Fact]
	public void Language_defaults_inherits_and_rejects_null()
	{
		var doc = new FlowDocument { Language = XmlLanguage.GetLanguage("fr-FR") };
		var p = new Paragraph();
		Assert.Equal("en-us", p.Language.IetfLanguageTag);
		doc.Blocks.Add(p);
		Assert.Equal("fr-fr", p.Language.IetfLanguageTag);
		Assert.Throws<ArgumentNullException>(() => p.Language = null!);
	}

	[Fact]
	public void Language_reaches_markup_only_when_set_on_the_element()
	{
		var doc = new FlowDocument { Language = XmlLanguage.GetLanguage("fr-FR") };
		var p = new Paragraph(new Run("a"));
		doc.Blocks.Add(p);
		Assert.Equal("a", doc.BuildMarkup());
		p.Language = XmlLanguage.GetLanguage("de-DE");
		Assert.Equal("<span lang=\"de-de\">a</span>", doc.BuildMarkup());
	}

	[Fact]
	public void Style_setters_apply_and_unapply()
	{
		var style = new Style(typeof(Paragraph));
		style.Setters.Add(new Setter(TextElement.FontSizeProperty, 30.0));
		var p = new Paragraph { Style = style };
		Assert.Equal(30.0, p.FontSize);
		p.Style = null;
		Assert.Equal(12.0, p.FontSize);
	}

	[Fact]
	public void Resources_are_found_up_the_logical_tree_to_the_hosting_element()
	{
		var host = new System.Windows.Controls.FlowDocumentScrollViewer();
		host.Resources["k"] = "fromHost";
		var doc = new FlowDocument();
		host.Document = doc;
		var p = new Paragraph();
		doc.Blocks.Add(p);
		Assert.Equal("fromHost", p.FindResource("k"));
		doc.Resources["k"] = "fromDoc";
		Assert.Equal("fromDoc", p.TryFindResource("k"));
		Assert.Null(p.TryFindResource("missing"));
		Assert.Throws<InvalidOperationException>(() => p.FindResource("missing"));
	}

	[Fact]
	public void Cursor_tooltip_and_forcecursor_are_stored()
	{
		var p = new Paragraph();
		Assert.False(p.ForceCursor);
		Assert.Null(p.Cursor);
		Assert.Null(p.ToolTip);
		p.ForceCursor = true;
		p.ToolTip = "tip";
		Assert.True(p.ForceCursor);
		Assert.Equal("tip", p.ToolTip);
	}

	[Fact]
	public void Instance_FontSize_is_the_attached_property_and_renders_when_set()
	{
		var p = new Paragraph(new Run("t"));
		Assert.Equal(12.0, p.FontSize);
		Assert.Equal("t", Render(p));
		p.FontSize = 20;
		Assert.Equal(20.0, TextElement.GetFontSize(p));
		Assert.Equal("<span size=\"15360\">t</span>", Render(p));
		Assert.Throws<ArgumentException>(() => p.FontSize = -1);
	}

	[Fact]
	public void Background_is_a_dependency_property()
	{
		var p = new Paragraph();
		var brush = Brushes.Red;
		p.Background = brush;
		Assert.Same(brush, p.GetValue(TextElement.BackgroundProperty));
	}

	[Fact]
	public void Dependency_property_route_and_clr_properties_agree_on_text_elements()
	{
		var run = new Run("x");
		Assert.Equal(FontWeights.Normal, run.FontWeight);
		Assert.Same(SystemColors.ControlTextBrush, run.Foreground);
		Assert.Equal("Segoe UI", run.FontFamily.Source);
		run.SetValue(TextElement.FontWeightProperty, FontWeights.Bold);
		run.SetValue(TextElement.FontStyleProperty, FontStyles.Italic);
		run.SetValue(TextElement.FontFamilyProperty, new FontFamily("Serif"));
		var red = new SolidColorBrush(Colors.Red);
		run.SetValue(TextElement.ForegroundProperty, red);
		Assert.Equal(FontWeights.Bold, run.FontWeight);
		Assert.Equal(FontStyles.Italic, run.FontStyle);
		Assert.Equal("Serif", run.FontFamily.Source);
		Assert.Same(red, run.Foreground);
		Assert.Equal("<span foreground=\"#FF0000\"><b><i>x</i></b></span>", Render(new Paragraph(run)));
		run.ClearValue(TextElement.FontWeightProperty);
		run.ClearValue(TextElement.ForegroundProperty);
		Assert.Same(DependencyProperty.UnsetValue, run.ReadLocalValue(TextElement.FontWeightProperty));
		Assert.Same(SystemColors.ControlTextBrush, run.Foreground);
	}

	[Fact]
	public void FlowDocument_font_members_read_the_effective_value_over_the_dps()
	{
		var doc = new FlowDocument();
		Assert.Equal(12.0, doc.FontSize);
		Assert.Equal("Segoe UI", doc.FontFamily.Source);
		Assert.Same(SystemColors.ControlTextBrush, doc.Foreground);
		doc.SetValue(FlowDocument.FontSizeProperty, 10.0);
		Assert.Equal(10.0, doc.FontSize);
		doc.ClearValue(FlowDocument.FontSizeProperty);
		Assert.Equal(12.0, doc.FontSize);
		doc.FontFamily = new FontFamily("Palatino Linotype");
		Assert.Equal("Palatino Linotype", ((FontFamily)doc.GetValue(FlowDocument.FontFamilyProperty)!).Source);
		Assert.Throws<ArgumentException>(() => doc.FontFamily = null!);
		var p = new Paragraph();
		doc.Blocks.Add(p);
		doc.SetValue(FlowDocument.FontSizeProperty, 22.0);
		Assert.Equal(22.0, p.FontSize);
	}

	static string Render(Paragraph p)
	{
		var d = new FlowDocument();
		d.Blocks.Add(p);
		return d.BuildMarkup();
	}
}
