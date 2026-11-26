using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class ContextMenuEventTests : WpfGtkTestBase
{
	static (Border root, Border inner) Tree()
	{
		var inner = new Border();
		var root = new Border { Child = inner };
		return (root, inner);
	}

	static void RightRelease(UIElement e) => e.DispatchGtkButton(null, 3, new Point(4, 5), 1, pressed: false);

	[Fact]
	public void A_right_release_raises_ContextMenuOpening_with_the_pointer_position() => Run(() =>
	{
		var (root, inner) = Tree();
		ContextMenuEventArgs? seen = null;
		object? sender = null;
		root.ContextMenuOpening += (s, e) => { sender = s; seen = e; };
		RightRelease(inner);
		Assert.Same(root, sender);
		Assert.Same(inner, seen!.Source);
		Assert.Equal(4, seen.CursorLeft);
		Assert.Equal(5, seen.CursorTop);
		Assert.Same(FrameworkElement.ContextMenuOpeningEvent, seen.RoutedEvent);
	});

	[Fact]
	public void A_left_release_does_not_raise_ContextMenuOpening() => Run(() =>
	{
		var (_, inner) = Tree();
		int n = 0;
		inner.ContextMenuOpening += (_, _) => n++;
		inner.DispatchGtkButton(null, 1, new Point(), 1, pressed: false);
		inner.DispatchGtkButton(null, 3, new Point(), 1, pressed: true);
		Assert.Equal(0, n);
	});

	[Fact]
	public void A_handler_can_assign_the_menu_and_an_unhandled_event_opens_it() => Run(() =>
	{
		var (_, inner) = Tree();
		var menu = new ContextMenu();
		int opened = 0;
		menu.Opened += (_, _) => opened++;
		inner.ContextMenuOpening += (_, _) => inner.ContextMenu = menu;
		RightRelease(inner);
		Assert.Equal(1, opened);
		Assert.True(menu.IsOpen);
	});

	[Fact]
	public void A_handled_ContextMenuOpening_suppresses_the_menu() => Run(() =>
	{
		var (_, inner) = Tree();
		var menu = new ContextMenu();
		inner.ContextMenu = menu;
		inner.ContextMenuOpening += (_, e) => e.Handled = true;
		RightRelease(inner);
		Assert.False(menu.IsOpen);
	});

	[Fact]
	public void The_menu_of_the_nearest_ancestor_opens() => Run(() =>
	{
		var (root, inner) = Tree();
		var menu = new ContextMenu();
		root.ContextMenu = menu;
		RightRelease(inner);
		Assert.True(menu.IsOpen);
	});

	[Fact]
	public void Closing_the_menu_raises_ContextMenuClosing_on_its_element() => Run(() =>
	{
		var (_, inner) = Tree();
		var menu = new ContextMenu();
		inner.ContextMenu = menu;
		var log = new List<string>();
		inner.ContextMenuClosing += (_, e) => log.Add("closing:" + (e.CursorLeft == -1));
		menu.Closed += (_, _) => log.Add("closed");
		RightRelease(inner);
		_ = menu.Widget;
		GLib.Signal.Emit(menu.GtkPopover!, "closed");
		Assert.Equal(new[] { "closed", "closing:True" }, log);
	});

	[Fact]
	public void OnContextMenuOpening_override_is_called_before_instance_handlers() => Run(() =>
	{
		var b = new Probe();
		var log = new List<string>();
		b.Log = log;
		b.ContextMenuOpening += (_, _) => log.Add("handler");
		RightRelease(b);
		Assert.Equal(new[] { "override", "handler" }, log);
	});

	sealed class Probe : Border
	{
		public List<string>? Log;
		protected override void OnContextMenuOpening(ContextMenuEventArgs e) => Log!.Add("override");
	}
}
