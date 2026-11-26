using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;
public class KeyboardMoveFocusTests : WpfGtkTestBase
{
	static StackPanel Panel(params UIElement[] items)
	{
		(Keyboard.FocusedElement as UIElement)?.SetFocusedInternal(false);
		var p = new StackPanel();
		foreach (var i in items) p.Children.Add(i);
		return p;
	}


	[Fact]
	public void MoveFocus_walks_tab_order() => Run(() =>
	{
		var a = new Button(); var b = new Button(); var c = new Button();
		var panel = Panel(a, b, c);
		a.Focus();
		Assert.True(a.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next)));
		Assert.True(b.IsKeyboardFocused);
		Assert.True(b.MoveFocus(new TraversalRequest(FocusNavigationDirection.Last)));
		Assert.True(c.IsKeyboardFocused);
		var plain = new TraversalRequest(FocusNavigationDirection.Next);
		Assert.False(c.MoveFocus(plain));
		Assert.False(plain.Wrapped);
		Assert.True(c.MoveFocus(new TraversalRequest(FocusNavigationDirection.First)));
		Assert.True(a.IsKeyboardFocused);
		a.SetFocusedInternal(false);
	});

	[Fact]
	public void PredictFocus_returns_the_target_without_moving_focus() => Run(() =>
	{
		var a = new Button(); var b = new Button(); var off = new Button { IsEnabled = false };
		var panel = Panel(a, off, b);
		a.Focus();
		Assert.Same(b, a.PredictFocus(FocusNavigationDirection.Down));
		foreach (var d in new[] { FocusNavigationDirection.Next, FocusNavigationDirection.Previous,
			FocusNavigationDirection.First, FocusNavigationDirection.Last })
			Assert.Throws<ArgumentOutOfRangeException>(() => a.PredictFocus(d));
		Assert.Same(a, b.PredictFocus(FocusNavigationDirection.Up));
		Assert.True(a.IsKeyboardFocused);
		Assert.Throws<InvalidEnumArgumentException>(() => a.PredictFocus((FocusNavigationDirection)99));
		a.SetFocusedInternal(false);
	});

	[Fact]
	public void TabIndex_orders_the_move() => Run(() =>
	{
		var a = new Button(); var b = new Button();
		var panel = Panel(a, b);
		KeyboardNavigation.SetTabIndex(a, 2);
		KeyboardNavigation.SetTabIndex(b, 1);
		Assert.Same(a, b.PredictFocus(FocusNavigationDirection.Right));
	});

	[Fact]
	public void TraversalRequest_validates_the_direction_and_defaults() => Run(() =>
	{
		Assert.Throws<InvalidEnumArgumentException>(() => new TraversalRequest((FocusNavigationDirection)42));
		var r = new TraversalRequest(FocusNavigationDirection.Left);
		Assert.False(r.Wrapped);
		Assert.Equal(FocusNavigationDirection.Left, r.FocusNavigationDirection);
		Assert.Throws<ArgumentNullException>(() => new Button().MoveFocus(null!));
	});

	[Fact]
	public void FocusVisualStyle_has_a_default_and_is_settable() => Run(() =>
	{
		var b = new Button();
		Assert.NotNull(b.FocusVisualStyle);
		Assert.Same(Button.FocusVisualStyleProperty, FrameworkElement.FocusVisualStyleProperty);
		var s = new Style();
		b.FocusVisualStyle = s;
		Assert.Same(s, b.FocusVisualStyle);
		b.FocusVisualStyle = null;
		Assert.Null(b.FocusVisualStyle);
		b.ClearValue(FrameworkElement.FocusVisualStyleProperty);
		Assert.NotNull(b.FocusVisualStyle);
	});

	[Fact]
	public void Window_cycles_and_reports_Wrapped() => Run(() =>
	{
		var a = new Button(); var b = new Button();
		var window = new Window { Content = Panel(a, b) };
		b.Focus();
		var request = new TraversalRequest(FocusNavigationDirection.Next);
		Assert.True(b.MoveFocus(request));
		Assert.True(a.IsKeyboardFocused);
		Assert.True(request.Wrapped);
		var plain = new TraversalRequest(FocusNavigationDirection.Next);
		Assert.True(a.MoveFocus(plain));
		Assert.False(plain.Wrapped);
		b.SetFocusedInternal(false);
		GC.KeepAlive(window);
	});

	[Fact]
	public void Contained_container_stops_at_its_end() => Run(() =>
	{
		var a = new Button(); var b = new Button(); var outside = new Button();
		var inner = Panel(a, b);
		KeyboardNavigation.SetTabNavigation(inner, KeyboardNavigationMode.Contained);
		var outer = new StackPanel();
		outer.Children.Add(inner);
		outer.Children.Add(outside);
		b.Focus();
		var request = new TraversalRequest(FocusNavigationDirection.Next);
		Assert.False(b.MoveFocus(request));
		Assert.False(request.Wrapped);
		Assert.True(b.IsKeyboardFocused);
		b.SetFocusedInternal(false);
	});

	[Fact]
	public void Focusable_non_control_takes_part_in_navigation() => Run(() =>
	{
		var first = new TextBox(); var last = new TextBox();
		var mid = new Border { Focusable = true };
		var panel = Panel(first, mid, last);
		first.Focus();
		Assert.Same(mid, first.PredictFocus(FocusNavigationDirection.Down));
		Assert.True(first.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next)));
		Assert.True(mid.IsKeyboardFocused);
		mid.SetFocusedInternal(false);
	});
}
