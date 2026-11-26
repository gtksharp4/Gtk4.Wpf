using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.ContentModel;

/// <summary>Unresolvable and external links fail like WPF, and a real GTK click reaches the Hyperlink.</summary>
public class HyperlinkNavigationTests : ContentModelTestBase
{
	static Hyperlink Link(string uri)
	{
		var link = new Hyperlink { NavigateUri = new Uri(uri, UriKind.RelativeOrAbsolute) };
		link.Inlines.Add(new Run("Go"));
		return link;
	}

	static (Frame Frame, ListBoxItem Page) PageInFrame(Hyperlink link)
	{
		var page = new ListBoxItem { Content = link };
		var frame = new Frame();
		_ = page.Widget;
		frame.Navigate(page);
		return (frame, page);
	}

	[Fact]
	public void An_external_link_raises_NavigationFailed_and_leaves_the_frame_unchanged() => Run(() =>
	{
		var link = Link("http://example.org/a");
		var (frame, page) = PageInFrame(link);
		NavigationFailedEventArgs? failed = null;
		frame.NavigationFailed += (s, e) =>
		{
			failed = e;
			e.Handled = true;
		};
		link.OnClick();
		Assert.Same(page, frame.Content);
		Assert.IsType<NotSupportedException>(failed!.Exception);
		Assert.Equal("http://example.org/a", failed.Uri!.ToString());
	});

	[Fact]
	public void An_unhandled_external_link_neither_throws_nor_blanks_the_frame() => Run(() =>
	{
		var link = Link("http://example.org/a");
		var (frame, page) = PageInFrame(link);
		NavigationFailedEventArgs? failed = null;
		frame.NavigationFailed += (s, e) => failed = e;
		link.OnClick();
		Assert.NotNull(failed);
		Assert.Same(page, frame.Content);
	});

	[Fact]
	public void An_unhandled_missing_page_throws_like_WPF_and_does_not_blank_the_frame() => Run(() =>
	{
		var link = Link("missing/page.xaml");
		var (frame, page) = PageInFrame(link);
		Assert.Throws<FileNotFoundException>(() => link.OnClick());
		Assert.Same(page, frame.Content);
	});

	[Fact]
	public void A_missing_page_fails_with_a_FileNotFoundException() => Run(() =>
	{
		var link = Link("missing/page.xaml");
		var (frame, page) = PageInFrame(link);
		Exception? error = null;
		frame.NavigationFailed += (s, e) =>
		{
			error = e.Exception;
			e.Handled = true;
		};
		link.OnClick();
		Assert.IsType<FileNotFoundException>(error);
		Assert.Same(page, frame.Content);
	});

	[Fact]
	public void An_external_link_in_a_NavigationWindow_fails_without_navigating() => Run(() =>
	{
		var link = Link("https://example.org/");
		var page = new ListBoxItem { Content = link };
		var window = new NavigationWindow();
		_ = page.Widget;
		window.NavigationService.Navigate(page);
		bool failed = false;
		window.NavigationFailed += (s, e) =>
		{
			failed = true;
			e.Handled = true;
		};
		link.OnClick();
		Assert.True(failed);
		Assert.Same(page, window.NavigationService.Content);
	});

	[Fact]
	public void A_real_GTK_click_on_the_link_text_raises_Click() => Run(() =>
	{
		var link = Link("http://example.org/a");
		var item = new ListBoxItem { Content = link };
		var list = new ListBox();
		list.Items.Add(item);
		var window = new Window { Content = list, Width = 300, Height = 200 };
		window.Show();
		GtkPump.Until(() => item.Widget.IsMapped && item.Widget.Width > 0);
		GtkPump.For(200);
		int clicks = 0;
		link.Click += (_, _) => clicks++;
		link.AddHandler(Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler((s, e) => e.Handled = true));

		var label = Descendants(item.Widget).OfType<Gtk.Label>().Single();
		var controllers = label.ObserveControllers();
		Gtk.GestureClick? click = null;
		for (uint i = 0; i < controllers.NItems; i++)
		{
			var controller = controllers.GetObject(i);
			if (controller is Gtk.GestureClick { PropagationPhase: Gtk.PropagationPhase.Capture } gc) click = gc;
			if (controller is Gtk.EventControllerMotion)
				GLib.Signal.Emit(controller, "motion", 4.0, label.Height / 2.0);
		}
		Assert.NotNull(click);
		Assert.Equal("http://example.org/a", label.CurrentUri);
		GLib.Signal.Emit(click!, "pressed", 1, 4.0, label.Height / 2.0);
		GLib.Signal.Emit(click!, "released", 1, 4.0, label.Height / 2.0);

		Assert.Equal(1, clicks);
		window.Close();
	});

	[Fact]
	public void A_real_click_on_an_external_link_in_a_Frame_does_not_throw_and_keeps_the_page() => Run(() =>
	{
		var link = Link("http://example.org/a");
		var item = new ListBoxItem { Content = link };
		var list = new ListBox();
		list.Items.Add(item);
		var frame = new Frame();
		var window = new Window { Content = frame, Width = 300, Height = 200 };
		window.Show();
		frame.Navigate(list);
		GtkPump.Until(() => item.Widget.IsMapped && item.Widget.Width > 0);
		GtkPump.For(200);
		NavigationFailedEventArgs? failed = null;
		frame.NavigationFailed += (s, e) => failed = e;

		var label = Descendants(item.Widget).OfType<Gtk.Label>().Single();
		var controllers = label.ObserveControllers();
		Gtk.GestureClick? click = null;
		for (uint i = 0; i < controllers.NItems; i++)
		{
			var controller = controllers.GetObject(i);
			if (controller is Gtk.GestureClick { PropagationPhase: Gtk.PropagationPhase.Capture } gc) click = gc;
			if (controller is Gtk.EventControllerMotion)
				GLib.Signal.Emit(controller, "motion", 4.0, label.Height / 2.0);
		}
		Assert.NotNull(click);
		GLib.Signal.Emit(click!, "pressed", 1, 4.0, label.Height / 2.0);
		GLib.Signal.Emit(click!, "released", 1, 4.0, label.Height / 2.0);

		Assert.NotNull(failed);
		Assert.Same(list, frame.Content);
		window.Close();
	});
}
