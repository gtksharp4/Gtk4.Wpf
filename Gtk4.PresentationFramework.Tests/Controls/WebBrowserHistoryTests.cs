using System;
using System.Windows.Controls;
using System.Windows.Navigation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// History (back/forward journal) and GoBack/GoForward/Refresh event tests for
/// <see cref="WebBrowser"/>. No GTK, no Photino. <see cref="FakeWebBrowserHost"/> injected.
/// </summary>
public class WebBrowserHistoryTests : IDisposable
{
	readonly Func<IWebBrowserHost> _originalFactory;
	readonly FakeWebBrowserHost _fake;
	readonly WebBrowser _wb;

	public WebBrowserHistoryTests()
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

	// ── CanGoBack / CanGoForward ──────────────────────────────────────────────

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

	// ── GoBack ────────────────────────────────────────────────────────────────

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

	[Fact] // B4
	public void GoBack_invokes_host_GoBack()
	{
		_wb.Navigate(new Uri("https://a.com/"));
		_wb.Navigate(new Uri("https://b.com/"));
		_wb.GoBack();
		Assert.True(_fake.GoBackCalled);
	}

	[Fact] // B3
	public void GoBack_raises_Navigating_with_mode_Back()
	{
		_wb.Navigate(new Uri("https://a.com/"));
		_wb.Navigate(new Uri("https://b.com/"));
		NavigationMode? capturedMode = null;
		_wb.Navigating += (_, e) => capturedMode = e.NavigationMode;
		_wb.GoBack();
		Assert.Equal(NavigationMode.Back, capturedMode);
	}

	[Fact] // B3
	public void GoBack_raises_Navigated_after_commit()
	{
		_wb.Navigate(new Uri("https://a.com/"));
		_wb.Navigate(new Uri("https://b.com/"));
		bool navigated = false;
		_wb.Navigated += (_, _) => navigated = true;
		_wb.GoBack();
		Assert.True(navigated);
	}

	[Fact] // B3
	public void GoBack_Navigating_Cancel_leaves_cursor_and_Source_unchanged()
	{
		var a = new Uri("https://a.com/"); var b = new Uri("https://b.com/");
		_wb.Navigate(a); _wb.Navigate(b);
		_wb.Navigating += (_, e) => e.Cancel = true;
		_wb.GoBack();
		Assert.Equal(b, _wb.Source);
		Assert.False(_fake.GoBackCalled);
	}

	// ── GoForward ─────────────────────────────────────────────────────────────

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

	[Fact] // B4
	public void GoForward_invokes_host_GoForward()
	{
		_wb.Navigate(new Uri("https://a.com/"));
		_wb.Navigate(new Uri("https://b.com/"));
		_wb.GoBack(); _wb.GoForward();
		Assert.True(_fake.GoForwardCalled);
	}

	[Fact] // B3
	public void GoForward_raises_Navigating_with_mode_Forward()
	{
		_wb.Navigate(new Uri("https://a.com/"));
		_wb.Navigate(new Uri("https://b.com/"));
		_wb.GoBack();
		NavigationMode? capturedMode = null;
		_wb.Navigating += (_, e) => capturedMode = e.NavigationMode;
		_wb.GoForward();
		Assert.Equal(NavigationMode.Forward, capturedMode);
	}

	[Fact] // B3
	public void GoForward_Navigating_Cancel_leaves_cursor_and_Source_unchanged()
	{
		var a = new Uri("https://a.com/"); var b = new Uri("https://b.com/");
		_wb.Navigate(a); _wb.Navigate(b); _wb.GoBack(); // cursor at a
		_wb.Navigating += (_, e) => e.Cancel = true;
		_wb.GoForward();
		Assert.Equal(a, _wb.Source);
		Assert.False(_fake.GoForwardCalled);
	}

	// ── Forward history trimming ───────────────────────────────────────────────

	[Fact]
	public void Navigate_after_GoBack_trims_forward_history()
	{
		_wb.Navigate(new Uri("https://a.com/"));
		_wb.Navigate(new Uri("https://b.com/"));
		_wb.Navigate(new Uri("https://c.com/"));
		_wb.GoBack();
		_wb.Navigate(new Uri("https://d.com/"));
		Assert.False(_wb.CanGoForward);
	}
}