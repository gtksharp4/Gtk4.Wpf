using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class KeyboardMoveFocusModeTests : WpfGtkTestBase
{
	class FocusableBorder : Border
	{
		static FocusableBorder() => FocusableProperty.OverrideMetadata(typeof(FocusableBorder),
			new PropertyMetadata(true));
	}

	static StackPanel Panel(params UIElement[] items)
	{
		(Keyboard.FocusedElement as UIElement)?.SetFocusedInternal(false);
		var p = new StackPanel();
		foreach (var i in items) p.Children.Add(i);
		return p;
	}

	[Fact]
	public void Explicit_Continue_defers_to_the_window_cycle() => Run(() =>
	{
		var a = new Button(); var b = new Button();
		var panel = Panel(a, b);
		KeyboardNavigation.SetTabNavigation(panel, KeyboardNavigationMode.Continue);
		var window = new Window { Content = panel };
		b.Focus();
		var request = new TraversalRequest(FocusNavigationDirection.Next);
		Assert.True(b.MoveFocus(request));
		Assert.True(a.IsKeyboardFocused);
		Assert.True(request.Wrapped);
		a.SetFocusedInternal(false);
		System.GC.KeepAlive(window);
	});

	[Fact]
	public void Contained_panel_in_a_window_does_not_leave_or_cycle() => Run(() =>
	{
		var a = new Button(); var b = new Button(); var outside = new Button();
		var inner = Panel(a, b);
		KeyboardNavigation.SetTabNavigation(inner, KeyboardNavigationMode.Contained);
		var outer = new StackPanel();
		outer.Children.Add(inner);
		outer.Children.Add(outside);
		var window = new Window { Content = outer };
		b.Focus();
		var request = new TraversalRequest(FocusNavigationDirection.Next);
		Assert.False(b.MoveFocus(request));
		Assert.False(request.Wrapped);
		Assert.True(b.IsKeyboardFocused);
		b.SetFocusedInternal(false);
		System.GC.KeepAlive(window);
	});

	[Fact]
	public void A_style_set_TabNavigation_is_honoured() => Run(() =>
	{
		var a = new Button(); var b = new Button();
		var panel = Panel(a, b);
		var style = new Style(typeof(StackPanel));
		style.Setters.Add(new Setter(KeyboardNavigation.TabNavigationProperty, KeyboardNavigationMode.Cycle));
		panel.Style = style;
		b.Focus();
		var request = new TraversalRequest(FocusNavigationDirection.Next);
		Assert.True(b.MoveFocus(request));
		Assert.True(a.IsKeyboardFocused);
		Assert.True(request.Wrapped);
		a.SetFocusedInternal(false);
	});

	[Fact]
	public void A_non_control_made_focusable_by_a_style_or_metadata_takes_part() => Run(() =>
	{
		var first = new TextBox(); var last = new TextBox();
		var styled = new Border();
		var style = new Style(typeof(Border));
		style.Setters.Add(new Setter(UIElement.FocusableProperty, true));
		styled.Style = style;
		var meta = new FocusableBorder();
		var panel = Panel(first, styled, meta, last);
		first.Focus();
		Assert.Same(styled, first.PredictFocus(FocusNavigationDirection.Down));
		Assert.Same(meta, styled.PredictFocus(FocusNavigationDirection.Down));
		first.SetFocusedInternal(false);
	});
}
