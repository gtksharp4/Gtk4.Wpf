using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;

namespace System.Windows.Documents;

/// <summary>
/// WPF's XAML text-range format: a Section in the presentation namespace holding Paragraph, Run, Span, Bold,
/// Italic, Underline, Hyperlink and LineBreak with font attributes. Written and read with plain XML, not XamlReader.
/// </summary>
static class TextRangeXaml
{
	internal const string Ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

	internal static string Write(List<Paragraph> paragraphs)
	{
		var sb = new StringBuilder();
		var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = false };
		using (var w = XmlWriter.Create(sb, settings))
		{
			w.WriteStartElement("Section", Ns);
			w.WriteAttributeString("xml", "space", null, "preserve");
			foreach (var p in paragraphs) WriteElement(w, "Paragraph", p, p.Inlines);
		}
		return sb.ToString();
	}

	static void WriteElement(XmlWriter w, string name, TextElement el, InlineCollection children)
	{
		w.WriteStartElement(name);
		WriteAttributes(w, el);
		foreach (var i in children)
			switch (i)
			{
				case Run r:
					w.WriteStartElement("Run");
					WriteAttributes(w, r);
					w.WriteString(r.Text);
					w.WriteEndElement();
					break;
				case LineBreak _:
					w.WriteStartElement("LineBreak");
					w.WriteEndElement();
					break;
				case Span s:
					WriteElement(w, s is Hyperlink ? "Hyperlink" : s is Bold ? "Bold" : s is Italic ? "Italic"
						: s is Underline ? "Underline" : "Span", s, s.Inlines);
					break;
			}
		w.WriteEndElement();
	}

	static void WriteAttributes(XmlWriter w, TextElement el)
	{
		var family = TextLocalValues.Family(el);
		if (family != null) w.WriteAttributeString("FontFamily", family.Source);
		var size = TextLocalValues.Size(el);
		if (size != null)
			w.WriteAttributeString("FontSize", size.Value.ToString("R", Globalization.CultureInfo.InvariantCulture));
		var weight = TextLocalValues.Weight(el);
		if (weight != null) w.WriteAttributeString("FontWeight", WeightName(weight.Value));
		var style = TextLocalValues.Style(el);
		if (style != null) w.WriteAttributeString("FontStyle", style.Value.ToString());
		if (TextLocalValues.Brush(el) is SolidColorBrush b)
			w.WriteAttributeString("Foreground", $"#{b.Color.A:X2}{b.Color.R:X2}{b.Color.G:X2}{b.Color.B:X2}");
		if (TextRangeRtf.IsUnderlined(el)) w.WriteAttributeString("TextDecorations", "Underline");
		if (el is Hyperlink h && h.NavigateUri != null) w.WriteAttributeString("NavigateUri", h.NavigateUri.OriginalString);
	}

	internal static string WeightName(FontWeight w)
	{
		switch (w.ToOpenTypeWeight())
		{
			case 100: return "Thin";
			case 200: return "ExtraLight";
			case 300: return "Light";
			case 400: return "Normal";
			case 500: return "Medium";
			case 600: return "SemiBold";
			case 700: return "Bold";
			case 800: return "ExtraBold";
			case 900: return "Black";
			case 950: return "ExtraBlack";
			default: return w.ToOpenTypeWeight().ToString();
		}
	}
}
