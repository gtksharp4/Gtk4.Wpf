using System.Windows;
using System.Windows.Controls;
using Xunit;
// Note: System using already in scope via global usings in net10.0; kept explicit for clarity.

namespace Gtk4.PresentationFramework.Tests.Input;

/// <summary>
/// GTK smoke tests for <see cref="UIElement.IsTabStop"/> and <see cref="UIElement.TabIndex"/>.
/// Each test body runs on the GTK thread via <c>Run(...)</c>.
/// Skipped when GTK is unavailable.
/// </summary>
public class TabNavigationGtkSmokeTests : WpfGtkTestBase
{
	// ── IsTabStop=false → widget.CanFocus=false ───────────────────────────────

	[Fact]
	public void IsTabStop_false_sets_widget_CanFocus_false() => Run(() =>
	{
		var tb = new TextBlock();
		tb.IsTabStop = false;
		_ = tb.Widget; // realize

		Assert.False(tb.Widget.CanFocus);
	});

	[Fact]
	public void IsTabStop_true_keeps_widget_CanFocus_true() => Run(() =>
	{
		var tb = new TextBlock();
		tb.IsTabStop = true;
		_ = tb.Widget; // realize

		Assert.True(tb.Widget.CanFocus);
	});

	// ── Set before realize → applied on realize ───────────────────────────────

	[Fact]
	public void IsTabStop_false_before_realize_applied_on_creation() => Run(() =>
	{
		var tb = new TextBlock();
		// No widget materialized yet — DP stores the value.
		tb.IsTabStop = false;
		Assert.False(tb.IsWidgetCreated);

		_ = tb.Widget; // trigger realization

		Assert.False(tb.Widget.CanFocus);
	});

	[Fact]
	public void IsTabStop_set_after_realize_updates_CanFocus() => Run(() =>
	{
		var tb = new TextBlock();
		_ = tb.Widget; // realize first
		Assert.True(tb.Widget.CanFocus);

		tb.IsTabStop = false;
		Assert.False(tb.Widget.CanFocus);

		tb.IsTabStop = true;
		Assert.True(tb.Widget.CanFocus);
	});

	// ── Focusable=false → CanFocus=false regardless of IsTabStop ─────────────

	[Fact]
	public void Focusable_false_sets_CanFocus_false() => Run(() =>
	{
		var tb = new TextBlock();
		tb.Focusable = false;
		_ = tb.Widget;

		Assert.False(tb.Widget.CanFocus);
	});

	[Fact]
	public void IsTabStop_false_and_Focusable_true_gives_CanFocus_false() => Run(() =>
	{
		var tb = new TextBlock();
		tb.IsTabStop = false;
		tb.Focusable = true;
		_ = tb.Widget;

		Assert.False(tb.Widget.CanFocus);
	});

	[Fact]
	public void IsTabStop_true_and_Focusable_false_gives_CanFocus_false() => Run(() =>
	{
		var tb = new TextBlock();
		tb.IsTabStop = true;
		tb.Focusable = false;
		_ = tb.Widget;

		Assert.False(tb.Widget.CanFocus);
	});

	// ── TabIndex: no throw, value persists ────────────────────────────────────

	[Fact]
	public void TabIndex_set_before_realize_does_not_throw_and_persists() => Run(() =>
	{
		var tb = new TextBlock();
		tb.TabIndex = 5;
		Assert.False(tb.IsWidgetCreated);

		var ex = Record.Exception(() => { _ = tb.Widget; });
		Assert.Null(ex);
		Assert.Equal(5, tb.TabIndex);
	});

	[Fact]
	public void TabIndex_set_after_realize_does_not_throw_and_persists() => Run(() =>
	{
		var tb = new TextBlock();
		_ = tb.Widget;

		var ex = Record.Exception(() => tb.TabIndex = 3);
		Assert.Null(ex);
		Assert.Equal(3, tb.TabIndex);
	});

	// Multiline TextBox: FocusWidget is the inner TextView, not the outer wrapper.
	// IsTabStop=false set BEFORE realize must reach the inner focus widget (F1).
	[Fact]
	public void IsTabStop_false_before_realize_applies_to_inner_FocusWidget() => Run(() =>
	{
		var tb = new TextBox { AcceptsReturn = true }; // multiline → inner TextView
		tb.IsTabStop = false;                          // set before realization
		_ = tb.Widget;                                 // realize

		Assert.False(tb.FocusWidget.CanFocus);
	});
}
