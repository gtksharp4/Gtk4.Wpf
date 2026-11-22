using System.Linq;
using System.Windows.Documents;
using Xunit;
using WpfListItem = System.Windows.Documents.ListItem;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>Edits of a TextRange over a List inside an attached FlowDocument.</summary>
public class TextRangeAttachedListTests
{
	static Paragraph NewPara(string t)
	{
		var p = new Paragraph();
		p.Inlines.Add(new Run { Text = t });
		return p;
	}

	[Fact]
	public void Delete_across_two_items_drops_the_emptied_item()
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
		var doc = new FlowDocument();
		doc.Blocks.Add(list);

		new TextRange(p1.ContentStart.GetPositionAtOffset(2)!, p2.ContentStart.GetPositionAtOffset(2)!).Text = "";

		Assert.Single(list.ListItems);
		Assert.Same(i1, list.ListItems[0]);
		Assert.Equal("ad", new TextRange(doc.ContentStart, doc.ContentEnd).Text.Trim());
		Assert.All(list.ListItems, i => Assert.NotEmpty(i.Blocks));
	}
}
