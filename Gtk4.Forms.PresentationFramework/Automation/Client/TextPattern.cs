using System.Windows.Automation.Provider;
using System.Windows.Automation.Text;

namespace System.Windows.Automation;

/// <summary>
/// Reads the text of a control and its selection as ranges. Text attributes (<c>AutomationTextAttribute</c>,
/// <c>FindAttribute</c>, <c>GetAttributeValue</c>) are not part of this layer.
/// </summary>
public class TextPattern : BasePattern
{
	public static readonly AutomationPattern Pattern = TextPatternIdentifiers.Pattern;
	public static readonly AutomationEvent TextSelectionChangedEvent = TextPatternIdentifiers.TextSelectionChangedEvent;
	public static readonly AutomationEvent TextChangedEvent = TextPatternIdentifiers.TextChangedEvent;
	public static readonly object MixedAttributeValue = new object();

	readonly ITextProvider _provider;

	internal TextPattern(AutomationElement element, ITextProvider provider, bool cached) : base(element)
	{
		_provider = provider;
	}

	public TextPatternRange DocumentRange => Run(() => new TextPatternRange(this, _provider.DocumentRange));

	public SupportedTextSelection SupportedTextSelection => Run(() => _provider.SupportedTextSelection);

	public TextPatternRange[] GetSelection() => Run(() => Wrap(_provider.GetSelection()));

	public TextPatternRange[] GetVisibleRanges() => Run(() => Wrap(_provider.GetVisibleRanges()));

	public TextPatternRange RangeFromChild(AutomationElement childElement)
	{
		if (childElement == null) throw new ArgumentNullException(nameof(childElement));
		return Run(() => new TextPatternRange(this, _provider.RangeFromChild(childElement.Peer.ProviderFromPeer(
			childElement.Peer))));
	}

	public TextPatternRange RangeFromPoint(Point screenLocation) =>
		Run(() => new TextPatternRange(this, _provider.RangeFromPoint(screenLocation)));

	internal TextPatternRange[] Wrap(ITextRangeProvider[] ranges)
	{
		var result = new TextPatternRange[ranges.Length];
		for (int i = 0; i < ranges.Length; i++) result[i] = new TextPatternRange(this, ranges[i]);
		return result;
	}

	internal T Call<T>(Func<T> work) => Run(work);
}

/// <summary>A span of the text of a <see cref="TextPattern"/>.</summary>
public class TextPatternRange
{
	readonly TextPattern _pattern;
	readonly ITextRangeProvider _range;

	internal TextPatternRange(TextPattern pattern, ITextRangeProvider range)
	{
		_pattern = pattern;
		_range = range;
	}

	public TextPattern TextPattern => _pattern;

	T Run<T>(Func<T> work) => _pattern.Call(work);

	void Run(Action work) => _pattern.Call(() =>
	{
		work();
		return true;
	});

	public TextPatternRange Clone() => Run(() => new TextPatternRange(_pattern, _range.Clone()));

	public bool Compare(TextPatternRange range) =>
		Run(() => _range.Compare((range ?? throw new ArgumentNullException(nameof(range)))._range));

	public int CompareEndpoints(TextPatternRangeEndpoint endpoint, TextPatternRange targetRange,
		TextPatternRangeEndpoint targetEndpoint) =>
		Run(() => _range.CompareEndpoints(endpoint,
			(targetRange ?? throw new ArgumentNullException(nameof(targetRange)))._range, targetEndpoint));

	public void ExpandToEnclosingUnit(TextUnit unit) => Run(() => _range.ExpandToEnclosingUnit(unit));

	public TextPatternRange? FindText(string text, bool backward, bool ignoreCase)
	{
		if (text == null) throw new ArgumentNullException(nameof(text));
		return Run(() => _range.FindText(text, backward, ignoreCase) is { } found
			? new TextPatternRange(_pattern, found) : null);
	}

	public double[] GetBoundingRectangles() => Run(_range.GetBoundingRectangles);

	public AutomationElement GetEnclosingElement() =>
		Run(() => AutomationElement.FromProvider(_range.GetEnclosingElement(), CacheRequest.ActiveOrNull)!);

	public AutomationElement[] GetChildren() => Run(() => AutomationElement.FromProviders(_range.GetChildren()));

	public string GetText(int maxLength) => Run(() => _range.GetText(maxLength));

	public int Move(TextUnit unit, int count) => Run(() => _range.Move(unit, count));

	public int MoveEndpointByUnit(TextPatternRangeEndpoint endpoint, TextUnit unit, int count) =>
		Run(() => _range.MoveEndpointByUnit(endpoint, unit, count));

	public void MoveEndpointByRange(TextPatternRangeEndpoint endpoint, TextPatternRange targetRange,
		TextPatternRangeEndpoint targetEndpoint) =>
		Run(() => _range.MoveEndpointByRange(endpoint,
			(targetRange ?? throw new ArgumentNullException(nameof(targetRange)))._range, targetEndpoint));

	public void Select() => Run(_range.Select);

	public void AddToSelection() => Run(_range.AddToSelection);

	public void RemoveFromSelection() => Run(_range.RemoveFromSelection);

	public void ScrollIntoView(bool alignToTop) => Run(() => _range.ScrollIntoView(alignToTop));
}
