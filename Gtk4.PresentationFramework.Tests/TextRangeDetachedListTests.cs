using System.Linq;
using System.Windows.Documents;
using Xunit;
using WpfListItem = System.Windows.Documents.ListItem;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Edits of a TextRange over a detached List root.</summary>
public class TextRangeDetachedListTests
{
	static Paragraph NewPara(string t)
	{
		var p = new Paragraph();
		p.Inlines.Add(new Run { Text = t });
		return p;
	}

	static (List list, Paragraph p1, Paragraph p2) Build()
	{
		var p1 = NewPara("ab");
		var p2 = NewPara("cd");
		var i1 = new WpfListItem();
		i1.Blocks.Add(p1);
		var i2 = new WpfListItem();
		i2.Blocks.Add(p2);
		var list = new List();
		list.ListItems.Add(i1);
		list.ListItems.Add(i2);
		return (list, p1, p2);
	}

	[Fact]
	public void Delete_across_two_items_removes_the_emptied_paragraph()
	{
		var (list, p1, p2) = Build();
		var range = new TextRange(p1.ContentStart.GetPositionAtOffset(2)!, p2.ContentStart.GetPositionAtOffset(2)!);

		range.Text = "";

		Assert.DoesNotContain(list.ListItems, i => i.Blocks.Any(b => ReferenceEquals(b, p2)));
		Assert.Equal("ad", new TextRange(list.ContentStart, list.ContentEnd).Text.Trim());
		Assert.Single(list.ListItems);
	}

	[Fact]
	public void Setting_the_full_range_text_keeps_a_list_item()
	{
		var (list, _, _) = Build();
		var range = new TextRange(list.ContentStart, list.ContentEnd);

		range.Text = "x";

		Assert.Equal("x", range.Text.TrimEnd((char)13, (char)10)); // the paragraph break stays in the range
		Assert.Single(list.ListItems);
		Assert.Single(list.ListItems[0].Blocks);
	}

	[Fact]
	public void Multi_line_text_inside_an_item_produces_two_paragraphs_in_it()
	{
		var (list, p1, _) = Build();
		var range = new TextRange(p1.ContentStart.GetPositionAtOffset(2)!, p1.ContentStart.GetPositionAtOffset(2)!);

		range.Text = "a\nb";

		Assert.Equal(2, list.ListItems[0].Blocks.Count);
	}
}
