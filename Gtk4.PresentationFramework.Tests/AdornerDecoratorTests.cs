using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>AdornerDecorator and the <see cref="AdornerLayer.GetAdornerLayer"/> tree walk.</summary>
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
