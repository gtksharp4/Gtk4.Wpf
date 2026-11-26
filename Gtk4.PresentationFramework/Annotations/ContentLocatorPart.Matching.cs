using System.Globalization;

namespace System.Windows.Annotations;

public sealed partial class ContentLocatorPart
{
	/// <summary>
	/// True when this query part matches <paramref name="part"/>: equal, or, when the query sets
	/// <c>IncludeOverlaps</c>, a character range that overlaps this one.
	/// </summary>
	internal bool Matches(ContentLocatorPart part)
	{
		if (!_pairs.TryGetValue(AnnotationXmlConstants.IncludeOverlaps, out var flag)
			|| !bool.TryParse(flag, out var overlaps) || !overlaps)
			return Equals(part);
		if (ReferenceEquals(part, this)) return true;
		if (!_type.Equals(part._type)) return false;
		Range(this, out int wantStart, out int wantEnd);
		Range(part, out int start, out int end);
		if (wantStart == start && wantEnd == end) return true;
		if (wantStart == int.MinValue) return false;
		return (start >= wantStart && start <= wantEnd) || (start < wantStart && end >= wantStart);
	}

	/// <summary>The lowest start and highest end over a character-range part's segments.</summary>
	internal static void Range(ContentLocatorPart part, out int min, out int max)
	{
		min = int.MinValue;
		max = int.MinValue;
		if (!part._pairs.TryGetValue(AnnotationXmlConstants.CountAttribute, out var countText)
			|| !int.TryParse(countText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int count)) return;
		int lo = int.MaxValue, hi = int.MinValue;
		for (int i = 0; i < count; i++)
		{
			if (!part._pairs.TryGetValue(AnnotationXmlConstants.SegmentAttribute + i.ToString(CultureInfo.InvariantCulture),
				out var segment)) return;
			var bits = segment.Split(AnnotationXmlConstants.Separator);
			if (bits.Length != 2 || !int.TryParse(bits[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int a)
				|| !int.TryParse(bits[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int b)) return;
			lo = Math.Min(lo, a);
			hi = Math.Max(hi, b);
		}
		if (count > 0)
		{
			min = lo;
			max = hi;
		}
	}
}
