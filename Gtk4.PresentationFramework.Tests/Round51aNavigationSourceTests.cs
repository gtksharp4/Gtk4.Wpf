using System;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>NavigationService keeps a source URI only for content it still shows or can return to.</summary>
public class Round51aNavigationSourceTests
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
	public void Return_with_RemoveFromJournal_false_keeps_the_callers_uri()
	{
		var uri = new Uri("Round51a/SourceCaller.xaml", UriKind.Relative);
		GtkFormsXamlRegistry.Register(uri.OriginalString, () => new StubPage());
		var svc = new NavigationService();
		svc.Navigate(uri);
		var caller = svc.Content;
		svc.Navigate(new IntFunction { RemoveFromJournal = false });
		Assert.Null(svc.CurrentSource);

		((IntFunction)svc.Content!).Finish(1);

		Assert.Same(caller, svc.Content);
		Assert.Equal(uri, svc.CurrentSource);
	}

	[Fact]
	public void Refresh_that_resolves_a_new_instance_forgets_the_old_one()
	{
		var uri = new Uri("Round51a/SourceRefresh.xaml", UriKind.Relative);
		GtkFormsXamlRegistry.Register(uri.OriginalString, () => new StubPage());
		var svc = new NavigationService();
		svc.Navigate(uri);
		var old = svc.Content!;
		Assert.True(svc.HasSource(old));

		svc.Refresh();

		Assert.NotSame(old, svc.Content);
		Assert.False(svc.HasSource(old));
		Assert.True(svc.HasSource(svc.Content!));
	}

	[Fact]
	public void Return_that_removes_the_page_function_forgets_it()
	{
		var uri = new Uri("Round51a/SourcePf.xaml", UriKind.Relative);
		GtkFormsXamlRegistry.Register(uri.OriginalString, () => new IntFunction());
		var svc = new NavigationService();
		svc.Navigate(new StubPage());
		svc.Navigate(uri);
		var pf = (IntFunction)svc.Content!;
		Assert.True(svc.HasSource(pf));

		pf.Finish(1);

		Assert.False(svc.HasSource(pf));
	}

	[Fact]
	public void Refresh_keeps_the_uri_of_an_instance_the_journal_still_holds()
	{
		var uri = new Uri("Round51a/SourceTwice.xaml", UriKind.Relative);
		var created = 0;
		GtkFormsXamlRegistry.Register(uri.OriginalString, () =>
		{
			created++;
			return created == 1 ? new StubPage() : new IntFunction();
		});
		var svc = new NavigationService();
		svc.Navigate(uri);
		var caller = svc.Content!;
		svc.Navigate(new IntFunction { RemoveFromJournal = false });
		((IntFunction)svc.Content!).Finish(1);
		Assert.Same(caller, svc.Content);

		svc.Refresh();
		svc.GoBack();
		svc.GoBack();

		Assert.Same(caller, svc.Content);
		Assert.Equal(uri, svc.CurrentSource);
	}
}
