using Xunit;
using WpfTextBlock = System.Windows.Controls.TextBlock;
using WpfToolTip   = System.Windows.Controls.ToolTip;

namespace Gtk4.Forms.PresentationFramework.Tests.Controls;

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
}
