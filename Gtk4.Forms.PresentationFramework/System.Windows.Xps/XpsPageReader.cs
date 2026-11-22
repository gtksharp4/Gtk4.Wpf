using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace System.Windows.Xps.Packaging;

/// <summary>
/// Builds a <see cref="FixedPage"/> from a fixed page part. Supported: Canvas, Path and Glyphs with solid,
/// linear-gradient and image brushes, resource references, RenderTransform, Clip, Opacity and NavigateUri. Other
/// elements (radial and visual brushes, opacity masks, remote resource dictionaries) are skipped.
/// </summary>
sealed class XpsPageReader
{
	static readonly XNamespace Ns = XpsNames.Markup;
	static readonly XNamespace KeyNs = XpsNames.ResourceKey;
	readonly XpsPackage _package;
	readonly string _partName;
	XpsBrushReader _brushes = null!;

	internal XpsPageReader(XpsPackage package, string partName)
	{
		_package = package;
		_partName = partName;
	}

	internal FixedPage Read()
	{
		var root = _package.ReadXml(_partName).Root!;
		if (root.Name != Ns + "FixedPage")
			throw new InvalidOperationException("The part " + _partName + " is not a FixedPage.");
		var resources = new Dictionary<string, XElement>();
		foreach (var dictionary in root.Descendants(Ns + "ResourceDictionary"))
			foreach (var entry in dictionary.Elements().Where(x => x.Attribute(KeyNs + "Key") != null))
				resources[(string)entry.Attribute(KeyNs + "Key")!] = entry;
		_brushes = new XpsBrushReader(_package, resources);
		var uri = _package.PartUri(_partName);
		var page = new FixedPage
		{
			Width = XpsValues.Double((string?)root.Attribute("Width")),
			Height = XpsValues.Double((string?)root.Attribute("Height")),
			SourcePart = uri,
		};
		((IUriContext)page).BaseUri = uri;
		Fill(page.Children, root);
		return page;
	}

	void Fill(UIElementCollection children, XElement parent)
	{
		foreach (var e in parent.Elements().Where(x => x.Name.Namespace == Ns && !x.Name.LocalName.Contains('.')))
		{
			UIElement? child = Element(e);
			if (child != null)
				children.Add(child);
		}
	}

	UIElement? Element(XElement e)
	{
		UIElement? element;
		switch (e.Name.LocalName)
		{
			case "Canvas":
				var canvas = new Canvas();
				Fill(canvas.Children, e);
				element = canvas;
				break;
			case "Path":
				element = ReadPath(e);
				break;
			case "Glyphs":
				element = ReadGlyphs(e);
				break;
			default:
				return null;
		}
		Common(element, e);
		return element;
	}

	Path ReadPath(XElement e)
	{
		var path = new Path
		{
			Fill = Brush(e, "Fill"),
			Stroke = Brush(e, "Stroke"),
			StrokeThickness = XpsValues.Double((string?)e.Attribute("StrokeThickness"), 1),
		};
		path.Data = Geometry(e, "Data");
		if (e.Attribute("StrokeDashArray") != null)
			path.StrokeDashArray = DoubleCollection.Parse((string)e.Attribute("StrokeDashArray")!);
		path.StrokeDashCap = XpsValues.Enum((string?)e.Attribute("StrokeDashCap"), path.StrokeDashCap);
		path.StrokeStartLineCap = XpsValues.Enum((string?)e.Attribute("StrokeStartLineCap"), path.StrokeStartLineCap);
		path.StrokeEndLineCap = XpsValues.Enum((string?)e.Attribute("StrokeEndLineCap"), path.StrokeEndLineCap);
		path.StrokeLineJoin = XpsValues.Enum((string?)e.Attribute("StrokeLineJoin"), path.StrokeLineJoin);
		path.StrokeMiterLimit = XpsValues.Double((string?)e.Attribute("StrokeMiterLimit"), path.StrokeMiterLimit);
		return path;
	}

	Glyphs ReadGlyphs(XElement e)
	{
		var glyphs = new Glyphs
		{
			UnicodeString = (string?)e.Attribute("UnicodeString") ?? string.Empty,
			FontUri = e.Attribute("FontUri") == null ? null : new Uri((string)e.Attribute("FontUri")!, UriKind.RelativeOrAbsolute),
			FontRenderingEmSize = XpsValues.Double((string?)e.Attribute("FontRenderingEmSize"), 1),
			OriginX = XpsValues.Double((string?)e.Attribute("OriginX")),
			OriginY = XpsValues.Double((string?)e.Attribute("OriginY")),
			Indices = (string?)e.Attribute("Indices") ?? string.Empty,
			StyleSimulations = XpsValues.Enum((string?)e.Attribute("StyleSimulations"), StyleSimulations.None),
			BidiLevel = (int)XpsValues.Double((string?)e.Attribute("BidiLevel")),
			IsSideways = string.Equals((string?)e.Attribute("IsSideways"), "true", StringComparison.OrdinalIgnoreCase),
			Fill = Brush(e, "Fill"),
			Package = _package,
		};
		return glyphs;
	}

	Brush? Brush(XElement e, string name)
	{
		var attribute = _brushes.FromAttribute((string?)e.Attribute(name));
		if (attribute != null)
			return attribute;
		var property = e.Element(e.Name + "." + name);
		return property == null ? null : _brushes.FromPropertyElement(property);
	}

	Geometry? Geometry(XElement e, string name)
	{
		string? text = (string?)e.Attribute(name);
		if (text == null)
		{
			var figures = e.Element(e.Name + "." + name)?.Elements(Ns + "PathGeometry").FirstOrDefault();
			text = (string?)figures?.Attribute("Figures");
		}
		if (string.IsNullOrWhiteSpace(text))
			return null;
		// XPS may start the path with a fill rule (F0 or F1); the geometry model has no fill rule, so it is dropped.
		text = Regex.Replace(text!, @"^s*Fs*[01]s*", string.Empty);
		try
		{
			return Media.Geometry.Parse(text);
		}
		catch (FormatException)
		{
			return null;
		}
	}

	void Common(UIElement element, XElement e)
	{
		if (e.Attribute("Opacity") != null)
			element.Opacity = XpsValues.Double((string?)e.Attribute("Opacity"), 1);
		var matrix = XpsValues.Matrix((string?)e.Attribute("RenderTransform"))
			?? XpsValues.Matrix((string?)e.Element(e.Name + ".RenderTransform")?.Element(Ns + "MatrixTransform")
				?.Attribute("Matrix"));
		if (matrix.HasValue)
			element.RenderTransform = new MatrixTransform(matrix.Value);
		var clip = Geometry(e, "Clip");
		if (clip != null)
			element.Clip = clip;
		string? uri = (string?)e.Attribute("FixedPage.NavigateUri");
		if (uri != null)
			FixedPage.SetNavigateUri(element, new Uri(uri, UriKind.RelativeOrAbsolute));
	}
}
