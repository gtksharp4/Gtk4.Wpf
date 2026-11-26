using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class RichTextBox
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new RichTextBoxAutomationPeer(this);

	// Reads the GTK buffer; the widget is created first so the document has been loaded into it.
	internal string AutomationText
	{
		get
		{
			_ = Widget;
			return _textView.Buffer.Text ?? string.Empty;
		}
	}

	internal (int Start, int Length) AutomationSelection
	{
		get
		{
			_ = Widget;
			_textView.Buffer.GetSelectionBounds(out var start, out var end);
			return (start.Offset, end.Offset - start.Offset);
		}
	}

	bool _suppressMarkSet;

	internal void AutomationSelect(int start, int length)
	{
		_ = Widget;
		var buffer = _textView.Buffer;
		int count = buffer.CharCount;
		start = Math.Max(0, Math.Min(start, count));
		int end = Math.Max(start, Math.Min(start + length, count));
		// SelectRange sets both marks; the event is raised once, not per mark.
		_suppressMarkSet = true;
		try { buffer.SelectRange(buffer.GetIterAtOffset(start), buffer.GetIterAtOffset(end)); }
		finally { _suppressMarkSet = false; }
		RaiseSelectionChanged();
	}
}
