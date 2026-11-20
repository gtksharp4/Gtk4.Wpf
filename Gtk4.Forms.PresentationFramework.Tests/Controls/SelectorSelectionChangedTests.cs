using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Controls;

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
	public void No_event_while_initializing_one_event_with_the_final_selection_at_EndInit(string kind) => Run(() =>
	{
		var s = Make(kind);
		var root = new Border();
		var events = new List<SelectionChangedEventArgs>();
		s.SelectionChanged += (_, e) => events.Add(e);

		root.BeginInit();
		Select(s, 0);
		Select(s, 2);
		Assert.Empty(events);
		root.EndInit();

		var e = Assert.Single(events);
		Assert.Single(e.AddedItems);
		Assert.Empty(e.RemovedItems);
		Assert.Equal(2, s.GetValue(IndexProperty(s)));
	});

	[Fact]
	public void A_selection_made_and_undone_during_initialization_raises_nothing() => Run(() =>
	{
		var lb = (ListBox)Make("ListBox");
		var root = new Border();
		var count = 0;
		lb.SelectionChanged += (_, _) => count++;

		root.BeginInit();
		lb.SelectedIndex = 1;
		lb.SelectedIndex = -1;
		root.EndInit();

		Assert.Equal(0, count);
	});

	[Fact]
	public void Events_are_raised_again_immediately_after_initialization() => Run(() =>
	{
		var lb = (ListBox)Make("ListBox");
		var root = new Border();
		var count = 0;
		lb.SelectionChanged += (_, _) => count++;
		root.BeginInit();
		root.EndInit();

		lb.SelectedIndex = 1;

		Assert.Equal(1, count);
	});

	[Fact]
	public void Nested_initialization_delivers_at_the_outermost_EndInit() => Run(() =>
	{
		var lb = (ListBox)Make("ListBox");
		var outer = new Border();
		var inner = new Border();
		var count = 0;
		lb.SelectionChanged += (_, _) => count++;

		outer.BeginInit();
		inner.BeginInit();
		lb.SelectedIndex = 0;
		inner.EndInit();
		Assert.Equal(0, count);
		outer.EndInit();

		Assert.Equal(1, count);
	});

	[Fact]
	public void The_handler_runs_after_the_whole_InitializeComponent_body() => Run(() =>
	{
		var lb = (ListBox)Make("ListBox");
		var root = new Border();
		var order = new List<string>();
		lb.SelectionChanged += (_, _) => order.Add("event");

		root.BeginInit();
		lb.SelectedIndex = 0;
		order.Add("rest of InitializeComponent");
		root.EndInit();

		Assert.Equal(new[] { "rest of InitializeComponent", "event" }, order);
	});
}
