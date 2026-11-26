using Xunit;
using WpfContextMenu = System.Windows.Controls.ContextMenu;
using WpfPopup       = System.Windows.Controls.Primitives.Popup;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Round-16 defect: <see cref="WpfPopup"/> and <see cref="WpfContextMenu"/>
/// dropped an <see cref="WpfPopup.IsOpen"/><c> = true</c> set before the backing
/// GTK widget existed. The fix stores the intent in <c>_pendingOpen</c> and replays
/// it in <c>OnWidgetCreated</c>; cancellation (<c>IsOpen=false</c> before realize)
/// must clear the flag so no stale open fires on realize.
/// </summary>
public class Round16DefectTests : WpfGtkTestBase
{
	// ── Popup – pure DP (no GTK required) ────────────────────────────────────

	/// <summary>
	/// Setting <see cref="WpfPopup.IsOpen"/><c>=true</c> before the backing widget exists
	/// must store the intent in <c>_pendingOpen</c> rather than silently dropping it.
	/// </summary>
	[Fact]
	public void Popup_IsOpen_true_before_widget_sets_pending_flag()
	{
		var p = new WpfPopup();
		Assert.False(p.IsWidgetCreated, "pre-condition: widget must not yet exist");
		p.IsOpen = true;
		Assert.True(p._pendingOpen);
	}

	/// <summary>
	/// <see cref="WpfPopup.IsOpen"/><c>=true</c> then <c>false</c> before widget exists
	/// must leave <c>_pendingOpen=false</c> so the popup does not appear on realize.
	/// </summary>
	[Fact]
	public void Popup_IsOpen_true_then_false_before_widget_clears_pending_flag()
	{
		var p = new WpfPopup();
		p.IsOpen = true;
		p.IsOpen = false;
		Assert.False(p._pendingOpen);
	}

	// ── ContextMenu – pure DP (no GTK required) ───────────────────────────────

	/// <summary>
	/// Setting <see cref="WpfContextMenu.IsOpen"/><c>=true</c> before the backing widget
	/// exists must store the intent in <c>_pendingOpen</c>.
	/// </summary>
	[Fact]
	public void ContextMenu_IsOpen_true_before_widget_sets_pending_flag()
	{
		var cm = new WpfContextMenu();
		Assert.False(cm.IsWidgetCreated, "pre-condition: widget must not yet exist");
		cm.IsOpen = true;
		Assert.True(cm._pendingOpen);
	}

	/// <summary>
	/// <see cref="WpfContextMenu.IsOpen"/><c>=true</c> then <c>false</c> before widget
	/// exists must leave <c>_pendingOpen=false</c>.
	/// </summary>
	[Fact]
	public void ContextMenu_IsOpen_true_then_false_before_widget_clears_pending_flag()
	{
		var cm = new WpfContextMenu();
		cm.IsOpen = true;
		cm.IsOpen = false;
		Assert.False(cm._pendingOpen);
	}

	// ── GTK smoke – cancel path: realize after IsOpen=false must leave flag clear ──

	/// <summary>
	/// On the GTK thread: cancel a deferred open (<c>IsOpen=true</c> then <c>false</c>
	/// before widget creation) and trigger realize. <c>_pendingOpen</c> must remain
	/// <c>false</c> and the popover must not attempt to show.
	/// </summary>
	[Fact]
	public void Popup_IsOpen_cancelled_before_widget_no_open_on_realize() => Run(() =>
	{
		var p = new WpfPopup();
		p.IsOpen = true;
		p.IsOpen = false;
		Assert.False(p._pendingOpen, "pre-condition: cancel must clear flag before realize");

		// Force widget creation — OnWidgetCreated must NOT call DoOpen.
		_ = p.Widget;

		Assert.False(p._pendingOpen, "flag must still be false after realize");
		Assert.NotNull(p.GtkPopover);
	});

	/// <summary>
	/// On the GTK thread: cancel a deferred open for <see cref="WpfContextMenu"/> and
	/// trigger realize. <c>_pendingOpen</c> must stay <c>false</c>.
	/// </summary>
	[Fact]
	public void ContextMenu_IsOpen_cancelled_before_widget_no_open_on_realize() => Run(() =>
	{
		var cm = new WpfContextMenu();
		cm.IsOpen = true;
		cm.IsOpen = false;
		Assert.False(cm._pendingOpen, "pre-condition: cancel must clear flag before realize");

		_ = cm.Widget;

		Assert.False(cm._pendingOpen, "flag must still be false after realize");
		Assert.NotNull(cm.GtkPopover);
	});
}
