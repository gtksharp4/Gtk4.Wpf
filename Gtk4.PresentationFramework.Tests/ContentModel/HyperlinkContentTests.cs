using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Navigation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.ContentModel;

/// <summary>Inline Hyperlink content: activation from GTK, RequestNavigate and TargetName navigation.</summary>
public class HyperlinkContentTests : ContentModelTestBase
{
	static Hyperlink Link(string text, string uri, string? target = null)
	{
		var link = new Hyperlink { NavigateUri = new Uri(uri, UriKind.RelativeOrAbsolute) };
		if (target != null) link.TargetName = target;
		link.Inlines.Add(new Run(text));
		return link;
	}

	[Fact]
	public void Activating_the_GTK_link_raises_Click_and_RequestNavigate() => Run(() =>
	{
		var link = Link("Go", "http://example.org/a");
		var item = new ListBoxItem { Content = link };
		_ = item.Widget;
		int clicks = 0;
		string? navigated = null;
		link.Click += (_, _) => clicks++;
		item.AddHandler(Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler((s, e) =>
		{
			navigated = e.Uri.ToString();
			e.Handled = true;
		}));

		var label = Descendants(item.Widget).OfType<Gtk.Label>().Single();
		GLib.Signal.Emit(label, "activate-link", "http://example.org/a");

		Assert.Equal(1, clicks);
		Assert.Equal("http://example.org/a", navigated);
	});

	[Fact]
	public void TargetName_navigation_reaches_the_named_Frame_with_a_backslash_relative_uri() => Run(() =>
	{
		GtkFormsXamlRegistry.Register("samps/animate_samp.xaml", () => new TextBlock { Text = "SAMPLE PAGE" });
		var link = Link("Animate", "samps\\animate_samp.xaml", "frame2");
		var toc = new ListBoxItem { Content = link };
		var frame1 = new Frame { Name = "frame1" };
		var frame2 = new Frame { Name = "frame2" };
		var panel = new StackPanel();
		panel.Children.Add(frame1);
		panel.Children.Add(frame2);
		var window = new Window { Content = panel };
		_ = panel.Widget;
		frame1.Navigate(toc);
		link.OnClick();

		var page = Assert.IsType<TextBlock>(frame2.Content);
		Assert.Equal("SAMPLE PAGE", page.Text);
		Assert.IsType<ListBoxItem>(frame1.Content);
	});

	[Fact]
	public void Without_a_TargetName_the_nearest_Frame_navigates() => Run(() =>
	{
		GtkFormsXamlRegistry.Register("pages/two.xaml", () => new TextBlock { Text = "TWO" });
		var link = Link("Two", "two.xaml");
		var host = new ListBoxItem { Content = link };
		var frame = new Frame { Name = "only", Source = null };
		var window = new Window { Content = frame };
		_ = frame.Widget;
		frame.Navigate(host);
		frame.NavigationService.RegisterContent(new Uri("pages/one.xaml", UriKind.Relative), host);
		frame.Navigate(new Uri("pages/one.xaml", UriKind.Relative));

		link.OnClick();

		Assert.Equal("TWO", Assert.IsType<TextBlock>(frame.Content).Text);
	});

	[Fact]
	public void A_handled_RequestNavigate_suppresses_the_default_navigation() => Run(() =>
	{
		var link = Link("Go", "pages/none.xaml");
		var frame = new Frame();
		var item = new ListBoxItem { Content = link };
		frame.Navigate(item);
		_ = item.Widget;
		item.AddHandler(Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler((s, e) => e.Handled = true));
		link.OnClick();
		Assert.Same(item, frame.Content);
	});
}
