using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Lifecycle;

/// <summary>
/// Pure (no GTK) tests for the <see cref="FrameworkElement.Loaded"/> /
/// <see cref="FrameworkElement.Unloaded"/> broadcast (<c>LifecycleBroadcaster</c>).
/// </summary>
public class LoadedUnloadedTests
{
	[Fact]
	public void LoadedEvent_is_Direct_and_owned_by_FrameworkElement()
	{
		Assert.Equal("Loaded", FrameworkElement.LoadedEvent.Name);
		Assert.Equal(RoutingStrategy.Direct, FrameworkElement.LoadedEvent.RoutingStrategy);
		Assert.Equal(typeof(FrameworkElement), FrameworkElement.LoadedEvent.OwnerType);
		Assert.Equal("Unloaded", FrameworkElement.UnloadedEvent.Name);
		Assert.Equal(RoutingStrategy.Direct, FrameworkElement.UnloadedEvent.RoutingStrategy);
	}

	[Fact]
	public void PreOrder_enumeration_is_root_child_grandchild_sibling()
	{
		var root = LifecycleTree.Build(out var inner, out var grand, out var sibling);

		var order = LifecycleBroadcaster.EnumeratePreOrder(root).ToList();

		Assert.Equal(new UIElement[] { root, inner, grand, sibling }, order);
	}

	[Fact]
	public void BroadcastLoaded_raises_pre_order_and_sets_IsLoaded()
	{
		var root = LifecycleTree.Build(out var inner, out var grand, out var sibling);
		var log = new List<string>();
		LifecycleTree.Record(log, "", root, inner, grand, sibling);

		LifecycleBroadcaster.BroadcastLoaded(root);

		Assert.Equal(new[] { "Loaded:root", "Loaded:inner", "Loaded:grand", "Loaded:sibling" }, log);
		Assert.True(root.IsLoaded && inner.IsLoaded && grand.IsLoaded && sibling.IsLoaded);
	}

	[Fact]
	public void BroadcastUnloaded_uses_same_pre_order_and_clears_IsLoaded()
	{
		var root = LifecycleTree.Build(out var inner, out var grand, out var sibling);
		LifecycleBroadcaster.BroadcastLoaded(root);
		var log = new List<string>();
		LifecycleTree.Record(log, "", root, inner, grand, sibling);

		LifecycleBroadcaster.BroadcastUnloaded(root);

		Assert.Equal(new[] { "Unloaded:root", "Unloaded:inner", "Unloaded:grand", "Unloaded:sibling" }, log);
		Assert.False(root.IsLoaded || inner.IsLoaded || grand.IsLoaded || sibling.IsLoaded);
	}

	[Fact]
	public void Loaded_is_not_raised_twice_while_loaded_but_cycle_is_allowed()
	{
		var border = new Border { Name = "b" };
		var log = new List<string>();
		LifecycleTree.Record(log, "", border);

		LifecycleBroadcaster.BroadcastLoaded(border);
		LifecycleBroadcaster.BroadcastLoaded(border);
		Assert.Single(log);

		LifecycleBroadcaster.BroadcastUnloaded(border);
		LifecycleBroadcaster.BroadcastUnloaded(border);
		LifecycleBroadcaster.BroadcastLoaded(border);

		Assert.Equal(new[] { "Loaded:b", "Unloaded:b", "Loaded:b" }, log);
	}

	[Fact]
	public void IsLoaded_is_true_inside_Loaded_handler()
	{
		var border = new Border();
		bool? seen = null;
		border.Loaded += (_, _) => seen = border.IsLoaded;

		Assert.False(border.IsLoaded);
		LifecycleBroadcaster.BroadcastLoaded(border);

		Assert.True(seen);
	}

	[Fact]
	public void Attaching_to_loaded_parent_defers_Loaded_to_dispatcher()
	{
		using var queue = new ManualDispatcherQueue();
		var parent = new StackPanel { Name = "parent" };
		LifecycleBroadcaster.BroadcastLoaded(parent);

		var child = new StackPanel { Name = "child" };
		var leaf = new Border { Name = "leaf" };
		child.Children.Add(leaf);
		var log = new List<string>();
		LifecycleTree.Record(log, "", child, leaf);

		parent.Children.Add(child);
		Assert.Empty(log); // not synchronous
		Assert.False(child.IsLoaded);

		queue.Pump();

		Assert.Equal(new[] { "Loaded:child", "Loaded:leaf" }, log);
	}

	[Fact]
	public void Detaching_from_loaded_parent_raises_Unloaded_on_subtree()
	{
		using var queue = new ManualDispatcherQueue();
		var root = LifecycleTree.Build(out var inner, out var grand, out _);
		LifecycleBroadcaster.BroadcastLoaded(root);
		var log = new List<string>();
		LifecycleTree.Record(log, "", inner, grand);

		root.Children.Remove(inner);
		queue.Pump();

		Assert.Equal(new[] { "Unloaded:inner", "Unloaded:grand" }, log);
		Assert.True(root.IsLoaded);
		Assert.False(inner.IsLoaded);
	}

	[Fact]
	public void Detach_then_reattach_before_pump_raises_nothing()
	{
		using var queue = new ManualDispatcherQueue();
		var root = LifecycleTree.Build(out var inner, out _, out _);
		LifecycleBroadcaster.BroadcastLoaded(root);
		var log = new List<string>();
		LifecycleTree.Record(log, "", inner);

		root.Children.Remove(inner);
		root.Children.Add(inner);
		queue.Pump();

		Assert.Empty(log);
		Assert.True(inner.IsLoaded);
	}

	[Fact]
	public void Reparenting_within_loaded_tree_raises_nothing_after_pump()
	{
		using var queue = new ManualDispatcherQueue();
		var root = LifecycleTree.Build(out var inner, out var grand, out var sibling);
		LifecycleBroadcaster.BroadcastLoaded(root);
		var log = new List<string>();
		LifecycleTree.Record(log, "", grand);

		// Move grand from inner to a loaded sibling container — it never leaves a loaded tree.
		inner.Children.Remove(grand);
		root.Children.Add(grand);
		queue.Pump();

		Assert.Empty(log);
		Assert.True(grand.IsLoaded);
	}

	[Fact]
	public void Loaded_Source_is_element_and_Direct_does_not_bubble_to_parent()
	{
		var root = LifecycleTree.Build(out var inner, out _, out _);
		var parentSources = new List<object?>();
		root.AddHandler(FrameworkElement.LoadedEvent,
			new RoutedEventHandler((_, e) => parentSources.Add(e.Source)));
		object? innerSource = null;
		object? innerSender = null;
		inner.Loaded += (s, e) => { innerSender = s; innerSource = e.Source; };

		LifecycleBroadcaster.BroadcastLoaded(root);

		Assert.Same(inner, innerSource);
		Assert.Same(inner, innerSender);
		Assert.Equal(new object?[] { root }, parentSources); // only its own Loaded
	}
}
