using System;
using System.Windows.Controls;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Pure (no GTK) tests for the adorner object model.</summary>
public class AdornerObjectModelTests
{
	static TextBlock MakeElement() => new TextBlock { Text = "target" };

	[Fact]
	public void Adorner_AdornedElement_is_set()
	{
		var el = MakeElement();
		var adorner = new BoxAdorner(el);
		Assert.Same(el, adorner.AdornedElement);
	}

	[Fact]
	public void Adorner_ctor_null_throws()
	{
		Assert.Throws<ArgumentNullException>(() => new BoxAdorner(null!));
	}

	[Fact]
	public void AdornerLayer_Add_GetAdorners_returns_adorner()
	{
		var layer = new AdornerLayer();
		var el = MakeElement();
		var adorner = new BoxAdorner(el);

		layer.Add(adorner);

		var result = layer.GetAdorners(el);
		Assert.NotNull(result);
		Assert.Single(result!);
		Assert.Same(adorner, result![0]);
	}

	[Fact]
	public void AdornerLayer_GetAdorners_returns_null_when_none()
	{
		var layer = new AdornerLayer();
		var el = MakeElement();
		Assert.Null(layer.GetAdorners(el));
	}

	[Fact]
	public void AdornerLayer_Remove_detaches_adorner()
	{
		var layer = new AdornerLayer();
		var el = MakeElement();
		var adorner = new BoxAdorner(el);

		layer.Add(adorner);
		layer.Remove(adorner);

		Assert.Null(layer.GetAdorners(el));
		Assert.Null(adorner.LogicalParent);
	}

	[Fact]
	public void AdornerLayer_Remove_nonexistent_is_noop()
	{
		var layer = new AdornerLayer();
		var el = MakeElement();
		var adorner = new BoxAdorner(el);

		// Must not throw.
		var ex = Record.Exception(() => layer.Remove(adorner));
		Assert.Null(ex);
	}

	[Fact]
	public void AdornerLayer_Add_duplicate_throws()
	{
		var layer = new AdornerLayer();
		var el = MakeElement();
		var adorner = new BoxAdorner(el);

		layer.Add(adorner);
		Assert.Throws<InvalidOperationException>(() => layer.Add(adorner));
	}

	[Fact]
	public void AdornerLayer_Add_sets_logical_parent()
	{
		var layer = new AdornerLayer();
		var el = MakeElement();
		var adorner = new BoxAdorner(el);

		layer.Add(adorner);

		Assert.Same(layer, adorner.LogicalParent);
	}

	[Fact]
	public void AdornerLayer_multiple_adorners_per_element()
	{
		var layer = new AdornerLayer();
		var el = MakeElement();
		var a1 = new BoxAdorner(el);
		var a2 = new BoxAdorner(el);

		layer.Add(a1);
		layer.Add(a2);

		var result = layer.GetAdorners(el);
		Assert.NotNull(result);
		Assert.Equal(2, result!.Length);
	}

	[Fact]
	public void AdornerLayer_adorners_for_different_elements_are_independent()
	{
		var layer = new AdornerLayer();
		var el1 = MakeElement();
		var el2 = MakeElement();
		var a1 = new BoxAdorner(el1);
		var a2 = new BoxAdorner(el2);

		layer.Add(a1);
		layer.Add(a2);

		Assert.Single(layer.GetAdorners(el1)!);
		Assert.Single(layer.GetAdorners(el2)!);
	}

	[Fact]
	public void AdornerLayer_Add_null_throws()
	{
		var layer = new AdornerLayer();
		Assert.Throws<ArgumentNullException>(() => layer.Add(null!));
	}

	[Fact]
	public void AdornerLayer_GetAdorners_null_throws()
	{
		var layer = new AdornerLayer();
		Assert.Throws<ArgumentNullException>(() => layer.GetAdorners(null!));
	}
}
