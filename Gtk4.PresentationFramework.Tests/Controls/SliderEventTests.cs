using Xunit;
using WpfSlider = System.Windows.Controls.Slider;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Pinning tests for G1: <see cref="WpfSlider.ValueChanged"/> must fire for
/// both programmatic and GTK-origin (drag) value changes.
/// </summary>
public class SliderEventTests : WpfGtkTestBase
{
	// ── Pure DP ──────────────────────────────────────────────────────────────

	[Fact]
	public void Slider_ValueChanged_fires_on_programmatic_set()
	{
		// Runs on GTK thread to materialise the widget.
		Run(() =>
		{
			var s = new WpfSlider { Maximum = 100 };
			_ = s.Widget;
			int count = 0;
			s.ValueChanged += (_, _) => count++;
			s.Value = 50.0;
			Assert.Equal(1, count);
		});
	}

	// ── GTK-origin ───────────────────────────────────────────────────────────

	/// <summary>
	/// G1 pinning test: simulates a GTK drag by writing to <c>Gtk.Scale.Value</c>
	/// (the same code path the GTK signal invokes) and asserts that
	/// <see cref="WpfSlider.ValueChanged"/> fires exactly once with the new value.
	/// Before the fix this test failed because <c>_updating=true</c> silenced the event.
	/// </summary>
	[Fact]
	public void Slider_ValueChanged_fires_on_GtkScale_drag()
	{
		Run(() =>
		{
			var s = new WpfSlider { Maximum = 100 };
			var scale = (Gtk.Scale)s.Widget;

			int count = 0;
			double reportedValue = -1;
			s.ValueChanged += (_, _) =>
			{
				count++;
				reportedValue = s.Value;
			};

			// Drive the GTK widget directly (same path as a user drag).
			scale.Value = 75.0;

			Assert.Equal(1, count);
			Assert.Equal(75.0, reportedValue);
		});
	}

	[Fact]
	public void Slider_ValueChanged_does_not_fire_when_value_unchanged()
	{
		Run(() =>
		{
			var s = new WpfSlider { Maximum = 100, Value = 30.0 };
			_ = s.Widget;
			int count = 0;
			s.ValueChanged += (_, _) => count++;
			s.Value = 30.0; // same value — DP coercion keeps it; no change.
			Assert.Equal(0, count);
		});
	}
}