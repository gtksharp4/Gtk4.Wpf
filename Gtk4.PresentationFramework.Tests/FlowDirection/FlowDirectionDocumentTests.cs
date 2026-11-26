using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.FlowDirectionTests;

public class FlowDirectionDocumentTests
{
	const string Rli = "\u2067";
	const string Lri = "\u2066";
	const string Pdi = "\u2069";
	const string Rlm = "\u200F";
	const string Lrm = "\u200E";

	[Fact]
	public void Inline_FlowDirection_defaults_to_LeftToRight_and_validates()
	{
		var run = new Run();
		Assert.Equal(FlowDirection.LeftToRight, run.FlowDirection);
		run.FlowDirection = FlowDirection.RightToLeft;
		Assert.Equal(FlowDirection.RightToLeft, run.FlowDirection);
		Assert.Throws<System.ArgumentException>(() => run.FlowDirection = (FlowDirection)9);
	}

	[Fact]
	public void Run_with_explicit_direction_is_wrapped_in_a_bidi_isolate()
	{
		var tb = new TextBlock { FlowDirection = FlowDirection.RightToLeft };
		tb.Inlines.Add(new Run { Text = "ab", FlowDirection = FlowDirection.LeftToRight });
		Assert.Equal(Lri + "ab" + Pdi, tb.BuildMarkup());
		tb.Inlines.Clear();
		tb.Inlines.Add(new Run { Text = "ab", FlowDirection = FlowDirection.RightToLeft });
		Assert.Equal(Rli + "ab" + Pdi, tb.BuildMarkup());
	}

	[Fact]
	public void Run_without_explicit_direction_emits_no_marks()
	{
		var tb = new TextBlock();
		tb.Inlines.Add(new Run { Text = "ab" });
		Assert.Equal("ab", tb.BuildMarkup());
	}

	[Fact]
	public void Span_direction_wraps_its_children()
	{
		var span = new Span { FlowDirection = FlowDirection.RightToLeft };
		span.Inlines.Add(new Run { Text = "x" });
		var tb = new TextBlock();
		tb.Inlines.Add(span);
		Assert.Equal(Rli + "x" + Pdi, tb.BuildMarkup());
	}

	[Fact]
	public void Language_on_inline_defaults_to_en_us_and_emits_lang()
	{
		var span = new Span();
		Assert.Equal("en-us", span.Language.IetfLanguageTag);
		span.Language = XmlLanguage.GetLanguage("ar-SA");
		span.Inlines.Add(new Run { Text = "x" });
		var tb = new TextBlock();
		tb.Inlines.Add(span);
		Assert.Equal("<span lang=\"ar-sa\">x</span>", tb.BuildMarkup());
		Assert.Throws<System.ArgumentNullException>(() => span.Language = null!);
	}

	[Fact]
	public void FlowDocument_and_Block_carry_FlowDirection()
	{
		var doc = new FlowDocument();
		Assert.Equal(FlowDirection.LeftToRight, doc.FlowDirection);
		var p = new Paragraph();
		p.Inlines.Add(new Run { Text = "t" });
		doc.Blocks.Add(p);
		doc.FlowDirection = FlowDirection.RightToLeft;
		Assert.Equal(Rlm + Rli + "t" + Pdi, doc.BuildMarkup());
		doc = new FlowDocument();
		doc.Blocks.Add(p);
		p.FlowDirection = FlowDirection.RightToLeft;
		Assert.Equal(FlowDirection.RightToLeft, p.FlowDirection);
		Assert.Equal(Rlm + Rli + "t" + Pdi, doc.BuildMarkup());
		Assert.Throws<System.ArgumentException>(() => doc.FlowDirection = (FlowDirection)3);
	}
}
