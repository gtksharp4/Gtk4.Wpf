using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>NavigationService and Frame: Refresh and StopLoading.</summary>
public class Round51aNavigationRefreshTests : WpfGtkTestBase
{
	sealed class StubPage : Page
	{
		protected override Gtk.Widget CreateWidget() => throw new NotSupportedException("pure test");
	}

	static List<string> Record(NavigationService svc)
	{
		var order = new List<string>();
		svc.Navigating += (_, e) => order.Add("Navigating:" + e.NavigationMode);
		svc.Navigated += (_, _) => order.Add("Navigated");
		svc.LoadCompleted += (_, _) => order.Add("LoadCompleted");
		svc.NavigationStopped += (_, _) => order.Add("NavigationStopped");
		return order;
	}

	[Fact]
	public void Refresh_raises_Navigating_Refresh_then_Navigated_then_LoadCompleted()
	{
		var svc = new NavigationService();
		svc.Navigate(new StubPage());
		var order = Record(svc);

		svc.Refresh();

		Assert.Equal(new[] { "Navigating:Refresh", "Navigated", "LoadCompleted" }, order);
	}

	[Fact]
	public void Refresh_keeps_the_object_content_and_adds_no_journal_entry()
	{
		var svc = new NavigationService();
		var first = new StubPage();
		svc.Navigate(first);

		svc.Refresh();

		Assert.Same(first, svc.Content);
		Assert.False(svc.CanGoBack);
		Assert.False(svc.CanGoForward);
	}

	[Fact]
	public void Refresh_reports_the_content_in_the_event_arguments()
	{
		var svc = new NavigationService();
		var page = new StubPage();
		svc.Navigate(page);
		NavigatingCancelEventArgs? navigating = null;
		NavigationEventArgs? navigated = null;
		svc.Navigating += (_, e) => navigating = e;
		svc.Navigated += (_, e) => navigated = e;

		svc.Refresh();

		Assert.Same(page, navigating!.Content);
		Assert.Same(page, navigated!.Content);
	}

	[Fact]
	public void Refresh_before_any_navigation_does_nothing()
	{
		var svc = new NavigationService();
		var order = Record(svc);

		svc.Refresh();

		Assert.Empty(order);
	}

	[Fact]
	public void Refresh_of_a_uri_navigation_reloads_a_new_instance_and_keeps_the_uri()
	{
		GtkFormsXamlRegistry.Register("Round51a/RefreshPage.xaml", () => new StubPage());
		var svc = new NavigationService();
		var uri = new Uri("Round51a/RefreshPage.xaml", UriKind.Relative);
		svc.Navigate(uri);
		var before = svc.Content;
		NavigationEventArgs? navigated = null;
		svc.Navigated += (_, e) => navigated = e;

		svc.Refresh();

		Assert.NotSame(before, svc.Content);
		Assert.IsType<StubPage>(svc.Content);
		Assert.Equal(uri, navigated!.Uri);
		Assert.Equal(uri, svc.CurrentSource);
		Assert.False(svc.CanGoBack);
	}

	[Fact]
	public void Refresh_cancelled_in_Navigating_raises_NavigationStopped_and_keeps_the_content()
	{
		var svc = new NavigationService();
		var page = new StubPage();
		svc.Navigate(page);
		var order = Record(svc);
		svc.Navigating += (_, e) => e.Cancel = true;

		svc.Refresh();

		Assert.Equal(new[] { "Navigating:Refresh", "NavigationStopped" }, order);
		Assert.Same(page, svc.Content);
	}

	[Fact]
	public void CurrentSource_follows_the_journal_on_GoBack()
	{
		GtkFormsXamlRegistry.Register("Round51a/SourceA.xaml", () => new StubPage());
		var svc = new NavigationService();
		var uriA = new Uri("Round51a/SourceA.xaml", UriKind.Relative);
		svc.Navigate(uriA);
		svc.Navigate(new StubPage());
		Assert.Null(svc.CurrentSource);

		svc.GoBack();

		Assert.Equal(uriA, svc.CurrentSource);
	}

	[Fact]
	public void StopLoading_raises_NavigationStopped_with_the_current_content()
	{
		var svc = new NavigationService();
		var page = new StubPage();
		svc.Navigate(page);
		NavigationEventArgs? stopped = null;
		svc.NavigationStopped += (_, e) => stopped = e;

		svc.StopLoading();

		Assert.Same(page, stopped!.Content);
	}

	[Fact]
	public void StopLoading_inside_Navigating_cancels_that_navigation_and_raises_NavigationStopped_once()
	{
		var svc = new NavigationService();
		var first = new StubPage();
		svc.Navigate(first);
		var order = Record(svc);
		svc.Navigating += (_, _) => svc.StopLoading();

		bool proceeded = svc.Navigate(new StubPage());

		Assert.False(proceeded);
		Assert.Same(first, svc.Content);
		Assert.Equal(new[] { "Navigating:New", "NavigationStopped" }, order);
	}

	[Fact]
	public void Frame_Refresh_reloads_the_Source_and_raises_Navigating_with_the_frame_as_sender() => Run(() =>
	{
		GtkFormsXamlRegistry.Register("Round51a/FramePage.xaml", () => new StubPage());
		var frame = new Frame { Source = new Uri("Round51a/FramePage.xaml", UriKind.Relative) };
		var before = frame.Content;
		var order = new List<string>();
		object? sender = null;
		frame.Navigating += (s, e) => { sender = s; order.Add("Navigating:" + e.NavigationMode); };
		frame.Navigated += (_, _) => order.Add("Navigated");
		frame.LoadCompleted += (_, _) => order.Add("LoadCompleted");

		frame.Refresh();

		Assert.Same(frame, sender);
		Assert.Equal(new[] { "Navigating:Refresh", "Navigated", "LoadCompleted" }, order);
		Assert.NotNull(frame.Content);
		Assert.NotSame(before, frame.Content);
		Assert.Equal(new Uri("Round51a/FramePage.xaml", UriKind.Relative), frame.Source);
	});

	[Fact]
	public void Frame_StopLoading_raises_NavigationStopped_with_the_frame_as_sender() => Run(() =>
	{
		var frame = new Frame();
		frame.Navigate(new StubPage());
		object? sender = null;
		frame.NavigationStopped += (s, _) => sender = s;

		frame.StopLoading();

		Assert.Same(frame, sender);
	});
}
