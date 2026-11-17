using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

// ── Minimal concrete Adorner for testing ─────────────────────────────────────

/// <summary>A minimal adorner that uses no GTK widget (pure object-model test).</summary>
sealed class BoxAdorner : Adorner
{
	public BoxAdorner(UIElement adornedElement) : base(adornedElement) { }

	protected override Gtk.Widget CreateWidget() =>
		throw new NotSupportedException("No GTK in pure adorner tests.");
}

// ── Adorner object-model tests ────────────────────────────────────────────────

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

// ── AdornerDecorator / GetAdornerLayer tree-walk tests ────────────────────────

public class AdornerDecoratorTests
{
	[Fact]
	public void AdornerDecorator_AdornerLayer_is_non_null()
	{
		var dec = new AdornerDecorator();
		Assert.NotNull(dec.AdornerLayer);
	}

	[Fact]
	public void GetAdornerLayer_returns_null_when_no_decorator_ancestor()
	{
		var panel = new TestPanelNoGtk();
		var child = new TextBlock();
		panel.Children.Add(child);

		Assert.Null(AdornerLayer.GetAdornerLayer(child));
	}

	[Fact]
	public void GetAdornerLayer_returns_null_for_isolated_element()
	{
		var el = new TextBlock();
		Assert.Null(AdornerLayer.GetAdornerLayer(el));
	}

	[Fact]
	public void GetAdornerLayer_walks_up_to_decorator()
	{
		var dec = new AdornerDecorator();
		var panel = new TestPanelNoGtk();
		var child = new TextBlock();

		panel.Children.Add(child);
		dec.Child = panel;

		// The walk goes child → panel → dec → returns dec.AdornerLayer.
		var found = AdornerLayer.GetAdornerLayer(child);
		Assert.NotNull(found);
		Assert.Same(dec.AdornerLayer, found);
	}

	[Fact]
	public void GetAdornerLayer_null_throws()
	{
		Assert.Throws<ArgumentNullException>(() => AdornerLayer.GetAdornerLayer(null!));
	}

	[Fact]
	public void AdornerDecorator_child_logical_parent_set()
	{
		var dec = new AdornerDecorator();
		var el = new TextBlock();
		dec.Child = el;
		Assert.Same(dec, el.LogicalParent);
	}

	[Fact]
	public void AdornerDecorator_child_null_clears_parent()
	{
		var dec = new AdornerDecorator();
		var el = new TextBlock();
		dec.Child = el;
		dec.Child = null;
		Assert.Null(el.LogicalParent);
	}

	[Fact]
	public void AdornerDecorator_add_remove_adorner_headless()
	{
		var dec = new AdornerDecorator();
		var el = new TextBlock();
		dec.Child = el;

		var adorner = new BoxAdorner(el);
		dec.AdornerLayer.Add(adorner);

		Assert.Same(adorner, dec.AdornerLayer.GetAdorners(el)![0]);

		dec.AdornerLayer.Remove(adorner);
		Assert.Null(dec.AdornerLayer.GetAdorners(el));
	}

	// ── Minimal GTK-free Panel ──────────────────────────────────────────────

	sealed class TestPanelNoGtk : Panel
	{
		internal override bool IsWidgetCreated => false;
		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("No GTK in pure tests.");
		protected override void AddChildWidget(UIElement child) { }
		protected override void RemoveChildWidget(UIElement child) { }
	}
}

// ── GTK smoke test: overlay wired when widget is realized ─────────────────────

public class AdornerGtkSmokeTests : WpfGtkTestBase
{
	/// <summary>
	/// AdornerDecorator.Widget is a Gtk.Overlay; adding a child sets the overlay's
	/// main child widget; the AdornerLayer has the overlay reference.
	/// </summary>
	[Fact]
	public void AdornerDecorator_widget_is_GtkOverlay() => Run(() =>
	{
		var dec = new AdornerDecorator();
		var inner = new TextBlock { Text = "hi" };
		dec.Child = inner;

		Assert.IsType<Gtk.Overlay>(dec.Widget);
	});

	[Fact]
	public void AdornerDecorator_AdornerLayer_has_overlay_after_realize() => Run(() =>
	{
		var dec = new AdornerDecorator();
		_ = dec.Widget; // realize
		Assert.NotNull(dec.AdornerLayer.GtkOverlay);
	});

	[Fact]
	public void AdornerDecorator_child_is_overlay_main_content() => Run(() =>
	{
		var dec = new AdornerDecorator();
		var inner = new TextBlock { Text = "content" };
		dec.Child = inner;

		var overlay = (Gtk.Overlay)dec.Widget;
		Assert.Same(inner.Widget, overlay.Child);
	});
}
