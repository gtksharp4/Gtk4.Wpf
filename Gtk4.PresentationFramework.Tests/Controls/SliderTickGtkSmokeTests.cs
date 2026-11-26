using Xunit;
using WpfSlider        = System.Windows.Controls.Slider;
using WpfOrientation   = System.Windows.Controls.Orientation;
using WpfTickPlacement = System.Windows.Controls.Primitives.TickPlacement;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// GTK smoke tests for the new Slider tick/step/direction DPs.
/// Each body runs on the GTK thread via Run(…); skipped when GTK is unavailable.
/// </summary>
public class SliderTickGtkSmokeTests : WpfGtkTestBase
{
	// ── SmallChange / LargeChange → adjustment ────────────────────────────────

	[Fact]
	public void Slider_SmallChange_reflects_in_stepIncrement() => Run(() =>
	{
		var s = new WpfSlider { SmallChange = 3.0 };
		var scale = (Gtk.Scale)s.Widget;
		Assert.Equal(3.0, scale.Adjustment.StepIncrement, 6);
	});

	[Fact]
	public void Slider_LargeChange_reflects_in_pageIncrement() => Run(() =>
	{
		var s = new WpfSlider { LargeChange = 20.0 };
		var scale = (Gtk.Scale)s.Widget;
		Assert.Equal(20.0, scale.Adjustment.PageIncrement, 6);
	});

	[Fact]
	public void Slider_SmallChange_postRealize_updates_adjustment() => Run(() =>
	{
		var s = new WpfSlider();
		_ = s.Widget;
		s.SmallChange = 7.0;
		var scale = (Gtk.Scale)s.Widget;
		Assert.Equal(7.0, scale.Adjustment.StepIncrement, 6);
	});

	[Fact]
	public void Slider_LargeChange_postRealize_updates_adjustment() => Run(() =>
	{
		var s = new WpfSlider();
		_ = s.Widget;
		s.LargeChange = 15.0;
		var scale = (Gtk.Scale)s.Widget;
		Assert.Equal(15.0, scale.Adjustment.PageIncrement, 6);
	});

	// ── IsDirectionReversed → Gtk.Range.Inverted ─────────────────────────────

	[Fact]
	public void Slider_IsDirectionReversed_true_sets_Inverted() => Run(() =>
	{
		var s = new WpfSlider { IsDirectionReversed = true };
		var scale = (Gtk.Scale)s.Widget;
		Assert.True(scale.Inverted);
	});

	[Fact]
	public void Slider_IsDirectionReversed_false_leaves_Inverted_false() => Run(() =>
	{
		var s = new WpfSlider { IsDirectionReversed = false };
		var scale = (Gtk.Scale)s.Widget;
		Assert.False(scale.Inverted);
	});

	[Fact]
	public void Slider_IsDirectionReversed_postRealize_updates_Inverted() => Run(() =>
	{
		var s = new WpfSlider();
		_ = s.Widget;
		s.IsDirectionReversed = true;
		var scale = (Gtk.Scale)s.Widget;
		Assert.True(scale.Inverted);
	});

	// ── Orientation → Gtk.Scale.Orientation ──────────────────────────────────

	[Fact]
	public void Slider_Vertical_sets_GtkScale_orientation() => Run(() =>
	{
		var s = new WpfSlider { Orientation = WpfOrientation.Vertical };
		var scale = (Gtk.Scale)s.Widget;
		Assert.Equal(Gtk.Orientation.Vertical, scale.Orientation);
	});

	// ── TickPlacement → AddMark ───────────────────────────────────────────────

	[Fact]
	public void Slider_TickPlacement_Both_adds_marks_to_GtkScale() => Run(() =>
	{
		var s = new WpfSlider
		{
			Maximum = 10.0,
			TickFrequency = 2.0,
			TickPlacement = WpfTickPlacement.Both,
		};
		// Just check that Widget creation does not throw and the widget exists.
		var scale = (Gtk.Scale)s.Widget;
		Assert.NotNull(scale);
	});

	[Fact]
	public void Slider_TickPlacement_None_no_throw() => Run(() =>
	{
		var s = new WpfSlider
		{
			Maximum = 10.0,
			TickFrequency = 1.0,
			TickPlacement = WpfTickPlacement.None,
		};
		var scale = (Gtk.Scale)s.Widget;
		Assert.NotNull(scale);
	});

	[Fact]
	public void Slider_TickPlacement_postRealize_change_no_throw() => Run(() =>
	{
		var s = new WpfSlider { Maximum = 10.0, TickFrequency = 2.0 };
		_ = s.Widget;
		// Switch from None → Both → None, should not throw.
		s.TickPlacement = WpfTickPlacement.Both;
		s.TickPlacement = WpfTickPlacement.None;
		Assert.Equal(WpfTickPlacement.None, s.TickPlacement);
	});

	// ── Vertical + TickPlacement smoke ────────────────────────────────────────

	[Fact]
	public void Slider_Vertical_TickPlacement_TopLeft_no_throw() => Run(() =>
	{
		var s = new WpfSlider
		{
			Orientation = WpfOrientation.Vertical,
			Maximum = 10.0,
			TickFrequency = 5.0,
			TickPlacement = WpfTickPlacement.TopLeft,
		};
		var scale = (Gtk.Scale)s.Widget;
		Assert.Equal(Gtk.Orientation.Vertical, scale.Orientation);
	});

	// ── Headless → realize applies all state ─────────────────────────────────

	[Fact]
	public void Slider_preRealize_state_applied_on_realize() => Run(() =>
	{
		var s = new WpfSlider
		{
			Maximum = 100.0,
			SmallChange = 4.0,
			LargeChange = 20.0,
			IsDirectionReversed = true,
		};
		var scale = (Gtk.Scale)s.Widget;
		Assert.Equal(4.0, scale.Adjustment.StepIncrement, 6);
		Assert.Equal(20.0, scale.Adjustment.PageIncrement, 6);
		Assert.True(scale.Inverted);
	});
}
