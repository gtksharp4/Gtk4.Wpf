using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Navigation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Navigation, event-ordering, InvokeScript and Refresh tests for <see cref="WebBrowser"/>.
/// No GTK, no Photino.  <see cref="FakeWebBrowserHost"/> is injected.
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

	// ── InvokeScript ─────────────────────────────────────────────────────────

	[Fact]
	public void InvokeScript_routes_to_host_ExecuteScript()
	{
		_fake.ExecuteScriptResult = "42";
		var result = _wb.InvokeScript("getCount");
		Assert.Equal("42", result);
	}

	/// <summary>B6: string args must be JSON-quoted.</summary>
	[Fact]
	public void InvokeScript_with_string_args_json_encodes_them()
	{
		_wb.InvokeScript("setVal", "a", "b");
		Assert.Equal("setVal(\"a\",\"b\")", _fake.LastExecutedScript);
	}

	[Fact]
	public void InvokeScript_with_numeric_args_passes_literals()
	{
		_wb.InvokeScript("setVal", 1, 2);
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
		_wb.Refresh();
		Assert.False(_fake.RefreshCalled);
	}
}