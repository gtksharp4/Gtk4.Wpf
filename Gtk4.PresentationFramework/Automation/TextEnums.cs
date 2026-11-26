namespace System.Windows.Automation.Text;

/// <summary>The end of a text range an operation applies to.</summary>
public enum TextPatternRangeEndpoint
{
	Start,
	End,
}

/// <summary>A unit of text a range can move or expand by.</summary>
public enum TextUnit
{
	Character,
	Format,
	Word,
	Line,
	Paragraph,
	Page,
	Document,
}
