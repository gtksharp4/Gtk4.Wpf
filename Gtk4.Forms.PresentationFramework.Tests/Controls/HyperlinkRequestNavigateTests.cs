using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Controls;

public class HyperlinkRequestNavigateTests : WpfGtkTestBase
{
	[Fact]
	public void RequestNavigateEvent_is_a_bubbling_event_with_the_navigate_handler_type()
	{
		var e = Hyperlink.RequestNavigateEvent;
		Assert.Equal("RequestNavigate", e.Name);
		Assert.Equal(RoutingStrategy.Bubble, e.RoutingStrategy);
		Assert.Equal(typeof(RequestNavigateEventHandler), e.HandlerType);
		Assert.Equal(typeof(Hyperlink), e.OwnerType);
	}

	[Fact]
	public void Args_are_routed_args_for_the_event()
	{
		var uri = new Uri("http://example.com/");
		var args = new RequestNavigateEventArgs(uri, "frame");
		Assert.IsAssignableFrom<RoutedEventArgs>(args);
		Assert.Same(Hyperlink.RequestNavigateEvent, args.RoutedEvent);
		Assert.Same(uri, args.Uri);
		Assert.Equal("frame", args.Target);
		Assert.False(args.Handled);
		args.Handled = true;
		Assert.True(args.Handled);
	}

	[Fact]
	public void Raising_reaches_the_link_handler_and_the_ancestor_handler_in_order() => Run(() =>
	{
		var text = new TextBlock();
		var link = new Hyperlink { NavigateUri = new Uri("http://x/a"), TargetName = "t" };
		text.Inlines.Add(link);
		var seen = new List<string>();
		link.RequestNavigate += (s, e) => seen.Add("link:" + ReferenceEquals(s, link));
		text.AddHandler(Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler((s, e) =>
			seen.Add("text:" + ReferenceEquals(e.Source, link) + ":" + e.Target)));
		link.OnClick();
		Assert.Equal(new[] { "link:True", "text:True:t" }, seen);
	});

	[Fact]
	public void Handled_stops_the_bubble() => Run(() =>
	{
		var text = new TextBlock();
		var link = new Hyperlink { NavigateUri = new Uri("http://x/a") };
		text.Inlines.Add(link);
		int ancestor = 0;
		link.RequestNavigate += (s, e) => e.Handled = true;
		text.AddHandler(Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler((s, e) => ancestor++));
		link.OnClick();
		Assert.Equal(0, ancestor);
	});

	[Fact]
	public void RequestNavigate_remove_unsubscribes_and_no_uri_raises_nothing() => Run(() =>
	{
		var link = new Hyperlink();
		int count = 0;
		RequestNavigateEventHandler h = (s, e) => count++;
		link.RequestNavigate += h;
		link.OnClick();
		Assert.Equal(0, count);
		link.NavigateUri = new Uri("http://x/");
		link.OnClick();
		Assert.Equal(1, count);
		link.RequestNavigate -= h;
		link.OnClick();
		Assert.Equal(1, count);
	});
}
