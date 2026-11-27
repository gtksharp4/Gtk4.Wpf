using System.Windows;
using System.Windows.Controls;
using Xunit;
using WpfFrame = System.Windows.Controls.Frame;
using WpfPage = System.Windows.Controls.Page;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>An event bubbling out of a Frame's navigated page carries the Frame as Source (WPF Frame.AdjustEventSource).</summary>
public class FrameEventSourceTests : WpfGtkTestBase
{
	static readonly RoutedEvent Ping = EventManager.RegisterRoutedEvent(
		"FrameSrcPing", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FrameEventSourceTests));

	[Fact]
	public void Source_seen_outside_the_frame_is_the_frame_and_OriginalSource_the_inner_element() => Run(() =>
	{
		var inner = new Button();
		var frame = new WpfFrame();
		var host = new Button { Content = frame };
		frame.Navigate(new WpfPage { Content = inner });

		object? outerSource = null, outerOriginal = null, innerSource = null;
		inner.AddHandler(Ping, new RoutedEventHandler((s, e) => innerSource = e.Source));
		host.AddHandler(Ping, new RoutedEventHandler((s, e) => { outerSource = e.Source; outerOriginal = e.OriginalSource; }));

		inner.RaiseEvent(new RoutedEventArgs(Ping));

		Assert.Same(inner, innerSource);
		Assert.Same(frame, outerSource);
		Assert.Same(inner, outerOriginal);
	});
}
