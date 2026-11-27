using System.Collections.Generic;
using System.Windows;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

public class RoutedEventSourceAdjustTests
{
	class Node : DependencyObject
	{
		public bool Adjusts;
		public Node? Parent { get => (Node?)InheritanceParent; set => InheritanceParent = value; }
		internal override object? AdjustEventSource(RoutedEventArgs e) => Adjusts ? this : null;
	}

	static readonly RoutedEvent Bubbled = EventManager.RegisterRoutedEvent(
		"SrcBubble", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RoutedEventSourceAdjustTests));
	static readonly RoutedEvent Tunnelled = EventManager.RegisterRoutedEvent(
		"SrcTunnel", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(RoutedEventSourceAdjustTests));

	static void Record(Node n, RoutedEvent ev, List<object?> seen) =>
		n.AddHandler(ev, new RoutedEventHandler((s, e) => seen.Add(e.Source)));

	[Fact]
	public void Bubble_source_is_retargeted_from_the_adjusting_element_outwards()
	{
		var root = new Node();
		var frame = new Node { Adjusts = true, Parent = root };
		var inner = new Node { Parent = frame };
		var seen = new List<object?>();
		Record(inner, Bubbled, seen);
		Record(frame, Bubbled, seen);
		Record(root, Bubbled, seen);

		inner.RaiseEvent(new RoutedEventArgs(Bubbled));

		Assert.Equal(new object?[] { inner, frame, frame }, seen);
	}

	[Fact]
	public void OriginalSource_stays_the_raising_element()
	{
		var frame = new Node { Adjusts = true };
		var inner = new Node { Parent = frame };
		object? original = null;
		frame.AddHandler(Bubbled, new RoutedEventHandler((s, e) => original = e.OriginalSource));

		inner.RaiseEvent(new RoutedEventArgs(Bubbled));

		Assert.Same(inner, original);
	}

	[Fact]
	public void Tunnel_source_matches_the_bubble_mapping_per_element()
	{
		var root = new Node();
		var frame = new Node { Adjusts = true, Parent = root };
		var inner = new Node { Parent = frame };
		var seen = new List<object?>();
		Record(root, Tunnelled, seen);
		Record(frame, Tunnelled, seen);
		Record(inner, Tunnelled, seen);

		inner.RaiseEvent(new RoutedEventArgs(Tunnelled));

		Assert.Equal(new object?[] { frame, frame, inner }, seen);
	}

	[Fact]
	public void No_adjuster_leaves_Source_untouched()
	{
		var root = new Node();
		var inner = new Node { Parent = root };
		var seen = new List<object?>();
		Record(root, Bubbled, seen);

		inner.RaiseEvent(new RoutedEventArgs(Bubbled));

		Assert.Same(inner, seen[0]);
	}
}
