using System.Collections.Generic;

namespace System.Windows.Documents;

/// <summary>
/// A position in a <see cref="FlowDocument"/> counted in WPF symbols: each element edge, embedded element and
/// text character is one symbol. Backed by an offset that re-anchors from the document's edit log, not by a
/// live text tree, so a pointer survives edits made through this API but not direct tree changes.
/// </summary>
public sealed partial class TextPointer
{
	readonly FlowDocument _doc;
	readonly LogicalDirection _direction;
	int _offset;
	int _applied;

	internal TextPointer(FlowDocument doc, int offset, LogicalDirection direction)
	{
		_doc = doc;
		_offset = offset;
		_direction = direction;
		_applied = doc.TextEditCount;
	}

	internal FlowDocument Document => _doc;

	/// <summary>The symbol offset from the document start, after replaying missed edits and clamping.</summary>
	internal int Offset
	{
		get
		{
			for (; _applied < _doc.TextEditCount; _applied++)
				_offset = Reanchor(_offset, _direction, _doc.GetTextEdit(_applied));
			// The symbol total is cached per content version, so repeated reads do not rebuild the tree.
			_offset = Math.Max(0, Math.Min(_offset, _doc.SymbolTotal));
			return _offset;
		}
	}

	static int Reanchor(int offset, LogicalDirection dir, TextEdit e)
	{
		bool forward = dir == LogicalDirection.Forward;
		if (offset < e.Position || (offset == e.Position && e.Removed == 0 && !forward)) return offset;
		if (offset >= e.Position + e.Removed && !(e.Removed > 0 && offset == e.Position + e.Removed && !forward))
			return offset + e.Inserted - e.Removed;
		return forward ? e.Position + e.Inserted : e.Position;
	}

	/// <summary>The direction this pointer sticks to.</summary>
	public LogicalDirection LogicalDirection => _direction;

	/// <summary>A pointer at the start of the document this pointer belongs to.</summary>
	public TextPointer DocumentStart => _doc.ContentStart;

	/// <summary>A pointer at the end of the document this pointer belongs to.</summary>
	public TextPointer DocumentEnd => _doc.ContentEnd;

	/// <summary>True when <paramref name="textPosition"/> belongs to the same document.</summary>
	public bool IsInSameDocument(TextPointer textPosition)
	{
		if (textPosition == null) throw new ArgumentNullException(nameof(textPosition));
		return ReferenceEquals(_doc, textPosition._doc);
	}

	/// <summary>Compares positions: negative before, zero equal, positive after.</summary>
	/// <exception cref="ArgumentException">The pointers belong to different documents.</exception>
	public int CompareTo(TextPointer position)
	{
		if (position == null) throw new ArgumentNullException(nameof(position));
		if (!IsInSameDocument(position))
			throw new ArgumentException("The position is in a different document.", nameof(position));
		return Offset.CompareTo(position.Offset);
	}

	/// <summary>The number of symbols from this pointer to <paramref name="position"/>.</summary>
	public int GetOffsetToPosition(TextPointer position)
	{
		if (position == null) throw new ArgumentNullException(nameof(position));
		if (!IsInSameDocument(position))
			throw new ArgumentException("The position is in a different document.", nameof(position));
		return position.Offset - Offset;
	}

	/// <summary>A pointer <paramref name="offset"/> symbols away, or null when outside the document.</summary>
	public TextPointer? GetPositionAtOffset(int offset) => GetPositionAtOffset(offset, _direction);

	/// <summary>A pointer <paramref name="offset"/> symbols away in the given direction, or null if outside.</summary>
	public TextPointer? GetPositionAtOffset(int offset, LogicalDirection direction)
	{
		long target = (long)Offset + offset;
		if (target < 0 || target > _doc.SymbolTotal) return null;
		return new TextPointer(_doc, (int)target, direction);
	}

	/// <summary>What lies next to this pointer in <paramref name="direction"/>.</summary>
	public TextPointerContext GetPointerContext(LogicalDirection direction) =>
		TextTree.Context(_doc.Symbols, Offset, direction);

	/// <summary>
	/// The element that contains this position (a TextElement, ListItem or the FlowDocument); null at the root level
	/// of a detached element.
	/// </summary>
	/// <remarks>Typed object: the flow elements are not DependencyObjects in this layer.</remarks>
	public object? Parent
	{
		get
		{
			var open = TextTree.OpenElements(_doc.Symbols, Offset);
			if (open.Count == 0) return _doc.DetachedRoot != null ? null : _doc;
			return open[open.Count - 1];
		}
	}

	/// <summary>The innermost <see cref="Documents.Paragraph"/> containing this position, or null.</summary>
	public Paragraph? Paragraph
	{
		get
		{
			var open = TextTree.OpenElements(_doc.Symbols, Offset);
			for (int i = open.Count - 1; i >= 0; i--)
				if (open[i] is Paragraph p) return p;
			return null;
		}
	}

	/// <inheritdoc/>
	public override string ToString() => $"TextPointer({Offset}, {_direction})";
}
