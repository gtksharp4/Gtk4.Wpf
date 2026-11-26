using System;
using System.Windows;
using Xunit;
using WpfBorder = System.Windows.Controls.Border;
using WpfGrid = System.Windows.Controls.Grid;
using WpfStackPanel = System.Windows.Controls.StackPanel;
using WpfCanvas = System.Windows.Controls.Canvas;
using WpfWrapPanel = System.Windows.Controls.WrapPanel;
using WpfUniformGrid = System.Windows.Controls.Primitives.UniformGrid;
using WpfOrientation = System.Windows.Controls.Orientation;
using WpfColumnDefinition = System.Windows.Controls.ColumnDefinition;
using WpfRowDefinition = System.Windows.Controls.RowDefinition;
using WpfDockPanel = System.Windows.Controls.DockPanel;
using WpfDock = System.Windows.Controls.Dock;

// Pure (no GTK) tests for the two-pass WPF layout contract.
// All panels are exercised headlessly; no Gtk.Widget is ever materialised.
namespace Gtk4.PresentationFramework.Tests;
// ── Stubs ──────────────────────────────────────────────────────────────────

/// <summary>A FrameworkElement with a fixed desired size for layout tests.</summary>
sealed class FixedSizeElement : FrameworkElement
{
	readonly Size _desiredSize;

	public FixedSizeElement(double width, double height)
	{
		_desiredSize = new Size(width, height);
	}

	protected override Gtk.Widget CreateWidget() =>
		throw new NotSupportedException("No GTK in pure tests.");

	protected override Size MeasureOverride(Size availableSize) => _desiredSize;
}

// ── FrameworkElement.Measure contract ─────────────────────────────────────

public class FrameworkElementMeasureTests
{
	[Fact]
	public void Width_explicit_overrides_child_desired()
	{
		var el = new FixedSizeElement(50, 50) { Width = 100 };
		el.Measure(new Size(200, 200));
		Assert.Equal(100, el.DesiredSize.Width);
	}

	[Fact]
	public void Margin_inflates_desired_size()
	{
		// 100-wide element, 10-left + 10-right margin → DesiredSize.Width = 120.
		var el = new FixedSizeElement(100, 50) { Margin = new Thickness(10, 5, 10, 5) };
		el.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		Assert.Equal(120, el.DesiredSize.Width);
		Assert.Equal(60, el.DesiredSize.Height);
	}

	[Fact]
	public void MinWidth_enforced_below_override()
	{
		var el = new FixedSizeElement(5, 10) { MinWidth = 50 };
		el.Measure(new Size(200, 200));
		Assert.Equal(50, el.DesiredSize.Width);
	}

	[Fact]
	public void MaxWidth_enforced_above_override()
	{
		var el = new FixedSizeElement(200, 10) { MaxWidth = 80 };
		el.Measure(new Size(300, 300));
		Assert.Equal(80, el.DesiredSize.Width);
	}

	[Fact]
	public void MinHeight_enforced()
	{
		var el = new FixedSizeElement(10, 3) { MinHeight = 20 };
		el.Measure(new Size(200, 200));
		Assert.Equal(20, el.DesiredSize.Height);
	}

	[Fact]
	public void MaxHeight_enforced()
	{
		var el = new FixedSizeElement(10, 100) { MaxHeight = 40 };
		el.Measure(new Size(200, 200));
		Assert.Equal(40, el.DesiredSize.Height);
	}
}

// ── FrameworkElement.Arrange (alignment) ─────────────────────────────────

public class FrameworkElementArrangeTests
{
	[Fact]
	public void Stretch_fills_slot()
	{
		var el = new FixedSizeElement(50, 50);
		el.Measure(new Size(200, 200));
		el.Arrange(new Rect(0, 0, 200, 200));
		Assert.Equal(200, el.RenderSize.Width);
		Assert.Equal(200, el.RenderSize.Height);
	}

	[Fact]
	public void Left_alignment_does_not_stretch_width()
	{
		var el = new FixedSizeElement(50, 50)
		{
			HorizontalAlignment = HorizontalAlignment.Left
		};
		el.Measure(new Size(200, 200));
		el.Arrange(new Rect(0, 0, 200, 200));
		Assert.Equal(50, el.RenderSize.Width);
	}

	[Fact]
	public void Margin_in_arrange_offsets_child()
	{
		var el = new FixedSizeElement(100, 50) { Margin = new Thickness(10, 0, 10, 0) };
		el.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		el.Arrange(new Rect(0, 0, 200, 50));
		// Content width = 200 - 20 margin = 180 (Stretch)
		Assert.Equal(180, el.RenderSize.Width);
	}
}

// ── UIElement invalidation ────────────────────────────────────────────────

public class UIElementLayoutInvalidationTests
{
	[Fact]
	public void InvalidateMeasure_causes_remeasure()
	{
		var el = new FixedSizeElement(50, 50);
		el.Measure(new Size(200, 200));
		var first = el.DesiredSize;
		el.InvalidateMeasure();
		el.Measure(new Size(200, 200));
		Assert.Equal(first, el.DesiredSize);
	}

	[Fact]
	public void UpdateLayout_measures_and_arranges()
	{
		var el = new FixedSizeElement(60, 40);
		el.UpdateLayout();
		Assert.Equal(60, el.DesiredSize.Width);
		Assert.Equal(40, el.DesiredSize.Height);
	}
}

// ── StackPanel ────────────────────────────────────────────────────────────

public class StackPanelLayoutTests
{
	[Fact]
	public void Vertical_desired_height_is_sum_of_children()
	{
		var sp = new WpfStackPanel { Orientation = WpfOrientation.Vertical };
		sp.Children.Add(new FixedSizeElement(100, 20));
		sp.Children.Add(new FixedSizeElement(80, 20));
		sp.Children.Add(new FixedSizeElement(60, 20));
		sp.Measure(new Size(200, 200));
		Assert.Equal(60, sp.DesiredSize.Height);
	}

	[Fact]
	public void Vertical_desired_width_is_max_child()
	{
		var sp = new WpfStackPanel { Orientation = WpfOrientation.Vertical };
		sp.Children.Add(new FixedSizeElement(100, 20));
		sp.Children.Add(new FixedSizeElement(80, 20));
		sp.Measure(new Size(200, 200));
		Assert.Equal(100, sp.DesiredSize.Width);
	}

	[Fact]
	public void Vertical_arrange_stacks_at_correct_y()
	{
		var sp = new WpfStackPanel { Orientation = WpfOrientation.Vertical };
		var c0 = new FixedSizeElement(100, 20);
		var c1 = new FixedSizeElement(100, 20);
		var c2 = new FixedSizeElement(100, 20);
		sp.Children.Add(c0);
		sp.Children.Add(c1);
		sp.Children.Add(c2);
		sp.Measure(new Size(200, 200));
		sp.Arrange(new Rect(0, 0, 200, 60));

		Assert.Equal(0, c0.ArrangeRect.Y);
		Assert.Equal(20, c1.ArrangeRect.Y);
		Assert.Equal(40, c2.ArrangeRect.Y);
	}

	[Fact]
	public void Horizontal_desired_width_is_sum_of_children()
	{
		var sp = new WpfStackPanel { Orientation = WpfOrientation.Horizontal };
		sp.Children.Add(new FixedSizeElement(30, 50));
		sp.Children.Add(new FixedSizeElement(40, 30));
		sp.Measure(new Size(200, 200));
		Assert.Equal(70, sp.DesiredSize.Width);
	}
}

// ── Grid ──────────────────────────────────────────────────────────────────

public class GridLayoutTests
{
	[Fact]
	public void Two_star_columns_split_equally()
	{
		var g = new WpfGrid();
		g.ColumnDefinitions.Add(new WpfColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
		g.ColumnDefinitions.Add(new WpfColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
		g.RowDefinitions.Add(new WpfRowDefinition { Height = GridLength.Auto });

		var c0 = new FixedSizeElement(10, 20); WpfGrid.SetColumn(c0, 0);
		var c1 = new FixedSizeElement(10, 20); WpfGrid.SetColumn(c1, 1);
		g.Children.Add(c0); g.Children.Add(c1);

		g.Measure(new Size(200, 200));
		g.Arrange(new Rect(0, 0, 200, 200));

		Assert.Equal(0, c0.ArrangeRect.X);
		Assert.Equal(100, c1.ArrangeRect.X);
		Assert.Equal(100, c0.ArrangeRect.Width);
		Assert.Equal(100, c1.ArrangeRect.Width);
	}

	[Fact]
	public void Two_star_one_star_split_is_two_to_one()
	{
		var g = new WpfGrid();
		g.ColumnDefinitions.Add(new WpfColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
		g.ColumnDefinitions.Add(new WpfColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
		g.RowDefinitions.Add(new WpfRowDefinition { Height = GridLength.Auto });

		var c0 = new FixedSizeElement(1, 10); WpfGrid.SetColumn(c0, 0);
		var c1 = new FixedSizeElement(1, 10); WpfGrid.SetColumn(c1, 1);
		g.Children.Add(c0); g.Children.Add(c1);

		g.Measure(new Size(300, 100));
		g.Arrange(new Rect(0, 0, 300, 100));

		Assert.Equal(200, c0.ArrangeRect.Width, 1);
		Assert.Equal(100, c1.ArrangeRect.Width, 1);
	}

	[Fact]
	public void Auto_column_sizes_to_child()
	{
		var g = new WpfGrid();
		g.ColumnDefinitions.Add(new WpfColumnDefinition { Width = GridLength.Auto });
		g.RowDefinitions.Add(new WpfRowDefinition { Height = GridLength.Auto });

		var c = new FixedSizeElement(75, 30); WpfGrid.SetColumn(c, 0);
		g.Children.Add(c);

		g.Measure(new Size(300, 300));
		g.Arrange(new Rect(0, 0, 300, 300));

		Assert.Equal(75, c.ArrangeRect.Width);
	}

	[Fact]
	public void Absolute_column_is_fixed()
	{
		var g = new WpfGrid();
		g.ColumnDefinitions.Add(new WpfColumnDefinition { Width = new GridLength(120) });
		g.RowDefinitions.Add(new WpfRowDefinition { Height = GridLength.Auto });

		var c = new FixedSizeElement(10, 10); WpfGrid.SetColumn(c, 0);
		g.Children.Add(c);

		g.Measure(new Size(300, 300));
		g.Arrange(new Rect(0, 0, 300, 300));

		Assert.Equal(120, c.ArrangeRect.Width);
	}

	[Fact]
	public void Grid_Row_Column_placement()
	{
		var g = new WpfGrid();
		g.ColumnDefinitions.Add(new WpfColumnDefinition { Width = new GridLength(100) });
		g.ColumnDefinitions.Add(new WpfColumnDefinition { Width = new GridLength(100) });
		g.RowDefinitions.Add(new WpfRowDefinition { Height = new GridLength(50) });
		g.RowDefinitions.Add(new WpfRowDefinition { Height = new GridLength(50) });

		var c = new FixedSizeElement(10, 10);
		WpfGrid.SetRow(c, 1); WpfGrid.SetColumn(c, 1);
		g.Children.Add(c);

		g.Measure(new Size(200, 100));
		g.Arrange(new Rect(0, 0, 200, 100));

		Assert.Equal(100, c.ArrangeRect.X);
		Assert.Equal(50, c.ArrangeRect.Y);
	}
}

// ── DockPanel ─────────────────────────────────────────────────────────────

public class DockPanelLayoutTests
{
	[Fact]
	public void Top_child_takes_full_width_top_strip()
	{
		var dp = new WpfDockPanel();
		var top = new FixedSizeElement(50, 30);
		WpfDockPanel.SetDock(top, WpfDock.Top);
		dp.Children.Add(top);
		var fill = new FixedSizeElement(50, 50);
		dp.Children.Add(fill);

		dp.Measure(new Size(200, 200));
		dp.Arrange(new Rect(0, 0, 200, 200));

		Assert.Equal(0, top.ArrangeRect.Y);
		Assert.Equal(200, top.ArrangeRect.Width);
		Assert.Equal(30, top.ArrangeRect.Height);
	}

	[Fact]
	public void Left_child_takes_full_height_left_strip()
	{
		var dp = new WpfDockPanel();
		var left = new FixedSizeElement(40, 50);
		WpfDockPanel.SetDock(left, WpfDock.Left);
		dp.Children.Add(left);
		var fill = new FixedSizeElement(10, 10);
		dp.Children.Add(fill);

		dp.Measure(new Size(200, 150));
		dp.Arrange(new Rect(0, 0, 200, 150));

		Assert.Equal(0, left.ArrangeRect.X);
		Assert.Equal(0, left.ArrangeRect.Y);
		Assert.Equal(40, left.ArrangeRect.Width);
		Assert.Equal(150, left.ArrangeRect.Height);
	}

	[Fact]
	public void LastChildFill_true_fill_child_gets_remaining_rect()
	{
		var dp = new WpfDockPanel { LastChildFill = true };
		var top = new FixedSizeElement(200, 30);
		WpfDockPanel.SetDock(top, WpfDock.Top);
		dp.Children.Add(top);
		var fill = new FixedSizeElement(1, 1);
		dp.Children.Add(fill);

		dp.Measure(new Size(200, 100));
		dp.Arrange(new Rect(0, 0, 200, 100));

		Assert.Equal(30, fill.ArrangeRect.Y);
		Assert.Equal(70, fill.ArrangeRect.Height);
	}
}

// ── Canvas ────────────────────────────────────────────────────────────────

public class CanvasLayoutTests
{
	[Fact]
	public void Canvas_desired_is_zero()
	{
		var c = new WpfCanvas();
		var child = new FixedSizeElement(50, 50);
		WpfCanvas.SetLeft(child, 10);
		WpfCanvas.SetTop(child, 10);
		c.Children.Add(child);
		c.Measure(new Size(200, 200));
		Assert.Equal(0, c.DesiredSize.Width);
		Assert.Equal(0, c.DesiredSize.Height);
	}

	[Fact]
	public void Canvas_child_placed_at_Left_Top()
	{
		var c = new WpfCanvas();
		var child = new FixedSizeElement(30, 20);
		WpfCanvas.SetLeft(child, 15);
		WpfCanvas.SetTop(child, 25);
		c.Children.Add(child);
		c.Measure(new Size(200, 200));
		c.Arrange(new Rect(0, 0, 200, 200));

		Assert.Equal(15, child.ArrangeRect.X);
		Assert.Equal(25, child.ArrangeRect.Y);
	}
}

// ── WrapPanel ─────────────────────────────────────────────────────────────

public class WrapPanelLayoutTests
{
	[Fact]
	public void Wraps_to_new_line_when_width_exceeded()
	{
		var wp = new WpfWrapPanel { Orientation = WpfOrientation.Horizontal };
		wp.Children.Add(new FixedSizeElement(60, 20));
		wp.Children.Add(new FixedSizeElement(60, 20));
		wp.Children.Add(new FixedSizeElement(60, 20));

		wp.Measure(new Size(100, 200));
		// First line fits c0+c1? No: 60+60=120>100 → c0 alone on line 1, c1+c2 on lines 2+3.
		// Actually: c0 alone (60≤100), c1 alone (60+60>100 wraps), c2 alone → 3 lines × 20 = 60.
		// But the test says "two lines". Let's check: line 1 = c0 (60), line 2 = c1 (60),
		// line 3 = c2 (60) → DesiredSize.Height = 60.
		// Revise expectation: 3 lines × 20 = 60.
		Assert.Equal(60, wp.DesiredSize.Height);
	}

	[Fact]
	public void Wraps_two_on_first_line_one_on_second()
	{
		var wp = new WpfWrapPanel { Orientation = WpfOrientation.Horizontal };
		wp.Children.Add(new FixedSizeElement(40, 20));
		wp.Children.Add(new FixedSizeElement(40, 20));
		wp.Children.Add(new FixedSizeElement(40, 20)); // 3rd overflows 100-wide slot

		wp.Measure(new Size(100, 200));
		// Line 1: c0(40)+c1(40)=80≤100, then c2: 80+40=120>100 → c2 wraps → 2 lines.
		Assert.Equal(40, wp.DesiredSize.Height);
	}

	[Fact]
	public void Second_line_child_starts_at_next_row()
	{
		var wp = new WpfWrapPanel { Orientation = WpfOrientation.Horizontal };
		var c0 = new FixedSizeElement(40, 20);
		var c1 = new FixedSizeElement(40, 20);
		var c2 = new FixedSizeElement(40, 20);
		wp.Children.Add(c0);
		wp.Children.Add(c1);
		wp.Children.Add(c2);

		wp.Measure(new Size(100, 200));
		wp.Arrange(new Rect(0, 0, 100, 200));

		Assert.Equal(0, c0.ArrangeRect.Y);
		Assert.Equal(0, c1.ArrangeRect.Y);    // c0+c1 fit on line 1
		Assert.Equal(20, c2.ArrangeRect.Y);   // c2 wraps to line 2
	}
}

// ── UniformGrid ───────────────────────────────────────────────────────────

public class UniformGridLayoutTests
{
	[Fact]
	public void Two_children_auto_rows_cols()
	{
		var ug = new WpfUniformGrid(); // auto: cols=2, rows=1
		ug.Children.Add(new FixedSizeElement(50, 30));
		ug.Children.Add(new FixedSizeElement(50, 30));

		ug.Measure(new Size(200, 100));
		ug.Arrange(new Rect(0, 0, 200, 100));

		Assert.Equal(0, ug.Children[0].ArrangeRect.X);
		Assert.Equal(100, ug.Children[1].ArrangeRect.X);
	}

	[Fact]
	public void Explicit_rows_cols()
	{
		var ug = new WpfUniformGrid { Rows = 2, Columns = 2 };
		for (int i = 0; i < 4; i++)
			ug.Children.Add(new FixedSizeElement(10, 10));

		ug.Measure(new Size(100, 100));
		ug.Arrange(new Rect(0, 0, 100, 100));

		Assert.Equal(0, ug.Children[0].ArrangeRect.X);
		Assert.Equal(50, ug.Children[1].ArrangeRect.X);
		Assert.Equal(50, ug.Children[2].ArrangeRect.Y);
	}
}

// ── Border ────────────────────────────────────────────────────────────────

public class BorderLayoutTests
{
	[Fact]
	public void Padding_inflates_desired_size()
	{
		var b = new WpfBorder { Padding = new Thickness(10) };
		b.Child = new FixedSizeElement(50, 30);
		b.Measure(new Size(200, 200));
		Assert.Equal(70, b.DesiredSize.Width);
		Assert.Equal(50, b.DesiredSize.Height);
	}

	[Fact]
	public void BorderThickness_inflates_desired_size()
	{
		var b = new WpfBorder { BorderThickness = new Thickness(5) };
		b.Child = new FixedSizeElement(40, 20);
		b.Measure(new Size(200, 200));
		Assert.Equal(50, b.DesiredSize.Width);
		Assert.Equal(30, b.DesiredSize.Height);
	}

	[Fact]
	public void Child_arranged_inside_padding()
	{
		var b = new WpfBorder { Padding = new Thickness(10) };
		var child = new FixedSizeElement(50, 30);
		b.Child = child;
		b.Measure(new Size(200, 200));
		b.Arrange(new Rect(0, 0, 70, 50));
		Assert.Equal(10, child.ArrangeRect.X);
		Assert.Equal(10, child.ArrangeRect.Y);
	}
}