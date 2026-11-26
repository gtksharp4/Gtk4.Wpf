using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Xunit;
using WpfFrame    = System.Windows.Controls.Frame;
using WpfPage     = System.Windows.Controls.Page;
using WpfTextBlock = System.Windows.Controls.TextBlock;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// GTK smoke tests for <see cref="WpfFrame"/> and <see cref="WpfPage"/> widget
/// plumbing. Runs on the GTK thread; skipped when GTK is unavailable.
/// </summary>
public class FrameGtkSmokeTests : WpfGtkTestBase
{
	[Fact]
	public void Frame_widget_is_GtkBox() => Run(() =>
	{
		var frame = new WpfFrame();
		Assert.IsType<Gtk.Box>(frame.Widget);
	});

	[Fact]
	public void Frame_Navigate_object_swaps_child_widget() => Run(() =>
	{
		var frame = new WpfFrame();
		var container = (Gtk.Box)frame.Widget;

		var tb = new WpfTextBlock { Text = "hello" };
		frame.Navigate(tb);

		Assert.Same(tb.Widget, GetFirstChild(container));
	});

	[Fact]
	public void Frame_Navigate_second_time_removes_first_widget() => Run(() =>
	{
		var frame = new WpfFrame();
		var container = (Gtk.Box)frame.Widget;

		var tb1 = new WpfTextBlock { Text = "first" };
		var tb2 = new WpfTextBlock { Text = "second" };

		frame.Navigate(tb1);
		frame.Navigate(tb2);

		Assert.Same(tb2.Widget, GetFirstChild(container));
	});

	[Fact]
	public void Frame_CanGoBack_true_after_two_navigations() => Run(() =>
	{
		var frame = new WpfFrame();
		frame.Navigate("page1");
		frame.Navigate("page2");
		Assert.True(frame.CanGoBack);
	});

	[Fact]
	public void Frame_GoBack_restores_previous_content() => Run(() =>
	{
		var frame = new WpfFrame();
		var tb1 = new WpfTextBlock { Text = "a" };
		var tb2 = new WpfTextBlock { Text = "b" };
		frame.Navigate(tb1);
		frame.Navigate(tb2);
		frame.GoBack();
		Assert.Same(tb1, frame.Content);
	});

	[Fact]
	public void Page_widget_is_GtkBox() => Run(() =>
	{
		var page = new WpfPage();
		Assert.IsType<Gtk.Box>(page.Widget);
	});

	[Fact]
	public void Page_Content_UIElement_added_to_GtkBox() => Run(() =>
	{
		var page = new WpfPage();
		var tb = new WpfTextBlock { Text = "in page" };
		page.Content = tb;

		var box = (Gtk.Box)page.Widget;
		Assert.Same(tb.Widget, GetFirstChild(box));
	});

	// ── Helpers ───────────────────────────────────────────────────────────────

	static Gtk.Widget? GetFirstChild(Gtk.Box box) => box.FirstChild;
}