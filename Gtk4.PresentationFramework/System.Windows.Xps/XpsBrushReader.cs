using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace System.Windows.Xps.Packaging;

/// <summary>Reads the brushes of an XPS page: colour attributes, resource references and brush elements.</summary>
sealed class XpsBrushReader
{
	static readonly XNamespace Ns = XpsNames.Markup;
	readonly XpsPackage _package;
	readonly Dictionary<string, XElement> _resources;
	readonly Dictionary<string, Brush?> _cache = new Dictionary<string, Brush?>();

	internal XpsBrushReader(XpsPackage package, Dictionary<string, XElement> resources)
	{
		_package = package;
		_resources = resources;
	}

	/// <summary>Reads a brush from an attribute value: a colour or <c>{StaticResource key}</c>.</summary>
	internal Brush? FromAttribute(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return null;
		value = value!.Trim();
		if (value.StartsWith("{StaticResource", StringComparison.Ordinal))
		{
			string key = value.Substring("{StaticResource".Length).Trim(' ', '}');
			if (_cache.TryGetValue(key, out var cached))
				return cached;
			Brush? brush = _resources.TryGetValue(key, out var element) ? FromElement(element) : null;
			_cache[key] = brush;
			return brush;
		}
		var color = XpsValues.Color(value);
		return color.HasValue ? new SolidColorBrush(color.Value) : null;
	}

	/// <summary>Reads a brush element; unsupported kinds give null and the element is skipped.</summary>
	internal Brush? FromElement(XElement element)
	{
		switch (element.Name.LocalName)
		{
			case "SolidColorBrush":
				var color = XpsValues.Color((string?)element.Attribute("Color"));
				return color.HasValue ? new SolidColorBrush(color.Value) : null;
			case "LinearGradientBrush":
				return Linear(element);
			case "ImageBrush":
				return Image(element);
			default:
				return null;
		}
	}

	Brush Linear(XElement e)
	{
		var brush = new LinearGradientBrush
		{
			StartPoint = XpsValues.Point((string?)e.Attribute("StartPoint"), new Point(0, 0)),
			EndPoint = XpsValues.Point((string?)e.Attribute("EndPoint"), new Point(1, 0)),
			MappingMode = XpsValues.Enum((string?)e.Attribute("MappingMode"), BrushMappingMode.RelativeToBoundingBox),
			SpreadMethod = XpsValues.Enum((string?)e.Attribute("SpreadMethod"), GradientSpreadMethod.Pad),
		};
		foreach (var stop in e.Descendants(Ns + "GradientStop"))
		{
			var c = XpsValues.Color((string?)stop.Attribute("Color")) ?? Colors.Transparent;
			brush.GradientStops.Add(new GradientStop(c, XpsValues.Double((string?)stop.Attribute("Offset"))));
		}
		return brush;
	}

	Brush? Image(XElement e)
	{
		string? path = (string?)e.Attribute("ImageSource");
		if (path == null)
			return null;
		BitmapImage image;
		try
		{
			string part = XpsPackage.ResolveTarget("/", path);
			image = new BitmapImage();
			image.BeginInit();
			image.StreamSource = new System.IO.MemoryStream(_package.ReadPart(part));
			image.CacheOption = BitmapCacheOption.OnLoad;
			image.EndInit();
		}
		catch (Exception ex) when (ex is System.IO.IOException || ex is InvalidOperationException)
		{
			return null;
		}
		var brush = new ImageBrush(image)
		{
			ViewportUnits = XpsValues.Enum((string?)e.Attribute("ViewportUnits"),
				BrushMappingMode.RelativeToBoundingBox),
			ViewboxUnits = XpsValues.Enum((string?)e.Attribute("ViewboxUnits"), BrushMappingMode.RelativeToBoundingBox),
			TileMode = XpsValues.Enum((string?)e.Attribute("TileMode"), TileMode.None),
		};
		var viewport = XpsValues.Rect((string?)e.Attribute("Viewport"));
		if (viewport.HasValue)
			brush.Viewport = viewport.Value;
		var viewbox = XpsValues.Rect((string?)e.Attribute("Viewbox"));
		if (viewbox.HasValue)
			brush.Viewbox = viewbox.Value;
		return brush;
	}

	/// <summary>Reads the first brush element child of a property element such as <c>Path.Fill</c>.</summary>
	internal Brush? FromPropertyElement(XElement property) =>
		property.Elements().Select(FromElement).FirstOrDefault(b => b != null);
}
