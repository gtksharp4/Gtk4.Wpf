using System.Collections.Generic;
using System.Windows.Automation.Peers;
using System.Windows.Documents;

namespace System.Windows.Controls;

public partial class TextBlock
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new TextBlockAutomationPeer(this);

	// Every hyperlink among the inlines, nested spans included.
	internal IEnumerable<Hyperlink> AutomationHyperlinks()
	{
		var found = new List<Hyperlink>();
		Collect(Inlines, found);
		return found;
	}

	static void Collect(IEnumerable<Inline> inlines, List<Hyperlink> into)
	{
		foreach (var inline in inlines)
		{
			if (inline is Hyperlink link) into.Add(link);
			else if (inline is Span span) Collect(span.Inlines, into);
		}
	}
}
