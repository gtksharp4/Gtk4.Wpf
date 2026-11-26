using System;
using System.Windows.Controls;
using System.Windows.Navigation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class PageFunctionTests
{
	sealed class StubPage : Page
	{
		protected override Gtk.Widget CreateWidget() => throw new NotSupportedException("pure test");
	}

	sealed class IntFunction : PageFunction<int>
	{
		protected override Gtk.Widget CreateWidget() => throw new NotSupportedException("pure test");
		public void Finish(int result) => OnReturn(new ReturnEventArgs<int>(result));
	}

	[Fact]
	public void PageFunctionBase_is_a_Page_and_RemoveFromJournal_defaults_to_true()
	{
		var pf = new IntFunction();
		Assert.IsAssignableFrom<PageFunctionBase>(pf);
		Assert.IsAssignableFrom<Page>(pf);
		Assert.True(pf.RemoveFromJournal);
	}

	[Fact]
	public void ReturnEventArgs_exposes_Result_and_is_an_EventArgs()
	{
		var args = new ReturnEventArgs<string>("done");
		Assert.Equal("done", args.Result);
		Assert.IsAssignableFrom<EventArgs>(args);
		Assert.Null(new ReturnEventArgs<string>().Result);
	}

	[Fact]
	public void OnReturn_raises_Return_with_the_result_and_the_PageFunction_as_sender()
	{
		var svc = new NavigationService();
		svc.Navigate(new StubPage());
		var pf = new IntFunction();
		object? sender = null;
		int? result = null;
		pf.Return += (s, e) => { sender = s; result = e.Result; };
		svc.Navigate(pf);

		pf.Finish(7);

		Assert.Same(pf, sender);
		Assert.Equal(7, result);
	}

	[Fact]
	public void OnReturn_navigates_back_to_the_caller_and_removes_the_PageFunction_from_the_journal()
	{
		var svc = new NavigationService();
		var caller = new StubPage();
		svc.Navigate(caller);
		var pf = new IntFunction();
		svc.Navigate(pf);

		pf.Finish(1);

		Assert.Same(caller, svc.Content);
		Assert.False(svc.CanGoBack);
		Assert.False(svc.CanGoForward);
	}

	[Fact]
	public void OnReturn_keeps_the_PageFunction_in_the_journal_when_RemoveFromJournal_is_false()
	{
		var svc = new NavigationService();
		var caller = new StubPage();
		svc.Navigate(caller);
		var pf = new IntFunction { RemoveFromJournal = false };
		svc.Navigate(pf);

		pf.Finish(1);

		Assert.Same(caller, svc.Content);
		Assert.True(svc.CanGoBack);
		svc.GoBack();
		Assert.Same(pf, svc.Content);
	}

	[Fact]
	public void Chained_PageFunctions_each_return_to_their_own_caller()
	{
		var svc = new NavigationService();
		var launcher = new IntFunction();
		svc.Navigate(new StubPage());
		svc.Navigate(launcher);
		var step = new IntFunction();
		int? stepResult = null;
		step.Return += (_, e) => stepResult = e.Result;
		svc.Navigate(step);

		step.Finish(3);

		Assert.Equal(3, stepResult);
		Assert.Same(launcher, svc.Content);
		launcher.Finish(4);
		Assert.IsType<StubPage>(svc.Content);
	}

	[Fact]
	public void OnReturn_with_no_caller_only_raises_Return()
	{
		var svc = new NavigationService();
		var pf = new IntFunction();
		int? result = null;
		pf.Return += (_, e) => result = e.Result;
		svc.Navigate(pf);

		pf.Finish(9);

		Assert.Equal(9, result);
		Assert.Same(pf, svc.Content);
	}

	[Fact]
	public void OnReturn_before_navigation_still_raises_Return()
	{
		var pf = new IntFunction();
		int? result = null;
		pf.Return += (_, e) => result = e.Result;

		pf.Finish(2);

		Assert.Equal(2, result);
	}
}
