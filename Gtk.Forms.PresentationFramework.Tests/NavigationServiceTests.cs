using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Navigation;
using Xunit;

namespace Gtk.Forms.PresentationFramework.Tests
{
	/// <summary>
	/// Pure tests for <see cref="NavigationService"/>. No GTK required.
	/// </summary>
	public class NavigationServiceTests
	{
		// Minimal Page stub — avoids GTK widget creation.
		sealed class TestPage : System.Windows.Controls.Page
		{
			protected override Gtk.Widget CreateWidget() =>
				throw new NotSupportedException("pure test");
		}

		[Fact]
		public void Navigate_object_sets_Content()
		{
			var svc = new NavigationService();
			var page = new TestPage();
			svc.Navigate(page);
			Assert.Same(page, svc.Content);
		}

		[Fact]
		public void Navigate_pushes_journal_entry()
		{
			var svc = new NavigationService();
			svc.Navigate("page1");
			svc.Navigate("page2");
			Assert.True(svc.CanGoBack);
			Assert.False(svc.CanGoForward);
		}

		[Fact]
		public void GoBack_moves_to_previous_content()
		{
			var svc = new NavigationService();
			svc.Navigate("page1");
			svc.Navigate("page2");
			svc.GoBack();
			Assert.Equal("page1", svc.Content);
		}

		[Fact]
		public void GoForward_moves_to_next_content()
		{
			var svc = new NavigationService();
			svc.Navigate("page1");
			svc.Navigate("page2");
			svc.GoBack();
			svc.GoForward();
			Assert.Equal("page2", svc.Content);
		}

		[Fact]
		public void CanGoBack_false_before_any_navigation()
		{
			var svc = new NavigationService();
			Assert.False(svc.CanGoBack);
		}

		[Fact]
		public void CanGoForward_false_on_first_navigate()
		{
			var svc = new NavigationService();
			svc.Navigate("page1");
			Assert.False(svc.CanGoForward);
		}

		[Fact]
		public void Navigating_event_fires_before_navigation()
		{
			var svc = new NavigationService();
			object? pendingContent = null;
			svc.Navigating += (_, e) => pendingContent = e.Content;
			svc.Navigate("hello");
			Assert.Equal("hello", pendingContent);
		}

		[Fact]
		public void Navigating_cancel_aborts_navigation()
		{
			var svc = new NavigationService();
			svc.Navigating += (_, e) => e.Cancel = true;
			var result = svc.Navigate("page");
			Assert.False(result);
			Assert.Null(svc.Content);
		}

		[Fact]
		public void NavigationStopped_fires_when_cancelled()
		{
			var svc = new NavigationService();
			svc.Navigating += (_, e) => e.Cancel = true;
			bool stopped = false;
			svc.NavigationStopped += (_, _) => stopped = true;
			svc.Navigate("page");
			Assert.True(stopped);
		}

		[Fact]
		public void Navigated_event_fires_after_navigation()
		{
			var svc = new NavigationService();
			object? navigatedTo = null;
			svc.Navigated += (_, e) => navigatedTo = e.Content;
			svc.Navigate("hello");
			Assert.Equal("hello", navigatedTo);
		}

		[Fact]
		public void Navigate_page_wires_NavigationService()
		{
			var svc = new NavigationService();
			var page = new TestPage();
			svc.Navigate(page);
			Assert.Same(svc, page.NavigationService);
		}

		[Fact]
		public void Navigate_uri_resolves_via_resolver()
		{
			var svc = new NavigationService();
			var uri = new Uri("/Home", UriKind.Relative);
			var page = new TestPage();
			svc.RegisterContent(uri, page);
			svc.Navigate(uri);
			Assert.Same(page, svc.Content);
		}

		[Fact]
		public void Navigate_unregistered_uri_sets_null_content()
		{
			var svc = new NavigationService();
			var uri = new Uri("/Missing", UriKind.Relative);
			svc.Navigate(uri);
			Assert.Null(svc.Content);
		}
	}
}
