using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Input;

/// <summary>
/// Pure unit tests for <see cref="TabOrderHelper.Order{T}"/>.
/// No GTK runtime or WPF elements required.
/// </summary>
public class TabOrderHelperTests
{
	record Elem(string Name, int TabIndex, bool IsTabStop = true);

	static List<Elem> Order(IEnumerable<Elem> elems) =>
		TabOrderHelper.Order(elems, e => e.TabIndex, e => e.IsTabStop);

	// ── Basic ordering ────────────────────────────────────────────────────────

	[Fact]
	public void Ascending_by_TabIndex()
	{
		var input = new[] { new Elem("C", 3), new Elem("A", 1), new Elem("B", 2) };
		var result = Order(input);
		Assert.Equal(new[] { "A", "B", "C" }, Names(result));
	}

	[Fact]
	public void Tab_index_5_1_1_MaxValue_declaration_order()
	{
		var input = new[]
		{
			new Elem("D", int.MaxValue),
			new Elem("A", 5),
			new Elem("B", 1),
			new Elem("C", 1),
		};
		var result = Order(input);
		// TabIndex 1 (B then C by declaration), then 5 (A), then MaxValue (D).
		Assert.Equal(new[] { "B", "C", "A", "D" }, Names(result));
	}

	[Fact]
	public void Ties_resolved_by_declaration_order()
	{
		var input = new[]
		{
			new Elem("First",  5),
			new Elem("Second", 5),
			new Elem("Third",  5),
		};
		var result = Order(input);
		Assert.Equal(new[] { "First", "Second", "Third" }, Names(result));
	}

	// ── IsTabStop=false excluded ──────────────────────────────────────────────

	[Fact]
	public void NonTabStop_excluded()
	{
		var input = new[]
		{
			new Elem("A", 1, IsTabStop: true),
			new Elem("B", 2, IsTabStop: false),
			new Elem("C", 3, IsTabStop: true),
		};
		var result = Order(input);
		Assert.Equal(new[] { "A", "C" }, Names(result));
	}

	[Fact]
	public void All_NonTabStop_returns_empty()
	{
		var input = new[]
		{
			new Elem("A", 1, IsTabStop: false),
			new Elem("B", 2, IsTabStop: false),
		};
		var result = Order(input);
		Assert.Empty(result);
	}

	// ── Edge cases ────────────────────────────────────────────────────────────

	[Fact]
	public void Empty_input_returns_empty()
	{
		var result = Order(System.Array.Empty<Elem>());
		Assert.Empty(result);
	}

	[Fact]
	public void Single_element_returned_as_is()
	{
		var input = new[] { new Elem("X", 7) };
		var result = Order(input);
		Assert.Equal(new[] { "X" }, Names(result));
	}

	[Fact]
	public void Null_elements_throws()
	{
		Assert.Throws<ArgumentNullException>(() =>
			TabOrderHelper.Order<Elem>(null!, e => e.TabIndex, e => e.IsTabStop));
	}

	[Fact]
	public void Null_tabIndex_selector_throws()
	{
		Assert.Throws<ArgumentNullException>(() =>
			TabOrderHelper.Order(System.Array.Empty<Elem>(), null!, e => e.IsTabStop));
	}

	[Fact]
	public void Null_isTabStop_selector_throws()
	{
		Assert.Throws<ArgumentNullException>(() =>
			TabOrderHelper.Order(System.Array.Empty<Elem>(), e => e.TabIndex, null!));
	}

	static string[] Names(List<Elem> elems)
	{
		var names = new string[elems.Count];
		for (int i = 0; i < elems.Count; i++)
			names[i] = elems[i].Name;
		return names;
	}
}
