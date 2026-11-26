using System.Globalization;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml.Linq;

namespace System.Windows.Xps.Packaging;

/// <summary>Serialises a page built in memory to fixed page markup. Only Canvas and Glyphs are supported.</summary>
static class XpsPageWriter
{
	static readonly XNamespace Ns = XpsNames.Markup;

	/// <param name="page">The page to serialise.</param>
	/// <param name="addFont">Called with the package and part name of each font the page uses.</param>
	/// <exception cref="NotSupportedException">The page holds an element other than Canvas or Glyphs, or a brush
	/// other than a solid colour.</exception>
	internal static XElement Write(FixedPage page, Action<XpsPackage, string> addFont)
	{
		var root = new XElement(Ns + "FixedPage",
			new XAttribute("Width", Number(page.Width)), new XAttribute("Height", Number(page.Height)));
		foreach (UIElement child in page.Children)
			root.Add(Element(child, addFont));
		return root;
	}

	static XElement Element(UIElement element, Action<XpsPackage, string> addFont)
	{
		XElement result;
		switch (element)
		{
			case Canvas canvas:
				result = new XElement(Ns + "Canvas");
				foreach (UIElement child in canvas.Children)
					result.Add(Element(child, addFont));
				break;
			case Glyphs glyphs:
				result = Glyph(glyphs, addFont);
				break;
			default:
				throw new NotSupportedException("An in-memory page cannot be saved with a "
					+ element.GetType().Name + "; only Canvas and Glyphs are serialised.");
		}
		if (element.Opacity != 1)
			result.SetAttributeValue("Opacity", Number(element.Opacity));
		if (element.RenderTransform != null && !element.RenderTransform.Value.IsIdentity)
		{
			var m = element.RenderTransform.Value;
			result.SetAttributeValue("RenderTransform", string.Join(",", Number(m.M11), Number(m.M12),
				Number(m.M21), Number(m.M22), Number(m.OffsetX), Number(m.OffsetY)));
		}
		return result;
	}

	static XElement Glyph(Glyphs g, Action<XpsPackage, string> addFont)
	{
		var e = new XElement(Ns + "Glyphs",
			new XAttribute("OriginX", Number(g.OriginX)), new XAttribute("OriginY", Number(g.OriginY)),
			new XAttribute("FontRenderingEmSize", Number(g.FontRenderingEmSize)),
			new XAttribute("UnicodeString", g.UnicodeString));
		if (g.FontUri != null)
		{
			e.SetAttributeValue("FontUri", g.FontUri.OriginalString);
			if (g.Package != null)
				addFont(g.Package, XpsPackage.ResolveTarget("/", g.FontUri.OriginalString));
		}
		if (g.Indices.Length > 0)
			e.SetAttributeValue("Indices", g.Indices);
		if (g.Fill is SolidColorBrush solid)
			e.SetAttributeValue("Fill", solid.Color.ToString());
		else if (g.Fill != null)
			throw new NotSupportedException("Only solid colour fills of Glyphs are serialised.");
		return e;
	}

	static string Number(double value) => value.ToString("R", CultureInfo.InvariantCulture);
}
