using Xunit;
using WpfPlacementMode = System.Windows.Controls.PlacementMode;
using WpfPopup        = System.Windows.Controls.Primitives.Popup;
using WpfTextBlock    = System.Windows.Controls.TextBlock;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Pure DP tests for <see cref="WpfPopup"/> placement properties.
/// No GTK runtime required.
/// </summary>
public class PopupDpTests
{
	// ── Placement ─────────────────────────────────────────────────────────────

	[Fact]
	public void Placement_default_Bottom()
	{
		var p = new WpfPopup();
		Assert.Equal(WpfPlacementMode.Bottom, p.Placement);
	}

	[Fact]
	public void Placement_roundtrip_Top()
	{
		var p = new WpfPopup { Placement = WpfPlacementMode.Top };
		Assert.Equal(WpfPlacementMode.Top, p.Placement);
	}

	[Fact]
	public void Placement_roundtrip_Left()
	{
		var p = new WpfPopup { Placement = WpfPlacementMode.Left };
		Assert.Equal(WpfPlacementMode.Left, p.Placement);
	}

	[Fact]
	public void Placement_roundtrip_Right()
	{
		var p = new WpfPopup { Placement = WpfPlacementMode.Right };
		Assert.Equal(WpfPlacementMode.Right, p.Placement);
	}

	[Fact]
	public void Placement_roundtrip_Center()
	{
		var p = new WpfPopup { Placement = WpfPlacementMode.Center };
		Assert.Equal(WpfPlacementMode.Center, p.Placement);
	}

	[Fact]
	public void Placement_roundtrip_Absolute()
	{
		var p = new WpfPopup { Placement = WpfPlacementMode.Absolute };
		Assert.Equal(WpfPlacementMode.Absolute, p.Placement);
	}

	// ── HorizontalOffset ──────────────────────────────────────────────────────

	[Fact]
	public void HorizontalOffset_default_zero()
	{
		var p = new WpfPopup();
		Assert.Equal(0.0, p.HorizontalOffset);
	}

	[Fact]
	public void HorizontalOffset_roundtrip()
	{
		var p = new WpfPopup { HorizontalOffset = 12.5 };
		Assert.Equal(12.5, p.HorizontalOffset);
	}

	[Fact]
	public void HorizontalOffset_negative_roundtrip()
	{
		var p = new WpfPopup { HorizontalOffset = -8.0 };
		Assert.Equal(-8.0, p.HorizontalOffset);
	}

	// ── VerticalOffset ────────────────────────────────────────────────────────

	[Fact]
	public void VerticalOffset_default_zero()
	{
		var p = new WpfPopup();
		Assert.Equal(0.0, p.VerticalOffset);
	}

	[Fact]
	public void VerticalOffset_roundtrip()
	{
		var p = new WpfPopup { VerticalOffset = 20.0 };
		Assert.Equal(20.0, p.VerticalOffset);
	}

	// ── Headless: set before realize ──────────────────────────────────────────

	[Fact]
	public void Placement_set_before_realize_no_throw()
	{
		var p  = new WpfPopup();
		var ex = Record.Exception(() => p.Placement = WpfPlacementMode.Top);
		Assert.Null(ex);
	}

	[Fact]
	public void Placement_set_before_realize_persists()
	{
		var p = new WpfPopup();
		p.Placement = WpfPlacementMode.Left;
		Assert.Equal(WpfPlacementMode.Left, p.Placement);
	}

	[Fact]
	public void HorizontalOffset_set_before_realize_no_throw()
	{
		var p  = new WpfPopup();
		var ex = Record.Exception(() => p.HorizontalOffset = 5.0);
		Assert.Null(ex);
	}

	[Fact]
	public void VerticalOffset_set_before_realize_no_throw()
	{
		var p  = new WpfPopup();
		var ex = Record.Exception(() => p.VerticalOffset = 5.0);
		Assert.Null(ex);
	}

	[Fact]
	public void Offsets_set_before_realize_persist()
	{
		var p = new WpfPopup { HorizontalOffset = 3.0, VerticalOffset = 7.0 };
		Assert.Equal(3.0, p.HorizontalOffset);
		Assert.Equal(7.0, p.VerticalOffset);
	}
}
