using System.Linq;
using System.Windows.Documents;

namespace System.Windows.Controls;

// TODO(lane K): TextBlock should raise Hyperlink.OnClick from the label's links for every TextBlock;
// remove this class (the only subscription) when that lands.
internal static class HyperlinkActivation
{
	/// <summary>Routes a click on a hyperlink shown by <paramref name="block"/> to the Hyperlink element.</summary>
	/// <remarks>
	/// A GtkListBox claims the click sequence for row selection, which denies the label's own link gesture, so
	/// a capture-phase click on the label activates the link under the pointer first.
	/// A Hyperlink without NavigateUri is drawn without an href, so GTK cannot activate it; a Click-only link needs
	/// the inline model (lane K) to emit one.
	/// </remarks>
	internal static void Attach(TextBlock block)
	{
		if (block.Widget is not Gtk.Label label) return;
		label.ActivateLink += (s, e) =>
		{
			var href = e.Args.Length > 0 ? e.Args[0] as string : null;
			if (!Activate(block, href)) return;
			e.RetVal = true;
		};

		string? pressed = null;
		var click = new Gtk.GestureClick { Button = 1, PropagationPhase = Gtk.PropagationPhase.Capture };
		click.Pressed += (o, e) =>
		{
			pressed = label.CurrentUri;
			if (pressed != null && Find(block, pressed) != null)
				click.SetState(Gtk.EventSequenceState.Claimed);
			else
				pressed = null;
		};
		click.Released += (o, e) =>
		{
			var href = pressed;
			pressed = null;
			if (href != null && label.CurrentUri == href) Activate(block, href);
		};
		label.AddController(click);
	}

	static bool Activate(TextBlock block, string? href)
	{
		var link = Find(block, href);
		if (link == null) return false;
		link.OnClick();
		return true;
	}

	internal static Hyperlink? Find(TextBlock block, string? href)
	{
		var links = block.AutomationHyperlinks().ToList();
		return links.FirstOrDefault(l => l.NavigateUri?.ToString() == href)
			?? links.FirstOrDefault(l => l.NavigateUri?.OriginalString == href);
	}
}
