using Xunit;
using WpfTextBlock      = System.Windows.Controls.TextBlock;
using WpfToolTip        = System.Windows.Controls.ToolTip;
using WpfToolTipService = System.Windows.Controls.ToolTipService;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// GTK smoke tests for tooltip wiring on <see cref="WpfTextBlock"/> (and any
/// <see cref="System.Windows.FrameworkElement"/>). Each body runs on the GTK thread;
/// skipped when GTK is unavailable.
/// </summary>
public class ToolTipGtkSmokeTests : WpfGtkTestBase
{
	// ── String tooltip applied before widget creation ─────────────────────────

	[Fact]
	public void String_ToolTip_before_realize_applied_on_creation() => Run(() =>
	{
		var tb = new WpfTextBlock();
		tb.ToolTip = "hover";
		var label = (Gtk.Label)tb.Widget;
		Assert.Equal("hover", label.TooltipText);
	});

	[Fact]
	public void String_ToolTip_before_realize_sets_HasTooltip() => Run(() =>
	{
		var tb = new WpfTextBlock();
		tb.ToolTip = "hover";
		var label = (Gtk.Label)tb.Widget;
		Assert.True(label.HasTooltip);
	});

	// ── String tooltip applied after widget creation ──────────────────────────

	[Fact]
	public void String_ToolTip_after_realize_applied_immediately() => Run(() =>
	{
		var tb    = new WpfTextBlock();
		var label = (Gtk.Label)tb.Widget; // force creation
		tb.ToolTip = "late";
		Assert.Equal("late", label.TooltipText);
	});

	[Fact]
	public void String_ToolTip_after_realize_sets_HasTooltip() => Run(() =>
	{
		var tb    = new WpfTextBlock();
		var label = (Gtk.Label)tb.Widget;
		tb.ToolTip = "late";
		Assert.True(label.HasTooltip);
	});

	// ── ToolTip control (Content = string) ───────────────────────────────────

	[Fact]
	public void ToolTip_control_string_content_before_realize_uses_Content_text() => Run(() =>
	{
		var tb  = new WpfTextBlock();
		var tip = new WpfToolTip { Content = "rich hint" };
		tb.ToolTip = tip;
		var label = (Gtk.Label)tb.Widget;
		Assert.Equal("rich hint", label.TooltipText);
	});

	[Fact]
	public void ToolTip_control_string_content_after_realize_uses_Content_text() => Run(() =>
	{
		var tb    = new WpfTextBlock();
		var label = (Gtk.Label)tb.Widget;
		var tip   = new WpfToolTip { Content = "late rich" };
		tb.ToolTip = tip;
		Assert.Equal("late rich", label.TooltipText);
	});

	// ── Clearing tooltip ──────────────────────────────────────────────────────

	[Fact]
	public void Clearing_ToolTip_clears_GTK_HasTooltip() => Run(() =>
	{
		var tb    = new WpfTextBlock();
		var label = (Gtk.Label)tb.Widget;
		tb.ToolTip = "gone";
		tb.ToolTip = null;
		Assert.False(label.HasTooltip);
	});

	// ── IsEnabled=false suppresses tooltip on realized widget ─────────────────

	[Fact]
	public void IsEnabled_false_after_realize_clears_HasTooltip() => Run(() =>
	{
		var tb    = new WpfTextBlock();
		tb.ToolTip = "visible";
		var label = (Gtk.Label)tb.Widget; // force creation
		Assert.True(label.HasTooltip);    // sanity: tooltip present before suppression
		WpfToolTipService.SetIsEnabled(tb, false);
		Assert.False(label.HasTooltip);
	});

	[Fact]
	public void IsEnabled_false_after_realize_clears_TooltipText() => Run(() =>
	{
		var tb    = new WpfTextBlock();
		tb.ToolTip = "visible";
		var label = (Gtk.Label)tb.Widget;
		WpfToolTipService.SetIsEnabled(tb, false);
		Assert.Equal(string.Empty, label.TooltipText ?? string.Empty);
	});

	[Fact]
	public void IsEnabled_true_after_false_restores_HasTooltip() => Run(() =>
	{
		var tb    = new WpfTextBlock();
		tb.ToolTip = "visible";
		var label = (Gtk.Label)tb.Widget;
		WpfToolTipService.SetIsEnabled(tb, false);
		Assert.False(label.HasTooltip);
		WpfToolTipService.SetIsEnabled(tb, true);
		Assert.True(label.HasTooltip);
	});

	[Fact]
	public void IsEnabled_false_before_realize_suppresses_tooltip_on_creation() => Run(() =>
	{
		var tb = new WpfTextBlock();
		tb.ToolTip = "suppressed";
		WpfToolTipService.SetIsEnabled(tb, false);
		var label = (Gtk.Label)tb.Widget; // widget created after IsEnabled=false
		Assert.False(label.HasTooltip);
	});

	// A new ToolTip set on an already-realized, disabled element must NOT re-show it.
	[Fact]
	public void Setting_ToolTip_while_disabled_and_realized_does_not_show() => Run(() =>
	{
		var tb    = new WpfTextBlock();
		WpfToolTipService.SetIsEnabled(tb, false);
		var label = (Gtk.Label)tb.Widget; // realize while disabled
		tb.ToolTip = "late";               // set AFTER realize — must stay suppressed
		Assert.False(label.HasTooltip);
	});
}
