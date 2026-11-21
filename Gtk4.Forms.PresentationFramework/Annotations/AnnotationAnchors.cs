using System.Collections.Generic;
using System.Globalization;
using System.Windows.Documents;

namespace System.Windows.Annotations;

/// <summary>Reads and writes the CharacterRange locator that anchors annotations on flow content.</summary>
static class AnnotationAnchors
{
	/// <summary>Stored segments are (start, end) symbol offsets from the document start, as in WPF.</summary>
	internal static ContentLocator Create(IList<(int Start, int End)> segments)
	{
		var part = new ContentLocatorPart(AnnotationXmlConstants.CharacterRange);
		for (int i = 0; i < segments.Count; i++)
			part.NameValuePairs.Add(AnnotationXmlConstants.SegmentAttribute + i.ToString(CultureInfo.InvariantCulture),
				segments[i].Start.ToString(CultureInfo.InvariantCulture) + AnnotationXmlConstants.Separator
				+ segments[i].End.ToString(CultureInfo.InvariantCulture));
		part.NameValuePairs.Add(AnnotationXmlConstants.CountAttribute, segments.Count.ToString(CultureInfo.InvariantCulture));
		var locator = new ContentLocator();
		locator.Parts.Add(part);
		return locator;
	}

	/// <summary>Replaces the annotation's anchor with one covering <paramref name="segments"/>.</summary>
	internal static void Set(Annotation annotation, IList<(int Start, int End)> segments)
	{
		var anchor = new AnnotationResource();
		anchor.ContentLocators.Add(Create(segments));
		annotation.Anchors.Clear();
		annotation.Anchors.Add(anchor);
	}

	/// <summary>The segments of the first anchor that holds a character range, or null.</summary>
	internal static List<(int Start, int End)>? Read(Annotation annotation, out AnnotationResource? anchor)
	{
		foreach (var resource in annotation.Anchors)
		{
			foreach (var baseLocator in resource.ContentLocators)
			foreach (var locator in baseLocator.Flatten())
			foreach (var part in locator.Parts)
			{
				if (!part.PartType.Equals(AnnotationXmlConstants.CharacterRange)) continue;
				var segments = Segments(part);
				if (segments == null) continue;
				anchor = resource;
				return segments;
			}
		}
		anchor = null;
		return null;
	}

	static List<(int Start, int End)>? Segments(ContentLocatorPart part)
	{
		var pairs = part.NameValuePairs;
		if (!pairs.TryGetValue(AnnotationXmlConstants.CountAttribute, out var countText)
			|| !int.TryParse(countText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int count)) return null;
		var result = new List<(int, int)>();
		for (int i = 0; i < count; i++)
		{
			if (!pairs.TryGetValue(AnnotationXmlConstants.SegmentAttribute + i.ToString(CultureInfo.InvariantCulture),
				out var text)) return null;
			var bits = text.Split(AnnotationXmlConstants.Separator);
			if (bits.Length != 2 || !int.TryParse(bits[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int a)
				|| !int.TryParse(bits[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int b)) return null;
			result.Add((a, b));
		}
		return result.Count == 0 ? null : result;
	}

	/// <summary>The segments of a range, as one segment.</summary>
	internal static List<(int Start, int End)> Of(TextRange range) =>
		new List<(int, int)> { (range.Start.Offset, range.End.Offset) };

	internal static TextRange ToRange(FlowDocument doc, int start, int end) =>
		new TextRange(new TextPointer(doc, start, LogicalDirection.Backward),
			new TextPointer(doc, end, LogicalDirection.Forward));

	/// <summary>The part of <paramref name="segments"/> left after cutting out [start, end).</summary>
	internal static List<(int Start, int End)> Subtract(IEnumerable<(int Start, int End)> segments, int start, int end)
	{
		var rest = new List<(int, int)>();
		foreach (var (s, e) in segments)
		{
			if (e <= start || s >= end) { rest.Add((s, e)); continue; }
			if (s < start) rest.Add((s, start));
			if (e > end) rest.Add((end, e));
		}
		return rest;
	}

	internal static bool Overlaps(IEnumerable<(int Start, int End)> segments, int start, int end)
	{
		foreach (var (s, e) in segments)
		{
			if (start == end) { if (s <= start && start <= e) return true; }
			else if (s < end && e > start) return true;
		}
		return false;
	}
}
