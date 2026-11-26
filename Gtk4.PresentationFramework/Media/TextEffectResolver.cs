using System.Collections.Generic;
using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>Finds the text elements a range covers and prepares one effect target for each.</summary>
public static class TextEffectResolver
{
	/// <summary>
	/// Returns one target per run in the range. Each target holds a clone of <paramref name="effectTemplate"/> whose
	/// PositionStart and PositionCount are relative to that run; none is enabled yet, as in WPF.
	/// </summary>
	/// <exception cref="ArgumentNullException">An argument is null.</exception>
	/// <exception cref="ArgumentException">The pointers are in different documents, or start follows end.</exception>
	public static TextEffectTarget[] Resolve(TextPointer startPosition, TextPointer endPosition,
		TextEffect effectTemplate)
	{
		if (startPosition == null) throw new ArgumentNullException(nameof(startPosition));
		if (endPosition == null) throw new ArgumentNullException(nameof(endPosition));
		if (effectTemplate == null) throw new ArgumentNullException(nameof(effectTemplate));
		if (!startPosition.IsInSameDocument(endPosition))
			throw new ArgumentException("The positions belong to different documents.", nameof(endPosition));
		if (startPosition.CompareTo(endPosition) > 0)
			throw new ArgumentException("The start position follows the end position.", nameof(startPosition));

		int from = startPosition.Offset;
		int to = endPosition.Offset;
		var targets = new List<TextEffectTarget>();
		foreach (var sym in TextTree.Build(startPosition.Document))
		{
			if (sym.Kind != TextSymKind.Text || !(sym.Node is Run run)) continue;
			int lo = Math.Max(from, sym.Offset);
			int hi = Math.Min(to, sym.End);
			if (hi <= lo) continue;
			var effect = (TextEffect)effectTemplate.Clone();
			effect.PositionStart = lo - sym.Offset;
			effect.PositionCount = hi - lo;
			targets.Add(new TextEffectTarget(run, effect));
		}
		return targets.ToArray();
	}
}
