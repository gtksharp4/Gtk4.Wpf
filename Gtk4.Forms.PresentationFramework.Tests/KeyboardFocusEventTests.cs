using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

public class KeyboardFocusEventTests : WpfGtkTestBase
{
	sealed class Probe : Button
	{
		public List<string> Calls = new List<string>();
		public bool CancelGot;
		protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			Calls.Add("OnPreviewGot");
			e.Handled = CancelGot;
			base.OnPreviewGotKeyboardFocus(e);
		}
		protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e) => Calls.Add("OnGot");
		protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e) => Calls.Add("OnLost");
	}

	static StackPanel Panel(params UIElement[] items)
	{
		(Keyboard.FocusedElement as UIElement)?.SetFocusedInternal(false);
		var p = new StackPanel();
		foreach (var i in items) p.Children.Add(i);
		return p;
	}

	[Fact]
	public void Events_fire_in_WPF_order_with_old_and_new_focus() => Run(() =>
	{
		var a = new Button();
		var b = new Button();
		var log = new List<string>();
		var panel = Panel(a, b);
		foreach (var (el, name) in new[] { (a, "a"), (b, "b") })
		{
			el.PreviewLostKeyboardFocus += (_, e) => log.Add(name + ":PreviewLost");
			el.PreviewGotKeyboardFocus += (_, e) => log.Add(name + ":PreviewGot");
			el.LostKeyboardFocus += (_, e) => log.Add(name + ":Lost");
			el.GotKeyboardFocus += (_, e) => log.Add(name + ":Got");
			el.LostFocus += (_, _) => log.Add(name + ":LostFocus");
			el.GotFocus += (_, _) => log.Add(name + ":GotFocus");
		}
		KeyboardFocusChangedEventArgs? got = null;
		b.GotKeyboardFocus += (_, e) => got = e;
		Assert.True(a.Focus());
		log.Clear();
		Assert.True(b.Focus());
		Assert.Equal(new[]
		{
			"a:PreviewLost", "b:PreviewGot", "a:Lost", "b:Got", "a:LostFocus", "b:GotFocus",
		}, log);
		Assert.Same(a, got!.OldFocus);
		Assert.Same(b, got.NewFocus);
		Assert.Same(Keyboard.PrimaryDevice, got.KeyboardDevice);
		b.SetFocusedInternal(false);
	});

	[Fact]
	public void Events_bubble_to_a_parent_and_report_the_source() => Run(() =>
	{
		var a = new Button();
		var panel = Panel(a);
		object? source = null;
		panel.AddHandler(UIElement.GotKeyboardFocusEvent,
			new KeyboardFocusChangedEventHandler((_, e) => source = e.Source));
		a.Focus();
		Assert.Same(a, source);
		a.SetFocusedInternal(false);
	});

	[Fact]
	public void A_handled_PreviewGotKeyboardFocus_cancels_the_move() => Run(() =>
	{
		var a = new Button();
		var p = new Probe { CancelGot = true };
		var panel = Panel(a, p);
		a.Focus();
		Assert.False(p.Focus());
		Assert.False(p.IsKeyboardFocused);
		Assert.Same(a, Keyboard.FocusedElement);
		Assert.Contains("OnPreviewGot", p.Calls);
		Assert.DoesNotContain("OnGot", p.Calls);
		Assert.Same(a, Keyboard.Focus(p));
		a.SetFocusedInternal(false);
	});

	[Fact]
	public void A_handled_PreviewLostKeyboardFocus_keeps_the_old_focus() => Run(() =>
	{
		var a = new Button();
		var b = new Button();
		var panel = Panel(a, b);
		KeyboardFocusChangedEventHandler cancel = (_, e) => e.Handled = true;
		a.PreviewLostKeyboardFocus += cancel;
		a.Focus();
		Assert.False(b.Focus());
		Assert.True(a.IsKeyboardFocused);
		a.PreviewLostKeyboardFocus -= cancel;
		a.SetFocusedInternal(false);
	});

	[Fact]
	public void Class_handlers_run_on_the_virtual_methods() => Run(() =>
	{
		var a = new Probe();
		var b = new Probe();
		var panel = Panel(a, b);
		a.Focus();
		b.Focus();
		Assert.Equal(new[] { "OnPreviewGot", "OnGot" }, a.Calls.GetRange(0, 2));
		Assert.Equal("OnLost", a.Calls[a.Calls.Count - 1]);
		b.SetFocusedInternal(false);
	});

	[Fact]
	public void IsKeyboardFocused_and_IsKeyboardFocusWithin_follow_focus() => Run(() =>
	{
		var a = new Button();
		var b = new Button();
		var panel = Panel(a, b);
		var changes = new List<bool>();
		a.IsKeyboardFocusedChanged += (_, e) => changes.Add(e.NewValue is true);
		Assert.False(a.IsKeyboardFocused);
		a.Focus();
		Assert.True(a.IsKeyboardFocused);
		Assert.True(a.IsFocused);
		Assert.True(a.IsKeyboardFocusWithin);
		Assert.True(panel.IsKeyboardFocusWithin);
		Assert.False(b.IsKeyboardFocusWithin);
		b.Focus();
		Assert.False(a.IsKeyboardFocused);
		Assert.False(a.IsKeyboardFocusWithin);
		Assert.True(panel.IsKeyboardFocusWithin);
		Keyboard.ClearFocus();
		Assert.False(panel.IsKeyboardFocusWithin);
		Assert.Null(Keyboard.FocusedElement);
		Assert.Equal(new[] { true, false }, changes);
	});

	[Fact]
	public void Keyboard_attached_handlers_add_and_remove() => Run(() =>
	{
		var a = new Button();
		var panel = Panel(a);
		int n = 0;
		KeyboardFocusChangedEventHandler h = (_, _) => n++;
		Keyboard.AddGotKeyboardFocusHandler(a, h);
		a.Focus();
		Assert.Equal(1, n);
		a.SetFocusedInternal(false);
		Keyboard.RemoveGotKeyboardFocusHandler(a, h);
		a.Focus();
		Assert.Equal(1, n);
		Assert.Throws<ArgumentNullException>(() => Keyboard.AddGotKeyboardFocusHandler(null!, h));
		a.SetFocusedInternal(false);
	});

	[Fact]
	public void The_event_identities_are_shared_with_Keyboard() => Run(() =>
	{
		Assert.Same(Keyboard.GotKeyboardFocusEvent, UIElement.GotKeyboardFocusEvent);
		Assert.Same(Keyboard.GotKeyboardFocusEvent, EventManager.GetRoutedEvent(typeof(Button), "GotKeyboardFocus"));
		Assert.Equal(RoutingStrategy.Tunnel, UIElement.PreviewLostKeyboardFocusEvent.RoutingStrategy);
		Assert.Equal(RoutingStrategy.Bubble, UIElement.LostKeyboardFocusEvent.RoutingStrategy);
	});

	[Fact]
	public void EventArgs_constructor_accepts_a_null_device_like_WPF() => Run(() =>
	{
		Assert.Null(new KeyboardFocusChangedEventArgs(null!, 0, null, null).KeyboardDevice);
		var e = new KeyboardFocusChangedEventArgs(Keyboard.PrimaryDevice, 7, null, null);
		Assert.Equal(7, e.Timestamp);
	});

}
