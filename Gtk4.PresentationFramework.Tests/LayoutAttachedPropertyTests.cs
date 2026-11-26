using System.Windows;
using Xunit;
using WpfCanvas = System.Windows.Controls.Canvas;
using WpfDockPanel = System.Windows.Controls.DockPanel;
using WpfGrid = System.Windows.Controls.Grid;
using ColumnDefinition = System.Windows.Controls.ColumnDefinition;
using RowDefinition = System.Windows.Controls.RowDefinition;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure (no GTK) tests for the attached-property get/set round-trips and
/// GridLength/ColumnDefinition/RowDefinition semantics.
/// </summary>
public class LayoutAttachedPropertyTests
{
	// ── Grid.Row / Column / RowSpan / ColumnSpan ─────────────────────────────────

	[Fact]
	public void Grid_SetRow_GetRow_roundtrip()
	{
		var e = new StubElement();
		WpfGrid.SetRow(e, 3);
		Assert.Equal(3, WpfGrid.GetRow(e));
	}

	[Fact]
	public void Grid_SetColumn_GetColumn_roundtrip()
	{
		var e = new StubElement();
		WpfGrid.SetColumn(e, 2);
		Assert.Equal(2, WpfGrid.GetColumn(e));
	}

	[Fact]
	public void Grid_SetRowSpan_GetRowSpan_roundtrip()
	{
		var e = new StubElement();
		WpfGrid.SetRowSpan(e, 4);
		Assert.Equal(4, WpfGrid.GetRowSpan(e));
	}

	[Fact]
	public void Grid_SetColumnSpan_GetColumnSpan_roundtrip()
	{
		var e = new StubElement();
		WpfGrid.SetColumnSpan(e, 2);
		Assert.Equal(2, WpfGrid.GetColumnSpan(e));
	}

	[Fact]
	public void Grid_Row_default_is_zero()
	{
		var e = new StubElement();
		Assert.Equal(0, WpfGrid.GetRow(e));
	}

	[Fact]
	public void Grid_RowSpan_default_is_one()
	{
		var e = new StubElement();
		Assert.Equal(1, WpfGrid.GetRowSpan(e));
	}

	// ── Canvas.Left / Top / Right / Bottom ───────────────────────────────────────

	[Fact]
	public void Canvas_SetLeft_GetLeft_roundtrip()
	{
		var e = new StubElement();
		WpfCanvas.SetLeft(e, 10.5);
		Assert.Equal(10.5, WpfCanvas.GetLeft(e));
	}

	[Fact]
	public void Canvas_SetTop_GetTop_roundtrip()
	{
		var e = new StubElement();
		WpfCanvas.SetTop(e, 20.0);
		Assert.Equal(20.0, WpfCanvas.GetTop(e));
	}

	[Fact]
	public void Canvas_SetRight_GetRight_roundtrip()
	{
		var e = new StubElement();
		WpfCanvas.SetRight(e, 15.0);
		Assert.Equal(15.0, WpfCanvas.GetRight(e));
	}

	[Fact]
	public void Canvas_SetBottom_GetBottom_roundtrip()
	{
		var e = new StubElement();
		WpfCanvas.SetBottom(e, 5.0);
		Assert.Equal(5.0, WpfCanvas.GetBottom(e));
	}

	[Fact]
	public void Canvas_Left_default_is_NaN()
	{
		var e = new StubElement();
		Assert.True(double.IsNaN(WpfCanvas.GetLeft(e)));
	}

	// ── DockPanel.Dock ────────────────────────────────────────────────────────────

	[Fact]
	public void DockPanel_SetDock_GetDock_roundtrip()
	{
		var e = new StubElement();
		WpfDockPanel.SetDock(e, System.Windows.Controls.Dock.Right);
		Assert.Equal(System.Windows.Controls.Dock.Right, WpfDockPanel.GetDock(e));
	}

	[Fact]
	public void DockPanel_Dock_default_is_Left()
	{
		var e = new StubElement();
		Assert.Equal(System.Windows.Controls.Dock.Left, WpfDockPanel.GetDock(e));
	}

	// ── GridLength ────────────────────────────────────────────────────────────────

	[Fact]
	public void GridLength_Auto_IsAuto()
	{
		Assert.True(GridLength.Auto.IsAuto);
		Assert.False(GridLength.Auto.IsStar);
		Assert.False(GridLength.Auto.IsAbsolute);
	}

	[Fact]
	public void GridLength_Star_IsStar()
	{
		var gl = new GridLength(2.0, GridUnitType.Star);
		Assert.True(gl.IsStar);
		Assert.Equal(2.0, gl.Value);
	}

	[Fact]
	public void GridLength_Pixel_IsAbsolute()
	{
		var gl = new GridLength(100.0);
		Assert.True(gl.IsAbsolute);
		Assert.Equal(100.0, gl.Value);
	}

	[Fact]
	public void GridLength_equality()
	{
		var a = new GridLength(1.0, GridUnitType.Star);
		var b = new GridLength(1.0, GridUnitType.Star);
		Assert.Equal(a, b);
	}

	[Fact]
	public void GridLength_inequality_different_type()
	{
		var a = new GridLength(1.0, GridUnitType.Star);
		var b = new GridLength(1.0, GridUnitType.Pixel);
		Assert.NotEqual(a, b);
	}

	[Fact]
	public void GridLength_Auto_toString()
	{
		Assert.Equal("Auto", GridLength.Auto.ToString());
	}

	[Fact]
	public void GridLength_Star_toString()
	{
		var gl = new GridLength(1.0, GridUnitType.Star);
		Assert.Equal("*", gl.ToString());
	}

	[Fact]
	public void GridLength_twoStar_toString()
	{
		var gl = new GridLength(2.0, GridUnitType.Star);
		Assert.Equal("2*", gl.ToString());
	}

	// ── ColumnDefinitions / RowDefinitions ────────────────────────────────────────

	[Fact]
	public void Grid_ColumnDefinitions_starts_empty()
	{
		var g = new WpfGrid();
		Assert.Empty(g.ColumnDefinitions);
	}

	[Fact]
	public void Grid_ColumnDefinitions_add_and_count()
	{
		var g = new WpfGrid();
		g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
		g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Star) });
		Assert.Equal(2, g.ColumnDefinitions.Count);
	}

	[Fact]
	public void Grid_RowDefinitions_add_and_count()
	{
		var g = new WpfGrid();
		g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50.0) });
		Assert.Single(g.RowDefinitions);
		Assert.Equal(50.0, g.RowDefinitions[0].Height.Value);
	}

	// ── Stub ──────────────────────────────────────────────────────────────────────

	/// <summary>Minimal UIElement that never creates a real GTK widget.</summary>
	sealed class StubElement : System.Windows.UIElement
	{
		protected override Gtk.Widget CreateWidget() =>
			throw new System.NotSupportedException("No GTK in pure tests.");
	}
}