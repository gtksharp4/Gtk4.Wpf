using System.Windows.Automation.Provider;
using System.Windows.Automation.Text;

namespace System.Windows.Automation.Peers;

/// <summary>A span <c>[start, end)</c> of the text of a <see cref="TextProvider"/>.</summary>
sealed partial class TextRangeProvider : ITextRangeProvider
{
	readonly TextProvider _owner;
	int _start;
	int _end;

	internal TextRangeProvider(TextProvider owner, int start, int end)
	{
		_owner = owner;
		_start = start;
		_end = end;
	}

	// The text may have shrunk since the range was taken.
	void Normalize()
	{
		int length = _owner.Text.Length;
		_end = Math.Max(0, Math.Min(_end, length));
		_start = Math.Max(0, Math.Min(_start, _end));
	}

	public ITextRangeProvider Clone()
	{
		Normalize();
		return new TextRangeProvider(_owner, _start, _end);
	}

	TextRangeProvider Same(ITextRangeProvider range)
	{
		if (range is TextRangeProvider other && ReferenceEquals(other._owner, _owner)) return other;
		throw new ArgumentException("The range belongs to another text provider.", nameof(range));
	}

	public bool Compare(ITextRangeProvider range)
	{
		var other = Same(range);
		Normalize();
		other.Normalize();
		return other._start == _start && other._end == _end;
	}

	public int CompareEndpoints(TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange,
		TextPatternRangeEndpoint targetEndpoint) =>
		Math.Sign(Get(endpoint) - Same(targetRange).Get(targetEndpoint));

	int Get(TextPatternRangeEndpoint endpoint)
	{
		Normalize();
		return endpoint == TextPatternRangeEndpoint.Start ? _start : _end;
	}

	void Set(TextPatternRangeEndpoint endpoint, int value)
	{
		if (endpoint == TextPatternRangeEndpoint.Start)
		{
			_start = value;
			if (_end < _start) _end = _start;
		}
		else
		{
			_end = value;
			if (_start > _end) _start = _end;
		}
	}

	public void MoveEndpointByRange(TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange,
		TextPatternRangeEndpoint targetEndpoint) => Set(endpoint, Same(targetRange).Get(targetEndpoint));

	public string GetText(int maxLength)
	{
		if (maxLength < -1) throw new ArgumentOutOfRangeException(nameof(maxLength));
		Normalize();
		var text = _owner.Text.Substring(_start, _end - _start);
		return maxLength >= 0 && text.Length > maxLength ? text.Substring(0, maxLength) : text;
	}

	public ITextRangeProvider FindText(string text, bool backward, bool ignoreCase)
	{
		if (text == null) throw new ArgumentNullException(nameof(text));
		if (text.Length == 0) throw new ArgumentException("The text to find must not be empty.", nameof(text));
		var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
		Normalize();
		var inside = _owner.Text.Substring(_start, _end - _start);
		int at = backward ? inside.LastIndexOf(text, comparison) : inside.IndexOf(text, comparison);
		return at < 0 ? null! : new TextRangeProvider(_owner, _start + at, _start + at + text.Length);
	}

	// Text attributes (font, colour...) are not modelled: no range carries one.
	public ITextRangeProvider FindAttribute(int attribute, object value, bool backward) => null!;

	public object GetAttributeValue(int attribute) => AutomationElement.NotSupported;

	public double[] GetBoundingRectangles()
	{
		Normalize();
		if (_start == _end) return new double[0];
		var r = _owner.Peer.GetBoundingRectangle();
		return r.IsEmpty ? new double[0] : new[] { r.X, r.Y, r.Width, r.Height };
	}

	public IRawElementProviderSimple GetEnclosingElement() => _owner.Peer.ProviderFromPeer(_owner.Peer);

	public IRawElementProviderSimple[] GetChildren() => new IRawElementProviderSimple[0];

	public void Select()
	{
		Normalize();
		_owner.Select(_start, _end - _start);
	}

	public void AddToSelection() => throw new InvalidOperationException("The text supports a single selection.");

	public void RemoveFromSelection() =>
		throw new InvalidOperationException("The text supports a single selection.");

	// The control scrolls its own caret into view; an arbitrary range cannot be scrolled to.
	public void ScrollIntoView(bool alignToTop) { }
}
