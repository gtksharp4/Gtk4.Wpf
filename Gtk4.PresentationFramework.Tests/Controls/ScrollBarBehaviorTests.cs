using Xunit;
using WpfScrollBar  = System.Windows.Controls.Primitives.ScrollBar;
using WpfScrollEventType = System.Windows.Controls.Primitives.ScrollEventType;
using WpfScrollEventArgs = System.Windows.Controls.Primitives.ScrollEventArgs;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Pure (no GTK) value-coercion, event, args and headless tests for <see cref="WpfScrollBar"/>.
/// </summary>
public class ScrollBarBehaviorTests
{
	// ── Value coercion ───────────────────────────────────────────────────────

	[Fact]
	public void ScrollBar_Value_clamped_to_Maximum()
	{
		var sb = new WpfScrollBar { Maximum = 1.0, Value = 5.0 };
		Assert.Equal(1.0, sb.Value);
	}

	[Fact]
	public void ScrollBar_Value_clamped_to_Minimum()
	{
		var sb = new WpfScrollBar { Minimum = 2.0, Maximum = 10.0, Value = -1.0 };
		Assert.Equal(2.0, sb.Value);
	}

	[Fact]
	public void ScrollBar_Value_recoerced_when_Maximum_shrinks()
	{
		var sb = new WpfScrollBar { Maximum = 100.0, Value = 80.0 };
		sb.Maximum = 50.0;
		Assert.Equal(50.0, sb.Value);
	}

	[Fact]
	public void ScrollBar_Value_recoerced_when_Minimum_rises()
	{
		var sb = new WpfScrollBar { Minimum = 0.0, Maximum = 100.0, Value = 10.0 };
		sb.Minimum = 20.0;
		Assert.Equal(20.0, sb.Value);
	}

	[Fact]
	public void ScrollBar_Value_within_range_unchanged()
	{
		var sb = new WpfScrollBar { Maximum = 100.0, Value = 50.0 };
		sb.Maximum = 200.0;
		Assert.Equal(50.0, sb.Value);
	}

	// ── ValueChanged event ───────────────────────────────────────────────────

	[Fact]
	public void ScrollBar_ValueChanged_fires_on_set()
	{
		var sb = new WpfScrollBar { Maximum = 100.0 };
		int count = 0;
		sb.ValueChanged += (_, _) => count++;
		sb.Value = 42.0;
		Assert.Equal(1, count);
	}

	[Fact]
	public void ScrollBar_ValueChanged_fires_once_per_change()
	{
		var sb = new WpfScrollBar { Maximum = 100.0 };
		int count = 0;
		sb.ValueChanged += (_, _) => count++;
		sb.Value = 10.0;
		sb.Value = 20.0;
		Assert.Equal(2, count);
	}

	// ── ScrollEventArgs ──────────────────────────────────────────────────────

	[Fact]
	public void ScrollEventArgs_stores_type_and_value()
	{
		var args = new WpfScrollEventArgs(WpfScrollEventType.SmallIncrement, 42.0);
		Assert.Equal(WpfScrollEventType.SmallIncrement, args.ScrollEventType);
		Assert.Equal(42.0, args.NewValue);
	}

	[Fact]
	public void ScrollEventArgs_ThumbTrack_roundtrip()
	{
		var args = new WpfScrollEventArgs(WpfScrollEventType.ThumbTrack, 0.75);
		Assert.Equal(WpfScrollEventType.ThumbTrack, args.ScrollEventType);
		Assert.Equal(0.75, args.NewValue);
	}

	// ── Headless: no widget, no throw ────────────────────────────────────────

	[Fact]
	public void ScrollBar_set_before_widget_does_not_throw()
	{
		var sb = new WpfScrollBar();
		var ex = Record.Exception(() =>
		{
			sb.Minimum = 0.0;
			sb.Maximum = 100.0;
			sb.Value = 50.0;
			sb.ViewportSize = 10.0;
			sb.SmallChange = 1.0;
			sb.LargeChange = 10.0;
		});
		Assert.Null(ex);
	}

	[Fact]
	public void ScrollBar_coercion_works_before_widget()
	{
		var sb = new WpfScrollBar { Maximum = 50.0, Value = 100.0 };
		// No widget created — coercion must still apply.
		Assert.Equal(50.0, sb.Value);
	}
}
