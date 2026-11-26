using System;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure (headless) tests for ItemsControl alternation:
/// ComputeAlternationIndex, AlternationCount DP, AlternationIndex DP round-trips.
/// All tests fail without ItemsControl.Alternation.cs.
/// </summary>
public class AlternationDpTests
{
	// ── ComputeAlternationIndex ───────────────────────────────────────────────

	[Fact]
	public void ComputeAlternationIndex_count2_index0_returns_0()
		=> Assert.Equal(0, ItemsControl.ComputeAlternationIndex(0, 2));

	[Fact]
	public void ComputeAlternationIndex_count2_index1_returns_1()
		=> Assert.Equal(1, ItemsControl.ComputeAlternationIndex(1, 2));

	[Fact]
	public void ComputeAlternationIndex_count2_index2_returns_0()
		=> Assert.Equal(0, ItemsControl.ComputeAlternationIndex(2, 2));

	[Fact]
	public void ComputeAlternationIndex_count3_index3_returns_0()
		=> Assert.Equal(0, ItemsControl.ComputeAlternationIndex(3, 3));

	[Fact]
	public void ComputeAlternationIndex_count0_returns_0_for_any_index()
	{
		Assert.Equal(0, ItemsControl.ComputeAlternationIndex(0, 0));
		Assert.Equal(0, ItemsControl.ComputeAlternationIndex(5, 0));
		Assert.Equal(0, ItemsControl.ComputeAlternationIndex(99, 0));
	}

	[Fact]
	public void ComputeAlternationIndex_count1_returns_0_for_any_index()
	{
		Assert.Equal(0, ItemsControl.ComputeAlternationIndex(0, 1));
		Assert.Equal(0, ItemsControl.ComputeAlternationIndex(7, 1));
	}

	// ── AlternationCount DP ───────────────────────────────────────────────────

	[Fact]
	public void AlternationCount_default_is_0()
	{
		var ic = new NoGtkItemsControl();
		Assert.Equal(0, ic.AlternationCount);
	}

	[Fact]
	public void AlternationCount_round_trips()
	{
		var ic = new NoGtkItemsControl { AlternationCount = 3 };
		Assert.Equal(3, ic.AlternationCount);
	}

	[Fact]
	public void AlternationCount_dp_identity()
	{
		Assert.NotNull(ItemsControl.AlternationCountProperty);
		Assert.Equal(0, (int)ItemsControl.AlternationCountProperty.DefaultMetadata.DefaultValue!);
	}

	// ── AlternationIndex attached DP ──────────────────────────────────────────

	[Fact]
	public void AlternationIndexProperty_exists_and_is_readonly()
	{
		Assert.NotNull(ItemsControl.AlternationIndexProperty);
		Assert.True(ItemsControl.AlternationIndexProperty.IsReadOnly);
	}

	[Fact]
	public void GetAlternationIndex_returns_0_when_not_set()
	{
		var dep = new NoGtkItemsControl();
		Assert.Equal(0, ItemsControl.GetAlternationIndex(dep));
	}

	[Fact]
	public void GetAlternationIndex_throws_on_null()
	{
		Assert.Throws<ArgumentNullException>(() => ItemsControl.GetAlternationIndex(null!));
	}

	// ── NoGtk helper ─────────────────────────────────────────────────────────

	/// <summary>
	/// ItemsControl subclass that refuses widget creation so tests stay headless.
	/// </summary>
	sealed class NoGtkItemsControl : ItemsControl
	{
		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("No GTK in pure tests.");
	}
}
