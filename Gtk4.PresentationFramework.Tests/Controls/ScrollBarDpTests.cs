using Xunit;
using WpfScrollBar  = System.Windows.Controls.Primitives.ScrollBar;
using WpfOrientation = System.Windows.Controls.Orientation;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Pure (no GTK) DP default and round-trip tests for <see cref="WpfScrollBar"/>.
/// </summary>
public class ScrollBarDpTests
{
	// ── Defaults ─────────────────────────────────────────────────────────────

	[Fact]
	public void ScrollBar_Orientation_default_is_Vertical()
	{
		var sb = new WpfScrollBar();
		Assert.Equal(WpfOrientation.Vertical, sb.Orientation);
	}

	[Fact]
	public void ScrollBar_Minimum_default_is_zero()
	{
		var sb = new WpfScrollBar();
		Assert.Equal(0.0, sb.Minimum);
	}

	[Fact]
	public void ScrollBar_Maximum_default_is_one()
	{
		var sb = new WpfScrollBar();
		Assert.Equal(1.0, sb.Maximum);
	}

	[Fact]
	public void ScrollBar_Value_default_is_zero()
	{
		var sb = new WpfScrollBar();
		Assert.Equal(0.0, sb.Value);
	}

	[Fact]
	public void ScrollBar_ViewportSize_default_is_zero()
	{
		var sb = new WpfScrollBar();
		Assert.Equal(0.0, sb.ViewportSize);
	}

	[Fact]
	public void ScrollBar_SmallChange_default_is_0_1()
	{
		var sb = new WpfScrollBar();
		Assert.Equal(0.1, sb.SmallChange);
	}

	[Fact]
	public void ScrollBar_LargeChange_default_is_1()
	{
		var sb = new WpfScrollBar();
		Assert.Equal(1.0, sb.LargeChange);
	}

	// ── Roundtrips ───────────────────────────────────────────────────────────

	[Fact]
	public void ScrollBar_Orientation_roundtrip()
	{
		var sb = new WpfScrollBar { Orientation = WpfOrientation.Horizontal };
		Assert.Equal(WpfOrientation.Horizontal, sb.Orientation);
	}

	[Fact]
	public void ScrollBar_Minimum_roundtrip()
	{
		var sb = new WpfScrollBar { Minimum = 10.0 };
		Assert.Equal(10.0, sb.Minimum);
	}

	[Fact]
	public void ScrollBar_Maximum_roundtrip()
	{
		var sb = new WpfScrollBar { Maximum = 200.0 };
		Assert.Equal(200.0, sb.Maximum);
	}

	[Fact]
	public void ScrollBar_Value_roundtrip()
	{
		var sb = new WpfScrollBar { Maximum = 100.0, Value = 50.0 };
		Assert.Equal(50.0, sb.Value);
	}

	[Fact]
	public void ScrollBar_ViewportSize_roundtrip()
	{
		var sb = new WpfScrollBar { ViewportSize = 0.25 };
		Assert.Equal(0.25, sb.ViewportSize);
	}

	[Fact]
	public void ScrollBar_SmallChange_roundtrip()
	{
		var sb = new WpfScrollBar { SmallChange = 0.05 };
		Assert.Equal(0.05, sb.SmallChange);
	}

	[Fact]
	public void ScrollBar_LargeChange_roundtrip()
	{
		var sb = new WpfScrollBar { LargeChange = 0.5 };
		Assert.Equal(0.5, sb.LargeChange);
	}
}
