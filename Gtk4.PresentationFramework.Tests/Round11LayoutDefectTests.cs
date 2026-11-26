using System;
using System.Windows;
using Xunit;
using WpfGrid = System.Windows.Controls.Grid;
using WpfStackPanel = System.Windows.Controls.StackPanel;
using WpfColumnDefinition = System.Windows.Controls.ColumnDefinition;
using WpfRowDefinition = System.Windows.Controls.RowDefinition;
using WpfOrientation = System.Windows.Controls.Orientation;

// Pure (no GTK) regression tests for the Round-11 Opus review defects.
// W1 — constraint-change re-measure; W2 — DesiredSize not clamped to availableSize;
// W3 — iterative star Min/Max redistribution; W4 — spanning child grows Auto tracks.
namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// A child whose MeasureOverride returns min(available, intrinsic) — constraint-DEPENDENT.
/// This is the class the existing FixedSizeElement cannot model (it always returns its
/// fixed size regardless of the constraint).
/// </summary>
sealed class ConstraintDependentElement : FrameworkElement
{
	readonly Size _intrinsic;

	public ConstraintDependentElement(double width, double height)
	{
		_intrinsic = new Size(width, height);
	}

	protected override Gtk.Widget CreateWidget() =>
		throw new NotSupportedException("No GTK in pure tests.");

	protected override Size MeasureOverride(Size availableSize) =>
		new Size(
			double.IsPositiveInfinity(availableSize.Width) ? _intrinsic.Width : Math.Min(_intrinsic.Width, availableSize.Width),
			double.IsPositiveInfinity(availableSize.Height) ? _intrinsic.Height : Math.Min(_intrinsic.Height, availableSize.Height));
}

// ── W1: constraint change forces re-measure ────────────────────────────────

public class W1_ConstraintChangeForcesRemeasure
{
	/// <summary>
	/// A ConstraintDependentElement inside a 1* Grid column.
	/// First unconstrained measure (auto pass) → intrinsic 200.
	/// Grid resolved cell = 100 (finite); second Measure(100,∞) must re-run MeasureCore.
	/// Child's DesiredSize after layout must reflect the constrained 100, not the stale 200.
	/// </summary>
	[Fact]
	public void Grid_star_child_desired_size_reflects_resolved_cell_not_initial_unconstrained()
	{
		var g = new WpfGrid();
		g.ColumnDefinitions.Add(new WpfColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
		g.RowDefinitions.Add(new WpfRowDefinition { Height = GridLength.Auto });

		// Intrinsic 200; constraint-dependent so it shrinks to available.
		var child = new ConstraintDependentElement(200, 50);
		WpfGrid.SetColumn(child, 0);
		g.Children.Add(child);

		// Grid is given 100-wide space → star column resolves to 100.
		g.Measure(new Size(100, double.PositiveInfinity));
		g.Arrange(new Rect(0, 0, 100, 50));

		// After layout the child must have been re-measured at 100, not left at 200.
		Assert.Equal(100, child.DesiredSize.Width);
	}
}

// ── W2: DesiredSize not clamped to availableSize ───────────────────────────

public class W2_DesiredSizeNotClampedToAvailable
{
	/// <summary>
	/// StackPanel with children summing to 500 measured at height=200.
	/// WPF lets DesiredSize exceed the available slot; the panel must report 500, not 200.
	/// </summary>
	[Fact]
	public void StackPanel_desired_height_exceeds_available_height()
	{
		var sp = new WpfStackPanel { Orientation = WpfOrientation.Vertical };
		sp.Children.Add(new FixedSizeElement(100, 100));
		sp.Children.Add(new FixedSizeElement(100, 100));
		sp.Children.Add(new FixedSizeElement(100, 100));
		sp.Children.Add(new FixedSizeElement(100, 100));
		sp.Children.Add(new FixedSizeElement(100, 100));

		sp.Measure(new Size(200, 200));

		// DesiredSize must NOT be clamped to availableSize.Height=200.
		Assert.Equal(500, sp.DesiredSize.Height);
	}

	/// <summary>
	/// A single FixedSizeElement with intrinsic 300 measured at 200 must report 300.
	/// </summary>
	[Fact]
	public void Single_element_desired_width_exceeds_available_width()
	{
		var el = new FixedSizeElement(300, 50);
		el.Measure(new Size(200, 200));
		Assert.Equal(300, el.DesiredSize.Width);
	}
}

// ── W3: iterative star Min/Max redistribution ──────────────────────────────

public class W3_StarDistributionWithMinMax
{
	/// <summary>
	/// Two 1* columns in 100-wide space, col0 has MaxWidth=20.
	/// WPF fixes col0 at 20 and gives col1 the full remaining 80.
	/// </summary>
	[Fact]
	public void Star_column_capped_by_MaxWidth_gives_surplus_to_other_star()
	{
		var g = new WpfGrid();
		g.ColumnDefinitions.Add(new WpfColumnDefinition
		{
			Width = new GridLength(1, GridUnitType.Star),
			MaxWidth = 20
		});
		g.ColumnDefinitions.Add(new WpfColumnDefinition
		{
			Width = new GridLength(1, GridUnitType.Star)
		});
		g.RowDefinitions.Add(new WpfRowDefinition { Height = GridLength.Auto });

		var c0 = new FixedSizeElement(1, 10); WpfGrid.SetColumn(c0, 0);
		var c1 = new FixedSizeElement(1, 10); WpfGrid.SetColumn(c1, 1);
		g.Children.Add(c0);
		g.Children.Add(c1);

		g.Measure(new Size(100, 100));
		g.Arrange(new Rect(0, 0, 100, 100));

		Assert.Equal(20, c0.ArrangeRect.Width, 1);
		Assert.Equal(80, c1.ArrangeRect.Width, 1);
	}

	/// <summary>
	/// Two 1* columns in 100-wide space, col0 has MinWidth=60.
	/// col0 is fixed at 60; col1 gets the remaining 40.
	/// </summary>
	[Fact]
	public void Star_column_floored_by_MinWidth_gives_deficit_to_other_star()
	{
		var g = new WpfGrid();
		g.ColumnDefinitions.Add(new WpfColumnDefinition
		{
			Width = new GridLength(1, GridUnitType.Star),
			MinWidth = 60
		});
		g.ColumnDefinitions.Add(new WpfColumnDefinition
		{
			Width = new GridLength(1, GridUnitType.Star)
		});
		g.RowDefinitions.Add(new WpfRowDefinition { Height = GridLength.Auto });

		var c0 = new FixedSizeElement(1, 10); WpfGrid.SetColumn(c0, 0);
		var c1 = new FixedSizeElement(1, 10); WpfGrid.SetColumn(c1, 1);
		g.Children.Add(c0);
		g.Children.Add(c1);

		g.Measure(new Size(100, 100));
		g.Arrange(new Rect(0, 0, 100, 100));

		Assert.Equal(60, c0.ArrangeRect.Width, 1);
		Assert.Equal(40, c1.ArrangeRect.Width, 1);
	}
}

// ── W4: spanning child grows Auto tracks ──────────────────────────────────

public class W4_SpanningChildGrowsAutoTracks
{
	/// <summary>
	/// A child with ColumnSpan=2 over two Auto columns with intrinsic width 100.
	/// Both Auto columns should grow to cover the child's desired 100 (sum ≥ 100).
	/// </summary>
	[Fact]
	public void ColumnSpan2_over_two_Auto_columns_grows_them_to_cover_desired_width()
	{
		var g = new WpfGrid();
		g.ColumnDefinitions.Add(new WpfColumnDefinition { Width = GridLength.Auto });
		g.ColumnDefinitions.Add(new WpfColumnDefinition { Width = GridLength.Auto });
		g.RowDefinitions.Add(new WpfRowDefinition { Height = GridLength.Auto });

		// Spanning child: desired 100 × 20.
		var span = new FixedSizeElement(100, 20);
		WpfGrid.SetColumnSpan(span, 2);
		g.Children.Add(span);

		g.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

		// The two Auto columns together must sum to ≥ 100 (child's desired width).
		Assert.True(g.DesiredSize.Width >= 100,
			$"DesiredSize.Width={g.DesiredSize.Width} should be ≥ 100");
	}

	/// <summary>
	/// Two Auto columns each have a span-1 child of width 30, plus a spanning child of 100.
	/// Sum before spanning = 60; deficit 40 is spread equally (20 per column) → 50/50.
	/// </summary>
	[Fact]
	public void Spanning_child_distributes_deficit_equally_across_Auto_columns()
	{
		var g = new WpfGrid();
		g.ColumnDefinitions.Add(new WpfColumnDefinition { Width = GridLength.Auto });
		g.ColumnDefinitions.Add(new WpfColumnDefinition { Width = GridLength.Auto });
		g.RowDefinitions.Add(new WpfRowDefinition { Height = GridLength.Auto });

		var c0 = new FixedSizeElement(30, 20); WpfGrid.SetColumn(c0, 0);
		var c1 = new FixedSizeElement(30, 20); WpfGrid.SetColumn(c1, 1);
		var span = new FixedSizeElement(100, 20);
		WpfGrid.SetColumnSpan(span, 2);

		g.Children.Add(c0);
		g.Children.Add(c1);
		g.Children.Add(span);

		g.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		g.Arrange(new Rect(0, 0, g.DesiredSize.Width, g.DesiredSize.Height));

		// col0 = 50, col1 = 50 (30 each base + 20 each from deficit distribution)
		Assert.Equal(50, c0.ArrangeRect.Width, 1);
		Assert.Equal(50, c1.ArrangeRect.Width, 1);
	}
}