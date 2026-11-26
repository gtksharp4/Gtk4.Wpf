using System.Collections.Generic;
using System.Windows.Media;
using System.Xml.Linq;

namespace System.Windows.Documents;

static class TextRangeXamlReader
{
	internal static List<Paragraph> Read(string xaml)
	{
		XElement root;
		try { root = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace).Root!; }
		catch (Xml.XmlException ex) { throw new ArgumentException("The XAML content is not well-formed.", ex); }
		var result = new List<Paragraph>();
		Blocks(root, result);
		return result;
	}

	static void Blocks(XElement container, List<Paragraph> result)
	{
		Paragraph? loose = null;
		foreach (var node in container.Nodes())
		{
			if (node is XElement el && (el.Name.LocalName == "Section" || el.Name.LocalName == "Paragraph"))
			{
				loose = null;
				if (el.Name.LocalName == "Section") Blocks(el, result);
				else
				{
					var p = new Paragraph();
					Attributes(el, p);
					Inlines(el, p.Inlines);
					result.Add(p);
				}
			}
			else if (node is XElement || (node is XText t && !string.IsNullOrWhiteSpace(t.Value)))
			{
				if (loose == null) result.Add(loose = new Paragraph());
				InlineNode(node, loose.Inlines);
			}
		}
	}

	static void Inlines(XElement parent, InlineCollection into)
	{
		foreach (var node in parent.Nodes()) InlineNode(node, into);
	}

	static void InlineNode(XNode node, InlineCollection into)
	{
		if (node is XText t)
		{
			if (t.Value.Length > 0) into.Add(new Run { Text = t.Value });
			return;
		}
		if (!(node is XElement el)) return;
		string name = el.Name.LocalName;
		switch (name)
		{
			case "Run":
				var run = new Run { Text = el.Value };
				Attributes(el, run);
				into.Add(run);
				break;
			case "LineBreak":
				into.Add(new LineBreak());
				break;
			case "Span":
			case "Bold":
			case "Italic":
			case "Underline":
			case "Hyperlink":
				Span span = name == "Bold" ? new Bold() : name == "Italic" ? new Italic()
					: name == "Underline" ? new Underline() : name == "Hyperlink"
					? new Hyperlink { NavigateUri = NavigateUri(el) } : new Span();
				Attributes(el, span);
				Inlines(el, span.Inlines);
				into.Add(span);
				break;
		}
	}

	static Uri? NavigateUri(XElement el)
	{
		var v = (string?)el.Attribute("NavigateUri");
		return v != null && Uri.TryCreate(v, UriKind.RelativeOrAbsolute, out var u) ? u : null;
	}

	static void Attributes(XElement el, TextElement target)
	{
		foreach (var a in el.Attributes())
		{
			try
			{
				switch (a.Name.LocalName)
				{
					case "FontFamily": target.FontFamily = new FontFamily(a.Value); break;
					case "FontSize": target.FontSize = FontConverters.ParseFontSize(a.Value); break;
					case "FontWeight": target.FontWeight = FontConverters.ParseFontWeight(a.Value); break;
					case "FontStyle": target.FontStyle = FontConverters.ParseFontStyle(a.Value); break;
					case "TextDecorations":
						if (a.Value.IndexOf("Underline", StringComparison.OrdinalIgnoreCase) >= 0)
							target.TextDecorations = TextDecorations.Underline;
						break;
					case "Foreground":
						var color = System.Windows.Markup.MediaConverters.ParseColor(a.Value);
						target.Foreground = new SolidColorBrush(color);
						break;
				}
			}
			catch (FormatException)
			{
				// An unreadable attribute is dropped.
			}
		}
	}
}
