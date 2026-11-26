using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// GTK smoke tests for ItemsControl alternation index assignment on realized containers.
/// Requires a live GTK runtime; skipped automatically when unavailable.
/// </summary>
public class AlternationGtkSmokeTests : WpfGtkTestBase
{
	// ── AlternationCount=2, 4 items → 0,1,0,1 ────────────────────────────────

	[Fact]
	public void AlternationCount2_four_items_assign_0101() => Run(() =>
	{
		var ic = new ItemsControl
		{
			AlternationCount = 2,
			ItemsSource = new[] { "a", "b", "c", "d" }
		};
		_ = ic.Widget;

		Assert.Equal(0, ItemsControl.GetAlternationIndex(ic.ContainerAt(0)!));
		Assert.Equal(1, ItemsControl.GetAlternationIndex(ic.ContainerAt(1)!));
		Assert.Equal(0, ItemsControl.GetAlternationIndex(ic.ContainerAt(2)!));
		Assert.Equal(1, ItemsControl.GetAlternationIndex(ic.ContainerAt(3)!));
	});

	[Fact]
	public void AlternationCount3_three_items_assign_012() => Run(() =>
	{
		var ic = new ItemsControl
		{
			AlternationCount = 3,
			ItemsSource = new[] { "x", "y", "z" }
		};
		_ = ic.Widget;

		Assert.Equal(0, ItemsControl.GetAlternationIndex(ic.ContainerAt(0)!));
		Assert.Equal(1, ItemsControl.GetAlternationIndex(ic.ContainerAt(1)!));
		Assert.Equal(2, ItemsControl.GetAlternationIndex(ic.ContainerAt(2)!));
	});

	[Fact]
	public void AlternationCount0_all_containers_get_index_0() => Run(() =>
	{
		var ic = new ItemsControl
		{
			AlternationCount = 0,
			ItemsSource = new[] { "p", "q", "r" }
		};
		_ = ic.Widget;

		Assert.Equal(0, ItemsControl.GetAlternationIndex(ic.ContainerAt(0)!));
		Assert.Equal(0, ItemsControl.GetAlternationIndex(ic.ContainerAt(1)!));
		Assert.Equal(0, ItemsControl.GetAlternationIndex(ic.ContainerAt(2)!));
	});

	// ── Changing AlternationCount recomputes indices ──────────────────────────

	[Fact]
	public void Changing_AlternationCount_recomputes_all_indices() => Run(() =>
	{
		var ic = new ItemsControl
		{
			AlternationCount = 0,
			ItemsSource = new[] { "a", "b", "c" }
		};
		_ = ic.Widget;

		// All 0 initially.
		Assert.Equal(0, ItemsControl.GetAlternationIndex(ic.ContainerAt(0)!));

		// Switch to alternation-2: 0,1,0.
		ic.AlternationCount = 2;
		Assert.Equal(0, ItemsControl.GetAlternationIndex(ic.ContainerAt(0)!));
		Assert.Equal(1, ItemsControl.GetAlternationIndex(ic.ContainerAt(1)!));
		Assert.Equal(0, ItemsControl.GetAlternationIndex(ic.ContainerAt(2)!));
	});

	// ── Adding an item assigns the right index ────────────────────────────────

	[Fact]
	public void Adding_item_assigns_correct_alternation_index() => Run(() =>
	{
		var source = new ObservableCollection<string> { "a", "b" };
		var ic = new ItemsControl { AlternationCount = 2, ItemsSource = source };
		_ = ic.Widget;

		source.Add("c"); // index 2 → 2 % 2 = 0
		Assert.Equal(0, ItemsControl.GetAlternationIndex(ic.ContainerAt(2)!));

		source.Add("d"); // index 3 → 3 % 2 = 1
		Assert.Equal(1, ItemsControl.GetAlternationIndex(ic.ContainerAt(3)!));
	});
}
