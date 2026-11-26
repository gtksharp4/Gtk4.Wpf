using System.Windows.Automation.Provider;
using System.Windows.Automation.Text;

namespace System.Windows.Automation.Peers;

/// <summary>The text a <see cref="TextProvider"/> exposes: the content and the selection of one control.</summary>
interface ITextSource
{
	string Text { get; }

	int SelectionStart { get; }

	int SelectionLength { get; }

	void Select(int start, int length);
}

/// <summary>
/// The <see cref="ITextProvider"/> of a text control. The model is flat character offsets: <c>Line</c> and
/// <c>Paragraph</c> units both split at line feeds (soft wrapping is not modelled), and <c>Format</c>, <c>Page</c>
/// and <c>Document</c> are the whole text. Visible ranges are the whole text, and a point maps to the start.
/// </summary>
sealed class TextProvider : ITextProvider
{
	readonly ITextSource _source;

	internal TextProvider(ITextSource source, AutomationPeer peer)
	{
		_source = source;
		Peer = peer;
	}

	internal AutomationPeer Peer { get; }

	internal string Text => _source.Text;

	internal void Select(int start, int length) => _source.Select(start, length);

	public ITextRangeProvider DocumentRange => new TextRangeProvider(this, 0, Text.Length);

	public SupportedTextSelection SupportedTextSelection => SupportedTextSelection.Single;

	public ITextRangeProvider[] GetSelection()
	{
		int length = Text.Length;
		int start = Math.Max(0, Math.Min(_source.SelectionStart, length));
		int end = Math.Max(start, Math.Min(start + _source.SelectionLength, length));
		return new ITextRangeProvider[] { new TextRangeProvider(this, start, end) };
	}

	public ITextRangeProvider[] GetVisibleRanges() => new[] { DocumentRange };

	public ITextRangeProvider RangeFromChild(IRawElementProviderSimple childElement)
	{
		if (childElement == null) throw new ArgumentNullException(nameof(childElement));
		return DocumentRange;
	}

	public ITextRangeProvider RangeFromPoint(Point screenLocation) => new TextRangeProvider(this, 0, 0);
}
