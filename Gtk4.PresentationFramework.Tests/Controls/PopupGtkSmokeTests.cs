using System.Windows.Controls.Primitives;
using Xunit;
using WpfPlacementMode = System.Windows.Controls.PlacementMode;
using WpfPopup        = System.Windows.Controls.Primitives.Popup;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// GTK smoke tests for <see cref="WpfPopup"/> placement: verifies that the backing
/// <see cref="Gtk.Popover"/> is configured with the correct position and offsets.
/// Each body runs on the GTK thread; skipped when GTK is unavailable.
/// </summary>
public class PopupGtkSmokeTests : WpfGtkTestBase
{
	// ── Position applied on widget creation ───────────────────────────────────

	[Fact]
	public void Placement_Top_sets_popover_Position_Top() => Run(() =>
	{
		var p = new WpfPopup { Placement = WpfPlacementMode.Top };
		_ = p.Widget; // force creation
		Assert.Equal(Gtk.PositionType.Top, p.GtkPopover!.Position);
	});

	[Fact]
	public void Placement_Bottom_sets_popover_Position_Bottom() => Run(() =>
	{
		var p = new WpfPopup { Placement = WpfPlacementMode.Bottom };
		_ = p.Widget;
		Assert.Equal(Gtk.PositionType.Bottom, p.GtkPopover!.Position);
	});

	[Fact]
	public void Placement_Left_sets_popover_Position_Left() => Run(() =>
	{
		var p = new WpfPopup { Placement = WpfPlacementMode.Left };
		_ = p.Widget;
		Assert.Equal(Gtk.PositionType.Left, p.GtkPopover!.Position);
	});

	[Fact]
	public void Placement_Right_sets_popover_Position_Right() => Run(() =>
	{
		var p = new WpfPopup { Placement = WpfPlacementMode.Right };
		_ = p.Widget;
		Assert.Equal(Gtk.PositionType.Right, p.GtkPopover!.Position);
	});

	// ── Position updated when DP changes after widget creation ────────────────

	[Fact]
	public void Placement_change_after_realize_updates_popover_Position() => Run(() =>
	{
		var p = new WpfPopup { Placement = WpfPlacementMode.Bottom };
		_ = p.Widget;
		p.Placement = WpfPlacementMode.Top;
		Assert.Equal(Gtk.PositionType.Top, p.GtkPopover!.Position);
	});

	// ── Offsets applied on widget creation ────────────────────────────────────

	[Fact]
	public void HorizontalOffset_applied_to_popover_on_creation() => Run(() =>
	{
		var p = new WpfPopup { HorizontalOffset = 10.0 };
		_ = p.Widget;
		p.GtkPopover!.GetOffset(out int x, out _);
		Assert.Equal(10, x);
	});

	[Fact]
	public void VerticalOffset_applied_to_popover_on_creation() => Run(() =>
	{
		var p = new WpfPopup { VerticalOffset = 15.0 };
		_ = p.Widget;
		p.GtkPopover!.GetOffset(out _, out int y);
		Assert.Equal(15, y);
	});

	// ── Offsets updated when DP changes after widget creation ─────────────────

	[Fact]
	public void HorizontalOffset_change_after_realize_updates_popover_offset() => Run(() =>
	{
		var p = new WpfPopup();
		_ = p.Widget;
		p.HorizontalOffset = 5.0;
		p.GtkPopover!.GetOffset(out int x, out _);
		Assert.Equal(5, x);
	});

	[Fact]
	public void VerticalOffset_change_after_realize_updates_popover_offset() => Run(() =>
	{
		var p = new WpfPopup();
		_ = p.Widget;
		p.VerticalOffset = 7.0;
		p.GtkPopover!.GetOffset(out _, out int y);
		Assert.Equal(7, y);
	});

	// ── Fallback: Center maps to Bottom ───────────────────────────────────────

	[Fact]
	public void Placement_Center_falls_back_to_Bottom() => Run(() =>
	{
		var p = new WpfPopup { Placement = WpfPlacementMode.Center };
		_ = p.Widget;
		Assert.Equal(Gtk.PositionType.Bottom, p.GtkPopover!.Position);
	});
}
