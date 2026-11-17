using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Navigation;
using Xunit;

namespace Gtk.Forms.PresentationFramework.Tests.Controls
{
	/// <summary>
	/// Pure model tests for <see cref="WebBrowser"/>. No GTK, no Photino, no display needed.
	/// A <see cref="FakeWebBrowserHost"/> is injected so all tests run in process.
	/// </summary>
	public class WebBrowserTests : IDisposable
	{
		readonly Func<IWebBrowserHost> _originalFactory;
		readonly FakeWebBrowserHost _fake;
		readonly WebBrowser _wb;

		public WebBrowserTests()
		{
			_originalFactory = WebBrowser.HostFactory;
			_fake = new FakeWebBrowserHost();
			WebBrowser.HostFactory = () => _fake;
			_wb = new WebBrowser();
		}

		public void Dispose()
		{
			WebBrowser.HostFactory = _originalFactory;
			_wb.Dispose();
		}

		// ── Navigate(uri) ─────────────────────────────────────────────────────────

		[Fact]
		public void Navigate_sets_Source()
		{
			var uri = new Uri("https://example.com/");
			_wb.Navigate(uri);
			Assert.Equal(uri, _wb.Source);
		}

		[Fact]
		public void Navigate_fires_Navigating_once()
		{
			int count = 0;
			_wb.Navigating += (_, _) => count++;
			_wb.Navigate(new Uri("https://a.com/"));
			Assert.Equal(1, count);
		}

		[Fact]
		public void Navigate_fires_Navigated_once()
		{
			int count = 0;
			_wb.Navigated += (_, _) => count++;
			_wb.Navigate(new Uri("https://a.com/"));
			Assert.Equal(1, count);
		}

		[Fact]
		public void Navigate_fires_LoadCompleted_once()
		{
			int count = 0;
			_wb.LoadCompleted += (_, _) => count++;
			_wb.Navigate(new Uri("https://a.com/"));
			Assert.Equal(1, count);
		}

		[Fact]
		public void Navigate_fires_events_in_order_Navigating_Navigated_LoadCompleted()
		{
			var order = new List<string>();
			_wb.Navigating    += (_, _) => order.Add("Navigating");
			_wb.Navigated     += (_, _) => order.Add("Navigated");
			_wb.LoadCompleted += (_, _) => order.Add("LoadCompleted");

			_wb.Navigate(new Uri("https://a.com/"));

			Assert.Equal(new[] { "Navigating", "Navigated", "LoadCompleted" }, order);
		}

		[Fact]
		public void Navigate_routes_to_host()
		{
			var uri = new Uri("https://example.com/");
			_wb.Navigate(uri);
			Assert.Equal(uri, _fake.LastNavigatedUri);
		}

		// ── Navigating.Cancel ─────────────────────────────────────────────────────

		[Fact]
		public void Navigating_Cancel_true_aborts_navigation()
		{
			_wb.Navigating += (_, e) => e.Cancel = true;
			var uri = new Uri("https://example.com/");
			_wb.Navigate(uri);

			// Source unchanged, host not called.
			Assert.Null(_wb.Source);
			Assert.Null(_fake.LastNavigatedUri);
		}

		[Fact]
		public void Navigating_Cancel_true_does_not_fire_Navigated()
		{
			bool fired = false;
			_wb.Navigating += (_, e) => e.Cancel = true;
			_wb.Navigated  += (_, _) => fired = true;
			_wb.Navigate(new Uri("https://example.com/"));
			Assert.False(fired);
		}

		// ── NavigateToString ──────────────────────────────────────────────────────

		[Fact]
		public void NavigateToString_routes_to_host()
		{
			_wb.NavigateToString("<html/>");
			Assert.Equal("<html/>", _fake.LastHtml);
		}

		[Fact]
		public void NavigateToString_fires_LoadCompleted()
		{
			bool fired = false;
			_wb.LoadCompleted += (_, _) => fired = true;
			_wb.NavigateToString("<html/>");
			Assert.True(fired);
		}

		// ── Back/forward history ──────────────────────────────────────────────────

		[Fact]
		public void CanGoBack_false_before_navigation()
		{
			Assert.False(_wb.CanGoBack);
		}

		[Fact]
		public void CanGoBack_false_after_one_navigation()
		{
			_wb.Navigate(new Uri("https://a.com/"));
			Assert.False(_wb.CanGoBack);
		}

		[Fact]
		public void CanGoBack_true_after_two_navigations()
		{
			_wb.Navigate(new Uri("https://a.com/"));
			_wb.Navigate(new Uri("https://b.com/"));
			Assert.True(_wb.CanGoBack);
		}

		[Fact]
		public void GoBack_moves_cursor_to_previous()
		{
			var a = new Uri("https://a.com/");
			var b = new Uri("https://b.com/");
			var c = new Uri("https://c.com/");
			_wb.Navigate(a);
			_wb.Navigate(b);
			_wb.Navigate(c);

			_wb.GoBack();

			Assert.Equal(b, _wb.Source);
		}

		[Fact]
		public void GoBack_sets_CanGoForward_true()
		{
			_wb.Navigate(new Uri("https://a.com/"));
			_wb.Navigate(new Uri("https://b.com/"));
			_wb.GoBack();
			Assert.True(_wb.CanGoForward);
		}

		[Fact]
		public void Navigate_after_GoBack_trims_forward_history()
		{
			_wb.Navigate(new Uri("https://a.com/"));
			_wb.Navigate(new Uri("https://b.com/"));
			_wb.Navigate(new Uri("https://c.com/"));
			_wb.GoBack(); // current=b
			_wb.Navigate(new Uri("https://d.com/")); // trims c
			Assert.False(_wb.CanGoForward);
		}

		[Fact]
		public void GoBack_then_GoForward_returns_to_original()
		{
			var a = new Uri("https://a.com/");
			var b = new Uri("https://b.com/");
			_wb.Navigate(a);
			_wb.Navigate(b);
			_wb.GoBack();
			_wb.GoForward();
			Assert.Equal(b, _wb.Source);
		}

		// ── InvokeScript ─────────────────────────────────────────────────────────

		[Fact]
		public void InvokeScript_routes_to_host_ExecuteScript()
		{
			_fake.ExecuteScriptResult = "42";
			var result = _wb.InvokeScript("getCount");
			Assert.Equal("42", result);
		}

		[Fact]
		public void InvokeScript_with_args_builds_call_expression()
		{
			_wb.InvokeScript("setVal", "1", "2");
			Assert.Equal("setVal(1,2)", _fake.LastExecutedScript);
		}

		// ── Refresh ───────────────────────────────────────────────────────────────

		[Fact]
		public void Refresh_calls_host_Refresh()
		{
			_wb.Navigate(new Uri("https://a.com/"));
			_wb.Refresh();
			Assert.True(_fake.RefreshCalled);
		}

		[Fact]
		public void Refresh_does_nothing_before_any_navigation()
		{
			_wb.Refresh(); // must not throw
			Assert.False(_fake.RefreshCalled);
		}
	}

	// ── Fake host ─────────────────────────────────────────────────────────────────

	/// <summary>
	/// In-process stand-in for <see cref="IWebBrowserHost"/>. No native window, no Photino.
	/// </summary>
	sealed class FakeWebBrowserHost : IWebBrowserHost
	{
		public event Action<Uri?>? LoadCompleted;

		public Uri? LastNavigatedUri { get; private set; }
		public string? LastHtml { get; private set; }
		public string? LastExecutedScript { get; private set; }
		public string? ExecuteScriptResult { get; set; }
		public bool GoBackCalled { get; private set; }
		public bool GoForwardCalled { get; private set; }
		public bool RefreshCalled { get; private set; }
		public bool IsDisposed { get; private set; }

		public void Navigate(Uri source) => LastNavigatedUri = source;
		public void NavigateToString(string html) => LastHtml = html;
		public string? ExecuteScript(string script)
		{
			LastExecutedScript = script;
			return ExecuteScriptResult;
		}
		public void GoBack() => GoBackCalled = true;
		public void GoForward() => GoForwardCalled = true;
		public void Refresh() => RefreshCalled = true;

		/// <summary>Triggers the load-completed callback (simulates page finish).</summary>
		public void SimulateLoadCompleted(Uri? uri) => LoadCompleted?.Invoke(uri);

		public void Dispose() => IsDisposed = true;
	}
}
