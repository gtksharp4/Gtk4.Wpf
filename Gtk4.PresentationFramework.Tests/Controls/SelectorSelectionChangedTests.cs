using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// <see cref="Selector.SelectionChanged"/> is a routed event of type <see cref="SelectionChangedEventHandler"/>
/// that is held back while an <c>InitializeComponent</c> (<c>BeginInit</c>..<c>EndInit</c>) is running.
/// </summary>
public class SelectorSelectionChangedTests : WpfGtkTestBase
{
	static Selector Make(string kind)
	{
		var items = new[] { "a", "b", "c" };
		switch (kind)
		{
			case "ListBox": return new ListBox { ItemsSource = items };
			case "ComboBox": return new ComboBox { ItemsSource = items };
			default:
				var tabs = new TabControl();
				foreach (var h in items) tabs.Items.Add(new TabItem { Header = h });
				return tabs;
		}
	}

	static DependencyProperty IndexProperty(Selector s) =>
		s is ListBox ? ListBox.SelectedIndexProperty
		: s is ComboBox ? ComboBox.SelectedIndexProperty : TabControl.SelectedIndexProperty;

	static void Select(Selector s, int index) => s.SetValue(IndexProperty(s), index);

	// ── Routed event shape ───────────────────────────────────────────────────

	[Fact]
	public void SelectionChanged_is_a_routed_event_with_the_SelectionChangedEventHandler_type()
	{
		Assert.Equal(typeof(SelectionChangedEventHandler), Selector.SelectionChangedEvent.HandlerType);
		Assert.Equal(RoutingStrategy.Bubble, Selector.SelectionChangedEvent.RoutingStrategy);
		Assert.Equal("SelectionChanged", Selector.SelectionChangedEvent.Name);
	}

	[Theory]
	[InlineData("ListBox")]
	[InlineData("ComboBox")]
	[InlineData("TabControl")]
	public void Handler_added_through_AddHandler_receives_the_event_with_Source_and_RoutedEvent(string kind) => Run(() =>
	{
		var s = Make(kind);
		SelectionChangedEventArgs? seen = null;
		object? sender = null;
		s.AddHandler(Selector.SelectionChangedEvent,
			new SelectionChangedEventHandler((o, e) => { sender = o; seen = e; }));

		Select(s, 1);

		Assert.NotNull(seen);
		Assert.Same(s, sender);
		Assert.Same(s, seen!.Source);
		Assert.Same(Selector.SelectionChangedEvent, seen.RoutedEvent);
		Assert.Single(seen.AddedItems);
	});

	// ── Deferral during initialization ───────────────────────────────────────

	[Theory]
	[InlineData("ListBox")]
	[InlineData("ComboBox")]
	[InlineData("TabControl")]
	public void A_selector_created_in_a_root_queues_one_event_per_change_until_its_EndInit(string kind) => Run(() =>
	{
		var root = new Border();
		root.BeginInit();
		var s = Make(kind);
		var events = new List<SelectionChangedEventArgs>();
		s.SelectionChanged += (_, e) => events.Add(e);

		Select(s, 0);
		Select(s, 2);
		Assert.Empty(events);
		root.EndInit();

		Assert.Equal(2, events.Count);
		Assert.Single(events[0].AddedItems);
		Assert.Empty(events[0].RemovedItems);
		Assert.Single(events[1].RemovedItems);
		Assert.Single(events[1].AddedItems);
		Assert.Equal(2, s.GetValue(IndexProperty(s)));
	});

	[Fact]
	public void A_change_that_adds_and_removes_nothing_raises_no_event() => Run(() =>
	{
		var root = new Border();
		root.BeginInit();
		var lb = new ListBox();
		var count = 0;
		lb.SelectionChanged += (_, _) => count++;

		lb.SetValue(ListBox.SelectedIndexProperty, 0);
		root.EndInit();
		lb.SetValue(ListBox.SelectedIndexProperty, 0);

		Assert.Equal(0, count);
	});

	[Fact]
	public void Several_changes_during_deferral_are_not_merged() => Run(() =>
	{
		var root = new Border();
		root.BeginInit();
		var lb = (ListBox)Make("ListBox");
		var count = 0;
		lb.SelectionChanged += (_, _) => count++;

		lb.SelectedIndex = 0;
		lb.SelectedIndex = 1;
		lb.SelectedIndex = 2;
		root.EndInit();

		Assert.Equal(3, count);
	});

	[Fact]
	public void An_unrelated_BeginInit_does_not_defer_a_standalone_selector() => Run(() =>
	{
		var lb = (ListBox)Make("ListBox");
		var count = 0;
		lb.SelectionChanged += (_, _) => count++;
		var unrelated = new Border();
		unrelated.BeginInit();

		lb.SelectedIndex = 1;

		Assert.Equal(1, count);
		unrelated.EndInit();
		Assert.Equal(1, count);
	});

	[Fact]
	public void Events_are_raised_again_immediately_after_initialization() => Run(() =>
	{
		var root = new Border();
		root.BeginInit();
		var lb = (ListBox)Make("ListBox");
		root.EndInit();
		var count = 0;
		lb.SelectionChanged += (_, _) => count++;

		lb.SelectedIndex = 1;

		Assert.Equal(1, count);
	});

	[Fact]
	public void A_nested_root_delivers_at_its_own_EndInit_before_the_outer_one() => Run(() =>
	{
		var outer = new Border();
		var inner = new Border();
		outer.BeginInit();
		inner.BeginInit();
		var lb = (ListBox)Make("ListBox");
		var count = 0;
		lb.SelectionChanged += (_, _) => count++;
		lb.SelectedIndex = 0;
		Assert.Equal(0, count);

		inner.EndInit();

		Assert.Equal(1, count);
		outer.EndInit();
		Assert.Equal(1, count);
	});

	[Fact]
	public void A_set_after_the_nested_root_ended_raises_synchronously() => Run(() =>
	{
		var outer = new Border();
		var inner = new Border();
		outer.BeginInit();
		inner.BeginInit();
		var lb = (ListBox)Make("ListBox");
		inner.EndInit();
		var count = 0;
		lb.SelectionChanged += (_, _) => count++;

		lb.SelectedIndex = 0;

		Assert.Equal(1, count);
		outer.EndInit();
	});

	[Fact]
	public void A_selectors_own_BeginInit_holds_its_events_until_its_EndInit() => Run(() =>
	{
		var lb = (ListBox)Make("ListBox");
		var count = 0;
		lb.SelectionChanged += (_, _) => count++;

		lb.BeginInit();
		lb.SelectedIndex = 1;
		Assert.Equal(0, count);
		lb.EndInit();

		Assert.Equal(1, count);
	});

	[Fact]
	public void The_handler_runs_after_the_whole_InitializeComponent_body() => Run(() =>
	{
		var root = new Border();
		root.BeginInit();
		var lb = (ListBox)Make("ListBox");
		var order = new List<string>();
		lb.SelectionChanged += (_, _) => order.Add("event");

		lb.SelectedIndex = 0;
		order.Add("rest of InitializeComponent");
		root.EndInit();

		Assert.Equal(new[] { "rest of InitializeComponent", "event" }, order);
	});

	[Fact]
	public void AbandonInitialization_releases_the_scope_and_drops_queued_events() => Run(() =>
	{
		var root = new Border();
		root.BeginInit();
		var held = (ListBox)Make("ListBox");
		var heldCount = 0;
		held.SelectionChanged += (_, _) => heldCount++;
		held.SelectedIndex = 0;

		FrameworkElement.AbandonInitialization(root);
		var other = (ListBox)Make("ListBox");
		var otherCount = 0;
		other.SelectionChanged += (_, _) => otherCount++;
		other.SelectedIndex = 1;

		Assert.Equal(1, otherCount);
		Assert.False(root.IsInitialized);
		held.SelectedIndex = 2;
		Assert.Equal(1, heldCount);
	});

	[Fact]
	public void A_throwing_handler_does_not_stop_the_other_queued_events() => Run(() =>
	{
		var root = new Border();
		root.BeginInit();
		var first = (ListBox)Make("ListBox");
		var second = (ListBox)Make("ListBox");
		var secondRan = false;
		var boom = true;
		first.SelectionChanged += (_, _) => { if (boom) throw new InvalidOperationException("boom"); };
		second.SelectionChanged += (_, _) => secondRan = true;
		first.SelectedIndex = 0;
		second.SelectedIndex = 0;

		Assert.Throws<InvalidOperationException>(() => root.EndInit());

		Assert.True(secondRan);
		boom = false;
		var again = 0;
		first.SelectionChanged += (_, _) => again++;
		first.SelectedIndex = 1;
		Assert.Equal(1, again);
	});
}
