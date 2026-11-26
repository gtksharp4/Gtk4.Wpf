using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Navigation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>PageFunction return: event order, journal removal and the wizard chain.</summary>
public class Round51aNavigationPageFunctionTests
{
	sealed class StubPage : Page
	{
		protected override Gtk.Widget CreateWidget() => throw new NotSupportedException("pure test");
	}

	sealed class IntFunction : PageFunction<int>
	{
		protected override Gtk.Widget CreateWidget() => throw new NotSupportedException("pure test");
		public void Finish(int result) => OnReturn(new ReturnEventArgs<int>(result));
		public void FinishNull() => OnReturn(null!);
	}

	static List<string> Record(NavigationService svc)
	{
		var order = new List<string>();
		svc.Navigating += (_, e) => order.Add("Navigating:" + e.NavigationMode);
		svc.Navigated += (_, _) => order.Add("Navigated");
		svc.LoadCompleted += (_, _) => order.Add("LoadCompleted");
		return order;
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Return_removes_the_page_function_and_shows_the_caller_before_raising_Return(bool keepAlive)
	{
		var svc = new NavigationService();
		var caller = new StubPage();
		svc.Navigate(caller);
		var pf = new IntFunction { KeepAlive = keepAlive };
		svc.Navigate(pf);
		var order = Record(svc);
		pf.Return += (_, _) => order.Add("Return:" + ReferenceEquals(svc.Content, caller));

		pf.Finish(5);

		Assert.Equal(new[] { "Navigating:Back", "Navigated", "LoadCompleted", "Return:True" }, order);
		Assert.Same(caller, svc.Content);
		Assert.False(svc.CanGoBack);
	}

	[Fact]
	public void Return_with_RemoveFromJournal_false_keeps_the_page_function_as_a_back_entry()
	{
		var svc = new NavigationService();
		var caller = new StubPage();
		svc.Navigate(caller);
		var pf = new IntFunction { RemoveFromJournal = false };
		svc.Navigate(pf);
		var order = Record(svc);

		pf.Finish(1);

		Assert.Equal(new[] { "Navigating:New", "Navigated", "LoadCompleted" }, order);
		svc.GoBack();
		Assert.Same(pf, svc.Content);
	}

	[Fact]
	public void Return_event_args_may_be_null()
	{
		var svc = new NavigationService();
		svc.Navigate(new StubPage());
		var pf = new IntFunction();
		ReturnEventArgs<int>? seen = new ReturnEventArgs<int>(0);
		pf.Return += (_, e) => seen = e;
		svc.Navigate(pf);

		pf.FinishNull();

		Assert.Null(seen);
	}

	[Fact]
	public void Return_is_raised_even_when_the_navigation_back_is_cancelled()
	{
		var svc = new NavigationService();
		svc.Navigate(new StubPage());
		var pf = new IntFunction();
		int? result = null;
		pf.Return += (_, e) => result = e.Result;
		svc.Navigate(pf);
		svc.Navigating += (_, e) => e.Cancel = true;

		pf.Finish(8);

		Assert.Equal(8, result);
		Assert.Same(pf, svc.Content);
	}

	[Fact]
	public void Wizard_chain_returns_through_every_level_and_ends_on_the_original_caller()
	{
		var svc = new NavigationService();
		var home = new StubPage();
		svc.Navigate(home);
		var launcher = new IntFunction();
		var page1 = new IntFunction();
		var page2 = new IntFunction();
		var results = new List<string>();
		launcher.Return += (_, e) => results.Add("launcher" + (e?.Result.ToString() ?? "null"));
		page1.Return += (_, e) => { results.Add("page1:" + e.Result); launcher.Finish(e.Result); };
		page2.Return += (_, e) => { results.Add("page2:" + e.Result); page1.Finish(e.Result); };
		svc.Navigate(launcher);
		svc.Navigate(page1);
		svc.Navigate(page2);

		page2.Finish(3);

		Assert.Equal(new[] { "page2:3", "page1:3", "launcher3" }, results);
		Assert.Same(home, svc.Content);
		Assert.False(svc.CanGoBack);
		Assert.False(svc.CanGoForward);
	}
}
