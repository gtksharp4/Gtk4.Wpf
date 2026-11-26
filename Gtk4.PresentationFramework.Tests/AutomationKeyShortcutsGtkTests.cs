using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>The accelerator key reaches GTK KeyShortcuts; observed through the PeerAccessible seam.</summary>
public class AutomationKeyShortcutsGtkTests : WpfGtkTestBase
{
	// "key=value" per push, "key=<reset>" per reset, for KeyShortcuts on one widget.
	static List<string> Record(Gtk.Widget widget, System.Action act)
	{
		var seen = new List<string>();
		PeerAccessible.Spy = (w, p, v) =>
		{
			if (ReferenceEquals(w, widget) && p == Gtk.AccessibleProperty.KeyShortcuts)
				seen.Add(v == null ? "<reset>" : (string)v);
		};
		try { act(); }
		finally { PeerAccessible.Spy = null; }
		return seen;
	}

	[Fact]
	public void AcceleratorKey_set_on_a_realised_element_is_pushed_and_clearing_resets() => Run(() =>
	{
		var button = new Button();
		var widget = button.Widget;
		Assert.Empty(Record(widget, () => GtkPump.For(20)));
		var set = Record(widget, () => AutomationProperties.SetAcceleratorKey(button, "Ctrl+K"));
		Assert.Equal("Ctrl+K", Assert.Single(set));
		var cleared = Record(widget, () => button.ClearValue(AutomationProperties.AcceleratorKeyProperty));
		Assert.Equal("<reset>", Assert.Single(cleared));
	});

	[Fact]
	public void MenuItem_InputGestureText_change_is_pushed_and_explicit_key_still_wins() => Run(() =>
	{
		var item = new MenuItem { InputGestureText = "Ctrl+O" };
		var widget = item.Widget;
		Assert.NotNull(UIElementAutomationPeer.CreatePeerForElement(item));
		var changed = Record(widget, () => item.InputGestureText = "Ctrl+S");
		Assert.Equal("Ctrl+S", changed[changed.Count - 1]);
		var explicitKey = Record(widget, () => AutomationProperties.SetAcceleratorKey(item, "Alt+X"));
		Assert.Equal("Alt+X", explicitKey[explicitKey.Count - 1]);
		var after = Record(widget, () => item.InputGestureText = "Ctrl+Z");
		Assert.Equal("Alt+X", after[after.Count - 1]);
	});
}
