using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class TextBoxSelectionChangedRoutedTests : WpfGtkTestBase
{
	[Fact]
	public void The_event_is_a_bubbling_RoutedEventHandler_on_TextBoxBase()
	{
		Assert.Equal(RoutingStrategy.Bubble, TextBoxBase.SelectionChangedEvent.RoutingStrategy);
		Assert.Equal(typeof(RoutedEventHandler), TextBoxBase.SelectionChangedEvent.HandlerType);
		Assert.Equal(typeof(TextBoxBase), TextBoxBase.SelectionChangedEvent.OwnerType);
		Assert.True(typeof(TextBoxBase).IsAbstract);
		Assert.True(typeof(TextBoxBase).IsAssignableFrom(typeof(TextBox)));
		Assert.True(typeof(TextBoxBase).IsAssignableFrom(typeof(RichTextBox)));
	}

	[Fact]
	public void TextBox_raises_it_once_per_selection_change_with_itself_as_source()
	{
		var tb = new TextBox { Text = "hello world" };
		RoutedEventArgs? seen = null;
		int count = 0;
		tb.SelectionChanged += (s, e) => { count++; seen = e; };
		tb.SelectionStart = 3;
		Assert.Equal(1, count);
		Assert.Same(tb, seen!.Source);
		Assert.Same(TextBoxBase.SelectionChangedEvent, seen.RoutedEvent);
	}

	[Fact]
	public void The_event_bubbles_to_an_ancestor_handler()
	{
		var tb = new TextBox { Text = "hello world" };
		var panel = new StackPanel();
		panel.Children.Add(tb);
		object? source = null;
		panel.AddHandler(TextBoxBase.SelectionChangedEvent, new RoutedEventHandler((s, e) => source = e.Source));
		tb.SelectionLength = 4;
		Assert.Same(tb, source);
	}

	[Fact]
	public void Handlers_are_removed_with_the_CLR_event_accessor()
	{
		var tb = new TextBox { Text = "abc" };
		int count = 0;
		RoutedEventHandler handler = (s, e) => count++;
		tb.SelectionChanged += handler;
		tb.SelectionChanged -= handler;
		tb.SelectionStart = 1;
		Assert.Equal(0, count);
	}

	[Fact]
	public void RichTextBox_raises_it_when_the_selection_changes() => Run(() =>
	{
		var rtb = new RichTextBox();
		_ = rtb.Widget;
		rtb.AppendText("abc");
		var panel = new StackPanel();
		panel.Children.Add(rtb);
		bool bubbled = false;
		panel.AddHandler(TextBoxBase.SelectionChangedEvent, new RoutedEventHandler((s, e) => bubbled = true));
		rtb.SelectAll();
		Assert.True(bubbled);
	});
}
