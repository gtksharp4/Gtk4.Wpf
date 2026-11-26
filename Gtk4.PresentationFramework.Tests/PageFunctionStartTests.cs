using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class PageFunctionStartTests
{
	sealed class StartingFunction : PageFunction<int>
	{
		public int Starts;
		public NavigationService? ServiceAtStart;
		protected override Gtk.Widget CreateWidget() => throw new NotSupportedException("pure test");
		protected override void Start()
		{
			Starts++;
			ServiceAtStart = NavigationService;
		}
	}

	sealed class StubPage : Page
	{
		protected override Gtk.Widget CreateWidget() => throw new NotSupportedException("pure test");
	}

	[Fact]
	public void Navigating_to_a_PageFunction_calls_Start_once_with_the_service_attached()
	{
		var svc = new NavigationService();
		var pf = new StartingFunction();
		svc.Navigate(pf);
		Assert.Equal(1, pf.Starts);
		Assert.Same(svc, pf.ServiceAtStart);
		svc.Navigate(new StubPage());
		svc.GoBack();
		Assert.Equal(1, pf.Starts);
	}

	[Fact]
	public void ResourceDictionary_Contains_accepts_a_key()
	{
		var rd = new ResourceDictionary();
		rd.Add("k", 1);
		Assert.True(rd.Contains("k"));
		Assert.False(rd.Contains("missing"));
	}
}
