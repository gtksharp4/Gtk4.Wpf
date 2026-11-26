using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Round 49a: Parent, LayoutUpdated, deferred resource references, the default Control host.</summary>
public class Round49aElementTests
{
	sealed class El : FrameworkElement
	{
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register("V49", typeof(object), typeof(El), new PropertyMetadata(null));

		public object? Value => GetValue(ValueProperty);

		protected override Gtk.Widget CreateWidget() => throw new NotSupportedException("pure test");

		protected override Size MeasureOverride(Size availableSize) => new Size(10, 10);
	}

	[Fact]
	public void Parent_is_the_logical_parent_and_null_for_a_root()
	{
		var root = new El();
		var child = new El();
		child.LogicalParent = root;
		Assert.Null(root.Parent);
		Assert.Same(root, child.Parent);
	}

	[Fact]
	public void Parent_is_null_for_a_template_part()
	{
		var host = new El();
		var part = new El { TemplatedParent = host };
		part.LogicalParent = host;
		Assert.Null(part.Parent);
	}

	static void Layout(El worker, double size)
	{
		worker.InvalidateMeasure();
		worker.Measure(new Size(size, size));
		worker.Arrange(new Rect(0, 0, size, size));
	}

	[Fact]
	public void LayoutUpdated_fires_every_subscriber_with_a_null_sender_after_a_layout_pass()
	{
		var a = new El();
		var b = new El();
		var senders = new List<object?>();
		a.LayoutUpdated += (s, _) => senders.Add(s);
		b.LayoutUpdated += (s, _) => senders.Add(s);
		Layout(new El(), 100);
		Assert.Equal(2, senders.Count);
		Assert.All(senders, s => Assert.Null(s));
	}

	[Fact]
	public void LayoutUpdated_does_not_fire_when_nothing_was_laid_out()
	{
		var worker = new El();
		Layout(worker, 50);
		int fired = 0;
		var a = new El();
		a.LayoutUpdated += (_, _) => fired++;
		worker.Measure(new Size(50, 50));
		worker.Arrange(new Rect(0, 0, 50, 50));
		Assert.Equal(0, fired);
	}

	[Fact]
	public void LayoutUpdated_unsubscribe_stops_notification()
	{
		int fired = 0;
		EventHandler h = (_, _) => fired++;
		var a = new El();
		a.LayoutUpdated += h;
		a.LayoutUpdated -= h;
		Layout(new El(), 30);
		Assert.Equal(0, fired);
	}

	[Fact]
	public void A_resource_reference_on_an_unparented_element_resolves_when_it_joins_the_tree()
	{
		var parent = new El();
		parent.Resources["k"] = "late";
		var child = new El();
		child.SetResourceReference(El.ValueProperty, "k");
		Assert.Null(child.Value);
		child.LogicalParent = parent;
		Assert.Equal("late", child.Value);
		parent.Resources["k"] = "changed";
		Assert.Equal("changed", child.Value);
	}

	[Fact]
	public void A_resource_reference_deeper_in_a_subtree_resolves_when_the_subtree_root_is_attached()
	{
		var top = new El();
		top.Resources["k"] = 7;
		var mid = new El();
		var leaf = new El();
		leaf.LogicalParent = mid;
		leaf.SetResourceReference(El.ValueProperty, "k");
		Assert.Null(leaf.Value);
		mid.LogicalParent = top;
		Assert.Equal(7, leaf.Value);
	}

	[Fact]
	public void A_missing_dynamic_resource_keeps_the_default_and_never_throws()
	{
		var parent = new El();
		var child = new El();
		child.SetResourceReference(El.ValueProperty, "absent");
		child.LogicalParent = parent;
		Assert.Null(child.Value);
	}
}

/// <summary>A user Control that does not override CreateWidget builds and hosts its template on GTK.</summary>
public class Round49aControlHostTests : WpfGtkTestBase
{
	sealed class Gauge : Control
	{
		public int Applied;
		public override void OnApplyTemplate() => Applied++;
	}

	[Fact]
	public void A_Control_subclass_without_CreateWidget_hosts_its_template() => Run(() =>
	{
		var gauge = new Gauge
		{
			Template = new ControlTemplate { Factory = () => new TextBlock { Text = "t" } }
		};
		Assert.NotNull(gauge.Widget);
		Assert.True(gauge.Applied >= 1);
	});

	[Fact]
	public void A_Window_content_reports_the_Window_as_its_Parent() => Run(() =>
	{
		var panel = new StackPanel();
		var window = new Window { Content = panel };
		Assert.Same(window, panel.Parent);
		Assert.Null(window.Parent);
	});

	[Fact]
	public void A_Control_subclass_without_a_template_still_gets_a_widget() => Run(() =>
		Assert.NotNull(new Gauge().Widget));
}
