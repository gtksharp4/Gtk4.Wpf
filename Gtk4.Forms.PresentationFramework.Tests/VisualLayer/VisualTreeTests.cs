using System;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.VisualLayer;

public class VisualTreeTests : WpfGtkTestBase
{
	[Fact]
	public void AddVisualChild_sets_parent_and_RemoveVisualChild_clears_it() => Run(() =>
	{
		var host = new VisualHost();
		var v = new DrawingVisual();
		host.AddV(v);
		Assert.Same(host, v.VisualParent);
		host.RemoveV(v);
		Assert.Null(v.VisualParent);
	});

	[Fact]
	public void Reparenting_a_visual_throws_ArgumentException() => Run(() =>
	{
		var a = new VisualHost();
		var b = new VisualHost();
		var v = new DrawingVisual();
		a.AddV(v);
		Assert.Throws<ArgumentException>(() => b.AddV(v));
		Assert.Throws<ArgumentException>(() => a.AddV(v));
	});

	[Fact]
	public void VisualCollection_rejects_a_child_that_already_has_a_parent() => Run(() =>
	{
		var c1 = new ContainerVisual();
		var c2 = new ContainerVisual();
		var v = new DrawingVisual();
		c1.Children.Add(v);
		Assert.Throws<ArgumentException>(() => c2.Children.Add(v));
		Assert.Empty(c2.Children);
	});

	[Fact]
	public void UIElement_visual_child_gets_its_widget_hosted_in_the_default_container() => Run(() =>
	{
		var host = new VisualHost();
		var child = new VisualHost();
		_ = host.Widget;
		host.AddV(child);
		Assert.Same(host.Widget, child.Widget.Parent);
		host.RemoveV(child);
		Assert.Null(child.Widget.Parent);
	});

	[Fact]
	public void UIElement_child_added_before_the_widget_exists_is_hosted_at_creation() => Run(() =>
	{
		var host = new VisualHost();
		var child = new VisualHost();
		host.AddV(child);
		Assert.Same(host.Widget, child.Widget.Parent);
	});

	[Fact]
	public void TransformToAncestor_composes_offsets_and_transforms() => Run(() =>
	{
		var root = new ContainerVisual();
		var mid = new ContainerVisual { Offset = new Vector(10, 20) };
		var leaf = new ContainerVisual { Transform = new ScaleTransform(2, 2), Offset = new Vector(1, 1) };
		root.Children.Add(mid);
		mid.Children.Add(leaf);
		Assert.Equal(new Point(17, 29), leaf.TransformToAncestor(root).Transform(new Point(3, 4)));
		Assert.Equal(new Point(3, 4), root.TransformToDescendant(leaf)!.Transform(new Point(17, 29)));
	});

	[Fact]
	public void TransformToAncestor_throws_when_argument_is_not_an_ancestor() => Run(() =>
	{
		var a = new ContainerVisual();
		var b = new ContainerVisual();
		Assert.Throws<InvalidOperationException>(() => a.TransformToAncestor(b));
		Assert.Throws<InvalidOperationException>(() => a.TransformToDescendant(b));
		Assert.Throws<ArgumentNullException>(() => a.TransformToAncestor(null!));
	});

	[Fact]
	public void TransformToVisual_between_siblings_and_without_common_ancestor() => Run(() =>
	{
		var root = new ContainerVisual();
		var l = new ContainerVisual { Offset = new Vector(5, 0) };
		var r = new ContainerVisual { Offset = new Vector(0, 7) };
		root.Children.Add(l);
		root.Children.Add(r);
		Assert.Equal(new Point(6, -6), l.TransformToVisual(r)!.Transform(new Point(1, 1)));
		Assert.Throws<InvalidOperationException>(() => l.TransformToVisual(new ContainerVisual()));
		Assert.Same(root, l.FindCommonVisualAncestor(r));
		Assert.True(root.IsAncestorOf(l));
		Assert.True(l.IsDescendantOf(root));
	});

	[Fact]
	public void TranslatePoint_uses_arrange_offsets_before_realization() => Run(() =>
	{
		var parent = new System.Windows.Controls.Canvas();
		var child = new System.Windows.Controls.Canvas { Width = 20, Height = 20 };
		parent.Children.Add(child);
		child.Arrange(new Rect(30, 40, 20, 20));
		Assert.Equal(new Point(35, 46), child.TranslatePoint(new Point(5, 6), parent));
		Assert.Equal(new Point(-29, -39), parent.TranslatePoint(new Point(1, 1), child));
	});

	[Fact]
	public void TranslatePoint_throws_without_a_common_ancestor() => Run(() =>
	{
		var a = new System.Windows.Controls.Canvas();
		var b = new System.Windows.Controls.Canvas();
		Assert.Throws<InvalidOperationException>(() => a.TranslatePoint(new Point(0, 0), b));
		Assert.Throws<ArgumentNullException>(() => a.TranslatePoint(new Point(0, 0), null!));
	});

	[Fact]
	public void TranslatePoint_resolves_through_the_realised_GTK_widgets() => Run(() =>
	{
		var parent = new System.Windows.Controls.Canvas();
		var child = new System.Windows.Controls.Canvas { Width = 20, Height = 20 };
		System.Windows.Controls.Canvas.SetLeft(child, 30);
		System.Windows.Controls.Canvas.SetTop(child, 40);
		parent.Children.Add(child);
		var win = new Gtk.Window();
		win.SetDefaultSize(200, 200);
		win.Child = parent.Widget;
		win.Present();
		for (int i = 0; i < 200 && Gtk.Application.EventsPending(); i++) Gtk.Application.RunIteration(false);
		Assert.Equal(new Point(35, 46), child.TranslatePoint(new Point(5, 6), parent));
	});
}
