using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.FlowDirectionTests;

/// <summary>Pango starts a new paragraph at every newline, so each line needs its own direction.</summary>
public class FlowDirectionLineTests : WpfGtkTestBase
{
	const string Rli = "⁧";
	const string Lri = "⁦";
	const string Pdi = "⁩";
	const string Rlm = "‏";
	const string Lrm = "‎";

	[Fact]
	public void A_LineBreak_inside_an_RTL_paragraph_keeps_the_direction_on_the_next_line()
	{
		var p = new Paragraph { FlowDirection = FlowDirection.RightToLeft };
		p.Inlines.Add(new Run { Text = "a" });
		p.Inlines.Add(new LineBreak());
		p.Inlines.Add(new Run { Text = "b" });
		var doc = new FlowDocument();
		doc.Blocks.Add(p);

		Assert.Equal(Rlm + Rli + "a" + Pdi + "\n" + Rlm + Rli + "b" + Pdi, doc.BuildMarkup());
	}

	[Fact]
	public void A_Span_with_its_own_direction_closes_its_isolate_at_a_LineBreak()
	{
		var span = new Span { FlowDirection = FlowDirection.RightToLeft };
		span.Inlines.Add(new Run { Text = "a" });
		span.Inlines.Add(new LineBreak());
		span.Inlines.Add(new Run { Text = "b" });
		var tb = new TextBlock();
		tb.Inlines.Add(span);

		Assert.Equal(Rli + "a" + Pdi + "\n" + Rli + "b" + Pdi, tb.BuildMarkup());
		Assert.DoesNotContain("\n" + Rlm, tb.BuildMarkup(), System.StringComparison.Ordinal);
	}

	[Fact]
	public void A_ListItem_honours_its_own_direction()
	{
		var list = new List { FlowDirection = FlowDirection.RightToLeft, MarkerStyle = TextMarkerStyle.None };
		var item = new ListItem { FlowDirection = FlowDirection.LeftToRight };
		var p = new Paragraph();
		p.Inlines.Add(new Run { Text = "x" });
		item.Blocks.Add(p);
		list.ListItems.Add(item);
		var doc = new FlowDocument();
		doc.Blocks.Add(list);

		Assert.StartsWith(Lrm + Lri, doc.BuildMarkup());
	}

	[Fact]
	public void A_nested_Section_in_a_ListItem_is_isolated_per_line()
	{
		var section = new Section();
		foreach (string t in new[] { "a", "b" })
		{
			var p = new Paragraph();
			p.Inlines.Add(new Run { Text = t });
			section.Blocks.Add(p);
		}
		var item = new ListItem();
		item.Blocks.Add(section);
		var list = new List { FlowDirection = FlowDirection.RightToLeft, MarkerStyle = TextMarkerStyle.None };
		list.ListItems.Add(item);
		var doc = new FlowDocument();
		doc.Blocks.Add(list);

		string markup = doc.BuildMarkup();

		Assert.Contains("a" + Pdi + "\n" + Rlm + Rli + "b", markup);
	}

	[Fact]
	public void A_multi_line_RTL_TextBlock_marks_every_line() => Run(() =>
	{
		var block = new TextBlock { Text = "a\nb", FlowDirection = FlowDirection.RightToLeft };
		_ = block.Widget;

		var label = (Gtk.Label)block.Widget;
		Assert.Equal(Rlm + "a\n" + Rlm + "b", label.Text);
		Assert.Equal("a\nb", block.Text);
	});
}
