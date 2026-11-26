using System.Windows.Automation.Text;

namespace System.Windows.Automation.Provider;

/// <summary>Supports controls that expose a text range model, such as a text box.</summary>
public interface ITextProvider
{
	ITextRangeProvider[] GetSelection();

	ITextRangeProvider[] GetVisibleRanges();

	ITextRangeProvider RangeFromChild(IRawElementProviderSimple childElement);

	ITextRangeProvider RangeFromPoint(Point screenLocation);

	ITextRangeProvider DocumentRange { get; }

	SupportedTextSelection SupportedTextSelection { get; }
}

/// <summary>A contiguous span of the text of an <see cref="ITextProvider"/>.</summary>
public interface ITextRangeProvider
{
	ITextRangeProvider Clone();

	bool Compare(ITextRangeProvider range);

	int CompareEndpoints(TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange,
		TextPatternRangeEndpoint targetEndpoint);

	void ExpandToEnclosingUnit(TextUnit unit);

	ITextRangeProvider FindAttribute(int attribute, object value, bool backward);

	ITextRangeProvider FindText(string text, bool backward, bool ignoreCase);

	object GetAttributeValue(int attribute);

	double[] GetBoundingRectangles();

	IRawElementProviderSimple GetEnclosingElement();

	string GetText(int maxLength);

	int Move(TextUnit unit, int count);

	int MoveEndpointByUnit(TextPatternRangeEndpoint endpoint, TextUnit unit, int count);

	void MoveEndpointByRange(TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange,
		TextPatternRangeEndpoint targetEndpoint);

	void Select();

	void AddToSelection();

	void RemoveFromSelection();

	void ScrollIntoView(bool alignToTop);

	IRawElementProviderSimple[] GetChildren();
}
