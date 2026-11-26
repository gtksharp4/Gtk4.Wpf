using System;
using System.Windows;
using System.Windows.Navigation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests for <see cref="NavigationWindow"/> — journal pass-throughs and
/// content wiring. No GTK required.
/// </summary>
public class NavigationWindowTests
{
	sealed class TestPage : System.Windows.Controls.Page
	{
		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	[Fact]
	public void Navigate_delegates_to_NavigationService()
	{
		var win = new NavigationWindow();
		win.Navigate("page1");
		Assert.Equal("page1", win.NavigationService.Content);
	}

	[Fact]
	public void CanGoBack_matches_service()
	{
		var win = new NavigationWindow();
		Assert.False(win.CanGoBack);
		win.Navigate("p1");
		win.Navigate("p2");
		Assert.True(win.CanGoBack);
	}

	[Fact]
	public void CanGoForward_matches_service()
	{
		var win = new NavigationWindow();
		win.Navigate("p1");
		win.Navigate("p2");
		win.GoBack();
		Assert.True(win.CanGoForward);
	}

	[Fact]
	public void GoBack_navigates_service_back()
	{
		var win = new NavigationWindow();
		win.Navigate("p1");
		win.Navigate("p2");
		win.GoBack();
		Assert.Equal("p1", win.NavigationService.Content);
	}

	[Fact]
	public void GoForward_navigates_service_forward()
	{
		var win = new NavigationWindow();
		win.Navigate("p1");
		win.Navigate("p2");
		win.GoBack();
		win.GoForward();
		Assert.Equal("p2", win.NavigationService.Content);
	}

	[Fact]
	public void Navigate_page_with_WindowTitle_sets_window_Title()
	{
		var win = new NavigationWindow();
		var page = new TestPage { WindowTitle = "My Title" };
		win.Navigate(page);
		Assert.Equal("My Title", win.Title);
	}

	[Fact]
	public void Navigate_page_with_null_WindowTitle_falls_back_to_Title()
	{
		var win = new NavigationWindow();
		var page = new TestPage { WindowTitle = null!, Title = "T" };
		win.Navigate(page);
		Assert.Equal("T", win.Title);
	}

	[Fact]
	public void Navigate_page_without_WindowTitle_does_not_clear_Title()
	{
		var win = new NavigationWindow();
		win.Title = "Original";
		var page = new TestPage(); // WindowTitle is empty
		win.Navigate(page);
		// Empty WindowTitle — title unchanged.
		Assert.Equal("Original", win.Title);
	}
}