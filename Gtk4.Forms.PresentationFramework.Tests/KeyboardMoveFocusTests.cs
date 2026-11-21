using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;
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
		Assert.False(c.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next)));
		Assert.True(c.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next) { Wrapped = true }));
		Assert.True(a.IsKeyboardFocused);
		Assert.True(a.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous) { Wrapped = true }));
		Assert.True(c.IsKeyboardFocused);
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
		Assert.Same(b, a.PredictFocus(FocusNavigationDirection.Next));
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
		Assert.Same(a, b.PredictFocus(FocusNavigationDirection.Next));
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

}
