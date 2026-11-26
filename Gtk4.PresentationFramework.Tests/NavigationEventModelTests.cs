using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>WPF navigation event model: NavigationEventArgs, NavigatedEventHandler and event order.</summary>
public class NavigationEventModelTests
{
	sealed class StubPage : Page
	{
		protected override Gtk.Widget CreateWidget() => throw new NotSupportedException("pure test");
	}

	[Fact]
	public void Navigated_handler_has_the_WPF_delegate_type()
	{
		var svc = new NavigationService();
		NavigatedEventHandler handler = (sender, e) => { };
		svc.Navigated += handler;
		svc.Navigated -= handler;
		Assert.Equal(typeof(void), typeof(NavigatedEventHandler).GetMethod("Invoke")!.ReturnType);
	}

	[Fact]
	public void Navigated_args_carry_content_uri_navigator_and_initiator()
	{
		var svc = new NavigationService();
		var page = new StubPage();
		var uri = new Uri("p.xaml", UriKind.Relative);
		svc.RegisterContent(uri, page);
		NavigationEventArgs? args = null;
		object? sender = null;
		svc.Navigated += (s, e) => { sender = s; args = e; };

		svc.Navigate(uri);

		Assert.Same(svc, sender);
		Assert.Same(page, args!.Content);
		Assert.Equal(uri, args.Uri);
		Assert.Same(svc, args.Navigator);
		Assert.True(args.IsNavigationInitiator);
		Assert.Null(args.WebResponse);
		Assert.Null(args.ExtraData);
	}

	[Fact]
	public void Navigate_with_extra_data_flows_to_Navigated()
	{
		var svc = new NavigationService();
		object? extra = null;
		svc.Navigated += (_, e) => extra = e.ExtraData;

		svc.Navigate(new StubPage(), "payload");

		Assert.Equal("payload", extra);
	}

	[Fact]
	public void Events_fire_in_WPF_order_Navigating_Navigated_LoadCompleted()
	{
		var svc = new NavigationService();
		var order = new List<string>();
		svc.Navigating += (_, _) => order.Add("Navigating");
		svc.Navigated += (_, _) => order.Add("Navigated");
		svc.LoadCompleted += (_, _) => order.Add("LoadCompleted");

		svc.Navigate(new StubPage());

		Assert.Equal(new[] { "Navigating", "Navigated", "LoadCompleted" }, order);
	}

	[Fact]
	public void Cancelled_navigation_raises_neither_Navigated_nor_LoadCompleted()
	{
		var svc = new NavigationService();
		bool fired = false;
		svc.Navigating += (_, e) => e.Cancel = true;
		svc.Navigated += (_, _) => fired = true;
		svc.LoadCompleted += (_, _) => fired = true;

		svc.Navigate(new StubPage());

		Assert.False(fired);
	}

	[Fact]
	public void GoBack_raises_Navigated_with_the_previous_content()
	{
		var svc = new NavigationService();
		var first = new StubPage();
		svc.Navigate(first);
		svc.Navigate(new StubPage());
		object? content = null;
		svc.Navigated += (_, e) => content = e.Content;

		svc.GoBack();

		Assert.Same(first, content);
	}

	[Fact]
	public void NavigationWindow_Navigated_uses_the_window_as_sender_and_navigator()
	{
		var window = new NavigationWindow();
		object? sender = null;
		NavigationEventArgs? args = null;
		window.Navigated += (s, e) => { sender = s; args = e; };

		window.Navigate(new StubPage());

		Assert.Same(window, sender);
		Assert.Same(window, args!.Navigator);
	}

	[Fact]
	public void Frame_Navigated_uses_the_frame_as_sender_and_navigator()
	{
		var frame = new Frame();
		object? sender = null;
		NavigationEventArgs? args = null;
		var order = new List<string>();
		frame.Navigated += (s, e) => { sender = s; args = e; order.Add("Navigated"); };
		frame.LoadCompleted += (_, _) => order.Add("LoadCompleted");

		frame.Navigate(new StubPage());

		Assert.Same(frame, sender);
		Assert.Same(frame, args!.Navigator);
		Assert.Equal(new[] { "Navigated", "LoadCompleted" }, order);
	}
}
