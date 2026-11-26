namespace System.Windows.Controls;

/// <summary>How <see cref="FlowDocumentReader"/> lays out its document.</summary>
public enum FlowDocumentReaderViewingMode
{
	/// <summary>One page at a time.</summary>
	Page,
	/// <summary>Two pages side by side.</summary>
	TwoPage,
	/// <summary>One continuous scrolling column.</summary>
	Scroll,
}
