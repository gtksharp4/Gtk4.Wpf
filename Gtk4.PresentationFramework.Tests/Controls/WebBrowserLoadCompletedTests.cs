using System;
using System.Windows.Controls;
using System.Windows.Navigation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// B1 (single-fire LoadCompleted) and B3 Refresh event tests for <see cref="WebBrowser"/>.
/// </summary>
public class WebBrowserLoadCompletedTests : IDisposable
{
	readonly Func<IWebBrowserHost> _originalFactory;
	readonly FakeWebBrowserHost _fake;
	readonly WebBrowser _wb;

	public WebBrowserLoadCompletedTests()
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

	// ── B1: LoadCompleted fires exactly once ──────────────────────────────────

	/// <summary>Navigate + host SimulateLoadCompleted yields exactly one LoadCompleted.</summary>
	[Fact]
	public void Navigate_plus_host_LoadCompleted_fires_exactly_once()
	{
		int count = 0;
		_wb.LoadCompleted += (_, _) => count++;
		var uri = new Uri("https://b1.com/");
		_wb.Navigate(uri);
		_fake.SimulateLoadCompleted(uri);
		Assert.Equal(1, count);
	}

	/// <summary>Navigate without host confirmation still fires LoadCompleted exactly once.</summary>
	[Fact]
	public void Navigate_without_host_confirmation_fires_exactly_once()
	{
		int count = 0;
		_wb.LoadCompleted += (_, _) => count++;
		_wb.Navigate(new Uri("https://b1b.com/"));
		Assert.Equal(1, count);
	}

	/// <summary>GoBack + host confirmation fires LoadCompleted exactly once.</summary>
	[Fact]
	public void GoBack_plus_host_LoadCompleted_fires_exactly_once()
	{
		_wb.Navigate(new Uri("https://a.com/"));
		_wb.Navigate(new Uri("https://b.com/"));
		int count = 0;
		_wb.LoadCompleted += (_, _) => count++;
		_wb.GoBack();
		_fake.SimulateLoadCompleted(new Uri("https://a.com/"));
		Assert.Equal(1, count);
	}

	// ── B3: Refresh raises Navigating / honours Cancel ────────────────────────

	[Fact]
	public void Refresh_raises_Navigating_with_mode_Refresh()
	{
		_wb.Navigate(new Uri("https://a.com/"));
		NavigationMode? capturedMode = null;
		_wb.Navigating += (_, e) => capturedMode = e.NavigationMode;
		_wb.Refresh();
		Assert.Equal(NavigationMode.Refresh, capturedMode);
	}

	[Fact]
	public void Refresh_Navigating_Cancel_leaves_Source_unchanged()
	{
		_wb.Navigate(new Uri("https://a.com/"));
		_wb.Navigating += (_, e) => e.Cancel = true;
		_wb.Refresh();
		Assert.False(_fake.RefreshCalled);
	}
}