using System.Windows;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.FlowDirectionTests;

/// <summary>Isolates cannot span a paragraph separator, so each paragraph carries its own direction.</summary>
public class FlowDirectionParagraphTests
{
	const string Rli = "\u2067";
	const string Lri = "\u2066";
	const string Pdi = "\u2069";
	const string Rlm = "\u200F";
	const string Lrm = "\u200E";

	static Paragraph Para(string text, FlowDirection? direction = null)
	{
		var p = new Paragraph();
		p.Inlines.Add(new Run { Text = text });
		if (direction is FlowDirection d) p.FlowDirection = d;
		return p;
	}

	static void AssertEachPartIsolated(string markup, string separator, string[] texts)
	{
		string[] parts = markup.Split(new[] { separator }, System.StringSplitOptions.RemoveEmptyEntries);
		Assert.Equal(texts.Length, parts.Length);
		for (int i = 0; i < texts.Length; i++)
			Assert.Equal(Rlm + Rli + texts[i] + Pdi, parts[i]);
	}

	[Fact]
	public void A_FlowDocument_isolates_each_paragraph()
	{
		var doc = new FlowDocument { FlowDirection = FlowDirection.RightToLeft };
		doc.Blocks.Add(Para("one"));
		doc.Blocks.Add(Para("two"));

		AssertEachPartIsolated(doc.BuildMarkup(), "\n\n", new[] { "one", "two" });
	}

	[Fact]
	public void A_Section_hands_its_direction_to_each_paragraph()
	{
		var section = new Section { FlowDirection = FlowDirection.RightToLeft };
		section.Blocks.Add(Para("one"));
		section.Blocks.Add(Para("two"));
		var doc = new FlowDocument();
		doc.Blocks.Add(section);

		AssertEachPartIsolated(doc.BuildMarkup(), "\n", new[] { "one", "two" });
	}

	[Fact]
	public void A_List_isolates_each_item_line()
	{
		var list = new List { FlowDirection = FlowDirection.RightToLeft, MarkerStyle = TextMarkerStyle.Decimal };
		foreach (string text in new[] { "one", "two" })
		{
			var item = new ListItem();
			item.Blocks.Add(Para(text));
			list.ListItems.Add(item);
		}
		var doc = new FlowDocument();
		doc.Blocks.Add(list);

		AssertEachPartIsolated(doc.BuildMarkup(), "\n", new[] { "1 one", "2 two" });
	}

	[Fact]
	public void An_explicit_LeftToRight_paragraph_overrides_an_RTL_document()
	{
		var doc = new FlowDocument { FlowDirection = FlowDirection.RightToLeft };
		doc.Blocks.Add(Para("rtl"));
		doc.Blocks.Add(Para("ltr", FlowDirection.LeftToRight));

		string[] parts = doc.BuildMarkup().Split(new[] { "\n\n" }, System.StringSplitOptions.None);

		Assert.Equal(Rlm + Rli + "rtl" + Pdi, parts[0]);
		Assert.Equal(Lrm + Lri + "ltr" + Pdi, parts[1]);
	}

	[Fact]
	public void A_document_without_direction_emits_no_marks()
	{
		var doc = new FlowDocument();
		doc.Blocks.Add(Para("a"));
		doc.Blocks.Add(Para("b"));

		Assert.Equal("a\n\nb", doc.BuildMarkup());
	}
}
