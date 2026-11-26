using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using WpfListItem = System.Windows.Documents.ListItem;

namespace Gtk4.PresentationFramework.Tests.Documents;

/// <summary>Documents whose rendering the rebase onto FrameworkContentElement must not change.</summary>
static class FlowContentSamples
{
	static Paragraph Para(params Inline[] inlines)
	{
		var p = new Paragraph();
		foreach (var i in inlines) p.Inlines.Add(i);
		return p;
	}

	internal static FlowDocument Styled()
	{
		var doc = new FlowDocument
		{
			FontFamily = new FontFamily("Serif"), FontSize = 14, TextAlignment = TextAlignment.Center,
		};
		var p = Para(
			new Run { Text = "plain & <esc> ", Foreground = Brushes.Red },
			new Run { Text = "semi", FontWeight = FontWeights.SemiBold },
			new Run { Text = "bold", FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic },
			new Run { Text = "deco", TextDecorations = TextDecorations.Underline },
			new LineBreak(),
			new Run { Text = "fam", FontFamily = new FontFamily("Mono") });
		p.Margin = new Thickness(1, 2, 3, 4);
		p.LineHeight = 20;
		doc.Blocks.Add(p);
		var span = new Span();
		span.Inlines.Add(new Run { Text = "in span" });
		var bold = new Bold();
		bold.Inlines.Add(new Run { Text = "b" });
		span.Inlines.Add(bold);
		doc.Blocks.Add(Para(span, new Italic(), new Underline()));
		return doc;
	}

	internal static FlowDocument Directional()
	{
		var doc = new FlowDocument { FlowDirection = FlowDirection.RightToLeft };
		var p = Para(new Run { Text = "rtl" });
		p.Language = XmlLanguage.GetLanguage("he-IL");
		doc.Blocks.Add(p);
		var s = new Section { FlowDirection = FlowDirection.LeftToRight };
		s.Language = XmlLanguage.GetLanguage("fr-FR");
		s.Blocks.Add(Para(new Run { Text = "ltr" }));
		var inner = Para();
		var span = new Span { FlowDirection = FlowDirection.RightToLeft, Language = XmlLanguage.GetLanguage("ar") };
		span.Inlines.Add(new Run { Text = "x" });
		inner.Inlines.Add(span);
		s.Blocks.Add(inner);
		doc.Blocks.Add(s);
		return doc;
	}

	internal static FlowDocument Lists()
	{
		var doc = new FlowDocument();
		foreach (var style in new[] { TextMarkerStyle.Disc, TextMarkerStyle.Decimal, TextMarkerStyle.UpperRoman })
		{
			var list = new List { MarkerStyle = style };
			for (int i = 0; i < 2; i++)
			{
				var item = new WpfListItem();
				item.Blocks.Add(Para(new Run { Text = "item" + i }));
				list.ListItems.Add(item);
			}
			doc.Blocks.Add(list);
		}
		var rtlList = new List { FlowDirection = FlowDirection.RightToLeft };
		var rtlItem = new WpfListItem { FlowDirection = FlowDirection.RightToLeft };
		rtlItem.Blocks.Add(Para(new Run { Text = "r" }));
		rtlList.ListItems.Add(rtlItem);
		doc.Blocks.Add(rtlList);
		return doc;
	}
}
